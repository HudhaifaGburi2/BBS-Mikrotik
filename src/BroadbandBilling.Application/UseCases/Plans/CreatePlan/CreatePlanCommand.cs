using BroadbandBilling.Application.UseCases.Plans.DTOs;

namespace BroadbandBilling.Application.UseCases.Plans.CreatePlan;

public class CreatePlanCommand
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int SpeedMbps { get; set; }
    public int DataLimitGB { get; set; }
    public int BillingCycleDays { get; set; } = 30;
    public int? BillingCycleHours { get; set; }
    public string MikroTikProfileName { get; set; } = string.Empty;
}

public class CreatePlanResult
{
    public PlanDto Plan { get; set; } = null!;
}
