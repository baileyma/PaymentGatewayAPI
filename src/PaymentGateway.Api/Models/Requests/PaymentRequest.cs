using FluentValidation;

using Microsoft.Extensions.Options;

using PaymentGateway.Api.Models.Common;
using PaymentGateway.Api.Models.Options;

namespace PaymentGateway.Api.Models.Requests;

public record PaymentRequest
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string CardNumber { get; init; } = null!;
    public Expiry Expiry { get; init; } = null!;
    public Money Money { get; init; } = null!;
    public string Cvv { get; init; } = null!;
}

public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
{
    public PaymentRequestValidator(IOptions<PaymentOptions> options)
    {
        RuleFor(x => x.CardNumber).
            NotEmpty().
            Length(14, 19).
            Matches(@"^\d+$").WithMessage("CardNumber must only contain numeric characters");

        RuleFor(x => x.Expiry).NotNull();
        RuleFor(x => x.Expiry.Month).InclusiveBetween(1, 12).When(x => x.Expiry != null);
        RuleFor(x => x.Expiry).
            Must(e => new DateTime(e.Year, e.Month, 1) >= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).WithMessage("Expiry date cannot be in the past").
            When(x => x.Expiry != null && x.Expiry.Month >= 1 && x.Expiry.Month <= 12);

        RuleFor(x => x.Money).NotNull();
        When(x => x.Money != null, () =>
        {
            RuleFor(x => x.Money.Amount).NotEmpty().GreaterThan(0);
            RuleFor(x => x.Money.Currency).
                NotEmpty().
                Length(3).
                Must(c => options.Value.SupportedISOCurrencyCodes.Contains(c)).WithMessage("Currency must be one of: " + string.Join(", ", options.Value.SupportedISOCurrencyCodes));
        });
        
        RuleFor(x => x.Cvv).
            NotEmpty().
            Length(3, 4).
            Matches(@"^\d+$").WithMessage("CardNumber must only contain numeric characters");
    }
}