using System.Net;
using System.Text.Json;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Clients;

public interface IAcquiringBankClient
{
    Task<BankResponse> SendPayment(BankRequest request);
}

public class AcquiringBankClient : IAcquiringBankClient
{
    private readonly HttpClient _client;

    public AcquiringBankClient(HttpClient client)
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
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<BankResponse>(jsonOptions),
            HttpStatusCode.BadRequest => throw new HttpRequestException("Acquiring bank rejected the request as invalid", null, HttpStatusCode.BadRequest),
            HttpStatusCode.ServiceUnavailable => throw new HttpRequestException("Bank service unavailable", null, HttpStatusCode.ServiceUnavailable),// pass an error saying that card ends in 0?...or are we just passing bankresponse with null values
             // if we validate properly, no need to catch this? shouldn't happen, if does should crash whole program
            _ => throw new HttpRequestException("Unexpected response from acquiring bank", null, response.StatusCode)
        };
    }
}
