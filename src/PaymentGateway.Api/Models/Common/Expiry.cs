namespace PaymentGateway.Api.Models.Common;

public class Expiry
{
    public Expiry(int year, int month)
    {
        Year = year;
        Month = month;
        Date = new DateOnly(Year, Month, 1);
    }
    public int Year { get; init; }

    public int Month { get; init; }

    public DateOnly Date { get; } 

    public override string ToString() => Date.ToString("MM/yyyy");
}
