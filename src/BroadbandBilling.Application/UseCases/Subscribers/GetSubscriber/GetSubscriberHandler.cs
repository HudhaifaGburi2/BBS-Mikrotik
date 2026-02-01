using AutoMapper;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Subscribers.DTOs;
using BroadbandBilling.Domain.Exceptions;

namespace BroadbandBilling.Application.UseCases.Subscribers.GetSubscriber;

public class GetSubscriberHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSubscriberHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<GetSubscriberResult> HandleAsync(GetSubscriberQuery query, CancellationToken cancellationToken = default)
    {
        var subscriber = await _unitOfWork.Subscribers.GetByIdAsync(query.SubscriberId, cancellationToken);
        
        if (subscriber == null)
        {
            throw new SubscriberNotFoundException(query.SubscriberId);
        }

        var subscriberDto = _mapper.Map<SubscriberDto>(subscriber);

        return new GetSubscriberResult { Subscriber = subscriberDto };
    }
}
