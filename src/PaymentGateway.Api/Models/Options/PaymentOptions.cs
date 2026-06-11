namespace PaymentGateway.Api.Models.Options
{
    public class PaymentOptions
    {
        public const string SectionName = "PaymentOptions";
        public required string[] SupportedISOCurrencyCodes { get; set; } = [];
    }
}
