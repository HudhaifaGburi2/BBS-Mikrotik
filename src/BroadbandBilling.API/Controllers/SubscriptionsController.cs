using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BroadbandBilling.Application.Common.DTOs;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Subscriptions.CreateSubscription;
using BroadbandBilling.Application.UseCases.Subscriptions.DTOs;

namespace BroadbandBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
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
}
