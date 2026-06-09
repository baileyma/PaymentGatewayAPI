using System.Net;
using AutoFixture;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models.Common;
using PaymentGateway.Api.Models.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests.Controllers;

public class PaymentsControllerTests
{
    private readonly Fixture _fixture;
    private readonly IAcquiringBankClient _bankClient;
    private readonly IValidator<PaymentRequest> _validator;
    private readonly PaymentsRepository _repo;
    private readonly PaymentsController _controller;

    public PaymentsControllerTests()
    {
        _fixture = new Fixture();
        _fixture.Customize<Expiry>(c => c.FromFactory(() => new Expiry(2027, 6)).OmitAutoProperties());
        _fixture.Customize<Money>(c => c.FromFactory(() => new Money("GBP", 1000)));
        _fixture.Customize<PaymentRequest>(c => c
            .With(x => x.CardNumber, "12345678901235")
            .With(x => x.Cvv, "123"));

        _bankClient = Substitute.For<IAcquiringBankClient>();
        _validator = Substitute.For<IValidator<PaymentRequest>>();
        _repo = new PaymentsRepository();

        // Default: validation passes
        _validator.Validate(Arg.Any<PaymentRequest>())
            .Returns(new ValidationResult());

        // Default: bank authorizes
        _bankClient.SendPayment(Arg.Any<BankRequest>())
            .Returns(new BankResponse { Authorized = true, AuthorizationCode = "OK" });

        _controller = new PaymentsController(
            _repo,
            _bankClient,
            _validator,
            new LoggerFactory().CreateLogger<PaymentsController>());
    }

    [Fact]
    public async Task PostPayment_ValidationFails_ReturnsBadRequest()
    {
        _validator.Validate(Arg.Any<PaymentRequest>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Field", "Invalid") }));

        var result = await _controller.PostPaymentAsync(_fixture.Create<PaymentRequest>());

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task PostPayment_BankAuthorizes_ReturnsOkWithAuthorizedStatus()
    {
        _bankClient.SendPayment(Arg.Any<BankRequest>())
            .Returns(new BankResponse { Authorized = true, AuthorizationCode = "ABC123" });

        var actionResult = await _controller.PostPaymentAsync(_fixture.Create<PaymentRequest>());

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var body = Assert.IsType<Result<PaymentResponse>>(okResult.Value);
        Assert.Equal(PaymentStatus.Authorized, body.Status);
    }

    [Fact]
    public async Task PostPayment_BankDeclines_ReturnsOkWithDeclinedStatus()
    {
        _bankClient.SendPayment(Arg.Any<BankRequest>())
            .Returns(new BankResponse { Authorized = false, AuthorizationCode = "" });

        var actionResult = await _controller.PostPaymentAsync(_fixture.Create<PaymentRequest>());

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var body = Assert.IsType<Result<PaymentResponse>>(okResult.Value);
        Assert.Equal(PaymentStatus.Declined, body.Status);
    }

    [Fact]
    public async Task PostPayment_BankReturns503_Returns502()
    {
        _bankClient.SendPayment(Arg.Any<BankRequest>())
            .Throws(new HttpRequestException("unavailable", null, HttpStatusCode.ServiceUnavailable));

        var actionResult = await _controller.PostPaymentAsync(_fixture.Create<PaymentRequest>());

        var statusResult = Assert.IsType<StatusCodeResult>(actionResult.Result);
        Assert.Equal(502, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetPayment_IdNotFound_Returns404()
    {
        var result = await _controller.GetPaymentAsync(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }
}
