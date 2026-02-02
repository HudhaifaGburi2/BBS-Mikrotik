using BroadbandBilling.Domain.Interfaces;
using BroadbandBilling.Domain.ValueObjects;

namespace BroadbandBilling.Domain.Entities;

public class PppoeAccount : IEntity
{
    public Guid Id { get; private set; }
    public Guid SubscriberId { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public Guid MikroTikDeviceId { get; private set; }
    public string Username { get; private set; }
    public string Password { get; private set; }
    public IpAddress? StaticIpAddress { get; private set; }
    public string ProfileName { get; private set; }
    public bool IsEnabled { get; private set; }
    
    // MikroTik Sync Status
    public bool IsSyncedWithMikroTik { get; private set; }
    public DateTime? LastSyncDate { get; private set; }
    public DateTime? LastValidationDate { get; private set; }
    public ValidationStatus ValidationStatus { get; private set; }
    
    // Connection Details
    public string? MacAddress { get; private set; }
    public DateTime? LastConnectedAt { get; private set; }
    public DateTime? LastDisconnectedAt { get; private set; }
    public int TotalSessions { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Subscriber Subscriber { get; private set; }
    public Subscription Subscription { get; private set; }
    public MikroTikDevice MikroTikDevice { get; private set; }

    private PppoeAccount() { }

    private PppoeAccount(Guid subscriberId, Guid subscriptionId, 
        Guid mikrotikDeviceId, string username, string password, 
        IpAddress? staticIpAddress, string profileName)
    {
        Id = Guid.NewGuid();
        SubscriberId = subscriberId;
        SubscriptionId = subscriptionId;
        MikroTikDeviceId = mikrotikDeviceId;
        Username = username ?? throw new ArgumentNullException(nameof(username));
        Password = password ?? throw new ArgumentNullException(nameof(password));
        StaticIpAddress = staticIpAddress;
        ProfileName = profileName ?? throw new ArgumentNullException(nameof(profileName));
        IsEnabled = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static PppoeAccount Create(Guid subscriberId, Guid subscriptionId,
        Guid mikrotikDeviceId, string username, string password,
        string profileName, string? staticIp = null)
    {
        if (subscriberId == Guid.Empty)
            throw new ArgumentException("Subscriber ID is required", nameof(subscriberId));

        if (subscriptionId == Guid.Empty)
            throw new ArgumentException("Subscription ID is required", nameof(subscriptionId));

        if (mikrotikDeviceId == Guid.Empty)
            throw new ArgumentException("MikroTik Device ID is required", nameof(mikrotikDeviceId));

        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required", nameof(username));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required", nameof(password));

        IpAddress? ipAddress = null;
        if (!string.IsNullOrWhiteSpace(staticIp))
        {
            ipAddress = IpAddress.Create(staticIp);
        }

        return new PppoeAccount(subscriberId, subscriptionId, mikrotikDeviceId,
            username, password, ipAddress, profileName);
    }

    public void Enable()
    {
        IsEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        IsEnabled = false;
        LastDisconnectedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentException("Password cannot be empty", nameof(newPassword));

        Password = newPassword;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName))
            throw new ArgumentException("Profile name cannot be empty", nameof(profileName));

        ProfileName = profileName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStaticIp(string ipAddress)
    {
        StaticIpAddress = IpAddress.Create(ipAddress);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveStaticIp()
    {
        StaticIpAddress = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordConnection()
    {
        LastConnectedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordDisconnection()
    {
        LastDisconnectedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsOnline()
    {
        return IsEnabled && 
               LastConnectedAt.HasValue && 
               (!LastDisconnectedAt.HasValue || LastConnectedAt > LastDisconnectedAt);
    }

    public TimeSpan? GetSessionDuration()
    {
        if (!LastConnectedAt.HasValue) return null;
        
        var endTime = LastDisconnectedAt ?? DateTime.UtcNow;
        return endTime - LastConnectedAt.Value;
    }
    
    public void MarkAsValidated(bool isValid)
    {
        ValidationStatus = isValid ? ValidationStatus.Valid : ValidationStatus.Invalid;
        LastValidationDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void MarkAsSynced()
    {
        IsSyncedWithMikroTik = true;
        LastSyncDate = DateTime.UtcNow;
        ValidationStatus = ValidationStatus.Valid;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void MarkAsSyncFailed()
    {
        IsSyncedWithMikroTik = false;
        ValidationStatus = ValidationStatus.Invalid;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetMacAddress(string macAddress)
    {
        MacAddress = macAddress;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void IncrementSessionCount()
    {
        TotalSessions++;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum ValidationStatus
{
    Pending,
    Valid,
    Invalid,
    NotFound,
    Disabled
}
