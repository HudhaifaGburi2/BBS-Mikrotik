using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using BroadbandBilling.Application.Common.DTOs;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Subscriptions.CreateSubscription;
using BroadbandBilling.Application.UseCases.Subscriptions.RenewSubscription;
using BroadbandBilling.Application.UseCases.Subscriptions.DTOs;

namespace BroadbandBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly CreateSubscriptionHandler _createSubscriptionHandler;
    private readonly RenewSubscriptionHandler _renewSubscriptionHandler;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SubscriptionsController(
        CreateSubscriptionHandler createSubscriptionHandler,
        RenewSubscriptionHandler renewSubscriptionHandler,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _createSubscriptionHandler = createSubscriptionHandler;
        _renewSubscriptionHandler = renewSubscriptionHandler;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<SubscriptionDto>>>> GetAll()
    {
        var subscriptions = await _unitOfWork.Subscriptions.GetAllAsync();
        var subscriptionDtos = _mapper.Map<IEnumerable<SubscriptionDto>>(subscriptions);
        
        return Ok(ApiResponse<IEnumerable<SubscriptionDto>>
            .SuccessResponse(subscriptionDtos, "Subscriptions retrieved successfully"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> GetById(Guid id)
    {
        var subscription = await _unitOfWork.Subscriptions.GetWithDetailsAsync(id);
        if (subscription == null)
        {
            return NotFound(ApiResponse<SubscriptionDto>
                .FailureResponse("Subscription not found", $"Subscription with ID {id} not found"));
        }

        var subscriptionDto = _mapper.Map<SubscriptionDto>(subscription);
        
        return Ok(ApiResponse<SubscriptionDto>
            .SuccessResponse(subscriptionDto, "Subscription retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> Create([FromBody] CreateSubscriptionCommand command)
    {
        var validator = new CreateSubscriptionValidator();
        var validationResult = await validator.ValidateAsync(command);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<SubscriptionDto>.FailureResponse("Validation failed", errors));
        }

        var result = await _createSubscriptionHandler.HandleAsync(command);
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Subscription.Id },
            ApiResponse<SubscriptionDto>.SuccessResponse(result.Subscription, "Subscription created successfully"));
    }

    [HttpPost("{id}/renew")]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> Renew(Guid id)
    {
        var command = new RenewSubscriptionCommand { SubscriptionId = id };
        var result = await _renewSubscriptionHandler.HandleAsync(command);
        
        return Ok(ApiResponse<SubscriptionDto>
            .SuccessResponse(result.Subscription, "Subscription renewed successfully"));
    }

    [HttpPost("{id}/suspend")]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> Suspend(Guid id, [FromBody] SuspendSubscriptionDto dto)
    {
        var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(id);
        if (subscription == null)
        {
            return NotFound(ApiResponse<SubscriptionDto>
                .FailureResponse("Subscription not found", $"Subscription with ID {id} not found"));
        }

        subscription.Suspend();
        _unitOfWork.Subscriptions.Update(subscription);
        await _unitOfWork.CommitAsync();

        var subscriptionDto = _mapper.Map<SubscriptionDto>(subscription);
        
        return Ok(ApiResponse<SubscriptionDto>
            .SuccessResponse(subscriptionDto, "Subscription suspended successfully"));
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> Cancel(Guid id, [FromBody] CancelSubscriptionDto dto)
    {
        var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(id);
        if (subscription == null)
        {
            return NotFound(ApiResponse<SubscriptionDto>
                .FailureResponse("Subscription not found", $"Subscription with ID {id} not found"));
        }

        subscription.Cancel(dto.Reason);
        _unitOfWork.Subscriptions.Update(subscription);
        await _unitOfWork.CommitAsync();

        var subscriptionDto = _mapper.Map<SubscriptionDto>(subscription);
        
        return Ok(ApiResponse<SubscriptionDto>
            .SuccessResponse(subscriptionDto, "Subscription cancelled successfully"));
    }
}
