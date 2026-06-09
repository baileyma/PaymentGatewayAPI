using PaymentGateway.Api.Models.Common;
using PaymentGateway.Api.Models.Enums;

namespace PaymentGateway.Api.Models.Responses;

public class PaymentResponse 
{
    public required Guid Id { get; init; }
    
    public required PaymentStatus Status { get; init; }
    public required string CardNumberLastFour { get; init; }
    public required Expiry Expiry { get; init; }
    public required Money Money { get; init; }
}