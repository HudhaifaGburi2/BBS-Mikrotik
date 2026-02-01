using AutoMapper;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Subscribers.DTOs;
using BroadbandBilling.Domain.Entities;

namespace BroadbandBilling.Application.UseCases.Subscribers.CreateSubscriber;

public class CreateSubscriberHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSubscriberHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CreateSubscriberResult> HandleAsync(CreateSubscriberCommand command, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Subscribers.EmailExistsAsync(command.Email, cancellationToken))
        {
            throw new InvalidOperationException($"Email '{command.Email}' already exists");
        }

        var subscriber = Subscriber.Create(
            command.FullName,
            command.Email,
            command.PhoneNumber,
            command.Address,
            command.NationalId
        );

        await _unitOfWork.Subscribers.AddAsync(subscriber, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var subscriberDto = _mapper.Map<SubscriberDto>(subscriber);

        return new CreateSubscriberResult { Subscriber = subscriberDto };
    }
}
