namespace PaymentGateway.Api.Models.Responses
{
    public class BankResponse
    {
        public required bool Authorized { get; init; }

        public string? AuthorizationCode { get; init; }
    }
}
