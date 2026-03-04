using BroadbandBilling.Application.Common.DTOs;
using BroadbandBilling.Application.UseCases.Subscriptions.CreateSubscription;
using BroadbandBilling.Application.UseCases.Subscriptions.DTOs;
using BroadbandBilling.Application.UseCases.Subscriptions.RenewSubscription;

namespace BroadbandBilling.Application.Common.Interfaces;

public interface ISubscriptionService
{
    Task<ApiResponse<IEnumerable<SubscriptionDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<SubscriptionDto>>> GetBySubscriberIdAsync(Guid subscriberId, CancellationToken cancellationToken = default);
    Task<ApiResponse<SubscriptionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<SubscriptionDto>> CreateAsync(CreateSubscriptionCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<SubscriptionDto>> RenewAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<SubscriptionDto>> SuspendAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<SubscriptionDto>> CancelAsync(Guid id, string reason, CancellationToken cancellationToken = default);
}
