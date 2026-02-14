using AutoMapper;
using BroadbandBilling.Application.Common.DTOs;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Subscriptions.CreateSubscription;
using BroadbandBilling.Application.UseCases.Subscriptions.DTOs;
using BroadbandBilling.Application.UseCases.Subscriptions.RenewSubscription;
using Microsoft.Extensions.Logging;

namespace BroadbandBilling.Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly CreateSubscriptionHandler _createSubscriptionHandler;
    private readonly RenewSubscriptionHandler _renewSubscriptionHandler;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        CreateSubscriptionHandler createSubscriptionHandler,
        RenewSubscriptionHandler renewSubscriptionHandler,
        ILogger<SubscriptionService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createSubscriptionHandler = createSubscriptionHandler;
        _renewSubscriptionHandler = renewSubscriptionHandler;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<SubscriptionDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var subscriptions = await _unitOfWork.Subscriptions.GetAllAsync(cancellationToken);
        var subscriptionDtos = _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);
        return ApiResponse<IEnumerable<SubscriptionDto>>.SuccessResponse(subscriptionDtos, "Subscriptions retrieved successfully");
    }

    public async Task<ApiResponse<SubscriptionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscription = await _unitOfWork.Subscriptions.GetWithDetailsAsync(id, cancellationToken);
        if (subscription == null)
        {
            return ApiResponse<SubscriptionDto>.FailureResponse("Subscription not found", $"Subscription with ID {id} not found");
        }

        var subscriptionDto = _mapper.Map<SubscriptionDto>(subscription);
        return ApiResponse<SubscriptionDto>.SuccessResponse(subscriptionDto, "Subscription retrieved successfully");
    }

    public async Task<ApiResponse<SubscriptionDto>> CreateAsync(CreateSubscriptionCommand command, CancellationToken cancellationToken = default)
    {
        var validator = new CreateSubscriptionValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ApiResponse<SubscriptionDto>.FailureResponse("Validation failed", errors);
        }

        var result = await _createSubscriptionHandler.HandleAsync(command, cancellationToken);
        return ApiResponse<SubscriptionDto>.SuccessResponse(result.Subscription, "Subscription created successfully");
    }

    public async Task<ApiResponse<SubscriptionDto>> RenewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new RenewSubscriptionCommand { SubscriptionId = id };
        var result = await _renewSubscriptionHandler.HandleAsync(command, cancellationToken);
        return ApiResponse<SubscriptionDto>.SuccessResponse(result.Subscription, "Subscription renewed successfully");
    }

    public async Task<ApiResponse<SubscriptionDto>> SuspendAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(id, cancellationToken);
        if (subscription == null)
        {
            return ApiResponse<SubscriptionDto>.FailureResponse("Subscription not found", $"Subscription with ID {id} not found");
        }

        subscription.Suspend();
        _unitOfWork.Subscriptions.Update(subscription);
        await _unitOfWork.CommitAsync(cancellationToken);

        var subscriptionDto = _mapper.Map<SubscriptionDto>(subscription);
        return ApiResponse<SubscriptionDto>.SuccessResponse(subscriptionDto, "Subscription suspended successfully");
    }

    public async Task<ApiResponse<SubscriptionDto>> CancelAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(id, cancellationToken);
        if (subscription == null)
        {
            return ApiResponse<SubscriptionDto>.FailureResponse("Subscription not found", $"Subscription with ID {id} not found");
        }

        subscription.Cancel(reason);
        _unitOfWork.Subscriptions.Update(subscription);
        await _unitOfWork.CommitAsync(cancellationToken);

        var subscriptionDto = _mapper.Map<SubscriptionDto>(subscription);
        return ApiResponse<SubscriptionDto>.SuccessResponse(subscriptionDto, "Subscription cancelled successfully");
    }
}
