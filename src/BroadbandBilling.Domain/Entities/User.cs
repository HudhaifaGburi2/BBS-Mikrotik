using BroadbandBilling.Domain.Interfaces;

namespace BroadbandBilling.Domain.Entities;

public class User : IEntity
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required UserType UserType { get; set; }
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public string? LastLoginIpAddress { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    // Foreign Keys
    public Guid? SubscriberId { get; set; }
    
    // Navigation Properties
    public Subscriber? Subscriber { get; set; }
    public Admin? Admin { get; set; }
    public ICollection<LoginHistory> LoginHistory { get; set; } = new List<LoginHistory>();
    
    private User() { }
    
    public static User CreateAdmin(string username, string email, string passwordHash)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            UserType = UserType.Admin,
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    public static User CreateSubscriber(string username, string email, string passwordHash, Guid subscriberId)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            UserType = UserType.Subscriber,
            SubscriberId = subscriberId,
            IsActive = true,
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    public bool IsLockedOut() => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    
    public bool IsLocked() => IsLockedOut();
    
    public void RecordSuccessfulLogin(string ipAddress)
    {
        LastLoginDate = DateTime.UtcNow;
        LastLoginIpAddress = ipAddress;
        AccessFailedCount = 0;
        LockoutEnd = null;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RecordFailedLogin()
    {
        AccessFailedCount++;
        if (AccessFailedCount >= 5)
        {
            LockoutEnd = DateTime.UtcNow.AddMinutes(30);
        }
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateRefreshToken(string refreshToken, DateTime expiryTime)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UnlockAccount()
    {
        LockoutEnd = null;
        AccessFailedCount = 0;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum UserType
{
    Admin,
    Subscriber
}
