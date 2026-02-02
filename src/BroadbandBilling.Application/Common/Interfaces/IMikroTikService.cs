namespace BroadbandBilling.Application.Common.Interfaces;

public interface IMikroTikService
{
    Task<bool> ValidateCredentialsAsync(string ipAddress, int port, string apiUsername, string apiPassword, string pppoeUsername, string pppoePassword);
    Task<bool> CreatePppoeUserAsync(string ipAddress, int port, string apiUsername, string apiPassword, string pppoeUsername, string pppoePassword, string profileName);
    Task<int> GetActiveUsersCountAsync(string ipAddress, int port, string apiUsername, string apiPassword);
    
    Task<bool> TestConnectionAsync(Guid deviceId, CancellationToken cancellationToken = default);
    Task<bool> CreatePppoeUserAsync(Guid deviceId, string username, string password, string profileName, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<bool> UpdatePppoeUserAsync(Guid deviceId, string username, string? newPassword = null, string? newProfile = null, CancellationToken cancellationToken = default);
    Task<bool> DeletePppoeUserAsync(Guid deviceId, string username, CancellationToken cancellationToken = default);
    Task<bool> EnablePppoeUserAsync(Guid deviceId, string username, CancellationToken cancellationToken = default);
    Task<bool> DisablePppoeUserAsync(Guid deviceId, string username, CancellationToken cancellationToken = default);
    Task<IEnumerable<OnlineUserDto>> GetOnlineUsersAsync(Guid deviceId, CancellationToken cancellationToken = default);
    Task<UserSessionDto?> GetUserSessionAsync(Guid deviceId, string username, CancellationToken cancellationToken = default);
    Task<bool> DisconnectUserAsync(Guid deviceId, string username, CancellationToken cancellationToken = default);
}

public class OnlineUserDto
{
    public string Username { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public long UploadBytes { get; set; }
    public long DownloadBytes { get; set; }
    public DateTime ConnectedAt { get; set; }
    public TimeSpan Uptime { get; set; }
}

public class UserSessionDto
{
    public string Username { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public long UploadBytes { get; set; }
    public long DownloadBytes { get; set; }
    public long TotalBytes { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public TimeSpan? Uptime { get; set; }
    public bool IsOnline { get; set; }
}
