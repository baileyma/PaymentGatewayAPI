using PaymentGateway.Api.Models.Common;
using PaymentGateway.Api.Models.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Mappers;

public static class PaymentMapper
{
    public static BankRequest Map(PaymentRequest paymentRequest) =>
        new BankRequest()
        {
            CardNumber = paymentRequest.CardNumber.ToString(),
            ExpiryDate = paymentRequest.Expiry.ToString(),
            Currency = paymentRequest.Money.Currency,
            Amount = paymentRequest.Money.Amount,
            CVV = paymentRequest.Cvv
        };
}
