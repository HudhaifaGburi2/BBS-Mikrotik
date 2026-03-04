using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.Interfaces;
using BroadbandBilling.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BroadbandBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Manager,Support,Accountant")]
public class MikroTikController : ControllerBase
{
    private readonly IMikroTikService _mikroTikService;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<MikroTikController> _logger;

    public MikroTikController(
        IMikroTikService mikroTikService, 
        IApplicationDbContext dbContext,
        ILogger<MikroTikController> logger)
    {
        _mikroTikService = mikroTikService;
        _dbContext = dbContext;
        _logger = logger;
    }

    #region Connection Test

    /// <summary>
    /// Test connection to MikroTik router with provided credentials
    /// </summary>
    [HttpPost("test-connection")]
    public async Task<ActionResult<MikroTikResult>> TestConnection(
        [FromBody] MikroTikConnectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _mikroTikService.TestConnectionAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    #endregion

    #region User Management (Add/Remove)

    /// <summary>
    /// Get list of all PPP users from MikroTik
    /// </summary>
    [HttpPost("ppp-users")]
    public async Task<ActionResult<MikroTikResult<List<PppUserDto>>>> GetPppUsers(
        [FromBody] MikroTikConnectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _mikroTikService.GetPppUsersAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Add a new PPP user (subscriber) to MikroTik
    /// </summary>
    [HttpPost("ppp-users/add")]
    public async Task<ActionResult<MikroTikResult<PppUserDto>>> AddPppUser(
        [FromBody] AddPppUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _mikroTikService.AddPppUserAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Remove/Delete a PPP user (subscriber) from MikroTik
    /// </summary>
    [HttpPost("ppp-users/delete")]
    public async Task<ActionResult<MikroTikResult>> DeletePppUser(
        [FromBody] DeletePppUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _mikroTikService.DeletePppUserAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    #endregion

    #region Subscription Activation/Deactivation

    /// <summary>
    /// Activate a subscription (enable PPP user)
    /// </summary>
    [HttpPost("ppp-users/activate")]
    public async Task<ActionResult<MikroTikResult>> ActivateUser(
        [FromBody] DeletePppUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _mikroTikService.ActivateUserAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Deactivate a subscription (disable PPP user)
    /// </summary>
    [HttpPost("ppp-users/deactivate")]
    public async Task<ActionResult<MikroTikResult>> DeactivateUser(
        [FromBody] DeletePppUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _mikroTikService.DeactivateUserAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    #endregion

    #region Active Sessions / Usage

    /// <summary>
    /// Get all active PPPoE sessions (online users)
    /// </summary>
    [HttpPost("active-sessions")]
    public async Task<ActionResult<MikroTikResult<List<ActiveSessionDto>>>> GetActiveSessions(
        [FromBody] MikroTikConnectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _mikroTikService.GetActiveSessionsAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get specific user session info
    /// </summary>
    [HttpPost("user-session")]
    public async Task<ActionResult<MikroTikResult<ActiveSessionDto?>>> GetUserSession(
        [FromBody] DeletePppUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _mikroTikService.GetUserSessionAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Disconnect/kick a user from active session
    /// </summary>
    [HttpPost("disconnect-user")]
    public async Task<ActionResult<MikroTikResult>> DisconnectUser(
        [FromBody] DeletePppUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _mikroTikService.DisconnectUserAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    #endregion

    #region Subscription Renewal / Profile Update

    /// <summary>
    /// Update user profile (for renewal or plan change - affects bandwidth/speed)
    /// </summary>
    [HttpPost("ppp-users/update-profile")]
    public async Task<ActionResult<MikroTikResult>> UpdateUserProfile(
        [FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _mikroTikService.UpdateUserProfileAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    #endregion

    #region PPP Profiles (Bandwidth/Speed Control)

    /// <summary>
    /// Get all PPP profiles (bandwidth plans)
    /// </summary>
    [HttpPost("ppp-profiles")]
    public async Task<ActionResult<MikroTikResult<List<PppProfileDto>>>> GetPppProfiles(
        [FromBody] MikroTikConnectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _mikroTikService.GetPppProfilesAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Create a new PPP profile (bandwidth plan)
    /// </summary>
    [HttpPost("ppp-profiles/add")]
    public async Task<ActionResult<MikroTikResult<PppProfileDto>>> AddPppProfile(
        [FromBody] AddProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _mikroTikService.AddPppProfileAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Update PPP profile rate limit (bandwidth control)
    /// </summary>
    [HttpPost("ppp-profiles/update")]
    public async Task<ActionResult<MikroTikResult>> UpdatePppProfile(
        [FromBody] UpdatePppProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _mikroTikService.UpdatePppProfileAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Delete a PPP profile
    /// </summary>
    [HttpPost("ppp-profiles/delete")]
    public async Task<ActionResult<MikroTikResult>> DeletePppProfile(
        [FromBody] DeleteProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _mikroTikService.DeletePppProfileAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    #endregion

    #region PPP User Management (Advanced)

    /// <summary>
    /// Get a single PPP user details
    /// </summary>
    [HttpPost("ppp-users/get")]
    public async Task<ActionResult<MikroTikResult<PppUserDto>>> GetPppUser(
        [FromBody] DeletePppUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _mikroTikService.GetPppUserAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Update PPP user (password, profile, quota, etc.)
    /// </summary>
    [HttpPost("ppp-users/update")]
    public async Task<ActionResult<MikroTikResult<PppUserDto>>> UpdatePppUser(
        [FromBody] UpdatePppUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _mikroTikService.UpdatePppUserAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Reset user quota (set limit bytes to 0/unlimited)
    /// </summary>
    [HttpPost("ppp-users/reset-quota")]
    public async Task<ActionResult<MikroTikResult>> ResetUserQuota(
        [FromBody] ResetUserQuotaRequest request, CancellationToken cancellationToken)
    {
        var result = await _mikroTikService.ResetUserQuotaAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get PPP users enriched with SQL subscription data (accumulated usage, remaining quota)
    /// </summary>
    [HttpPost("ppp-users/enriched")]
    public async Task<ActionResult<MikroTikResult<List<EnrichedPppUserDto>>>> GetEnrichedPppUsers(
        [FromBody] MikroTikConnectionRequest request, CancellationToken cancellationToken)
    {
        // Get MikroTik users
        var mikroTikResult = await _mikroTikService.GetPppUsersAsync(request, cancellationToken);
        if (!mikroTikResult.Success || mikroTikResult.Data == null)
        {
            return StatusCode(500, MikroTikResult<List<EnrichedPppUserDto>>.FailureResult(
                mikroTikResult.Message, mikroTikResult.Error));
        }

        // Get SQL subscription data
        var pppoeAccounts = await _dbContext.PppoeAccounts
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Plan)
            .ToListAsync(cancellationToken);

        var accountsDict = pppoeAccounts.ToDictionary(p => p.Username, p => p);

        // Enrich MikroTik users with SQL data
        var enrichedUsers = mikroTikResult.Data.Select(u =>
        {
            var enriched = new EnrichedPppUserDto
            {
                Id = u.Id,
                Name = u.Name,
                Password = u.Password,
                Profile = u.Profile,
                Service = u.Service,
                Disabled = u.Disabled,
                RemoteAddress = u.RemoteAddress,
                LocalAddress = u.LocalAddress,
                CallerId = u.CallerId,
                Comment = u.Comment,
                LimitBytesIn = u.LimitBytesIn,
                LimitBytesOut = u.LimitBytesOut,
                LimitBytesTotal = u.LimitBytesTotal,
                IsOnline = u.IsOnline,
                LastLoggedOut = u.LastLoggedOut
            };

            // Enrich with SQL data if available
            if (accountsDict.TryGetValue(u.Name, out var account) && account.Subscription != null)
            {
                var subscription = account.Subscription;
                var plan = subscription.Plan;

                enriched.SubscriptionId = subscription.Id;
                enriched.SubscriptionStatus = subscription.Status.ToString();
                enriched.PlanName = plan?.Name;
                enriched.PlanDataLimitGB = plan?.DataLimitGB ?? 0;
                enriched.PlanDataLimitBytes = plan != null ? (long)plan.DataLimitGB * 1024 * 1024 * 1024 : 0;
                enriched.DataUsedBytes = subscription.DataUsedBytes;
                enriched.DataRemainingBytes = plan != null && plan.DataLimitGB > 0 
                    ? Math.Max(0, enriched.PlanDataLimitBytes - subscription.DataUsedBytes) 
                    : 0;
                enriched.DataUsagePercent = plan != null && plan.DataLimitGB > 0 
                    ? (int)Math.Min(100, (subscription.DataUsedBytes * 100) / enriched.PlanDataLimitBytes) 
                    : 0;
                enriched.IsUnlimited = plan == null || plan.DataLimitGB == 0;
                enriched.DataLimitExceeded = subscription.DataLimitExceeded;
                enriched.IsSuspended = subscription.Status == SubscriptionStatus.Suspended;
            }
            else
            {
                // No SQL data - use MikroTik limit as fallback
                enriched.IsUnlimited = u.LimitBytesTotal == 0;
            }

            return enriched;
        }).ToList();

        return Ok(MikroTikResult<List<EnrichedPppUserDto>>.SuccessResult(
            enrichedUsers, "تم جلب المستخدمين مع بيانات الاشتراك"));
    }

    /// <summary>
    /// Get active sessions enriched with SQL subscription data
    /// </summary>
    [HttpPost("active-sessions/enriched")]
    public async Task<ActionResult<MikroTikResult<List<EnrichedActiveSessionDto>>>> GetEnrichedActiveSessions(
        [FromBody] MikroTikConnectionRequest request, CancellationToken cancellationToken)
    {
        // Get active sessions from MikroTik
        var sessionsResult = await _mikroTikService.GetActiveSessionsAsync(request, cancellationToken);
        if (!sessionsResult.Success || sessionsResult.Data == null)
        {
            return StatusCode(500, MikroTikResult<List<EnrichedActiveSessionDto>>.FailureResult(
                sessionsResult.Message, sessionsResult.Error));
        }

        // Get SQL subscription data
        var pppoeAccounts = await _dbContext.PppoeAccounts
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Plan)
            .ToListAsync(cancellationToken);

        var accountsDict = pppoeAccounts.ToDictionary(p => p.Username, p => p);

        // Enrich sessions with SQL data
        var enrichedSessions = sessionsResult.Data.Select(s =>
        {
            var enriched = new EnrichedActiveSessionDto
            {
                Id = s.Id,
                Name = s.Name,
                Service = s.Service,
                CallerId = s.CallerId,
                Address = s.Address,
                Uptime = s.Uptime,
                Encoding = s.Encoding,
                SessionId = s.SessionId,
                BytesIn = s.BytesIn,
                BytesOut = s.BytesOut,
                SessionBytesUsed = s.BytesUsed,
                LimitBytesIn = s.LimitBytesIn,
                LimitBytesOut = s.LimitBytesOut
            };

            // Enrich with SQL data if available
            if (accountsDict.TryGetValue(s.Name, out var account) && account.Subscription != null)
            {
                var subscription = account.Subscription;
                var plan = subscription.Plan;

                enriched.SubscriptionId = subscription.Id;
                enriched.PlanName = plan?.Name;
                enriched.PlanDataLimitGB = plan?.DataLimitGB ?? 0;
                enriched.PlanDataLimitBytes = plan != null ? (long)plan.DataLimitGB * 1024 * 1024 * 1024 : 0;
                
                // Total usage = SQL accumulated + current session bytes
                // Note: Current session bytes are already being accumulated by background job
                // So we use SQL total which includes current session
                enriched.TotalDataUsedBytes = subscription.DataUsedBytes;
                
                enriched.DataRemainingBytes = plan != null && plan.DataLimitGB > 0 
                    ? Math.Max(0, enriched.PlanDataLimitBytes - subscription.DataUsedBytes) 
                    : 0;
                enriched.DataUsagePercent = plan != null && plan.DataLimitGB > 0 
                    ? (int)Math.Min(100, (subscription.DataUsedBytes * 100) / enriched.PlanDataLimitBytes) 
                    : 0;
                enriched.IsUnlimited = plan == null || plan.DataLimitGB == 0;
                enriched.DataLimitExceeded = subscription.DataLimitExceeded;
            }
            else
            {
                // No SQL data - use session bytes only
                enriched.TotalDataUsedBytes = s.BytesUsed;
                enriched.IsUnlimited = s.LimitBytesIn == 0;
            }

            return enriched;
        }).ToList();

        return Ok(MikroTikResult<List<EnrichedActiveSessionDto>>.SuccessResult(
            enrichedSessions, "تم جلب الجلسات النشطة مع بيانات الاشتراك"));
    }

    #endregion

    #region Helper Methods

    private ActionResult ToActionResult(MikroTikResult result)
    {
        if (result.Success)
            return Ok(result);

        return StatusCode(500, result);
    }

    private ActionResult ToActionResult<T>(MikroTikResult<T> result)
    {
        if (result.Success)
            return Ok(result);

        return StatusCode(500, result);
    }

    #endregion
}
