# Payment Gateway

An ASP.NET Core Web API implementing the payment gateway, with an in-memory store for retrieval and a Dockerised bank simulator as the acquirer.

## How it works

A merchant `POST`s to `/api/payments`, where the request is run through a FluentValidation validator first.

- If validation fails, the bank is **never called** and the response is **Rejected** (400).
- Otherwise the request is mapped to the bank's contract via `PaymentMapper`, sent through `IAcquiringBankClient`, and the bank's response determines whether the outcome is **Authorized** or **Declined** — both stored in the singleton `PaymentsRepository` and returned `200`.
- `GET /api/payments/{id}` looks up the stored response or returns `404`.

The full set of merchant ↔ gateway ↔ bank interactions, including the unavailable-bank path, is documented in [`paymentflow.md`](./paymentflow.md).

## Result pattern

The three terminal states (`Authorized`, `Declined`, `Rejected`) are modelled with a `Result<T>` wrapper carrying a `PaymentStatus` enum and any errors.

This lets business outcomes flow through the return type, while exceptions are reserved for genuine transport failures — keeping exception-as-flow-control out of the controller and giving the merchant a consistent response shape across all outcomes.

## Logging

Observability is handled with structured logging keyed on `PaymentId`, with deliberate level choices:

- **`Warning`** — expected-but-notable events (validation rejected, bank returned `503`)
- **`Error`** — "shouldn't happen" cases (bank returned `400`, signalling a gap in our validator, or any unexpected status)

No card data ever reaches the logs.

## Bank failure translation

Bank HTTP failures are translated rather than passed through:

| Bank response | Gateway response | Reason |
|---|---|---|
| `503 Service Unavailable` | `502 Bad Gateway` | Semantically correct — upstream unavailable |
| `400 Bad Request` | `500 Internal Server Error` | Indicates a gap in our validator |
| Any other unexpected status | `500 Internal Server Error` | Defensive fallback |
