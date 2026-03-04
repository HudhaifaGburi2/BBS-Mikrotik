namespace BroadbandBilling.Application.Common.Interfaces;

public interface IMikroTikService
{
    Task<MikroTikResult> TestConnectionAsync(MikroTikConnectionRequest request, CancellationToken cancellationToken = default);
    Task<MikroTikResult<List<PppUserDto>>> GetPppUsersAsync(MikroTikConnectionRequest request, CancellationToken cancellationToken = default);
    Task<MikroTikResult<PppUserDto>> AddPppUserAsync(AddPppUserRequest request, CancellationToken cancellationToken = default);
    Task<MikroTikResult> DeletePppUserAsync(DeletePppUserRequest request, CancellationToken cancellationToken = default);
    Task<MikroTikResult> ActivateUserAsync(DeletePppUserRequest request, CancellationToken cancellationToken = default);
    Task<MikroTikResult> DeactivateUserAsync(DeletePppUserRequest request, CancellationToken cancellationToken = default);
    Task<MikroTikResult<List<ActiveSessionDto>>> GetActiveSessionsAsync(MikroTikConnectionRequest request, CancellationToken cancellationToken = default);
    Task<MikroTikResult<ActiveSessionDto?>> GetUserSessionAsync(DeletePppUserRequest request, CancellationToken cancellationToken = default);
    Task<MikroTikResult> DisconnectUserAsync(DeletePppUserRequest request, CancellationToken cancellationToken = default);
    Task<MikroTikResult> UpdateUserProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<MikroTikResult<List<PppProfileDto>>> GetPppProfilesAsync(MikroTikConnectionRequest request, CancellationToken cancellationToken = default);
    Task<MikroTikResult<PppProfileDto>> AddPppProfileAsync(AddProfileRequest request, CancellationToken cancellationToken = default);
    Task<MikroTikResult> UpdatePppProfileAsync(UpdatePppProfileRequest request, CancellationToken cancellationToken = default);
    Task<MikroTikResult> DeletePppProfileAsync(DeleteProfileRequest request, CancellationToken cancellationToken = default);
    Task<MikroTikResult<PppUserDto>> UpdatePppUserAsync(UpdatePppUserRequest request, CancellationToken cancellationToken = default);
    Task<MikroTikResult> ResetUserQuotaAsync(ResetUserQuotaRequest request, CancellationToken cancellationToken = default);
    Task<MikroTikResult<PppUserDto>> GetPppUserAsync(DeletePppUserRequest request, CancellationToken cancellationToken = default);
}

public record MikroTikResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Error { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static MikroTikResult SuccessResult(string message) => new() { Success = true, Message = message };
    public static MikroTikResult FailureResult(string message, string? error = null) => new() { Success = false, Message = message, Error = error };
}

public record MikroTikResult<T> : MikroTikResult
{
    public T? Data { get; init; }

    public static MikroTikResult<T> SuccessResult(T data, string message) => new() { Success = true, Message = message, Data = data };
    public static new MikroTikResult<T> FailureResult(string message, string? error = null) => new() { Success = false, Message = message, Error = error };
}

public record MikroTikConnectionRequest
{
    public string? Host { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
}

public record AddPppUserRequest : MikroTikConnectionRequest
{
    public required string PppUsername { get; init; }
    public required string PppPassword { get; init; }
    public string? Profile { get; init; }
    public string? Service { get; init; }
    public long? LimitBytesTotal { get; init; }
    public string? Comment { get; init; }
}

public record DeletePppUserRequest : MikroTikConnectionRequest
{
    public required string PppUsername { get; init; }
}

public record UpdateProfileRequest : MikroTikConnectionRequest
{
    public required string PppUsername { get; init; }
    public required string NewProfile { get; init; }
    public string? NewPassword { get; init; }
}

public record AddProfileRequest : MikroTikConnectionRequest
{
    public required string ProfileName { get; init; }
    public required string RateLimit { get; init; }
    public string? LocalAddress { get; init; }
    public string? RemoteAddress { get; init; }
}

public record UpdatePppProfileRequest : MikroTikConnectionRequest
{
    public required string ProfileName { get; init; }
    public required string RateLimit { get; init; }
}

public record DeleteProfileRequest : MikroTikConnectionRequest
{
    public required string ProfileName { get; init; }
}

public record UpdatePppUserRequest : MikroTikConnectionRequest
{
    public required string PppUsername { get; init; }
    public string? NewPassword { get; init; }
    public string? NewProfile { get; init; }
    public long? LimitBytesIn { get; init; }
    public long? LimitBytesOut { get; init; }
    public string? Comment { get; init; }
    public string? CallerId { get; init; }
}

public record ResetUserQuotaRequest : MikroTikConnectionRequest
{
    public required string PppUsername { get; init; }
}

public record PppUserDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Profile { get; init; } = string.Empty;
    public string Service { get; init; } = string.Empty;
    public bool Disabled { get; init; }
    public string? RemoteAddress { get; init; }
    public string? LocalAddress { get; init; }
    public string? CallerId { get; init; }
    public string? Comment { get; init; }
    public long LimitBytesIn { get; init; }
    public long LimitBytesOut { get; init; }
    public long LimitBytesTotal { get; init; }
    public bool IsOnline { get; init; }
    public string? LastLoggedOut { get; init; }
}

public record ActiveSessionDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Service { get; init; } = string.Empty;
    public string? CallerId { get; init; }
    public string? Address { get; init; }
    public string? Uptime { get; init; }
    public string? Encoding { get; init; }
    public string? SessionId { get; init; }
    public long BytesIn { get; init; }
    public long BytesOut { get; init; }
    public long LimitBytesIn { get; init; }
    public long LimitBytesOut { get; init; }
    public long LimitBytesTotal => LimitBytesIn; // Use LimitBytesIn as total limit (ISP standard)
    public long BytesUsed => BytesIn + BytesOut;
    public long BytesRemaining => LimitBytesTotal > 0 ? Math.Max(0, LimitBytesTotal - BytesUsed) : 0;
    public bool HasQuota => LimitBytesTotal > 0;
    public bool IsQuotaExceeded => HasQuota && BytesUsed >= LimitBytesTotal;
    public int UsagePercent => HasQuota ? (int)Math.Min(100, (BytesUsed * 100) / LimitBytesTotal) : 0;
}

public record PppProfileDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? LocalAddress { get; init; }
    public string? RemoteAddress { get; init; }
    public string? RateLimit { get; init; }
    public bool OnlyOne { get; init; }
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

/// <summary>
/// PPP User enriched with SQL subscription data
/// </summary>
public class EnrichedPppUserDto
{
    // MikroTik data
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Profile { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public bool Disabled { get; set; }
    public string? RemoteAddress { get; set; }
    public string? LocalAddress { get; set; }
    public string? CallerId { get; set; }
    public string? Comment { get; set; }
    public long LimitBytesIn { get; set; }
    public long LimitBytesOut { get; set; }
    public long LimitBytesTotal { get; set; }
    public bool IsOnline { get; set; }
    public string? LastLoggedOut { get; set; }
    
    // SQL subscription data
    public Guid? SubscriptionId { get; set; }
    public string? SubscriptionStatus { get; set; }
    public string? PlanName { get; set; }
    public int PlanDataLimitGB { get; set; }
    public long PlanDataLimitBytes { get; set; }
    public long DataUsedBytes { get; set; }
    public long DataRemainingBytes { get; set; }
    public int DataUsagePercent { get; set; }
    public bool IsUnlimited { get; set; }
    public bool DataLimitExceeded { get; set; }
    public bool IsSuspended { get; set; }
}

/// <summary>
/// Active session enriched with SQL subscription data
/// </summary>
public class EnrichedActiveSessionDto
{
    // MikroTik session data
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string? CallerId { get; set; }
    public string? Address { get; set; }
    public string? Uptime { get; set; }
    public string? Encoding { get; set; }
    public string? SessionId { get; set; }
    public long BytesIn { get; set; }
    public long BytesOut { get; set; }
    public long SessionBytesUsed { get; set; }
    public long LimitBytesIn { get; set; }
    public long LimitBytesOut { get; set; }
    
    // SQL subscription data
    public Guid? SubscriptionId { get; set; }
    public string? PlanName { get; set; }
    public int PlanDataLimitGB { get; set; }
    public long PlanDataLimitBytes { get; set; }
    public long TotalDataUsedBytes { get; set; }
    public long DataRemainingBytes { get; set; }
    public int DataUsagePercent { get; set; }
    public bool IsUnlimited { get; set; }
    public bool DataLimitExceeded { get; set; }
}
