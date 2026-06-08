namespace PaymentGateway.Api.IntegrationTests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;

using Models.Enums;

using PaymentGateway.Api.Models.Common;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;


using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

public class PostPaymentTests : IClassFixture<CustomWebApplicationFactory>

    // idisposable
{
    private CustomWebApplicationFactory _factory;

    private readonly static JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };

    public PostPaymentTests(CustomWebApplicationFactory factory)
    {
        //_wireMockFixture = wireMockFixture;
        //_factory = new CustomWebApplicationFactory();
        //_client = _factory.CreateClient();

        _factory = factory;
        _factory.WireMockServer.Reset();
    }

    

    [Fact]
    public async Task HappyPath()
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
    public async Task EvenNumber()
    {
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

        // make helper method

        // do we need these requests?
        var request = CreatePaymentRequest("2222405343248876");

        var httpResponse = await _factory.Client.PostAsJsonAsync("/api/payments", request);

        httpResponse.EnsureSuccessStatusCode();
        var response = await httpResponse.Content.ReadFromJsonAsync<Result<PaymentResponse>>(JsonOptions);
        Assert.Equal(PaymentStatus.Declined, response.Status);

    }

    [Fact]
    public async Task Zero()
    {
        var bankResponse = """
            {}
""";
        var request = CreatePaymentRequest("2222405343248876");

        var httpResponse = await _factory.Client.PostAsJsonAsync("/api/payments", request);

        Assert.Equal(HttpStatusCode.InternalServerError, httpResponse.StatusCode);
        //var response = await httpResponse.Content.ReadFromJsonAsync<PaymentResponse>(JsonOptions);

        //Assert.Equal(response.);


    }

    [Fact]
    public async Task ValidFailure()
    {
        var bankResponse = """
            {
    "error_message": "Not all required properties were sent in the request"
}
""";
        var invalidRequest = new PaymentRequest()
        {
            CardNumber = "2222405343248877",
            Expiry = new Expiry(DateTime.Today.AddYears(1).Year, 1),
            Money = new Money("GBP", 100),
            Cvv = "not-a-number"
        };

        var httpResponse = await _factory.Client.PostAsJsonAsync("/api/payments", invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);

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
