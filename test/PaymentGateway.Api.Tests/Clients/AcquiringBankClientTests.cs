using System.Net;
using System.Text.Json;

using AutoFixture;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Tests.Clients;

public class AcquiringBankClientTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly Fixture _fixture;

    public AcquiringBankClientTests()
    {
        _fixture = new Fixture();
    }

    private static HttpClient CreateHttpClient(HttpStatusCode statusCode, object? content = null)
    {
        var json = content is not null
            ? JsonSerializer.Serialize(content, JsonOptions)
            : "";
        var handler = new FakeHttpHandler(statusCode, json);
        return new HttpClient(handler) { BaseAddress = new Uri("http://localhost:8080/") };
    }

    [Fact]
    public async Task SendPayment_BankReturns200Authorized_ReturnsAuthorizedResponse()
    {
        var httpClient = CreateHttpClient(HttpStatusCode.OK,
            new { authorized = true, authorization_code = "AUTH123" });
        var client = new AcquiringBankClient(httpClient);

        var result = await client.SendPayment(_fixture.Create<BankRequest>());

        Assert.True(result.Authorized);
        Assert.Equal("AUTH123", result.AuthorizationCode);
    }

    [Fact]
    public async Task SendPayment_BankReturns400_ThrowsHttpRequestException()
    {
        var httpClient = CreateHttpClient(HttpStatusCode.BadRequest);
        var client = new AcquiringBankClient(httpClient);

        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => client.SendPayment(_fixture.Create<BankRequest>()));

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
    }

    [Fact]
    public async Task SendPayment_BankReturns503_ThrowsHttpRequestException()
    {
        var httpClient = CreateHttpClient(HttpStatusCode.ServiceUnavailable);
        var client = new AcquiringBankClient(httpClient);

        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => client.SendPayment(_fixture.Create<BankRequest>()));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
    }

    private class FakeHttpHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseBody;

        public FakeHttpHandler(HttpStatusCode statusCode, string responseBody)
        {
            _statusCode = statusCode;
            _responseBody = responseBody;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseBody, System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
