using BroadbandBilling.Application.Interfaces;
using BroadbandBilling.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BroadbandBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public ReportsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats(CancellationToken cancellationToken)
    {
        var totalSubscribers = await _context.Subscribers.CountAsync(cancellationToken);
        var activeSubscriptions = await _context.Subscriptions
            .CountAsync(s => s.Status == SubscriptionStatus.Active, cancellationToken);

        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        decimal monthlyRevenue = 0;
        try
        {
            var payments = await _context.Payments
                .Where(p => p.PaymentDate >= startOfMonth)
                .Select(p => p.Amount.Amount)
                .ToListAsync(cancellationToken);
            monthlyRevenue = payments.Sum();
        }
        catch { /* No payments yet */ }

        var overdueInvoices = await _context.Invoices
            .CountAsync(i => i.Status == InvoiceStatus.Overdue, cancellationToken);

        return Ok(new
        {
            totalSubscribers,
            activeSubscriptions,
            monthlyRevenue,
            overdueInvoices
        });
    }
}
