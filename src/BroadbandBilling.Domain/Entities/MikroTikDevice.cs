using BroadbandBilling.Domain.Interfaces;
using BroadbandBilling.Domain.ValueObjects;

namespace BroadbandBilling.Domain.Entities;

public class MikroTikDevice : IEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public IpAddress IpAddress { get; private set; }
    public int Port { get; private set; }
    public string Username { get; private set; }
    public string Password { get; private set; }
    public bool IsActive { get; private set; }
    public string? Location { get; private set; }
    public string? Description { get; private set; }
    
    // Connection Status
    public bool IsOnline { get; private set; }
    public DateTime? LastPingDate { get; private set; }
    public DateTime? LastConnectedAt { get; private set; }
    
    // Capacity
    public int MaxUsers { get; private set; }
    public int CurrentActiveUsers { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private MikroTikDevice()
    {
        Name = null!;
        IpAddress = null!;
        Username = null!;
        Password = null!;
    }

    private MikroTikDevice(string name, IpAddress ipAddress, int port,
        string username, string password, string? location)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
        Port = port > 0 && port <= 65535 ? port : throw new ArgumentException("Invalid port number", nameof(port));
        Username = username ?? throw new ArgumentNullException(nameof(username));
        Password = password ?? throw new ArgumentNullException(nameof(password));
        Location = location;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static MikroTikDevice Create(string name, string ipAddress, int port,
        string username, string password, string? location = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Device name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required", nameof(username));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required", nameof(password));

        return new MikroTikDevice(
            name,
            IpAddress.Create(ipAddress),
            port,
            username,
            password,
            location
        );
    }

    public void UpdateCredentials(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required", nameof(username));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required", nameof(password));

        Username = username;
        Password = password;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateConnectionInfo(string ipAddress, int port)
    {
        IpAddress = IpAddress.Create(ipAddress);
        Port = port > 0 && port <= 65535 ? port : throw new ArgumentException("Invalid port number");
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLocation(string location)
    {
        Location = location;
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

    public void RecordConnection()
    {
        LastConnectedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsRecentlyConnected(int minutesThreshold = 30)
    {
        if (!LastConnectedAt.HasValue) return false;
        return (DateTime.UtcNow - LastConnectedAt.Value).TotalMinutes <= minutesThreshold;
    }
}
