using System.Text.Json.Serialization;
using FluentValidation;
using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;

// LOGGING...WHAT DO I NEED TO LOG? JUST ERROS MAYBE AS DON'T WANT TOO NOISY? MAYBE THAT CAN BE ONE DESIGN DECISION

// keep in mind that scenario where our fluentvalidation fails and bankapi gives 400 which we map back as 500, check that is good practice
// deg log this scenario

// 4 Validation to dos from claude...need to revisit all models and fluent valdiation
// 1 Expiry.ToString() — broken for month ≥ 10 (always prepends "0"). DONE
// 2 Expiry validation — checks year only, not month+year combination. 
// 3 CVV validation — missing numeric-only check.
// 4 Currency validation — not constrained to a specific list of ISO codes.

// what are the big design decisions I am making
// logging and clear exception/error messaging
// make a flow chart of the api to the other api and back with all scenarios

// 1 OBSERVABILITY - EXCEPTIONS (GLOBAL HANDLER?) AND LOGGING
// 2 VALIDATION - MAKE SURE FLUENT RULES ARE PERFECT
// 3 DESIGN DECISIONS - DOCUMENT THESE, PROBS JUST USE CLAUDE
// 4 INTEGRATION AND UNIT TESTS - E2E? - AGAIN, PROBS MAINLY USE CLAUDE
// 5 GIT COMMITS...
// 6 DO LOTS OF MANUAL TESTING FOR EDGE CASES...MAKE AGENT ORCHESTRATION TO REVIEW IT?




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();
builder.Services.AddSingleton<PaymentsRepository>();

//builder.Services.Configure<AcquiringBankOptions>(
//    builder.Configuration.GetSection("AcquiringBank"));

builder.Services.AddHttpClient<IAcquiringBankClient, AcquiringBankClient>(client => 
{
    client.BaseAddress = new Uri(builder.Configuration["AcquiringBank:BaseAddress"]);
});

builder.Services.AddScoped<IValidator<PaymentRequest>, PaymentRequestValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
