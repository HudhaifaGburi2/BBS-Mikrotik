using BroadbandBilling.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BroadbandBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class MikroTikController : ControllerBase
{
    private readonly IMikroTikService _mikroTikService;
    private readonly ILogger<MikroTikController> _logger;

    public MikroTikController(IMikroTikService mikroTikService, ILogger<MikroTikController> logger)
    {
        _mikroTikService = mikroTikService;
        _logger = logger;
    }

    #region Connection Test

    /// <summary>
    /// Test connection to MikroTik router with provided credentials
    /// </summary>
    [HttpPost("test-connection")]
    [AllowAnonymous]
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
