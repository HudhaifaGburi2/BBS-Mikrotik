using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using BroadbandBilling.Application.Common.DTOs;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Subscribers.CreateSubscriber;
using BroadbandBilling.Application.UseCases.Subscribers.GetSubscriber;
using BroadbandBilling.Application.UseCases.Subscribers.ListSubscribers;

namespace BroadbandBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscribersController : ControllerBase
{
    private readonly CreateSubscriberHandler _createSubscriberHandler;
    private readonly GetSubscriberHandler _getSubscriberHandler;
    private readonly ListSubscribersHandler _listSubscribersHandler;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SubscribersController(
        CreateSubscriberHandler createSubscriberHandler,
        GetSubscriberHandler getSubscriberHandler,
        ListSubscribersHandler listSubscribersHandler,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _createSubscriberHandler = createSubscriberHandler;
        _getSubscriberHandler = getSubscriberHandler;
        _listSubscribersHandler = listSubscribersHandler;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<Application.UseCases.Subscribers.DTOs.SubscriberDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool activeOnly = false)
    {
        var query = new ListSubscribersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            ActiveOnly = activeOnly
        };

        var result = await _listSubscribersHandler.HandleAsync(query);
        
        return Ok(ApiResponse<PaginatedResult<Application.UseCases.Subscribers.DTOs.SubscriberDto>>
            .SuccessResponse(result.Subscribers, "Subscribers retrieved successfully"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Application.UseCases.Subscribers.DTOs.SubscriberDto>>> GetById(Guid id)
    {
        var query = new GetSubscriberQuery { SubscriberId = id };
        var result = await _getSubscriberHandler.HandleAsync(query);
        
        return Ok(ApiResponse<Application.UseCases.Subscribers.DTOs.SubscriberDto>
            .SuccessResponse(result.Subscriber, "Subscriber retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Application.UseCases.Subscribers.DTOs.SubscriberDto>>> Create(
        [FromBody] CreateSubscriberCommand command)
    {
        var validator = new CreateSubscriberValidator();
        var validationResult = await validator.ValidateAsync(command);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<Application.UseCases.Subscribers.DTOs.SubscriberDto>
                .FailureResponse("Validation failed", errors));
        }

        var result = await _createSubscriberHandler.HandleAsync(command);
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Subscriber.Id },
            ApiResponse<Application.UseCases.Subscribers.DTOs.SubscriberDto>
                .SuccessResponse(result.Subscriber, "Subscriber created successfully"));
    }

    [HttpGet("{id}/subscriptions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Application.UseCases.Subscriptions.DTOs.SubscriptionDto>>>> GetSubscriptions(Guid id)
    {
        var subscriptions = await _unitOfWork.Subscriptions.GetBySubscriberIdAsync(id);
        var subscriptionDtos = _mapper.Map<IEnumerable<Application.UseCases.Subscriptions.DTOs.SubscriptionDto>>(subscriptions);
        
        return Ok(ApiResponse<IEnumerable<Application.UseCases.Subscriptions.DTOs.SubscriptionDto>>
            .SuccessResponse(subscriptionDtos, "Subscriptions retrieved successfully"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Application.UseCases.Subscribers.DTOs.SubscriberDto>>> Update(
        Guid id,
        [FromBody] Application.UseCases.Subscribers.DTOs.UpdateSubscriberDto dto)
    {
        var subscriber = await _unitOfWork.Subscribers.GetByIdAsync(id);
        if (subscriber == null)
        {
            return NotFound(ApiResponse<Application.UseCases.Subscribers.DTOs.SubscriberDto>
                .FailureResponse("Subscriber not found", $"Subscriber with ID {id} not found"));
        }

        _unitOfWork.Subscribers.Update(subscriber);
        await _unitOfWork.CommitAsync();

        var subscriberDto = _mapper.Map<Application.UseCases.Subscribers.DTOs.SubscriberDto>(subscriber);
        
        return Ok(ApiResponse<Application.UseCases.Subscribers.DTOs.SubscriberDto>
            .SuccessResponse(subscriberDto, "Subscriber updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var subscriber = await _unitOfWork.Subscribers.GetByIdAsync(id);
        if (subscriber == null)
        {
            return NotFound(ApiResponse<bool>
                .FailureResponse("Subscriber not found", $"Subscriber with ID {id} not found"));
        }

        _unitOfWork.Subscribers.Remove(subscriber);
        await _unitOfWork.CommitAsync();
        
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Subscriber deleted successfully"));
    }
}
