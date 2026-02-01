using BroadbandBilling.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Infrastructure.Services;

public class MikroTikService : IMikroTikService
{
    private readonly ILogger<MikroTikService> _logger;

    public MikroTikService(ILogger<MikroTikService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> TestConnectionAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Testing connection to MikroTik device {DeviceId}", deviceId);
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> CreatePppoeUserAsync(Guid deviceId, string username, string password, string profileName, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating PPPoE user {Username} on device {DeviceId}", username, deviceId);
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> UpdatePppoeUserAsync(Guid deviceId, string username, string? newPassword = null, string? newProfile = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating PPPoE user {Username} on device {DeviceId}", username, deviceId);
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> DeletePppoeUserAsync(Guid deviceId, string username, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting PPPoE user {Username} from device {DeviceId}", username, deviceId);
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> EnablePppoeUserAsync(Guid deviceId, string username, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Enabling PPPoE user {Username} on device {DeviceId}", username, deviceId);
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> DisablePppoeUserAsync(Guid deviceId, string username, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Disabling PPPoE user {Username} on device {DeviceId}", username, deviceId);
        await Task.CompletedTask;
        return true;
    }

    public async Task<IEnumerable<OnlineUserDto>> GetOnlineUsersAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching online users from device {DeviceId}", deviceId);
        await Task.CompletedTask;
        return new List<OnlineUserDto>();
    }

    public async Task<UserSessionDto?> GetUserSessionAsync(Guid deviceId, string username, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching session for user {Username} on device {DeviceId}", username, deviceId);
        await Task.CompletedTask;
        return null;
    }

    public async Task<bool> DisconnectUserAsync(Guid deviceId, string username, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Disconnecting user {Username} on device {DeviceId}", username, deviceId);
        await Task.CompletedTask;
        return true;
    }
}
