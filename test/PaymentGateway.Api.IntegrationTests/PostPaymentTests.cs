namespace PaymentGateway.Api.IntegrationTests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Models.Enums;
using PaymentGateway.Api.Models.Common;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

public class PostPaymentTests : IClassFixture<CustomWebApplicationFactory>
{
    private CustomWebApplicationFactory _factory;

    private readonly static JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };

    public PostPaymentTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.WireMockServer.Reset();
    }   

    [Fact]
    public async Task PostPaymentsEndpoint_Returns200AndAuthorizedPayment_WhenCardNumberIsOdd()
    {
        var bankResponse = """
        {
            "authorized": true,
            "authorization_code": "d2277fbe-68ef-4908-852b-b0ffac7858d9"
        }
        """;

    _factory.WireMockServer.Given(Request.Create().WithPath("/payments").UsingPost())
        .RespondWith(Response.Create().WithStatusCode(200)
        .WithHeader("Content-Type", "application/json")
        .WithBody(bankResponse));

        var request = CreatePaymentRequest("2222405343248877");

        var httpResponse = await _factory.Client.PostAsJsonAsync("/api/payments", request);
        
        httpResponse.EnsureSuccessStatusCode();
        var response = await httpResponse.Content.ReadFromJsonAsync<Result<PaymentResponse>>(JsonOptions);
        Assert.Equal(PaymentStatus.Authorized, response.Status);
    }

    [Fact]
    public async Task PostPaymentsEndpoint_Returns200AndDeclinedPayment_WhenCardNumberIsEven()
    {
        // Arrange
        var bankResponse = """
        {
            "authorized": false,
            "authorization_code": ""
        }
        """;

        _factory.WireMockServer.Given(Request.Create().WithPath("/payments").UsingPost())
        .RespondWith(Response.Create().WithStatusCode(200)
        .WithHeader("Content-Type", "application/json")
        .WithBody(bankResponse));

        var request = CreatePaymentRequest("2222405343248876");

        // Act
        var httpResponse = await _factory.Client.PostAsJsonAsync("/api/payments", request);

        // Assert
        httpResponse.EnsureSuccessStatusCode();
        var response = await httpResponse.Content.ReadFromJsonAsync<Result<PaymentResponse>>(JsonOptions);
        Assert.Equal(PaymentStatus.Declined, response.Status);
        Assert.Equal("8876", response.Value.CardNumberLastFour);
    }

    [Fact]
    public async Task PostPaymentEndpoint_Returns502_WhenCardNumberEndsInZero()
    {
        // Arrange
        var bankResponse = """{}""";
        var request = CreatePaymentRequest("2222405343248870");

        _factory.WireMockServer.Given(Request.Create().WithPath("/payments").UsingPost())
        .RespondWith(Response.Create().WithStatusCode(503)
        .WithHeader("Content-Type", "application/json")
        .WithBody(bankResponse));

        // Act
        var httpResponse = await _factory.Client.PostAsJsonAsync("/api/payments", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadGateway, httpResponse.StatusCode);
    }

    [Fact]
    public async Task PostPaymentEndpoint_Returns400WithoutCallingBankClient_WhenInvalidRequest()
    {
        var invalidRequest = new PaymentRequest()
        {
            CardNumber = "2222405343248877",
            Expiry = new Expiry(DateTime.Today.AddYears(1).Year, 1),
            Money = new Money("GBP", 100),
            Cvv = "not-a-number"
        };

        var httpResponse = await _factory.Client.PostAsJsonAsync("/api/payments", invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
        Assert.Empty(_factory.WireMockServer.LogEntries);

    }

    private PaymentRequest CreatePaymentRequest(string cardNumber)
    {
        return new()
        {
            CardNumber = cardNumber,
            Expiry = new Expiry(DateTime.Today.AddYears(1).Year, 1),
            Money = new Money("GBP", 100),
            Cvv = "123"
        };
    }

}
