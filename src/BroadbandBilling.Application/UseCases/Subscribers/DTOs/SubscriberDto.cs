using BroadbandBilling.Application.Common.DTOs;

namespace BroadbandBilling.Application.UseCases.Subscribers.DTOs;

public class SubscriberDto : BaseDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSubscriberDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? NationalId { get; set; }
}

public class UpdateSubscriberDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? NationalId { get; set; }
}
