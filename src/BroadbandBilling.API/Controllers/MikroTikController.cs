using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tik4net;
using tik4net.Objects;
using tik4net.Objects.Ppp;

namespace BroadbandBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MikroTikController : ControllerBase
{
    private readonly ILogger<MikroTikController> _logger;

    public MikroTikController(ILogger<MikroTikController> logger)
    {
        _logger = logger;
    }

    #region Connection Test

    /// <summary>
    /// Test connection to MikroTik router with provided credentials
    /// </summary>
    [HttpPost("test-connection")]
    [AllowAnonymous]
    public ActionResult<object> TestConnection([FromBody] MikroTikConnectionRequest request)
    {
        ITikConnection? connection = null;
        
        try
        {
            connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            connection.Open(request.Host, request.Username, request.Password);
            
            _logger.LogInformation("Successfully connected to MikroTik at {Host}", request.Host);
            
            return Ok(new
            {
                success = true,
                message = "تم الاتصال بنجاح",
                host = request.Host
            });
        }
        catch (TikConnectionException ex)
        {
            _logger.LogError(ex, "Failed to connect to MikroTik router at {Host}", request.Host);
            return StatusCode(500, new
            {
                success = false,
                message = "فشل الاتصال بجهاز MikroTik",
                error = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while connecting to MikroTik");
            return StatusCode(500, new
            {
                success = false,
                message = "حدث خطأ غير متوقع",
                error = ex.Message
            });
        }
        finally
        {
            CloseConnection(connection);
        }
    }

    #endregion

    #region User Management (Add/Remove)

    /// <summary>
    /// Get list of all PPP users from MikroTik
    /// </summary>
    [HttpPost("ppp-users")]
    [AllowAnonymous]
    public ActionResult<object> GetPppUsers([FromBody] MikroTikConnectionRequest request)
    {
        ITikConnection? connection = null;
        
        try
        {
            connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            connection.Open(request.Host, request.Username, request.Password);
            
            var pppSecrets = connection.LoadAll<PppSecret>();
            var users = pppSecrets.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                password = p.Password,
                profile = p.Profile,
                service = p.Service,
                disabled = p.Disabled,
                remoteAddress = p.RemoteAddress,
                localAddress = p.LocalAddress
            }).ToList();
            
            _logger.LogInformation("Retrieved {Count} PPP users from MikroTik at {Host}", users.Count, request.Host);
            
            return Ok(new
            {
                success = true,
                message = "تم جلب قائمة المستخدمين بنجاح",
                count = users.Count,
                users = users
            });
        }
        catch (TikConnectionException ex)
        {
            _logger.LogError(ex, "Failed to get PPP users from MikroTik at {Host}", request.Host);
            return StatusCode(500, new
            {
                success = false,
                message = "فشل جلب قائمة المستخدمين",
                error = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting PPP users");
            return StatusCode(500, new
            {
                success = false,
                message = "حدث خطأ غير متوقع",
                error = ex.Message
            });
        }
        finally
        {
            CloseConnection(connection);
        }
    }

    /// <summary>
    /// Add a new PPP user (subscriber) to MikroTik
    /// </summary>
    [HttpPost("ppp-users/add")]
    [AllowAnonymous]
    public ActionResult<object> AddPppUser([FromBody] AddPppUserRequest request)
    {
        ITikConnection? connection = null;
        
        try
        {
            connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            connection.Open(request.Host, request.Username, request.Password);
            
            var newUser = new PppSecret
            {
                Name = request.PppUsername,
                Password = request.PppPassword,
                Profile = request.Profile ?? "default",
                Service = request.Service ?? "pppoe"
            };
            
            connection.Save(newUser);
            
            _logger.LogInformation("Added new PPP user '{PppUser}' to MikroTik at {Host}", request.PppUsername, request.Host);
            
            return Ok(new
            {
                success = true,
                message = "تم إضافة المستخدم بنجاح",
                user = new
                {
                    name = newUser.Name,
                    profile = newUser.Profile,
                    service = newUser.Service
                }
            });
        }
        catch (TikConnectionException ex)
        {
            _logger.LogError(ex, "Failed to add PPP user to MikroTik at {Host}", request.Host);
            return StatusCode(500, new
            {
                success = false,
                message = "فشل إضافة المستخدم",
                error = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding PPP user");
            return StatusCode(500, new
            {
                success = false,
                message = "حدث خطأ غير متوقع",
                error = ex.Message
            });
        }
        finally
        {
            CloseConnection(connection);
        }
    }

    /// <summary>
    /// Remove/Delete a PPP user (subscriber) from MikroTik
    /// </summary>
    [HttpPost("ppp-users/delete")]
    [AllowAnonymous]
    public ActionResult<object> DeletePppUser([FromBody] DeletePppUserRequest request)
    {
        ITikConnection? connection = null;
        
        try
        {
            connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            connection.Open(request.Host, request.Username, request.Password);
            
            var pppSecrets = connection.LoadAll<PppSecret>();
            var userToDelete = pppSecrets.FirstOrDefault(p => p.Name == request.PppUsername);
            
            if (userToDelete == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "المستخدم غير موجود",
                    username = request.PppUsername
                });
            }
            
            connection.Delete(userToDelete);
            
            _logger.LogInformation("Deleted PPP user '{PppUser}' from MikroTik at {Host}", request.PppUsername, request.Host);
            
            return Ok(new
            {
                success = true,
                message = "تم حذف المستخدم بنجاح",
                username = request.PppUsername
            });
        }
        catch (TikConnectionException ex)
        {
            _logger.LogError(ex, "Failed to delete PPP user from MikroTik at {Host}", request.Host);
            return StatusCode(500, new
            {
                success = false,
                message = "فشل حذف المستخدم",
                error = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting PPP user");
            return StatusCode(500, new
            {
                success = false,
                message = "حدث خطأ غير متوقع",
                error = ex.Message
            });
        }
        finally
        {
            CloseConnection(connection);
        }
    }

    #endregion

    #region Subscription Activation/Deactivation

    /// <summary>
    /// Activate a subscription (enable PPP user)
    /// </summary>
    [HttpPost("ppp-users/activate")]
    [AllowAnonymous]
    public ActionResult<object> ActivateUser([FromBody] DeletePppUserRequest request)
    {
        ITikConnection? connection = null;
        
        try
        {
            connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            connection.Open(request.Host, request.Username, request.Password);
            
            var pppSecrets = connection.LoadAll<PppSecret>();
            var user = pppSecrets.FirstOrDefault(p => p.Name == request.PppUsername);
            
            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "المستخدم غير موجود",
                    username = request.PppUsername
                });
            }
            
            user.Disabled = false;
            connection.Save(user);
            
            _logger.LogInformation("Activated PPP user '{PppUser}' on MikroTik at {Host}", request.PppUsername, request.Host);
            
            return Ok(new
            {
                success = true,
                message = "تم تفعيل الاشتراك بنجاح",
                username = request.PppUsername,
                status = "active"
            });
        }
        catch (TikConnectionException ex)
        {
            _logger.LogError(ex, "Failed to activate PPP user on MikroTik at {Host}", request.Host);
            return StatusCode(500, new
            {
                success = false,
                message = "فشل تفعيل الاشتراك",
                error = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while activating PPP user");
            return StatusCode(500, new
            {
                success = false,
                message = "حدث خطأ غير متوقع",
                error = ex.Message
            });
        }
        finally
        {
            CloseConnection(connection);
        }
    }

    /// <summary>
    /// Deactivate a subscription (disable PPP user)
    /// </summary>
    [HttpPost("ppp-users/deactivate")]
    [AllowAnonymous]
    public ActionResult<object> DeactivateUser([FromBody] DeletePppUserRequest request)
    {
        ITikConnection? connection = null;
        
        try
        {
            connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            connection.Open(request.Host, request.Username, request.Password);
            
            var pppSecrets = connection.LoadAll<PppSecret>();
            var user = pppSecrets.FirstOrDefault(p => p.Name == request.PppUsername);
            
            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "المستخدم غير موجود",
                    username = request.PppUsername
                });
            }
            
            user.Disabled = true;
            connection.Save(user);
            
            _logger.LogInformation("Deactivated PPP user '{PppUser}' on MikroTik at {Host}", request.PppUsername, request.Host);
            
            return Ok(new
            {
                success = true,
                message = "تم إيقاف الاشتراك بنجاح",
                username = request.PppUsername,
                status = "disabled"
            });
        }
        catch (TikConnectionException ex)
        {
            _logger.LogError(ex, "Failed to deactivate PPP user on MikroTik at {Host}", request.Host);
            return StatusCode(500, new
            {
                success = false,
                message = "فشل إيقاف الاشتراك",
                error = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deactivating PPP user");
            return StatusCode(500, new
            {
                success = false,
                message = "حدث خطأ غير متوقع",
                error = ex.Message
            });
        }
        finally
        {
            CloseConnection(connection);
        }
    }

    #endregion

    #region Active Sessions / Usage

    /// <summary>
    /// Get all active PPPoE sessions (online users)
    /// </summary>
    [HttpPost("active-sessions")]
    [AllowAnonymous]
    public ActionResult<object> GetActiveSessions([FromBody] MikroTikConnectionRequest request)
    {
        ITikConnection? connection = null;
        
        try
        {
            connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            connection.Open(request.Host, request.Username, request.Password);
            
            var activeSessions = connection.LoadAll<PppActive>();
            var sessions = activeSessions.Select(s => new
            {
                id = s.Id,
                name = s.Name,
                service = s.Service,
                callerId = s.CallerId,
                address = s.Address,
                uptime = s.Uptime,
                encoding = s.Encoding,
                sessionId = s.SessionId,
                limitBytesIn = s.LimitBytesIn,
                limitBytesOut = s.LimitBytesOut
            }).ToList();
            
            _logger.LogInformation("Retrieved {Count} active sessions from MikroTik at {Host}", sessions.Count, request.Host);
            
            return Ok(new
            {
                success = true,
                message = "تم جلب الجلسات النشطة بنجاح",
                count = sessions.Count,
                sessions = sessions
            });
        }
        catch (TikConnectionException ex)
        {
            _logger.LogError(ex, "Failed to get active sessions from MikroTik at {Host}", request.Host);
            return StatusCode(500, new
            {
                success = false,
                message = "فشل جلب الجلسات النشطة",
                error = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting active sessions");
            return StatusCode(500, new
            {
                success = false,
                message = "حدث خطأ غير متوقع",
                error = ex.Message
            });
        }
        finally
        {
            CloseConnection(connection);
        }
    }

    /// <summary>
    /// Get specific user session info
    /// </summary>
    [HttpPost("user-session")]
    [AllowAnonymous]
    public ActionResult<object> GetUserSession([FromBody] DeletePppUserRequest request)
    {
        ITikConnection? connection = null;
        
        try
        {
            connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            connection.Open(request.Host, request.Username, request.Password);
            
            var activeSessions = connection.LoadAll<PppActive>();
            var userSession = activeSessions.FirstOrDefault(s => s.Name == request.PppUsername);
            
            if (userSession == null)
            {
                return Ok(new
                {
                    success = true,
                    message = "المستخدم غير متصل حالياً",
                    username = request.PppUsername,
                    isOnline = false
                });
            }
            
            _logger.LogInformation("Retrieved session for user '{PppUser}' from MikroTik at {Host}", request.PppUsername, request.Host);
            
            return Ok(new
            {
                success = true,
                message = "تم جلب معلومات الجلسة بنجاح",
                isOnline = true,
                session = new
                {
                    name = userSession.Name,
                    service = userSession.Service,
                    callerId = userSession.CallerId,
                    address = userSession.Address,
                    uptime = userSession.Uptime,
                    sessionId = userSession.SessionId
                }
            });
        }
        catch (TikConnectionException ex)
        {
            _logger.LogError(ex, "Failed to get user session from MikroTik at {Host}", request.Host);
            return StatusCode(500, new
            {
                success = false,
                message = "فشل جلب معلومات الجلسة",
                error = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting user session");
            return StatusCode(500, new
            {
                success = false,
                message = "حدث خطأ غير متوقع",
                error = ex.Message
            });
        }
        finally
        {
            CloseConnection(connection);
        }
    }

    /// <summary>
    /// Disconnect/kick a user from active session
    /// </summary>
    [HttpPost("disconnect-user")]
    [AllowAnonymous]
    public ActionResult<object> DisconnectUser([FromBody] DeletePppUserRequest request)
    {
        ITikConnection? connection = null;
        
        try
        {
            connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            connection.Open(request.Host, request.Username, request.Password);
            
            var activeSessions = connection.LoadAll<PppActive>();
            var userSession = activeSessions.FirstOrDefault(s => s.Name == request.PppUsername);
            
            if (userSession == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "المستخدم غير متصل حالياً",
                    username = request.PppUsername
                });
            }
            
            connection.Delete(userSession);
            
            _logger.LogInformation("Disconnected user '{PppUser}' from MikroTik at {Host}", request.PppUsername, request.Host);
            
            return Ok(new
            {
                success = true,
                message = "تم قطع اتصال المستخدم بنجاح",
                username = request.PppUsername
            });
        }
        catch (TikConnectionException ex)
        {
            _logger.LogError(ex, "Failed to disconnect user from MikroTik at {Host}", request.Host);
            return StatusCode(500, new
            {
                success = false,
                message = "فشل قطع اتصال المستخدم",
                error = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while disconnecting user");
            return StatusCode(500, new
            {
                success = false,
                message = "حدث خطأ غير متوقع",
                error = ex.Message
            });
        }
        finally
        {
            CloseConnection(connection);
        }
    }

    #endregion

    #region Subscription Renewal / Profile Update

    /// <summary>
    /// Update user profile (for renewal or plan change - affects bandwidth/speed)
    /// </summary>
    [HttpPost("ppp-users/update-profile")]
    [AllowAnonymous]
    public ActionResult<object> UpdateUserProfile([FromBody] UpdateProfileRequest request)
    {
        ITikConnection? connection = null;
        
        try
        {
            connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            connection.Open(request.Host, request.Username, request.Password);
            
            var pppSecrets = connection.LoadAll<PppSecret>();
            var user = pppSecrets.FirstOrDefault(p => p.Name == request.PppUsername);
            
            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "المستخدم غير موجود",
                    username = request.PppUsername
                });
            }
            
            var oldProfile = user.Profile;
            user.Profile = request.NewProfile;
            
            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                user.Password = request.NewPassword;
            }
            
            connection.Save(user);
            
            _logger.LogInformation("Updated profile for PPP user '{PppUser}' from '{OldProfile}' to '{NewProfile}' on MikroTik at {Host}", 
                request.PppUsername, oldProfile, request.NewProfile, request.Host);
            
            return Ok(new
            {
                success = true,
                message = "تم تحديث الباقة بنجاح",
                username = request.PppUsername,
                oldProfile = oldProfile,
                newProfile = request.NewProfile
            });
        }
        catch (TikConnectionException ex)
        {
            _logger.LogError(ex, "Failed to update user profile on MikroTik at {Host}", request.Host);
            return StatusCode(500, new
            {
                success = false,
                message = "فشل تحديث الباقة",
                error = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating user profile");
            return StatusCode(500, new
            {
                success = false,
                message = "حدث خطأ غير متوقع",
                error = ex.Message
            });
        }
        finally
        {
            CloseConnection(connection);
        }
    }

    #endregion

    #region PPP Profiles (Bandwidth/Speed Control)

    /// <summary>
    /// Get all PPP profiles (bandwidth plans)
    /// </summary>
    [HttpPost("ppp-profiles")]
    [AllowAnonymous]
    public ActionResult<object> GetPppProfiles([FromBody] MikroTikConnectionRequest request)
    {
        ITikConnection? connection = null;
        
        try
        {
            connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            connection.Open(request.Host, request.Username, request.Password);
            
            var profiles = connection.LoadAll<PppProfile>();
            var profileList = profiles.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                localAddress = p.LocalAddress,
                remoteAddress = p.RemoteAddress,
                rateLimit = p.RateLimit,
                onlyOne = p.OnlyOne
            }).ToList();
            
            _logger.LogInformation("Retrieved {Count} PPP profiles from MikroTik at {Host}", profileList.Count, request.Host);
            
            return Ok(new
            {
                success = true,
                message = "تم جلب قائمة الباقات بنجاح",
                count = profileList.Count,
                profiles = profileList
            });
        }
        catch (TikConnectionException ex)
        {
            _logger.LogError(ex, "Failed to get PPP profiles from MikroTik at {Host}", request.Host);
            return StatusCode(500, new
            {
                success = false,
                message = "فشل جلب قائمة الباقات",
                error = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting PPP profiles");
            return StatusCode(500, new
            {
                success = false,
                message = "حدث خطأ غير متوقع",
                error = ex.Message
            });
        }
        finally
        {
            CloseConnection(connection);
        }
    }

    /// <summary>
    /// Create a new PPP profile (bandwidth plan)
    /// </summary>
    [HttpPost("ppp-profiles/add")]
    [AllowAnonymous]
    public ActionResult<object> AddPppProfile([FromBody] AddProfileRequest request)
    {
        ITikConnection? connection = null;
        
        try
        {
            connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            connection.Open(request.Host, request.Username, request.Password);
            
            var newProfile = new PppProfile
            {
                Name = request.ProfileName,
                RateLimit = request.RateLimit,
                LocalAddress = request.LocalAddress ?? "",
                RemoteAddress = request.RemoteAddress ?? ""
            };
            
            connection.Save(newProfile);
            
            _logger.LogInformation("Added new PPP profile '{ProfileName}' with rate limit '{RateLimit}' to MikroTik at {Host}", 
                request.ProfileName, request.RateLimit, request.Host);
            
            return Ok(new
            {
                success = true,
                message = "تم إضافة الباقة بنجاح",
                profile = new
                {
                    name = newProfile.Name,
                    rateLimit = newProfile.RateLimit
                }
            });
        }
        catch (TikConnectionException ex)
        {
            _logger.LogError(ex, "Failed to add PPP profile to MikroTik at {Host}", request.Host);
            return StatusCode(500, new
            {
                success = false,
                message = "فشل إضافة الباقة",
                error = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding PPP profile");
            return StatusCode(500, new
            {
                success = false,
                message = "حدث خطأ غير متوقع",
                error = ex.Message
            });
        }
        finally
        {
            CloseConnection(connection);
        }
    }

    /// <summary>
    /// Update PPP profile rate limit (bandwidth control)
    /// </summary>
    [HttpPost("ppp-profiles/update")]
    [AllowAnonymous]
    public ActionResult<object> UpdatePppProfile([FromBody] UpdatePppProfileRequest request)
    {
        ITikConnection? connection = null;
        
        try
        {
            connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            connection.Open(request.Host, request.Username, request.Password);
            
            var profiles = connection.LoadAll<PppProfile>();
            var profile = profiles.FirstOrDefault(p => p.Name == request.ProfileName);
            
            if (profile == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "الباقة غير موجودة",
                    profileName = request.ProfileName
                });
            }
            
            var oldRateLimit = profile.RateLimit;
            profile.RateLimit = request.RateLimit;
            
            connection.Save(profile);
            
            _logger.LogInformation("Updated PPP profile '{ProfileName}' rate limit from '{OldRate}' to '{NewRate}' on MikroTik at {Host}", 
                request.ProfileName, oldRateLimit, request.RateLimit, request.Host);
            
            return Ok(new
            {
                success = true,
                message = "تم تحديث سرعة الباقة بنجاح",
                profileName = request.ProfileName,
                oldRateLimit = oldRateLimit,
                newRateLimit = request.RateLimit
            });
        }
        catch (TikConnectionException ex)
        {
            _logger.LogError(ex, "Failed to update PPP profile on MikroTik at {Host}", request.Host);
            return StatusCode(500, new
            {
                success = false,
                message = "فشل تحديث الباقة",
                error = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating PPP profile");
            return StatusCode(500, new
            {
                success = false,
                message = "حدث خطأ غير متوقع",
                error = ex.Message
            });
        }
        finally
        {
            CloseConnection(connection);
        }
    }

    /// <summary>
    /// Delete a PPP profile
    /// </summary>
    [HttpPost("ppp-profiles/delete")]
    [AllowAnonymous]
    public ActionResult<object> DeletePppProfile([FromBody] DeleteProfileRequest request)
    {
        ITikConnection? connection = null;
        
        try
        {
            connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            connection.Open(request.Host, request.Username, request.Password);
            
            var profiles = connection.LoadAll<PppProfile>();
            var profile = profiles.FirstOrDefault(p => p.Name == request.ProfileName);
            
            if (profile == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "الباقة غير موجودة",
                    profileName = request.ProfileName
                });
            }
            
            connection.Delete(profile);
            
            _logger.LogInformation("Deleted PPP profile '{ProfileName}' from MikroTik at {Host}", request.ProfileName, request.Host);
            
            return Ok(new
            {
                success = true,
                message = "تم حذف الباقة بنجاح",
                profileName = request.ProfileName
            });
        }
        catch (TikConnectionException ex)
        {
            _logger.LogError(ex, "Failed to delete PPP profile from MikroTik at {Host}", request.Host);
            return StatusCode(500, new
            {
                success = false,
                message = "فشل حذف الباقة",
                error = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting PPP profile");
            return StatusCode(500, new
            {
                success = false,
                message = "حدث خطأ غير متوقع",
                error = ex.Message
            });
        }
        finally
        {
            CloseConnection(connection);
        }
    }

    #endregion

    #region Helper Methods

    private void CloseConnection(ITikConnection? connection)
    {
        if (connection != null)
        {
            try
            {
                connection.Close();
                connection.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while closing MikroTik connection");
            }
        }
    }

    #endregion
}

#region Request DTOs

public record MikroTikConnectionRequest
{
    public required string Host { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}

public record AddPppUserRequest : MikroTikConnectionRequest
{
    public required string PppUsername { get; init; }
    public required string PppPassword { get; init; }
    public string? Profile { get; init; }
    public string? Service { get; init; }
}

public record DeletePppUserRequest : MikroTikConnectionRequest
{
    public required string PppUsername { get; init; }
}

public record UpdateProfileRequest : MikroTikConnectionRequest
{
    public required string PppUsername { get; init; }
    public required string NewProfile { get; init; }
    public string? NewPassword { get; init; }
}

public record AddProfileRequest : MikroTikConnectionRequest
{
    public required string ProfileName { get; init; }
    public required string RateLimit { get; init; }
    public string? LocalAddress { get; init; }
    public string? RemoteAddress { get; init; }
}

public record UpdatePppProfileRequest : MikroTikConnectionRequest
{
    public required string ProfileName { get; init; }
    public required string RateLimit { get; init; }
}

public record DeleteProfileRequest : MikroTikConnectionRequest
{
    public required string ProfileName { get; init; }
}

#endregion
