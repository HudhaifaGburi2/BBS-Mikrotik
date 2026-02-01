using AutoMapper;
using BroadbandBilling.Application.Common.Interfaces;
using BroadbandBilling.Application.UseCases.Payments.DTOs;
using BroadbandBilling.Domain.Entities;
using BroadbandBilling.Domain.Enums;
using BroadbandBilling.Domain.Exceptions;
using BroadbandBilling.Domain.ValueObjects;

namespace BroadbandBilling.Application.UseCases.Billing.ProcessPayment;

public class ProcessPaymentHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProcessPaymentHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProcessPaymentResult> HandleAsync(ProcessPaymentCommand command, CancellationToken cancellationToken = default)
    {
        var invoice = await _unitOfWork.Invoices.GetWithPaymentsAsync(command.InvoiceId, cancellationToken);
        if (invoice == null)
        {
            throw new InvalidInvoiceException($"Invoice with ID '{command.InvoiceId}' not found");
        }

        if (!Enum.TryParse<PaymentMethod>(command.Method, out var paymentMethod))
        {
            throw new InvalidPaymentException($"Invalid payment method: {command.Method}");
        }

        var paymentReference = await _unitOfWork.Payments.GeneratePaymentReferenceAsync(cancellationToken);

        var payment = Payment.Create(
            command.InvoiceId,
            invoice.SubscriberId,
            paymentReference,
            command.Amount,
            paymentMethod,
            command.PaymentDate,
            command.TransactionId,
            command.Notes
        );

        payment.MarkAsCompleted(command.TransactionId);

        invoice.AddPayment(Money.Create(command.Amount, invoice.TotalAmount.Currency));

        await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
        _unitOfWork.Invoices.Update(invoice);
        await _unitOfWork.CommitAsync(cancellationToken);

        var paymentDto = _mapper.Map<PaymentDto>(payment);

        return new ProcessPaymentResult
        {
            Payment = paymentDto,
            InvoiceFullyPaid = invoice.IsFullyPaid()
        };
    }
}
