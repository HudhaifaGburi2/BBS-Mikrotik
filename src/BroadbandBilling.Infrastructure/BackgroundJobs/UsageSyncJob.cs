using BroadbandBilling.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Infrastructure.BackgroundJobs;

public class UsageSyncJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMikroTikService _mikrotikService;
    private readonly ILogger<UsageSyncJob> _logger;

    public UsageSyncJob(
        IUnitOfWork unitOfWork,
        IMikroTikService mikrotikService,
        ILogger<UsageSyncJob> logger)
    {
        _unitOfWork = unitOfWork;
        _mikrotikService = mikrotikService;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting usage sync job");

        try
        {
            var activeDevices = await _unitOfWork.MikroTikDevices.GetActiveDevicesAsync();

            foreach (var device in activeDevices)
            {
                try
                {
                    var onlineUsers = await _mikrotikService.GetOnlineUsersAsync(device.Id);

                    _logger.LogInformation("Found {Count} online users on device {DeviceId}", 
                        onlineUsers.Count(), device.Id);

                    await _unitOfWork.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing usage for device {DeviceId}", device.Id);
                }
            }

            _logger.LogInformation("Usage sync job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during usage sync job");
            throw;
        }
    }
}
