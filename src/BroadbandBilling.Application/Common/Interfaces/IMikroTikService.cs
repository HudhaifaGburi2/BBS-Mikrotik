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
    public long? LimitBytesIn { get; init; }
    public long? LimitBytesOut { get; init; }
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
