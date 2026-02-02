using BroadbandBilling.Domain.Interfaces;

namespace BroadbandBilling.Domain.Entities;

public class Admin : IEntity
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid UserId { get; set; }
    public required string FullName { get; set; }
    public required AdminRole Role { get; set; }
    public string? Permissions { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public required User User { get; set; }
    
    private Admin() { }
    
    public static Admin Create(Guid userId, string fullName, AdminRole role, string? permissions = null)
    {
        return new Admin
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FullName = fullName,
            Role = role,
            Permissions = permissions,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            User = null!
        };
    }
    
    public void UpdateRole(AdminRole newRole)
    {
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdatePermissions(string permissions)
    {
        Permissions = permissions;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum AdminRole
{
    SuperAdmin,
    Manager,
    Support,
    Accountant
}
