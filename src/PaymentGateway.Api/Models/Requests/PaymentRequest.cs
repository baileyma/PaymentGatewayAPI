using System.Data;

using FluentValidation;

using PaymentGateway.Api.Models.Common;

namespace PaymentGateway.Api.Models.Requests;

public class PaymentRequest
{
    public Guid Id { get; set; }
    public string CardNumber { get; set; }
    public Expiry Expiry { get; set; }
    public Money Money { get; set; }
    public string Cvv { get; set; }
}

public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
{
    public PaymentRequestValidator()
    {
        RuleFor(x => x.CardNumber).NotEmpty().Length(14, 19).Matches(@"^\d+$");
        RuleFor(x => x.Expiry.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Expiry.Year).GreaterThanOrEqualTo(DateTime.Today.Year);
        RuleFor(x => x.Expiry.Date).GreaterThanOrEqualTo(DateTime.Now);
        RuleFor(x => x.Money.Currency).Length(3);
        // check iso thing
        RuleFor(x => x.Cvv).Length(3, 4);
        RuleFor(x => x.Cvv).Matches(@"^\d+$");
    }
}