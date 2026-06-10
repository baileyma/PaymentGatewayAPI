# Payment Gateway — CLAUDE.md

## What this is
ASP.NET Core Web API (C#) implementing a payment gateway. Merchants can process and retrieve card payments. A simulated acquiring bank runs locally via Docker.

See `README.md` for the high-level summary and `paymentflow.md` for the merchant ↔ gateway ↔ bank interaction diagrams.

## Build & run
```bash
dotnet build PaymentGateway.sln
dotnet run --project src/PaymentGateway.Api
dotnet test

docker-compose up   # starts bank simulator on localhost:8080
```

## Project structure
```
src/PaymentGateway.Api/
  Controllers/       PaymentsController    — POST /api/payments, GET /api/payments/{id}
  Clients/           BankClient            — HTTP client to bank simulator (impl of IAcquiringBankClient)
  Services/          PaymentsRepository    — in-memory store (List<PaymentResponse>)
  Mappers/           PaymentMapper         — PaymentRequest → BankRequest, BankResponse → PaymentResponse
  Models/
    Requests/        PaymentRequest + PaymentRequestValidator (FluentValidation)
                     BankRequest                                — bank simulator contract (outbound)
    Responses/       PaymentResponse       — what we store and return to merchants
                     BankResponse                               — bank simulator contract (inbound)
    Common/          Expiry, Money, Result<T>, Error
    Enums/           PaymentStatus         — Authorized | Declined | Rejected

test/PaymentGateway.Api.Tests/             — unit tests
  Controllers/       PaymentsControllerTests
  Mappers/           PaymentMapperTests
  Services/          PaymentRepositoryTests
  Models/Requests/   PaymentRequestValidatorTests

PaymentGateway.Api.IntegrationTests/       — integration tests via CustomWebApplicationFactory
  PostPaymentTests, GetPaymentTests
```

## Key design decisions
- **FluentValidation** for request validation; invalid requests return `Rejected` (400) without calling the bank.
- **`Result<T>`** wraps the outcome and carries `PaymentStatus` + any errors — avoids exception-as-flow-control. Exceptions are reserved for transport failures.
- **Three terminal states** (`Authorized`, `Declined`, `Rejected`) distinguish "bank approved", "bank refused", and "bank never called".
- **`PaymentsRepository`** is a singleton in-memory store — no real DB, per spec.
- **`IAcquiringBankClient`** is an interface to allow mocking in tests.
- **Bank failure translation** in the controller: bank `503` → gateway `502 Bad Gateway`; bank `400` → gateway `500` (signals a validator gap); other unexpected → `500`.
- **Structured logging** keyed on `PaymentId`. `Warning` for expected-but-notable events (validation rejected, bank 503); `Error` for "shouldn't happen" cases (bank 400, unexpected statuses). No card data in logs.
- **PCI-conscious response shape**: `PaymentResponse` stores only `CardNumberLastFour`; full PAN and CVV are never persisted.

## Bank simulator behaviour
- POST `http://localhost:8080/payments` with `{ card_number, expiry_date, currency, amount, cvv }`
- Any required field missing → `400 Bad Request` with error message
- Card ending odd → `200 { authorized: true, authorization_code: "..." }`
- Card ending even → `200 { authorized: false }`
- Card ending 0 → `503 Service Unavailable`

## Known assumptions / out-of-scope (as of June 2026)
- Single-instance deployment — repository is in-memory and not thread-safe; restarts lose data.
- No authentication or merchant identity (no API key, no `merchant_id` on the request/response).
- `Amount` is an `int` in minor units (assumed, not documented in the contract).
- Supported currencies hardcoded to GBP / EUR / USD.
- No retries, circuit breaker, or explicit HTTP timeout on the bank client.
