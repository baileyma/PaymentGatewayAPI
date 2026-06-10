using System.Net;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Tests.Clients;

public class BankClientTests
{
    [Fact]
    public async Task SendPayment_ReturnsAuthorizedResponse_WhenBankReturns200()
    {
        var body = """{ "authorized": true, "authorization_code": "abc123" }""";
        var client = CreateClient(HttpStatusCode.OK, body);

        var result = await client.SendPayment(Request);

        Assert.True(result.Authorized);
        Assert.Equal("abc123", result.AuthorizationCode);
    }

    [Fact]
    public async Task SendPayment_ReturnsUnauthorizedResponse_WhenBankReturns200()
    {
        var body = """{ "authorized": false, "authorization_code": "" }""";
        var client = CreateClient(HttpStatusCode.OK, body);

        var result = await client.SendPayment(Request);

        Assert.False(result.Authorized);
        Assert.Equal("", result.AuthorizationCode);
    }

    [Fact]
    public async Task SendPayment_Throws_WhenBankReturns200WithNullBody()
    {
        var client = CreateClient(HttpStatusCode.OK, "null");

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => client.SendPayment(Request));

        Assert.Equal(HttpStatusCode.OK, ex.StatusCode);
        Assert.Equal("Bank returned empty response", ex.Message);
    }

    [Fact]
    public async Task SendPayment_Throws_WhenBankReturns400()
    {
        var client = CreateClient(HttpStatusCode.BadRequest, """{ "error": "bad request" }""");

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => client.SendPayment(Request));

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
    }

    [Fact]
    public async Task SendPayment_Throws_WhenBankReturns503()
    {
        var client = CreateClient(HttpStatusCode.ServiceUnavailable, "");

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => client.SendPayment(Request));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    [InlineData((HttpStatusCode)418)]
    public async Task SendPayment_ThrowsPreservingStatus_WhenBankReturnsUnexpectedStatus(HttpStatusCode status)
    {
        var client = CreateClient(status, "");

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => client.SendPayment(Request));

        Assert.Equal(status, ex.StatusCode);
    }

    private static BankClient CreateClient(HttpStatusCode status, string? body)
    {
        var handler = new StubHttpMessageHandler(status, body);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        return new BankClient(httpClient);
    }

    private static readonly BankRequest Request = new()
    {
        CardNumber = "12345678901234",
        ExpiryDate = "03/2027",
        Currency = "GBP",
        Amount = 1050,
        CVV = "123"
    };

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string? _body;

        public StubHttpMessageHandler(HttpStatusCode status, string? body)
        {
            _status = status;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_status)
            {
                Content = new StringContent(_body ?? "", System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
