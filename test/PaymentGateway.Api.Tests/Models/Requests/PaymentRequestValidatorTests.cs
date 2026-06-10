using FluentValidation.TestHelper;

using Microsoft.Extensions.Options;

using PaymentGateway.Api.Models.Common;
using PaymentGateway.Api.Models.Options;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.UnitTests.Models.Requests;

public class PaymentRequestValidatorTests
{
    private readonly PaymentRequestValidator _validator = new(
    Options.Create(new PaymentOptions { SupportedISOCurrencyCodes = ["GBP", "EUR", "USD"] }));

    // ── CardNumber ──────────────────────────────────────────

    [Theory]
    [InlineData("", false)]                      // empty
    [InlineData("1234567890123", false)]          // 13 chars – too short
    [InlineData("12345678901234567890", false)]   // 20 chars – too long
    [InlineData("1234abcd901234", false)]         // non-numeric
    [InlineData("12345678901234", true)]          // 14 chars – min
    [InlineData("1234567890123456789", true)]     // 19 chars – max
    public void CardNumber_Validation(string cardNumber, bool shouldBeValid)
    {
        var result = _validator.TestValidate(ValidRequest() with { CardNumber = cardNumber });
        if (shouldBeValid)
            result.ShouldNotHaveValidationErrorFor(x => x.CardNumber);
        else
            result.ShouldHaveValidationErrorFor(x => x.CardNumber);
    }

    // ── Expiry ──────────────────────────────────────────────

    [Fact]
    public void Expiry_Null_Fails()
    {
        var request = ValidRequest() with { Expiry = null! };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Expiry);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void Expiry_MonthOutOfRange_Fails(int month)
    {
        var request = ValidRequest() with { Expiry = new Expiry(DateTime.Today.Year + 1, month) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Expiry.Month);
    }

    [Fact]
    public void Expiry_CurrentMonth_IsValid()
    {
        var today = DateTime.Today;
        var request = ValidRequest() with { Expiry = new Expiry(today.Year, today.Month) };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Expiry);
    }

    [Fact]
    public void Expiry_LastMonth_Fails()
    {
        var lastMonth = DateTime.Today.AddMonths(-1);
        var request = ValidRequest() with { Expiry = new Expiry(lastMonth.Year, lastMonth.Month) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Expiry);
    }

    [Fact]
    public void Expiry_PastYear_Fails()
    {
        var request = ValidRequest() with { Expiry = new Expiry(DateTime.Today.Year - 1, 6) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Expiry);
    }

    [Fact]
    public void Expiry_FutureMonth_IsValid()
    {
        var nextMonth = DateTime.Today.AddMonths(1);
        var request = ValidRequest() with { Expiry = new Expiry(nextMonth.Year, nextMonth.Month) };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Expiry);
    }

    // ── Money ───────────────────────────────────────────────

    [Fact]
    public void Money_Null_Fails()
    {
        var request = ValidRequest() with { Money = null! };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Money);
    }

    [Theory]
    [InlineData(0, false)]      // zero
    [InlineData(-1, false)]     // negative
    [InlineData(1, true)]       // minimum valid
    [InlineData(1050, true)]    // typical amount
    public void Money_Amount_Validation(int amount, bool shouldBeValid)
    {
        var request = ValidRequest() with { Money = new Money("GBP", amount) };
        var result = _validator.TestValidate(request);
        if (shouldBeValid)
            result.ShouldNotHaveValidationErrorFor(x => x.Money.Amount);
        else
            result.ShouldHaveValidationErrorFor(x => x.Money.Amount);
    }

    [Theory]
    [InlineData("", false)]     // empty
    [InlineData("US", false)]   // too short
    [InlineData("USDX", false)] // too long
    [InlineData("JPY", false)]  // unsupported ISO code
    [InlineData("GBP", true)]
    [InlineData("EUR", true)]
    [InlineData("USD", true)]
    public void Money_Currency_Validation(string currency, bool shouldBeValid)
    {
        var request = ValidRequest() with { Money = new Money(currency, 100) };
        var result = _validator.TestValidate(request);
        if (shouldBeValid)
            result.ShouldNotHaveValidationErrorFor(x => x.Money.Currency);
        else
            result.ShouldHaveValidationErrorFor(x => x.Money.Currency);
    }

    // ── CVV ─────────────────────────────────────────────────

    [Theory]
    [InlineData("", false)]       // empty
    [InlineData("12", false)]     // too short
    [InlineData("12345", false)]  // too long
    [InlineData("12a", false)]    // non-numeric
    [InlineData("abc", false)]    // all alpha
    [InlineData("123", true)]     // 3 digits
    [InlineData("1234", true)]    // 4 digits
    public void Cvv_Validation(string cvv, bool shouldBeValid)
    {
        var result = _validator.TestValidate(ValidRequest() with { Cvv = cvv });
        if (shouldBeValid)
            result.ShouldNotHaveValidationErrorFor(x => x.Cvv);
        else
            result.ShouldHaveValidationErrorFor(x => x.Cvv);
    }

    // ── Full valid request ──────────────────────────────────

    [Fact]
    public void ValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── Helper methods ──────────────────────────────────

    private static PaymentRequest ValidRequest() => new()
    {
        CardNumber = "12345678901234",
        Expiry = new Expiry(DateTime.Today.Year + 1, 6),
        Money = new Money("GBP", 1050),
        Cvv = "123"
    };
}
