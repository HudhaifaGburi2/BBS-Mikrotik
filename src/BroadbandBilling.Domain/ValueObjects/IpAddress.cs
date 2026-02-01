using System.Net;
using System.Text.RegularExpressions;

namespace BroadbandBilling.Domain.ValueObjects;

public sealed class IpAddress : IEquatable<IpAddress>
{
    public string Value { get; }

    private IpAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("IP Address cannot be empty", nameof(value));

        if (!IsValidIpAddress(value))
            throw new ArgumentException($"Invalid IP Address format: {value}", nameof(value));

        Value = value;
    }

    public static IpAddress Create(string value)
    {
        return new IpAddress(value);
    }

    private static bool IsValidIpAddress(string value)
    {
        return IPAddress.TryParse(value, out _);
    }

    public bool IsPrivate()
    {
        if (!IPAddress.TryParse(Value, out var ip))
            return false;

        var bytes = ip.GetAddressBytes();
        
        return bytes[0] == 10 ||
               (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
               (bytes[0] == 192 && bytes[1] == 168);
    }

    public bool Equals(IpAddress? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is IpAddress address && Equals(address);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(IpAddress ipAddress)
    {
        return ipAddress.Value;
    }
}
