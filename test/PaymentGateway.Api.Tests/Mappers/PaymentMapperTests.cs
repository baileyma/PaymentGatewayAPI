using PaymentGateway.Api.Mappers;
using PaymentGateway.Api.Models.Common;
using PaymentGateway.Api.Models.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Tests.Mappers;

public class PaymentMapperTests
{
    [Fact]
    public void MapFromPaymentRequest_MapsAllFieldsCorrectly()
    {
        var request = new PaymentRequest
        {
            CardNumber = "12345678901234",
            Expiry = new Expiry(2027, 3),
            Money = new Money("GBP", 1050),
            Cvv = "123"
        };

        var result = PaymentMapper.MapFromPaymentRequest(request);

        Assert.Equal("12345678901234", result.CardNumber);
        Assert.Equal("03/2027", result.ExpiryDate);
        Assert.Equal("GBP", result.Currency);
        Assert.Equal(1050, result.Amount);
        Assert.Equal("123", result.CVV);
    }

    [Fact]
    public void MapToPaymentResponse_MapsAllFieldsCorrectly()
    {
        var request = new PaymentRequest
        {
            CardNumber = "12345678901234",
            Expiry = new Expiry(2027, 3),
            Money = new Money("GBP", 1050),
            Cvv = "123"
        };

        var bankResponse = new BankResponse
        {
            Authorized = true,
            AuthorizationCode = "abc123"
        };

        var result = PaymentMapper.MapToPaymentReponse(bankResponse, request);

        Assert.Equal("1234", result.CardNumberLastFour);
        Assert.Equal(request.Expiry, result.Expiry);
        Assert.Equal(request.Money, result.Money);
        Assert.Equal(PaymentStatus.Authorized, result.Status);
    }
}
