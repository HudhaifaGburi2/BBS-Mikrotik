using AutoMapper;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Invoices.DTOs;
using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Domain.Exceptions;

namespace BroadbandBilling.Application.UseCases.Billing.GenerateInvoice;

public class GenerateInvoiceHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GenerateInvoiceHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<GenerateInvoiceResult> HandleAsync(GenerateInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        var subscriber = await _unitOfWork.Subscribers.GetByIdAsync(command.SubscriberId, cancellationToken);
        if (subscriber == null)
        {
            throw new SubscriberNotFoundException(command.SubscriberId);
        }

        if (command.SubscriptionId.HasValue)
        {
            var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(command.SubscriptionId.Value, cancellationToken);
            if (subscription == null)
            {
                throw new InvalidSubscriptionException($"Subscription with ID '{command.SubscriptionId}' not found");
            }
        }

        var invoiceNumber = await _unitOfWork.Invoices.GenerateInvoiceNumberAsync(cancellationToken);

        var invoice = Invoice.Create(
            invoiceNumber,
            command.SubscriberId,
            command.SubscriptionId,
            command.IssueDate,
            command.DueDays,
            command.Subtotal,
            command.TaxAmount,
            command.DiscountAmount,
            command.Notes
        );

        await _unitOfWork.Invoices.AddAsync(invoice, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var invoiceWithDetails = await _unitOfWork.Invoices
            .GetWithPaymentsAsync(invoice.Id, cancellationToken);

        var invoiceDto = _mapper.Map<InvoiceDto>(invoiceWithDetails);

        return new GenerateInvoiceResult { Invoice = invoiceDto };
    }
}
