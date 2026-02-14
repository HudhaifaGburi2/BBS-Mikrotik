using BroadbandBilling.Domain.Interfaces;

namespace BroadbandBilling.Domain.Entities;

public class Subscriber : IEntity
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string FullName { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Address { get; private set; }
    public string? NationalId { get; private set; }
    public string? City { get; private set; }
    public string? PostalCode { get; private set; }
    public bool IsActive { get; private set; }
    
    // Device Information
    public string? LastLoginDevice { get; private set; }
    public string? LastLoginBrowser { get; private set; }
    public string? LastLoginOS { get; private set; }
    
    // Network Information
    public string? MacAddress { get; private set; }
    public string? IpAddress { get; private set; }
    public string? MikroTikUsername { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation Properties
    public User? User { get; set; }
    private readonly List<Subscription> _subscriptions = new();
    public IReadOnlyCollection<Subscription> Subscriptions => _subscriptions.AsReadOnly();
    private readonly List<PppoeAccount> _pppoeAccounts = new();
    public IReadOnlyCollection<PppoeAccount> PppoeAccounts => _pppoeAccounts.AsReadOnly();

    private Subscriber()
    {
        FullName = null!;
        Email = null!;
        PhoneNumber = null!;
        Address = null!;
    }

    private Subscriber(string fullName, string email, string phoneNumber, 
        string address, string? nationalId)
    {
        Id = Guid.NewGuid();
        FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
        Address = address ?? throw new ArgumentNullException(nameof(address));
        NationalId = nationalId;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static Subscriber Create(string fullName, string email, 
        string phoneNumber, string address, string? nationalId = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required", nameof(fullName));

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            throw new ArgumentException("Valid email is required", nameof(email));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required", nameof(phoneNumber));

        return new Subscriber(fullName, email, phoneNumber, address, nationalId);
    }

    public void UpdateContactInfo(string email, string phoneNumber, string address)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            throw new ArgumentException("Valid email is required", nameof(email));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required", nameof(phoneNumber));

        Email = email;
        PhoneNumber = phoneNumber;
        Address = address ?? Address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePersonalInfo(string fullName, string? nationalId)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required", nameof(fullName));

        FullName = fullName;
        NationalId = nationalId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasActiveSubscription()
    {
        return _subscriptions.Any(s => s.IsActive());
    }

    public Subscription? GetActiveSubscription()
    {
        return _subscriptions.FirstOrDefault(s => s.IsActive());
    }
    
    public void UpdateDeviceInfo(string? deviceName, string? browser, string? os)
    {
        LastLoginDevice = deviceName;
        LastLoginBrowser = browser;
        LastLoginOS = os;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateNetworkInfo(string? macAddress, string? ipAddress, string? mikroTikUsername)
    {
        MacAddress = macAddress;
        IpAddress = ipAddress;
        MikroTikUsername = mikroTikUsername;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetUserId(Guid userId)
    {
        UserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }
}
