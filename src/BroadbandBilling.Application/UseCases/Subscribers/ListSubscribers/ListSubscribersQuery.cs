using BroadbandBilling.Application.Common.DTOs;
using BroadbandBilling.Application.UseCases.Subscribers.DTOs;

namespace BroadbandBilling.Application.UseCases.Subscribers.ListSubscribers;

public class ListSubscribersQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public bool ActiveOnly { get; set; } = false;
}

public class ListSubscribersResult
{
    public PaginatedResult<SubscriberDto> Subscribers { get; set; } = null!;
}
