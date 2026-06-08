using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models.Enums;

namespace PaymentGateway.Api.Mappers;

public static class PaymentMapper
{
    public static BankRequest Map(PaymentRequest paymentRequest) => 
        new BankRequest(paymentRequest.CardNumber.ToString(), paymentRequest.Expiry.ToString(), paymentRequest.Money.Currency, paymentRequest.Money.Amount, paymentRequest.Cvv);
    
}
