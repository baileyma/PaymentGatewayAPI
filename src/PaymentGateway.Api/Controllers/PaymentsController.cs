using System.Net;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Mappers;
using PaymentGateway.Api.Models.Common;
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
            var errors = validation.Errors.Select(error => new Error($"{error.ErrorMessage}")).ToArray();
            _logger.LogWarning("Payment {PaymentId} failed validation: {Errors}", paymentRequest.Id, string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(Result<PaymentResponse>.Rejected(errors));
        }

        var bankClientRequest = PaymentMapper.MapFromPaymentRequest(paymentRequest);

        try
        {
            var bankClientResponse = await _client.SendPayment(bankClientRequest);

            var paymentResponse = PaymentMapper.MapToPaymentReponse(bankClientResponse, paymentRequest);

            _paymentsRepository.Add(paymentResponse);

            if (!bankClientResponse.Authorized)
                return Ok(Result<PaymentResponse>.Declined(paymentResponse));

            return Ok(Result<PaymentResponse>.Authorized(paymentResponse));
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                _logger.LogError(ex, "Unexpected (400) BadRequest received from bank server - possible validator gap - for payment {PaymentId}", paymentRequest.Id);
                return StatusCode(500);
            }

            if (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                _logger.LogWarning("(503) ServiceUnavailable received from bank server for payment {PaymentId}", paymentRequest.Id);
                return StatusCode(502);
            }

            _logger.LogError(ex, "Unexpected ({StatusCode}) received from bank server for payment {PaymentId}", ex.StatusCode, paymentRequest.Id);
            return StatusCode(500);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsRepository.Get(id);

        if (payment is null)
        {
            _logger.LogWarning("Payment not found for Id: {Id}", id);
            return NotFound();
        }

        return new OkObjectResult(payment);
    }
}