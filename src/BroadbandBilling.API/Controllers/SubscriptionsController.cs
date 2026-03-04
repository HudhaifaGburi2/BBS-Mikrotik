using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BroadbandBilling.Application.Common.DTOs;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Features.Subscriptions.Commands;
using BroadbandBilling.Application.UseCases.Subscriptions.CreateSubscription;
using BroadbandBilling.Application.UseCases.Subscriptions.DTOs;
using BroadbandBilling.Domain.Enums;
using MediatR;

namespace BroadbandBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IMediator _mediator;

    public SubscriptionsController(ISubscriptionService subscriptionService, IMediator mediator)
    {
        _subscriptionService = subscriptionService;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<SubscriptionDto>>>> GetAll(CancellationToken cancellationToken)
    {
        // Check if user is a subscriber - if so, return only their subscriptions
        var subscriberIdClaim = User.FindFirst("SubscriberId")?.Value;
        if (!string.IsNullOrEmpty(subscriberIdClaim) && Guid.TryParse(subscriberIdClaim, out var subscriberId))
        {
            var myResult = await _subscriptionService.GetBySubscriberIdAsync(subscriberId, cancellationToken);
            return Ok(myResult);
        }
        
        // Admin users get all subscriptions
        var result = await _subscriptionService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SubscriptionDto>>>> GetMySubscriptions(CancellationToken cancellationToken)
    {
        var subscriberIdClaim = User.FindFirst("SubscriberId")?.Value;
        if (string.IsNullOrEmpty(subscriberIdClaim) || !Guid.TryParse(subscriberIdClaim, out var subscriberId))
        {
            return Unauthorized(new { error = "لا يوجد معرف مشترك في الجلسة" });
        }

        var result = await _subscriptionService.GetBySubscriberIdAsync(subscriberId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _subscriptionService.GetByIdAsync(id, cancellationToken);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> Create(
        [FromBody] CreateSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var result = await _subscriptionService.CreateAsync(command, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPost("{id}/renew")]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> Renew(Guid id, CancellationToken cancellationToken)
    {
        var result = await _subscriptionService.RenewAsync(id, cancellationToken);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("{id}/suspend")]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> Suspend(
        Guid id, [FromBody] SuspendSubscriptionDto dto, CancellationToken cancellationToken)
    {
        var result = await _subscriptionService.SuspendAsync(id, cancellationToken);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> Cancel(
        Guid id, [FromBody] CancelSubscriptionDto dto, CancellationToken cancellationToken)
    {
        var result = await _subscriptionService.CancelAsync(id, dto.Reason, cancellationToken);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Activate a pending subscription (after payment confirmation)
    /// </summary>
    [HttpPost("{id}/activate")]
    [Authorize(Roles = "SuperAdmin,Manager,Support")]
    public async Task<ActionResult<ActivateSubscriptionResult>> Activate(
        Guid id, [FromBody] ActivateSubscriptionDto? dto, CancellationToken cancellationToken)
    {
        var command = new ActivateSubscriptionCommand(id, dto?.PaymentReference);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all subscriptions with PendingActivation status
    /// </summary>
    [HttpGet("pending-activation")]
    [Authorize(Roles = "SuperAdmin,Manager,Support")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PendingActivationDto>>>> GetPendingActivations(CancellationToken cancellationToken)
    {
        var result = await _subscriptionService.GetPendingActivationsAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Reject a pending activation request
    /// </summary>
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "SuperAdmin,Manager")]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> Reject(
        Guid id, [FromBody] RejectActivationDto dto, CancellationToken cancellationToken)
    {
        var result = await _subscriptionService.RejectActivationAsync(id, dto.Reason, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}

public record ActivateSubscriptionDto(string? PaymentReference = null);
public record RejectActivationDto(string Reason);
