using BroadbandBilling.Application.Interfaces;
using BroadbandBilling.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DomainUser = BroadbandBilling.Domain.Entities.User;

namespace BroadbandBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Manager")]
public class UsersController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UsersController(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u => u.Username.Contains(search) || u.Email.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            if (role == "Admin")
                query = query.Where(u => u.UserType == UserType.Admin);
            else if (role == "Client")
                query = query.Where(u => u.UserType == UserType.Subscriber);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(u => u.Admin)
            .Include(u => u.Subscriber)
            .Select(u => new
            {
                id = u.Id,
                username = u.Username,
                email = u.Email,
                fullName = u.UserType == UserType.Admin && u.Admin != null
                    ? u.Admin.FullName
                    : u.Subscriber != null ? u.Subscriber.FullName : u.Username,
                role = u.UserType == UserType.Admin ? "Admin" : "Client",
                isActive = u.IsActive,
                subscriberId = u.SubscriberId,
                subscriberName = u.Subscriber != null ? u.Subscriber.FullName : (string?)null,
                lastLoginAt = u.LastLoginDate,
                createdAt = u.CreatedAt,
                updatedAt = u.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            items = users,
            totalCount,
            page,
            pageSize,
            totalPages
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Admin)
            .Include(u => u.Subscriber)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
            return NotFound(new { error = "المستخدم غير موجود" });

        return Ok(new
        {
            id = user.Id,
            username = user.Username,
            email = user.Email,
            fullName = user.UserType == UserType.Admin && user.Admin != null
                ? user.Admin.FullName
                : user.Subscriber != null ? user.Subscriber.FullName : user.Username,
            role = user.UserType == UserType.Admin ? "Admin" : "Client",
            isActive = user.IsActive,
            subscriberId = user.SubscriberId,
            subscriberName = user.Subscriber?.FullName,
            lastLoginAt = user.LastLoginDate,
            createdAt = user.CreatedAt,
            updatedAt = user.UpdatedAt
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        // Check username uniqueness
        var exists = await _context.Users.AnyAsync(u => u.Username == request.Username, cancellationToken);
        if (exists)
            return BadRequest(new { error = "اسم المستخدم موجود بالفعل" });

        var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (emailExists)
            return BadRequest(new { error = "البريد الإلكتروني موجود بالفعل" });

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        if (request.Role == "Admin")
        {
            var user = DomainUser.CreateAdmin(request.Username, request.Email, passwordHash);
            _context.Users.Add(user);

            var admin = Admin.Create(user.Id, request.FullName, AdminRole.Support);
            _context.Admins.Add(admin);

            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                fullName = request.FullName,
                role = "Admin",
                isActive = user.IsActive,
                subscriberId = (Guid?)null,
                subscriberName = (string?)null,
                lastLoginAt = (DateTime?)null,
                createdAt = user.CreatedAt,
                updatedAt = user.UpdatedAt
            });
        }
        else
        {
            // Client user - link to subscriber if provided
            Guid? subscriberId = null;
            string fullName = request.FullName;

            if (request.SubscriberId.HasValue)
            {
                var subscriber = await _context.Subscribers.FindAsync(new object[] { request.SubscriberId.Value }, cancellationToken);
                if (subscriber == null)
                    return BadRequest(new { error = "المشترك غير موجود" });
                subscriberId = subscriber.Id;
                fullName = subscriber.FullName;
            }

            var user = subscriberId.HasValue
                ? DomainUser.CreateSubscriber(request.Username, request.Email, passwordHash, subscriberId.Value)
                : DomainUser.CreateAdmin(request.Username, request.Email, passwordHash);

            if (!subscriberId.HasValue)
            {
                user.UserType = UserType.Subscriber;
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                fullName,
                role = "Client",
                isActive = user.IsActive,
                subscriberId,
                subscriberName = fullName,
                lastLoginAt = (DateTime?)null,
                createdAt = user.CreatedAt,
                updatedAt = user.UpdatedAt
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Admin)
            .Include(u => u.Subscriber)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
            return NotFound(new { error = "المستخدم غير موجود" });

        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            var usernameExists = await _context.Users.AnyAsync(u => u.Username == request.Username && u.Id != id, cancellationToken);
            if (usernameExists)
                return BadRequest(new { error = "اسم المستخدم موجود بالفعل" });
            user.Username = request.Username;
        }

        user.Email = request.Email;
        user.IsActive = request.IsActive;

        if (user.Admin != null)
        {
            user.Admin.FullName = request.FullName;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            id = user.Id,
            username = user.Username,
            email = user.Email,
            fullName = user.Admin?.FullName ?? user.Subscriber?.FullName ?? user.Username,
            role = user.UserType == UserType.Admin ? "Admin" : "Client",
            isActive = user.IsActive,
            subscriberId = user.SubscriberId,
            subscriberName = user.Subscriber?.FullName,
            lastLoginAt = user.LastLoginDate,
            createdAt = user.CreatedAt,
            updatedAt = user.UpdatedAt
        });
    }

    [HttpPut("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        if (user == null)
            return NotFound(new { error = "المستخدم غير موجود" });

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "تم إعادة تعيين كلمة المرور بنجاح" });
    }

    [HttpPut("{id}/unlock")]
    public async Task<IActionResult> UnlockAccount(Guid id, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        if (user == null)
            return NotFound(new { error = "المستخدم غير موجود" });

        user.UnlockAccount();
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "تم فتح قفل الحساب بنجاح" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Admin)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
            return NotFound(new { error = "المستخدم غير موجود" });

        if (user.Admin != null)
        {
            _context.Admins.Remove(user.Admin);
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "تم حذف المستخدم بنجاح" });
    }
}

public record CreateUserRequest
{
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string FullName { get; init; }
    public required string Password { get; init; }
    public string ConfirmPassword { get; init; } = string.Empty;
    public required string Role { get; init; }
    public Guid? SubscriberId { get; init; }
}

public record UpdateUserRequest
{
    public string? Username { get; init; }
    public required string Email { get; init; }
    public required string FullName { get; init; }
    public required string Role { get; init; }
    public bool IsActive { get; init; } = true;
}

public record ResetPasswordRequest
{
    public required string NewPassword { get; init; }
    public string ConfirmPassword { get; init; } = string.Empty;
}
