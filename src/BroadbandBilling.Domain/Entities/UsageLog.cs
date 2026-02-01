using BroadbandBilling.Domain.Interfaces;
using BroadbandBilling.Domain.ValueObjects;

namespace BroadbandBilling.Domain.Entities;

public class UsageLog : IEntity
{
    public Guid Id { get; private set; }
    public Guid SubscriberId { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public Guid PppoeAccountId { get; private set; }
    public DateTime SessionStart { get; private set; }
    public DateTime? SessionEnd { get; private set; }
    public long UploadBytes { get; private set; }
    public long DownloadBytes { get; private set; }
    public long TotalBytes { get; private set; }
    public IpAddress? IpAddress { get; private set; }
    public string? CallingStationId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Subscriber Subscriber { get; private set; }
    public Subscription Subscription { get; private set; }
    public PppoeAccount PppoeAccount { get; private set; }

    private UsageLog() { }

    private UsageLog(Guid subscriberId, Guid subscriptionId, Guid pppoeAccountId,
        DateTime sessionStart, IpAddress? ipAddress, string? callingStationId)
    {
        Id = Guid.NewGuid();
        SubscriberId = subscriberId;
        SubscriptionId = subscriptionId;
        PppoeAccountId = pppoeAccountId;
        SessionStart = sessionStart;
        IpAddress = ipAddress;
        CallingStationId = callingStationId;
        UploadBytes = 0;
        DownloadBytes = 0;
        TotalBytes = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public static UsageLog Create(Guid subscriberId, Guid subscriptionId,
        Guid pppoeAccountId, DateTime sessionStart, string? ipAddress = null,
        string? callingStationId = null)
    {
        if (subscriberId == Guid.Empty)
            throw new ArgumentException("Subscriber ID is required", nameof(subscriberId));

        if (subscriptionId == Guid.Empty)
            throw new ArgumentException("Subscription ID is required", nameof(subscriptionId));

        if (pppoeAccountId == Guid.Empty)
            throw new ArgumentException("PPPoE Account ID is required", nameof(pppoeAccountId));

        IpAddress? ip = null;
        if (!string.IsNullOrWhiteSpace(ipAddress))
        {
            ip = IpAddress.Create(ipAddress);
        }

        return new UsageLog(subscriberId, subscriptionId, pppoeAccountId,
            sessionStart, ip, callingStationId);
    }

    public void UpdateUsage(long uploadBytes, long downloadBytes)
    {
        if (uploadBytes < 0)
            throw new ArgumentException("Upload bytes cannot be negative", nameof(uploadBytes));

        if (downloadBytes < 0)
            throw new ArgumentException("Download bytes cannot be negative", nameof(downloadBytes));

        UploadBytes = uploadBytes;
        DownloadBytes = downloadBytes;
        TotalBytes = uploadBytes + downloadBytes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EndSession(DateTime sessionEnd, long uploadBytes, long downloadBytes)
    {
        if (SessionEnd.HasValue)
            throw new InvalidOperationException("Session has already ended");

        if (sessionEnd < SessionStart)
            throw new ArgumentException("Session end cannot be before session start");

        SessionEnd = sessionEnd;
        UpdateUsage(uploadBytes, downloadBytes);
        UpdatedAt = DateTime.UtcNow;
    }

    public TimeSpan? GetSessionDuration()
    {
        if (!SessionEnd.HasValue) return null;
        return SessionEnd.Value - SessionStart;
    }

    public double GetDataUsageGB()
    {
        return TotalBytes / (1024.0 * 1024.0 * 1024.0);
    }

    public double GetUploadGB()
    {
        return UploadBytes / (1024.0 * 1024.0 * 1024.0);
    }

    public double GetDownloadGB()
    {
        return DownloadBytes / (1024.0 * 1024.0 * 1024.0);
    }

    public bool IsActive()
    {
        return !SessionEnd.HasValue;
    }
}
