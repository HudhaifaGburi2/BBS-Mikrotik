using BroadbandBilling.Domain.Interfaces;

namespace BroadbandBilling.Domain.Entities;

public class LoginHistory : IEntity
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid UserId { get; set; }
    public required UserType UserType { get; set; }
    public required LoginStatus LoginStatus { get; set; }
    public string? FailureReason { get; set; }
    
    // Device/Location
    public string? IpAddress { get; set; }
    public string? DeviceName { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public string? Location { get; set; }
    
    public DateTime LoginDate { get; set; }
    
    // Navigation Properties
    public required User User { get; set; }
    
    private LoginHistory() { }
    
    public static LoginHistory CreateSuccess(
        Guid userId,
        UserType userType,
        string ipAddress,
        string? deviceName = null,
        string? browser = null,
        string? os = null,
        string? location = null)
    {
        return new LoginHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserType = userType,
            LoginStatus = LoginStatus.Success,
            IpAddress = ipAddress,
            DeviceName = deviceName,
            Browser = browser,
            OperatingSystem = os,
            Location = location,
            LoginDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            User = null!
        };
    }
    
    public static LoginHistory CreateFailure(
        Guid userId,
        UserType userType,
        string failureReason,
        string ipAddress,
        string? deviceName = null)
    {
        return new LoginHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserType = userType,
            LoginStatus = LoginStatus.Failed,
            FailureReason = failureReason,
            IpAddress = ipAddress,
            DeviceName = deviceName,
            LoginDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            User = null!
        };
    }
    
    public static LoginHistory CreateBlocked(
        Guid userId,
        UserType userType,
        string reason,
        string ipAddress)
    {
        return new LoginHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserType = userType,
            LoginStatus = LoginStatus.Blocked,
            FailureReason = reason,
            IpAddress = ipAddress,
            LoginDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            User = null!
        };
    }
}

public enum LoginStatus
{
    Success,
    Failed,
    Blocked
}
