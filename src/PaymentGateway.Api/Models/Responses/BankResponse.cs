namespace PaymentGateway.Api.Models.Responses
{
    public class BankResponse
    {
        public required bool Authorized { get; init; }

        public required string AuthorizationCode { get; init; }
    }
}
