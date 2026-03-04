using AutoMapper;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Plans.DTOs;
using BroadbandBilling.Domain.Entities;

namespace BroadbandBilling.Application.UseCases.Plans.CreatePlan;

public class CreatePlanHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreatePlanHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CreatePlanResult> HandleAsync(CreatePlanCommand command, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Plans.PlanNameExistsAsync(command.Name, null, cancellationToken))
        {
            throw new InvalidOperationException($"Plan with name '{command.Name}' already exists");
        }

        var plan = Plan.Create(
            command.Name,
            command.Description,
            command.Price,
            command.SpeedMbps,
            command.DataLimitGB,
            command.BillingCycleDays,
            command.MikroTikProfileName,
            command.Currency,
            command.BillingCycleHours
        );

        await _unitOfWork.Plans.AddAsync(plan, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var planDto = _mapper.Map<PlanDto>(plan);

        return new CreatePlanResult { Plan = planDto };
    }
}
