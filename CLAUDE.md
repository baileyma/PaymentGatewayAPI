# Payment Gateway — CLAUDE.md

## What this is
ASP.NET Core Web API (C#) implementing a payment gateway for the Checkout.com engineering assessment. Merchants can process and retrieve card payments. A simulated acquiring bank runs locally via Docker.

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
  Clients/           AcquiringBankClient   — HTTP client to bank simulator
  Services/          PaymentsRepository    — in-memory store (List<PaymentResponse>)
  Mappers/           PaymentMapper         — PaymentRequest → BankRequest
  Models/
    Requests/        PaymentRequest + FluentValidation validator
    Responses/       PaymentResponse       — what we store and return to merchants
    BankRequest/Response                   — bank simulator contract
    Common/          Expiry, Money
    Enums/           PaymentStatus         — Authorized | Declined | Rejected
    Result<T>                              — discriminated union wrapping bank response + status

test/PaymentGateway.Api.Tests/
  Controllers/       PaymentsControllerTests
  Mappers/           PaymentMapperTests
  Services/          PaymentRepositoryTests
```

## Key design decisions
- **FluentValidation** for request validation; invalid requests return `Rejected` without calling the bank.
- **`Result<T>`** wraps bank responses and carries `PaymentStatus` + any errors — avoids exception-as-flow-control.
- **`PaymentsRepository`** is a singleton in-memory store — no real DB, per spec.
- **`IAcquiringBankClient`** is an interface to allow mocking in tests.

## Bank simulator behaviour
- POST `http://localhost:8080/payments` with `{ card_number, expiry_date, currency, amount, cvv }`
- Any required field missing → `400 Bad Request` with error message
- Card ending odd → `200 { authorized: true, authorization_code: "..." }`
- Card ending even → `200 { authorized: false }`
- Card ending 0 → `503 Service Unavailable`

## Known incomplete areas (as of June 2026)
- `AcquiringBankClient` has a broken JSON serializer options block — does not compile.
- `PostPaymentAsync` never stores the payment in the repository and has a missing `return` for the fall-through path.
- `Expiry.ToString()` always prepends "0" to month — broken for month >= 10.
- Expiry validation only checks year, not the month+year combination.
- `GET /api/payments/{id}` returns 200 with `null` body if ID not found — should be 404.
- CVV numeric-only validation is missing from the validator.
- Accepted currencies are not constrained to a specific list (spec says ≤ 3 ISO codes).
