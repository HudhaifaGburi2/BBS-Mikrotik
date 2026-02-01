using BroadbandBilling.Application.Common.DTOs;

namespace BroadbandBilling.Application.UseCases.Plans.DTOs;

public class PlanDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int SpeedMbps { get; set; }
    public int DataLimitGB { get; set; }
    public int BillingCycleDays { get; set; }
    public bool IsActive { get; set; }
    public string MikroTikProfileName { get; set; } = string.Empty;
}

public class CreatePlanDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int SpeedMbps { get; set; }
    public int DataLimitGB { get; set; }
    public int BillingCycleDays { get; set; }
    public string MikroTikProfileName { get; set; } = string.Empty;
}

public class UpdatePlanDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int SpeedMbps { get; set; }
    public int DataLimitGB { get; set; }
}
