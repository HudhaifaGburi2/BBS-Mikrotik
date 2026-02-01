using AutoMapper;
using BroadbandBilling.Application.Common.DTOs;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Subscribers.DTOs;

namespace BroadbandBilling.Application.UseCases.Subscribers.ListSubscribers;

public class ListSubscribersHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ListSubscribersHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ListSubscribersResult> HandleAsync(ListSubscribersQuery query, CancellationToken cancellationToken = default)
    {
        var subscribers = query.ActiveOnly
            ? await _unitOfWork.Subscribers.GetActiveSubscribersAsync(cancellationToken)
            : await _unitOfWork.Subscribers.GetAllAsync(cancellationToken);

        var subscriberList = subscribers.ToList();
        var totalCount = subscriberList.Count;

        var pagedSubscribers = subscriberList
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var subscriberDtos = _mapper.Map<List<SubscriberDto>>(pagedSubscribers);

        var paginatedResult = PaginatedResult<SubscriberDto>.Create(
            subscriberDtos,
            totalCount,
            query.PageNumber,
            query.PageSize
        );

        return new ListSubscribersResult { Subscribers = paginatedResult };
    }
}
