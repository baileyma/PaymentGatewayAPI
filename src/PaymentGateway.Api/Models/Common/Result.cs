using PaymentGateway.Api.Models.Enums;

namespace PaymentGateway.Api.Models.Common;

public class Result<T>
{
    public Error[] Error { get; init; }

    public PaymentStatus Status { get; init; }

    public bool IsSuccess { get; init; }

    public T? Value { get; init; }

    public Result() { }
    private Result(Error[] errors, bool isSuccess, T? value, PaymentStatus status)
    {
        Error = errors;
        IsSuccess = isSuccess;
        Value = value;
        Status = status;
    }

    public static Result<T> Authorized(T value) => new([], true, value, PaymentStatus.Authorized);

    public static Result<T> Declined(T value) => new([], true, value, PaymentStatus.Declined);

    public static Result<T> Rejected(Error[] errors) => new(errors, false, default, PaymentStatus.Rejected);
}

public class Error
{
    public Error(string message)
    {
        Message = message;
    }
    public string Message { get; init; }
}
