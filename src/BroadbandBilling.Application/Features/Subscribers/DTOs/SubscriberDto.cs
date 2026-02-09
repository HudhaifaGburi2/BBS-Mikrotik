using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Domain.Enums;
using MediatR;

namespace BroadbandBilling.Application.Features.Subscribers.DTOs;

public record SubscriberDto(
    Guid Id,
    string FullName,
    string Email,
    string PhoneNumber,
    string Address,
    string? NationalId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<SubscriptionDto>? Subscriptions = null,
    List<PppoeAccountDto>? PppoeAccounts = null
);

public record SubscriptionDto(
    Guid Id,
    Guid PlanId,
    string PlanName,
    SubscriptionStatus Status,
    DateTime StartDate,
    DateTime EndDate,
    DateTime? ActivatedAt,
    DateTime? SuspendedAt
);

public record PppoeAccountDto(
    Guid Id,
    string Username,
    string ProfileName,
    bool IsEnabled,
    bool IsSyncedWithMikroTik,
    DateTime? LastSyncDate,
    ValidationStatus ValidationStatus,
    bool IsOnline,
    DateTime? LastConnectedAt
);
