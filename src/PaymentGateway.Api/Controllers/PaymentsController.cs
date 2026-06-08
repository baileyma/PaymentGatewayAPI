using System.Net;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Mappers;
using PaymentGateway.Api.Models.Common;
using PaymentGateway.Api.Models.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly PaymentsRepository _paymentsRepository;
    private readonly IAcquiringBankClient _client;
    private readonly IValidator<PaymentRequest> _validator;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(PaymentsRepository paymentsRepository, IAcquiringBankClient client, IValidator<PaymentRequest> validator, ILogger<PaymentsController> logger)
    {
        _paymentsRepository = paymentsRepository;
        _client = client;
        _validator = validator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Result<PaymentResponse>>> PostPaymentAsync([FromBody] PaymentRequest paymentRequest)
    {
        var validation = _validator.Validate(paymentRequest);

        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(valError => new Error($"{valError.ErrorMessage}")).ToArray();
            // CHECK
            _logger.LogInformation("Request failed validation", paymentRequest.Id);
            // should it be rejected? doesn't even get to the bank?
            return BadRequest(Result<PaymentResponse>.Rejected(errors));
        }
        
        var mappedPayment = PaymentMapper.Map(paymentRequest);
        
        try
        {
            var response = await _client.SendPayment(mappedPayment);

            var paymentResponse = new PaymentResponse()
            {
                Id = Guid.NewGuid(),
                CardNumberLastFour = paymentRequest.CardNumber[^4..],
                Expiry = paymentRequest.Expiry,
                Money = paymentRequest.Money,
                Status = response.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined
            };

            _paymentsRepository.Add(paymentResponse);

            if (!response.Authorized)
                return Ok(Result<PaymentResponse>.Declined(paymentResponse));

            return Ok(Result<PaymentResponse>.Authorized(paymentResponse));
        }

        catch(HttpRequestException ex)
        {
            if (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                // ASP.NET Core's built-in logging via ILogger<PaymentsController> injected into the controller would handle this. You'd call _logger.LogError(ex, "Acquiring bank returned 400 - possible contract change") in the catch 
                // this could mean why have missed a validation error...the bank picked it up but we didn't
                _logger.LogError(ex, "Unexpected response from acquiring bank: {StatusCode}", ex.StatusCode);
            }

            if (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                // CHECK
                _logger.LogInformation("(503) ServiceUnavailable received from the bank");
                return StatusCode(502);
            }

            _logger.LogWarning($"{ex.StatusCode.ToString()}received from the bank");
            return StatusCode(500);

                    }       
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentResponse?>> GetPaymentAsync(Guid id)
    {
        // experiment: what if Id is not a guid? does it need validating?
        var payment = _paymentsRepository.Get(id);

        if (payment is null)
        {
            // CHECK
            _logger.LogInformation("Payment not found");
            return NotFound();
        }

        return new OkObjectResult(payment);
    }
}
