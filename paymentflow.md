## Payment Flow Diagrams

### 1. Authorized (card ends in odd digit)

```
Merchant                    Payment Gateway API              Acquiring Bank API
   |                                |                                |
   |--- POST /api/payments -------->|                                |
   |                                |--- POST /payments ------------>|
   |                                |                         card ends odd
   |                                |<-- 200 { authorized: true } --|
   |<-- 200 Authorized -------------|                                |
```

### 2. Declined (card ends in even digit)

```
Merchant                    Payment Gateway API              Acquiring Bank API
   |                                |                                |
   |--- POST /api/payments -------->|                                |
   |                                |--- POST /payments ------------>|
   |                                |                        card ends even
   |                                |<-- 200 { authorized: false } -|
   |<-- 200 Declined ----------------|                               |
```

### 3. Rejected (invalid request — bank never called)

```
Merchant                    Payment Gateway API              Acquiring Bank API
   |                                |                                |
   |--- POST /api/payments -------->|                                |
   |                         validation fails                        |
   |                         bank never called                       |
   |<-- 400 Rejected ----------------|                               |
```

### 4. Bank Unavailable (card ends in 0)

```
Merchant                    Payment Gateway API              Acquiring Bank API
   |                                |                                |
   |--- POST /api/payments -------->|                                |
   |                                |--- POST /payments ------------>|
   |                                |                         card ends in 0
   |                                |<-- 503 -----------------------|
   |                         catch HttpRequestException              |
   |<-- 502 Bad Gateway ------------|                                |
```
