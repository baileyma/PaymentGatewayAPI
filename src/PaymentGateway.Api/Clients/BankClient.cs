using System.Net;
using System.Text.Json;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Clients;

public interface IAcquiringBankClient
{
    Task<BankResponse> SendPayment(BankRequest request);
}

public class BankClient : IAcquiringBankClient
{
    private readonly HttpClient _client;

    public BankClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<BankResponse> SendPayment(BankRequest request)
    {
        var jsonOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        var response = await _client.PostAsJsonAsync("payments", request, jsonOptions);

        return response.StatusCode switch
        {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<BankResponse>(jsonOptions) ?? throw new HttpRequestException("Bank returned empty response", null, HttpStatusCode.OK),
            HttpStatusCode.BadRequest => throw new HttpRequestException("Acquiring bank rejected the request as invalid", null, HttpStatusCode.BadRequest),
            HttpStatusCode.ServiceUnavailable => throw new HttpRequestException("Bank service unavailable", null, HttpStatusCode.ServiceUnavailable),
            _ => throw new HttpRequestException("Unexpected response from acquiring bank", null, response.StatusCode)
        };
    }
}