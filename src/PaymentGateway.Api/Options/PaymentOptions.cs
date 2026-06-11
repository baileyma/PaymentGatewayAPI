namespace PaymentGateway.Api.Options
{
    public class PaymentOptions
    {
        public const string SectionName = "PaymentOptions";
        public required string[] SupportedISOCurrencyCodes { get; set; } = [];
    }
}
