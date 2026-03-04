using AutoMapper;
using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Application.UseCases.Subscribers.DTOs;
using BroadbandBilling.Application.UseCases.Plans.DTOs;
using BroadbandBilling.Application.UseCases.Subscriptions.DTOs;
using BroadbandBilling.Application.UseCases.Invoices.DTOs;
using BroadbandBilling.Application.UseCases.Payments.DTOs;

namespace BroadbandBilling.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Subscriber, SubscriberDto>();
        CreateMap<CreateSubscriberDto, Subscriber>();
        CreateMap<UpdateSubscriberDto, Subscriber>();

        CreateMap<Plan, PlanDto>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Price.Currency));

        CreateMap<Subscription, SubscriptionDto>()
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.BillingPeriod.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.BillingPeriod.EndDate))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.Plan != null ? src.Plan.Name : string.Empty))
            .ForMember(dest => dest.RemainingDays, opt => opt.MapFrom(src => src.GetRemainingDays()))
            .ForMember(dest => dest.DataLimitGB, opt => opt.MapFrom(src => src.Plan != null ? src.Plan.DataLimitGB : 0));

        CreateMap<Invoice, InvoiceDto>()
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal.Amount))
            .ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount.Amount))
            .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => src.DiscountAmount.Amount))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount.Amount))
            .ForMember(dest => dest.PaidAmount, opt => opt.MapFrom(src => src.PaidAmount.Amount))
            .ForMember(dest => dest.RemainingAmount, opt => opt.MapFrom(src => src.GetRemainingAmount().Amount))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.SubscriberName, opt => opt.MapFrom(src => src.Subscriber != null ? src.Subscriber.FullName : string.Empty))
            .ForMember(dest => dest.DaysOverdue, opt => opt.MapFrom(src => src.GetDaysOverdue()));

        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Amount))
            .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.InvoiceNumber : string.Empty))
            .ForMember(dest => dest.SubscriberName, opt => opt.MapFrom(src => src.Subscriber != null ? src.Subscriber.FullName : string.Empty));
    }
}
