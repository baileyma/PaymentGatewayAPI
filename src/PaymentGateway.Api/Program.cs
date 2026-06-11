using System.Text.Json.Serialization;

using FluentValidation;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Options;
using PaymentGateway.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();
builder.Services.AddSingleton<PaymentsRepository>();

var baseAddress = builder.Configuration["AcquiringBank:BaseAddress"];

if (String.IsNullOrWhiteSpace(baseAddress))
    throw new InvalidOperationException("Configuration value 'AcquiringBank:BaseAddress' is not set");

builder.Services.AddHttpClient<IAcquiringBankClient, BankClient>(client => 
{
    client.BaseAddress = new Uri(baseAddress);
});

builder.Services.AddScoped<IValidator<PaymentRequest>, PaymentRequestValidator>();
builder.Services.Configure<PaymentOptions>(
    builder.Configuration.GetRequiredSection(PaymentOptions.SectionName));

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
