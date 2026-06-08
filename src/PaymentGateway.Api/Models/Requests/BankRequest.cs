using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Requests;

public class BankRequest
{
    public BankRequest(string cardNumber, string expiryDate, string currency, int amount, string cvv)
    {
        CardNumber = cardNumber;
        ExpiryDate = expiryDate;
        Currency = currency;
        Amount = amount;
        CVV = cvv;
    }

    public string CardNumber { get; init; }

    public string ExpiryDate { get; init; }
    
    public string Currency { get; init; }
    
    public int Amount { get; init; }
    
    public string CVV { get; init; }
};
