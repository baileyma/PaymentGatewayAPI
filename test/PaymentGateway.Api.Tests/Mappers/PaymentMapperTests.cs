using AutoFixture;

using PaymentGateway.Api.Mappers;
using PaymentGateway.Api.Models.Common;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Tests.Mappers;

public class PaymentMapperTests
{
    private readonly Fixture _fixture;

    public PaymentMapperTests()
    {
        _fixture = new Fixture();
        _fixture.Customize<Expiry>(c => c.FromFactory(() => new Expiry(2027, 6)));
        _fixture.Customize<Money>(c => c.FromFactory(() => new Money("GBP", _fixture.Create<int>())));
        _fixture.Customize<PaymentRequest>(c => c
            .With(x => x.CardNumber, "12345678901234")
            .With(x => x.Cvv, "123"));
    }

    [Fact]
    public void Map_SetsCardNumberAsString()
    {
        var request = _fixture.Create<PaymentRequest>();

        var result = PaymentMapper.Map(request);

        Assert.Equal(request.CardNumber, result.CardNumber);
    }

    [Fact]
    public void Map_SetsExpiryDateFromExpiry()
    {
        _fixture.Customize<Expiry>(c => c.FromFactory(() => new Expiry(2027, 3)));
        var request = _fixture.Create<PaymentRequest>();

        var result = PaymentMapper.Map(request);

        Assert.Equal("03/2027", result.ExpiryDate);
    }

    [Fact]
    public void Map_SetsCurrencyFromMoney()
    {
        _fixture.Customize<Money>(c => c.FromFactory(() => new Money("USD", 500)));
        var request = _fixture.Create<PaymentRequest>();

        var result = PaymentMapper.Map(request);

        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void Map_SetsAmountFromMoney()
    {
        _fixture.Customize<Money>(c => c.FromFactory(() => new Money("GBP", 9999)));
        var request = _fixture.Create<PaymentRequest>();

        var result = PaymentMapper.Map(request);

        Assert.Equal(9999, result.Amount);
    }

    [Fact]
    public void Map_SetsCvv()
    {
        _fixture.Customize<PaymentRequest>(c => c
            .With(x => x.CardNumber, "12345678901234")
            .With(x => x.Cvv, "456"));
        var request = _fixture.Create<PaymentRequest>();

        var result = PaymentMapper.Map(request);

        Assert.Equal("456", result.CVV);
    }
}
