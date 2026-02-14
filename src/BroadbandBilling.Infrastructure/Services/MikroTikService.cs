using BroadbandBilling.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using tik4net;
using tik4net.Objects;
using tik4net.Objects.Ppp;

namespace BroadbandBilling.Infrastructure.Services;

public class MikroTikService : IMikroTikService
{
    private readonly ILogger<MikroTikService> _logger;
    private readonly MikroTikSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    public MikroTikService(ILogger<MikroTikService> logger, IOptions<MikroTikSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<MikroTikResult> TestConnectionAsync(MikroTikConnectionRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(
            async () =>
            {
                using var connection = await CreateConnectionAsync(request);
                _logger.LogInformation("Successfully connected to MikroTik at {Host}", request.Host);
                return MikroTikResult.SuccessResult("تم الاتصال بنجاح");
            },
            $"Test connection to {request.Host}",
            cancellationToken
        );
    }

    public async Task<MikroTikResult<List<PppUserDto>>> GetPppUsersAsync(MikroTikConnectionRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(
            async () =>
            {
                using var connection = await CreateConnectionAsync(request);
                var pppSecrets = connection.LoadAll<PppSecret>();
                var users = pppSecrets.Select(p => new PppUserDto
                {
                    Id = p.Id?.ToString() ?? string.Empty,
                    Name = p.Name,
                    Password = p.Password,
                    Profile = p.Profile,
                    Service = p.Service,
                    Disabled = p.Disabled,
                    RemoteAddress = p.RemoteAddress,
                    LocalAddress = p.LocalAddress
                }).ToList();

                _logger.LogInformation("Retrieved {Count} PPP users from MikroTik at {Host}", users.Count, request.Host);
                return MikroTikResult<List<PppUserDto>>.SuccessResult(users, "تم جلب قائمة المستخدمين بنجاح");
            },
            $"Get PPP users from {request.Host}",
            cancellationToken
        );
    }

    public async Task<MikroTikResult<PppUserDto>> AddPppUserAsync(AddPppUserRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(
            async () =>
            {
                using var connection = await CreateConnectionAsync(request);
                
                // Check if user already exists
                var existingUsers = connection.LoadAll<PppSecret>();
                if (existingUsers.Any(p => p.Name == request.PppUsername))
                {
                    return MikroTikResult<PppUserDto>.FailureResult("المستخدم موجود بالفعل", $"PPP user '{request.PppUsername}' already exists");
                }

                var newUser = new PppSecret
                {
                    Name = request.PppUsername,
                    Password = request.PppPassword,
                    Profile = request.Profile ?? "default",
                    Service = request.Service ?? "pppoe"
                };

                connection.Save(newUser);

                var userDto = new PppUserDto
                {
                    Id = newUser.Id?.ToString() ?? string.Empty,
                    Name = newUser.Name,
                    Password = newUser.Password,
                    Profile = newUser.Profile,
                    Service = newUser.Service,
                    Disabled = newUser.Disabled
                };

                _logger.LogInformation("Added new PPP user '{PppUser}' to MikroTik at {Host}", request.PppUsername, request.Host);
                return MikroTikResult<PppUserDto>.SuccessResult(userDto, "تم إضافة المستخدم بنجاح");
            },
            $"Add PPP user '{request.PppUsername}' to {request.Host}",
            cancellationToken
        );
    }

    public async Task<MikroTikResult> DeletePppUserAsync(DeletePppUserRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(
            async () =>
            {
                using var connection = await CreateConnectionAsync(request);
                var pppSecrets = connection.LoadAll<PppSecret>();
                var userToDelete = pppSecrets.FirstOrDefault(p => p.Name == request.PppUsername);

                if (userToDelete == null)
                {
                    return MikroTikResult.FailureResult("المستخدم غير موجود", $"PPP user '{request.PppUsername}' not found");
                }

                connection.Delete(userToDelete);
                _logger.LogInformation("Deleted PPP user '{PppUser}' from MikroTik at {Host}", request.PppUsername, request.Host);
                return MikroTikResult.SuccessResult("تم حذف المستخدم بنجاح");
            },
            $"Delete PPP user '{request.PppUsername}' from {request.Host}",
            cancellationToken
        );
    }

    public async Task<MikroTikResult> ActivateUserAsync(DeletePppUserRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(
            async () =>
            {
                using var connection = await CreateConnectionAsync(request);
                var pppSecrets = connection.LoadAll<PppSecret>();
                var user = pppSecrets.FirstOrDefault(p => p.Name == request.PppUsername);

                if (user == null)
                {
                    return MikroTikResult.FailureResult("المستخدم غير موجود", $"PPP user '{request.PppUsername}' not found");
                }

                user.Disabled = false;
                connection.Save(user);

                _logger.LogInformation("Activated PPP user '{PppUser}' on MikroTik at {Host}", request.PppUsername, request.Host);
                return MikroTikResult.SuccessResult("تم تفعيل الاشتراك بنجاح");
            },
            $"Activate PPP user '{request.PppUsername}' on {request.Host}",
            cancellationToken
        );
    }

    public async Task<MikroTikResult> DeactivateUserAsync(DeletePppUserRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(
            async () =>
            {
                using var connection = await CreateConnectionAsync(request);
                var pppSecrets = connection.LoadAll<PppSecret>();
                var user = pppSecrets.FirstOrDefault(p => p.Name == request.PppUsername);

                if (user == null)
                {
                    return MikroTikResult.FailureResult("المستخدم غير موجود", $"PPP user '{request.PppUsername}' not found");
                }

                user.Disabled = true;
                connection.Save(user);

                _logger.LogInformation("Deactivated PPP user '{PppUser}' on MikroTik at {Host}", request.PppUsername, request.Host);
                return MikroTikResult.SuccessResult("تم إيقاف الاشتراك بنجاح");
            },
            $"Deactivate PPP user '{request.PppUsername}' on {request.Host}",
            cancellationToken
        );
    }

    public async Task<MikroTikResult<List<ActiveSessionDto>>> GetActiveSessionsAsync(MikroTikConnectionRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(
            async () =>
            {
                using var connection = await CreateConnectionAsync(request);
                var activeSessions = connection.LoadAll<PppActive>();
                var sessions = activeSessions.Select(s => new ActiveSessionDto
                {
                    Id = s.Id?.ToString() ?? string.Empty,
                    Name = s.Name,
                    Service = s.Service,
                    CallerId = s.CallerId,
                    Address = s.Address,
                    Uptime = s.Uptime,
                    Encoding = s.Encoding,
                    SessionId = s.SessionId,
                    LimitBytesIn = s.LimitBytesIn,
                    LimitBytesOut = s.LimitBytesOut
                }).ToList();

                _logger.LogInformation("Retrieved {Count} active sessions from MikroTik at {Host}", sessions.Count, request.Host);
                return MikroTikResult<List<ActiveSessionDto>>.SuccessResult(sessions, "تم جلب الجلسات النشطة بنجاح");
            },
            $"Get active sessions from {request.Host}",
            cancellationToken
        );
    }

    public async Task<MikroTikResult<ActiveSessionDto?>> GetUserSessionAsync(DeletePppUserRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(
            async () =>
            {
                using var connection = await CreateConnectionAsync(request);
                var activeSessions = connection.LoadAll<PppActive>();
                var userSession = activeSessions.FirstOrDefault(s => s.Name == request.PppUsername);

                if (userSession == null)
                {
                    return MikroTikResult<ActiveSessionDto?>.SuccessResult(null, "المستخدم غير متصل حالياً");
                }

                var sessionDto = new ActiveSessionDto
                {
                    Id = userSession.Id?.ToString() ?? string.Empty,
                    Name = userSession.Name,
                    Service = userSession.Service,
                    CallerId = userSession.CallerId,
                    Address = userSession.Address,
                    Uptime = userSession.Uptime,
                    SessionId = userSession.SessionId
                };

                _logger.LogInformation("Retrieved session for user '{PppUser}' from MikroTik at {Host}", request.PppUsername, request.Host);
                return MikroTikResult<ActiveSessionDto?>.SuccessResult(sessionDto, "تم جلب معلومات الجلسة بنجاح");
            },
            $"Get user session for '{request.PppUsername}' from {request.Host}",
            cancellationToken
        );
    }

    public async Task<MikroTikResult> DisconnectUserAsync(DeletePppUserRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(
            async () =>
            {
                using var connection = await CreateConnectionAsync(request);
                var activeSessions = connection.LoadAll<PppActive>();
                var userSession = activeSessions.FirstOrDefault(s => s.Name == request.PppUsername);

                if (userSession == null)
                {
                    return MikroTikResult.FailureResult("المستخدم غير متصل حالياً", $"User '{request.PppUsername}' is not currently connected");
                }

                connection.Delete(userSession);
                _logger.LogInformation("Disconnected user '{PppUser}' from MikroTik at {Host}", request.PppUsername, request.Host);
                return MikroTikResult.SuccessResult("تم قطع اتصال المستخدم بنجاح");
            },
            $"Disconnect user '{request.PppUsername}' from {request.Host}",
            cancellationToken
        );
    }

    public async Task<MikroTikResult> UpdateUserProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(
            async () =>
            {
                using var connection = await CreateConnectionAsync(request);
                var pppSecrets = connection.LoadAll<PppSecret>();
                var user = pppSecrets.FirstOrDefault(p => p.Name == request.PppUsername);

                if (user == null)
                {
                    return MikroTikResult.FailureResult("المستخدم غير موجود", $"PPP user '{request.PppUsername}' not found");
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
                return MikroTikResult.SuccessResult("تم تحديث الباقة بنجاح");
            },
            $"Update profile for PPP user '{request.PppUsername}' on {request.Host}",
            cancellationToken
        );
    }

    public async Task<MikroTikResult<List<PppProfileDto>>> GetPppProfilesAsync(MikroTikConnectionRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(
            async () =>
            {
                using var connection = await CreateConnectionAsync(request);
                var profiles = connection.LoadAll<PppProfile>();
                var profileList = profiles.Select(p => new PppProfileDto
                {
                    Id = p.Id?.ToString() ?? string.Empty,
                    Name = p.Name,
                    LocalAddress = p.LocalAddress,
                    RemoteAddress = p.RemoteAddress,
                    RateLimit = p.RateLimit,
                    OnlyOne = bool.TryParse(p.OnlyOne, out var onlyOne) && onlyOne
                }).ToList();

                _logger.LogInformation("Retrieved {Count} PPP profiles from MikroTik at {Host}", profileList.Count, request.Host);
                return MikroTikResult<List<PppProfileDto>>.SuccessResult(profileList, "تم جلب قائمة الباقات بنجاح");
            },
            $"Get PPP profiles from {request.Host}",
            cancellationToken
        );
    }

    public async Task<MikroTikResult<PppProfileDto>> AddPppProfileAsync(AddProfileRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(
            async () =>
            {
                using var connection = await CreateConnectionAsync(request);
                
                // Check if profile already exists
                var existingProfiles = connection.LoadAll<PppProfile>();
                if (existingProfiles.Any(p => p.Name == request.ProfileName))
                {
                    return MikroTikResult<PppProfileDto>.FailureResult("الباقة موجودة بالفعل", $"PPP profile '{request.ProfileName}' already exists");
                }

                var newProfile = new PppProfile
                {
                    Name = request.ProfileName,
                    RateLimit = request.RateLimit,
                    LocalAddress = request.LocalAddress ?? "",
                    RemoteAddress = request.RemoteAddress ?? ""
                };

                connection.Save(newProfile);

                var profileDto = new PppProfileDto
                {
                    Id = newProfile.Id?.ToString() ?? string.Empty,
                    Name = newProfile.Name,
                    RateLimit = newProfile.RateLimit,
                    LocalAddress = newProfile.LocalAddress,
                    RemoteAddress = newProfile.RemoteAddress
                };

                _logger.LogInformation("Added new PPP profile '{ProfileName}' with rate limit '{RateLimit}' to MikroTik at {Host}", 
                    request.ProfileName, request.RateLimit, request.Host);
                return MikroTikResult<PppProfileDto>.SuccessResult(profileDto, "تم إضافة الباقة بنجاح");
            },
            $"Add PPP profile '{request.ProfileName}' to {request.Host}",
            cancellationToken
        );
    }

    public async Task<MikroTikResult> UpdatePppProfileAsync(UpdatePppProfileRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(
            async () =>
            {
                using var connection = await CreateConnectionAsync(request);
                var profiles = connection.LoadAll<PppProfile>();
                var profile = profiles.FirstOrDefault(p => p.Name == request.ProfileName);

                if (profile == null)
                {
                    return MikroTikResult.FailureResult("الباقة غير موجودة", $"PPP profile '{request.ProfileName}' not found");
                }

                var oldRateLimit = profile.RateLimit;
                profile.RateLimit = request.RateLimit;
                connection.Save(profile);

                _logger.LogInformation("Updated PPP profile '{ProfileName}' rate limit from '{OldRate}' to '{NewRate}' on MikroTik at {Host}", 
                    request.ProfileName, oldRateLimit, request.RateLimit, request.Host);
                return MikroTikResult.SuccessResult("تم تحديث سرعة الباقة بنجاح");
            },
            $"Update PPP profile '{request.ProfileName}' on {request.Host}",
            cancellationToken
        );
    }

    public async Task<MikroTikResult> DeletePppProfileAsync(DeleteProfileRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(
            async () =>
            {
                using var connection = await CreateConnectionAsync(request);
                var profiles = connection.LoadAll<PppProfile>();
                var profile = profiles.FirstOrDefault(p => p.Name == request.ProfileName);

                if (profile == null)
                {
                    return MikroTikResult.FailureResult("الباقة غير موجودة", $"PPP profile '{request.ProfileName}' not found");
                }

                connection.Delete(profile);
                _logger.LogInformation("Deleted PPP profile '{ProfileName}' from MikroTik at {Host}", request.ProfileName, request.Host);
                return MikroTikResult.SuccessResult("تم حذف الباقة بنجاح");
            },
            $"Delete PPP profile '{request.ProfileName}' from {request.Host}",
            cancellationToken
        );
    }

    private async Task<ITikConnection> CreateConnectionAsync(MikroTikConnectionRequest request)
    {
        var connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
        
        await Task.Run(() => 
        {
            connection.Open(request.Host, request.Username, request.Password);
        });

        return connection;
    }

    private async Task<MikroTikResult> ExecuteWithRetryAsync(Func<Task<MikroTikResult>> operation, string operationName, CancellationToken cancellationToken)
    {
        var maxRetries = _settings.RetryAttempts;
        var delay = TimeSpan.FromMilliseconds(_settings.InitialRetryDelayMs);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (TikConnectionException ex)
            {
                _logger.LogError(ex, "MikroTik connection error during {OperationName} (attempt {Attempt}/{MaxRetries})", 
                    operationName, attempt, maxRetries);

                if (attempt == maxRetries)
                {
                    return MikroTikResult.FailureResult("فشل الاتصال بجهاز MikroTik", ex.Message);
                }

                await Task.Delay(delay * attempt, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during {OperationName} (attempt {Attempt}/{MaxRetries})", 
                    operationName, attempt, maxRetries);

                if (attempt == maxRetries)
                {
                    return MikroTikResult.FailureResult("حدث خطأ غير متوقع", ex.Message);
                }

                await Task.Delay(delay * attempt, cancellationToken);
            }
        }

        return MikroTikResult.FailureResult("فشلت العملية بعد عدة محاولات", "Operation failed after multiple retries");
    }

    private async Task<MikroTikResult<T>> ExecuteWithRetryAsync<T>(Func<Task<MikroTikResult<T>>> operation, string operationName, CancellationToken cancellationToken)
    {
        var maxRetries = _settings.RetryAttempts;
        var delay = TimeSpan.FromMilliseconds(_settings.InitialRetryDelayMs);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (TikConnectionException ex)
            {
                _logger.LogError(ex, "MikroTik connection error during {OperationName} (attempt {Attempt}/{MaxRetries})", 
                    operationName, attempt, maxRetries);

                if (attempt == maxRetries)
                {
                    return MikroTikResult<T>.FailureResult("فشل الاتصال بجهاز MikroTik", ex.Message);
                }

                await Task.Delay(delay * attempt, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during {OperationName} (attempt {Attempt}/{MaxRetries})", 
                    operationName, attempt, maxRetries);

                if (attempt == maxRetries)
                {
                    return MikroTikResult<T>.FailureResult("حدث خطأ غير متوقع", ex.Message);
                }

                await Task.Delay(delay * attempt, cancellationToken);
            }
        }

        return MikroTikResult<T>.FailureResult("فشلت العملية بعد عدة محاولات", "Operation failed after multiple retries");
    }
}

public class MikroTikSettings
{
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryAttempts { get; set; } = 3;
    public int InitialRetryDelayMs { get; set; } = 1000;
}
