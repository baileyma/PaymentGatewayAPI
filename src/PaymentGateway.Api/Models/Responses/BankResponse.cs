using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Responses
{
    public class BankResponse
    {
        public bool Authorized { get; init; }

        public string AuthorizationCode { get; init; }
    }
}
