using Microsoft.Extensions.DependencyInjection;
using BroadbandBilling.Application.UseCases.Subscribers.CreateSubscriber;
using BroadbandBilling.Application.UseCases.Subscribers.GetSubscriber;
using BroadbandBilling.Application.UseCases.Subscribers.ListSubscribers;
using BroadbandBilling.Application.UseCases.Plans.CreatePlan;
using BroadbandBilling.Application.UseCases.Subscriptions.CreateSubscription;
using BroadbandBilling.Application.UseCases.Subscriptions.RenewSubscription;
using BroadbandBilling.Application.UseCases.Billing.GenerateInvoice;
using BroadbandBilling.Application.UseCases.Billing.ProcessPayment;

namespace BroadbandBilling.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateSubscriberHandler>();
        services.AddScoped<GetSubscriberHandler>();
        services.AddScoped<ListSubscribersHandler>();
        
        services.AddScoped<CreatePlanHandler>();
        
        services.AddScoped<CreateSubscriptionHandler>();
        services.AddScoped<RenewSubscriptionHandler>();
        
        services.AddScoped<GenerateInvoiceHandler>();
        services.AddScoped<ProcessPaymentHandler>();

        return services;
    }
}
