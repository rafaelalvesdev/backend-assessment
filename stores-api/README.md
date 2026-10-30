# Stores API

REST API for managing companies and their stores. Built for a multi-company, multi-country setup: every store belongs to a company and carries its own address, currency (ISO 4217), time zone (IANA) and culture.

.NET 10 · ASP.NET Core · EF Core · Azure SQL · hosted on Azure App Service.

## Architecture

Clean Architecture, dependencies point inwards:

```
Stores.Api ──────────► Stores.Application ──► Stores.Domain
     │                        ▲
     └──► Stores.Infrastructure ┘
```

| Project | Responsibility |
|---|---|
| `Stores.Domain` | Entities (`Company`, `Store`) and value objects (`CountryCode`, `CurrencyCode`, `TimeZoneId`, `CultureCode`, `Address`). No dependencies. |
| `Stores.Application` | One handler per use case, request validation (FluentValidation), ports (`IStoreRepository`, `ICompanyRepository`, `IUnitOfWork`). Returns `Result<T>` instead of throwing for expected failures. |
| `Stores.Infrastructure` | EF Core `DbContext`, mappings and repositories. |
| `Stores.Api` | Controllers, problem details, OpenAPI. Composition root. |

Validation runs in a `ValidationHandler<,>` decorator wrapped around every handler, so handlers only deal with business rules.

## API

Base path: `/api/v1`. Interactive docs at `/scalar`, OpenAPI document at `/openapi/v1.json`.

| Method | Route | Success | Errors |
|---|---|---|---|
| GET | `/companies?page=&pageSize=` | 200 | 400 |
| GET | `/companies/{companyId}` | 200 | 404 |
| POST | `/companies` | 201 + `Location` | 400 |
| DELETE | `/companies/{companyId}` | 204 | 404, 409 (company still has stores) |
| GET | `/companies/{companyId}/stores?country=&page=&pageSize=` | 200 | 400, 404 |
| GET | `/companies/{companyId}/stores/{storeId}` | 200 | 404 |
| POST | `/companies/{companyId}/stores` | 201 + `Location` | 400, 404, 409 |
| PUT | `/companies/{companyId}/stores/{storeId}` | 200 | 400, 404, 409 |
| DELETE | `/companies/{companyId}/stores/{storeId}` | 204 | 404 |

Errors use RFC 9457 problem details. Validation errors list every invalid field (`address.countryCode`, `currency`, ...).

A company can only be deleted once it has no stores left.

Store codes are case-insensitive and unique per company; two companies can both have a store `HQ`. A store is only reachable through its own company.

Example store:

```json
{
  "code": "SP-001",
  "name": "Paulista",
  "address": {
    "street": "Av. Paulista, 1000",
    "city": "São Paulo",
    "state": "SP",
    "postalCode": "01310-100",
    "countryCode": "BR"
  },
  "currency": "BRL",
  "timeZone": "America/Sao_Paulo",
  "culture": "pt-BR"
}
```

## Running locally

```bash
docker compose up -d                       # SQL Server on localhost:14330
dotnet run --project src/Stores.Api        # migrations are applied on startup in Development
```

Then open `/scalar` on the URL printed by `dotnet run`.

## Tests

```bash
dotnet test
```

- `Stores.Domain.Tests` – entities and value objects
- `Stores.Application.Tests` – use cases with NSubstitute fakes
- `Stores.Api.IntegrationTests` – full HTTP pipeline via `WebApplicationFactory` on in-memory SQLite

The project was built test-first; the git history has the failing tests committed before each layer's implementation.
