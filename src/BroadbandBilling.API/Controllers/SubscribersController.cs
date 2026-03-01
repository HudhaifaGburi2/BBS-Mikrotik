using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Features.Subscribers.Commands;
using BroadbandBilling.Application.Features.Subscribers.DTOs;
using BroadbandBilling.Application.Features.Subscribers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BroadbandBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscribersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SubscribersController> _logger;

    public SubscribersController(IMediator mediator, ILogger<SubscribersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<SubscriberDto>>> GetSubscribers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? hasActiveSubscription = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        try
        {
            var query = new GetSubscribersQuery(page, pageSize, search, isActive, hasActiveSubscription, sortBy, sortDescending);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscribers");
            return StatusCode(500, new { error = "حدث خطأ أثناء جلب المشتركين", details = ex.Message });
        }
    }

    [HttpGet("me")]
    public async Task<ActionResult<SubscriberDto>> GetCurrentSubscriber()
    {
        try
        {
            var subscriberIdClaim = User.FindFirst("SubscriberId")?.Value;
            if (string.IsNullOrEmpty(subscriberIdClaim) || !Guid.TryParse(subscriberIdClaim, out var subscriberId))
            {
                return Unauthorized(new { error = "لا يوجد معرف مشترك في الجلسة" });
            }

            var query = new GetSubscriberByIdQuery(subscriberId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "المشترك غير موجود" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current subscriber profile");
            return StatusCode(500, new { error = "حدث خطأ أثناء جلب بيانات المشترك" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SubscriberDto>> GetSubscriber(Guid id)
    {
        try
        {
            var query = new GetSubscriberByIdQuery(id);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Subscriber not found: {SubscriberId}", id);
            return NotFound(new { error = "المشترك غير موجود" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscriber: {SubscriberId}", id);
            return StatusCode(500, new { error = "حدث خطأ أثناء جلب بيانات المشترك" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<SubscriberDto>> CreateSubscriber([FromBody] CreateSubscriberCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetSubscriber), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid input for subscriber creation");
            return BadRequest(new { error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Plan not found during subscriber creation");
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscriber");
            return StatusCode(500, new { error = "حدث خطأ أثناء إنشاء المشترك", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SubscriberDto>> UpdateSubscriber(Guid id, [FromBody] UpdateSubscriberCommand command)
    {
        try
        {
            var updatedCommand = command with { SubscriberId = id };
            var result = await _mediator.Send(updatedCommand);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Subscriber not found for update: {SubscriberId}", id);
            return NotFound(new { error = "المشترك غير موجود" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid input for subscriber update: {SubscriberId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscriber: {SubscriberId}", id);
            return StatusCode(500, new { error = "حدث خطأ أثناء تحديث بيانات المشترك" });
        }
    }

    [HttpPost("{id}/suspend")]
    public async Task<ActionResult> SuspendSubscriber(Guid id, [FromBody] SuspendSubscriberCommand command)
    {
        try
        {
            var updatedCommand = command with { SubscriberId = id };
            await _mediator.Send(updatedCommand);
            return Ok(new { message = "تم إيقاف المشترك بنجاح" });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Subscriber not found for suspension: {SubscriberId}", id);
            return NotFound(new { error = "المشترك غير موجود" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending subscriber: {SubscriberId}", id);
            return StatusCode(500, new { error = "حدث خطأ أثناء إيقاف المشترك" });
        }
    }

    [HttpPost("{id}/unsuspend")]
    public async Task<ActionResult> UnsuspendSubscriber(Guid id, [FromBody] UnsuspendSubscriberCommand command)
    {
        try
        {
            var updatedCommand = command with { SubscriberId = id };
            await _mediator.Send(updatedCommand);
            return Ok(new { message = "تم تفعيل المشترك بنجاح" });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Subscriber not found for unsuspension: {SubscriberId}", id);
            return NotFound(new { error = "المشترك غير موجود" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsuspending subscriber: {SubscriberId}", id);
            return StatusCode(500, new { error = "حدث خطأ أثناء تفعيل المشترك" });
        }
    }

    [HttpPost("{id}/change-plan")]
    public async Task<ActionResult<SubscriptionDto>> ChangeSubscriberPlan(Guid id, [FromBody] ChangeSubscriberPlanCommand command)
    {
        try
        {
            var updatedCommand = command with { SubscriberId = id };
            var result = await _mediator.Send(updatedCommand);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Subscriber or plan not found for plan change: {SubscriberId}", id);
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation for plan change: {SubscriberId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing subscriber plan: {SubscriberId}", id);
            return StatusCode(500, new { error = "حدث خطأ أثناء تغيير باقة المشترك" });
        }
    }

    [HttpPost("{id}/reset-password")]
    public async Task<ActionResult<string>> ResetSubscriberPassword(Guid id, [FromBody] ResetSubscriberPasswordCommand command)
    {
        try
        {
            var updatedCommand = command with { SubscriberId = id };
            var newPassword = await _mediator.Send(updatedCommand);
            return Ok(new { message = "تم إعادة تعيين كلمة المرور بنجاح", password = newPassword });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Subscriber not found for password reset: {SubscriberId}", id);
            return NotFound(new { error = "المشترك غير موجود" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting subscriber password: {SubscriberId}", id);
            return StatusCode(500, new { error = "حدث خطأ أثناء إعادة تعيين كلمة المرور" });
        }
    }

    [HttpPost("{id}/reset-system-password")]
    public async Task<ActionResult> ResetSystemPassword(Guid id, [FromBody] ResetSystemPasswordCommand command)
    {
        try
        {
            var updatedCommand = command with { SubscriberId = id };
            var newPassword = await _mediator.Send(updatedCommand);
            return Ok(new { message = "تم تغيير كلمة مرور النظام بنجاح", password = newPassword });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting system password: {SubscriberId}", id);
            return StatusCode(500, new { error = "حدث خطأ أثناء تغيير كلمة مرور النظام" });
        }
    }

    [HttpPost("{id}/reset-mikrotik-password")]
    public async Task<ActionResult> ResetMikroTikPassword(Guid id, [FromBody] ResetMikroTikPasswordCommand command)
    {
        try
        {
            var updatedCommand = command with { SubscriberId = id };
            var newPassword = await _mediator.Send(updatedCommand);
            return Ok(new { message = "تم تغيير كلمة مرور MikroTik بنجاح", password = newPassword });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting MikroTik password: {SubscriberId}", id);
            return StatusCode(500, new { error = "حدث خطأ أثناء تغيير كلمة مرور MikroTik" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSubscriber(Guid id, [FromQuery] bool forceDelete = false)
    {
        try
        {
            var command = new DeleteSubscriberCommand(id, forceDelete);
            await _mediator.Send(command);
            return Ok(new { message = "تم حذف المشترك بنجاح" });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Subscriber not found for deletion: {SubscriberId}", id);
            return NotFound(new { error = "المشترك غير موجود" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete subscriber with active subscriptions: {SubscriberId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subscriber: {SubscriberId}", id);
            return StatusCode(500, new { error = "حدث خطأ أثناء حذف المشترك" });
        }
    }
}
