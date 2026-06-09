using System.Text.Json.Serialization;
using FluentValidation;
using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();
builder.Services.AddSingleton<PaymentsRepository>();
builder.Services.AddHttpClient<IAcquiringBankClient, BankClient>(client => 
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
