using PaymentGateway.Api.Models.Common;
using PaymentGateway.Api.Models.Enums;

namespace PaymentGateway.Api.Models.Responses;

public class PaymentResponse 
{
    public Guid Id { get; set; }
    public PaymentStatus Status { get; set; }
    public string CardNumberLastFour { get; set; }
    public Expiry Expiry { get; set; }
    public Money Money { get; set; }
}