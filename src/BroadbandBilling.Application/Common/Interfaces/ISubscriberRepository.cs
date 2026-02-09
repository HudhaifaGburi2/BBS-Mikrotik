using BroadbandBilling.Domain.Entities;

namespace BroadbandBilling.Application.Common.Interfaces;

public interface ISubscriberRepository : IRepository<Subscriber>
{
    Task<Subscriber?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Subscriber?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<Subscriber?> GetWithSubscriptionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscriber>> GetActiveSubscribersAsync(CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<PagedResult<Subscriber>> GetPagedAsync(int page, int pageSize, string? search = null, bool? isActive = null, bool? hasActiveSubscription = null, string? sortBy = null, bool sortDescending = false, CancellationToken cancellationToken = default);
    void Delete(Subscriber subscriber);
}

public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
