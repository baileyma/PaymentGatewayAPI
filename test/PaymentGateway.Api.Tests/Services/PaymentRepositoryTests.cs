using AutoFixture;

using PaymentGateway.Api.Models.Common;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests.Services;

public class PaymentRepositoryTests
{
    private readonly Fixture _fixture;
    private readonly PaymentsRepository _repo;

    public PaymentRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Customize<Expiry>(c => c.FromFactory(() => new Expiry(2027, 6)));
        _fixture.Customize<Money>(c => c.FromFactory(() => new Money("GBP", 1000)));
        _repo = new PaymentsRepository();
    }

    [Fact]
    public void Add_ThenGet_ReturnsSamePayment()
    {
        var payment = _fixture.Create<PaymentResponse>();
        _repo.Add(payment);

        var result = _repo.Get(payment.Id);

        Assert.Equal(payment, result);
    }

    [Fact]
    public void Get_UnknownId_ReturnsNull()
    {
        var result = _repo.Get(Guid.NewGuid());

        Assert.Null(result);
    }
}
