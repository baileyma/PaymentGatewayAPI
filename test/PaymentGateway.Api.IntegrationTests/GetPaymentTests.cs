namespace PaymentGateway.Api.IntegrationTests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Models.Enums;
using PaymentGateway.Api.Models.Common;
using PaymentGateway.Api.Models.Responses;

public class GetPaymentTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    private readonly static JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };

    public GetPaymentTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.Repository.Payments.Clear();
    }

    [Fact]
    public async Task GetPaymentEndpoint_Returns200AndPayment_WhenPaymentExists()
    {
        // Arrange
        var payment = new PaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = "8877",
            Expiry = new Expiry(DateTime.Today.AddYears(1).Year, 1),
            Money = new Money("GBP", 100)
        };
        _factory.Repository.Add(payment);

        // Act
        var httpResponse = await _factory.Client.GetAsync($"/api/payments/{payment.Id}");

        // Assert
        httpResponse.EnsureSuccessStatusCode();
        var response = await httpResponse.Content.ReadFromJsonAsync<PaymentResponse>(JsonOptions);
        Assert.Equal(payment.Id, response!.Id);
        Assert.Equal(PaymentStatus.Authorized, response.Status);
        Assert.Equal("8877", response.CardNumberLastFour);
    }

    [Fact]
    public async Task GetPaymentEndpoint_Returns404_WhenPaymentNotFound()
    {
        var httpResponse = await _factory.Client.GetAsync($"/api/payments/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task GetPaymentEndpoint_Returns404_WhenIdIsNotAGuid()
    {
        var httpResponse = await _factory.Client.GetAsync("/api/payments/not-a-guid");

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }
}
