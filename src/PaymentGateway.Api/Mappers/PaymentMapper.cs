using PaymentGateway.Api.Models.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Mappers;

public static class PaymentMapper
{
    public static BankRequest MapFromPaymentRequest(PaymentRequest paymentRequest) => new ()
    {
        CardNumber = paymentRequest.CardNumber.ToString(),
        ExpiryDate = paymentRequest.Expiry.ToString(),
        Currency = paymentRequest.Money.Currency,
        Amount = paymentRequest.Money.Amount,
        CVV = paymentRequest.Cvv
    };

    public static PaymentResponse MapToPaymentReponse(BankResponse bankClientResponse, PaymentRequest paymentRequest) => new()
    {
        Id = Guid.NewGuid(),
        CardNumberLastFour = paymentRequest.CardNumber[^4..],
        Expiry = paymentRequest.Expiry,
        Money = paymentRequest.Money,
        Status = bankClientResponse.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined
    };
}
