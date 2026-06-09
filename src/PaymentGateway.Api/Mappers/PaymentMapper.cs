using PaymentGateway.Api.Models.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Mappers;

public static class PaymentMapper
{
    public static BankRequest MapFromPaymentRequest(PaymentRequest paymentRequest)
    {
        var expiry = paymentRequest.Expiry;

        return new()
        {
            CardNumber = paymentRequest.CardNumber.ToString(),
            ExpiryDate = new DateOnly(expiry.Year, expiry.Month, 1).ToString("MM/yyyy"),
            Currency = paymentRequest.Money.Currency,
            Amount = paymentRequest.Money.Amount,
            CVV = paymentRequest.Cvv
        };
    }

    public static PaymentResponse MapToPaymentReponse(BankResponse bankClientResponse, PaymentRequest paymentRequest) => new()
    {
        Id = paymentRequest.Id,
        CardNumberLastFour = paymentRequest.CardNumber[^4..],
        Expiry = paymentRequest.Expiry,
        Money = paymentRequest.Money,
        Status = bankClientResponse.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined
    };
}
