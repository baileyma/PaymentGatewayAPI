namespace PaymentGateway.Api.IntegrationTests;

using System.Text.Json;

using Microsoft.VisualStudio.TestPlatform.Utilities.Helpers;

using WireMock;

public class PostPaymentTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime

    // idisposable
{
    private readonly WireMockFixture _wireMockFixture;

    private CustomWebApplicationFactory _factory;

    private HttpClient _client;

    public PostPaymentTests(WireMockFixture wireMockFixture)
    {
        _wireMockFixture = wireMockFixture;
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HappyPath()
    {

        var bankResponse = """
    {
        "authorized": false,
        "authorization_code": ""
    }
""";

        _wireMockFixture.Server.Given(Request.Create().WithPath("/payments").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200))
            .WithHeader("???")
            .WithBody(bankResponse);


        var response = await _wireMockFixture.

        response
    }

    [Fact]
    public async Task OddNumber()
    {
        var bankResponse = """
            {
    "authorized": true,
    "authorization_code": "d2277fbe-68ef-4908-852b-b0ffac7858d9"
    }
""";
    }

    [Fact]
    public async Task Zero()
    {
        var bankResponse = """
            {}
""";
    }

    [Fact]
    public async Task ValidFailure()
    {
        var bankResponse = """
            {
    "error_message": "Not all required properties were sent in the request"
}
""";
    }

    

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
