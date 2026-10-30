# Products Service

Serverless CRUD API for products, built as a C# Azure Function (isolated worker, .NET 10) on top of SQL Server.

Products belong to a store of a company. Names and descriptions are Unicode, prices are `DECIMAL(19,4)` with an ISO 4217 currency per product, and timestamps are stored in UTC.

## Database

Scripts in [`sql/`](sql), run in order:

| Script | What it does |
|---|---|
| `001_create_products_table.sql` | `dbo.Products` table, constraints and index |
| `002_fn_get_products_json.sql` | `dbo.fn_GetProductsJson(@CompanyId, @StoreId)` – scalar function returning one JSON object with the products |
| `003_usp_insert_product.sql` | `dbo.usp_InsertProduct` – validates and inserts a product, returns the new row |
| `900_sample_data.sql` | optional sample rows |

`dbo.Products`

| Column | Type | Notes |
|---|---|---|
| ProductId | `UNIQUEIDENTIFIER` | PK, `NEWSEQUENTIALID()` |
| CompanyId | `UNIQUEIDENTIFIER` | tenant |
| StoreId | `UNIQUEIDENTIFIER` | store of the company |
| Sku | `VARCHAR(50)` | unique per (company, store) |
| Name | `NVARCHAR(200)` | |
| Description | `NVARCHAR(2000)` | nullable |
| Price | `DECIMAL(19,4)` | `>= 0` |
| CurrencyCode | `CHAR(3)` | ISO 4217 |
| StockQuantity | `INT` | `>= 0` |
| IsActive | `BIT` | default 1 |
| CreatedAt / UpdatedAt | `DATETIMEOFFSET(3)` | UTC |

```sql
SELECT dbo.fn_GetProductsJson('8E0A5C1E-2B1F-4C8E-9E57-0D6A0E2C1A01', DEFAULT);
```

```json
{
  "companyId": "8E0A5C1E-2B1F-4C8E-9E57-0D6A0E2C1A01",
  "storeId": null,
  "totalCount": 1,
  "products": [
    { "id": "…", "storeId": "…", "sku": "PASTEL-6", "name": "Pastéis de nata (6)", "price": 7.2000, "currency": "EUR", "…": "…" }
  ]
}
```

`usp_InsertProduct` raises error `50400` for invalid input and `50409` when the SKU already exists in the store.

## API

Base path `/api`. Full spec in [`docs/openapi.yaml`](docs/openapi.yaml).

| Method | Route | Success | Errors |
|---|---|---|---|
| GET | `/companies/{companyId}/products?storeId=&page=&pageSize=` | 200 | 400 |
| GET | `/companies/{companyId}/products/catalog?storeId=` | 200 (output of `fn_GetProductsJson`) | 400 |
| GET | `/companies/{companyId}/products/{productId}` | 200 | 404 |
| POST | `/companies/{companyId}/products` | 201 + `Location` | 400, 409, 415 |
| PUT | `/companies/{companyId}/products/{productId}` | 200 | 400, 404, 409, 415 |
| DELETE | `/companies/{companyId}/products/{productId}` | 204 | 404 |

Functions use `AuthorizationLevel.Function`; send the key in `x-functions-key` when calling Azure.

## Code layout

```
src/Products.Functions
├─ Functions/   HTTP triggers – parse the request, map results to status codes
├─ Products/    model, request validation, ProductService
└─ Data/        SqlProductRepository (Dapper)
```

`ProductFunctions` → `IProductService` → `IProductRepository`. Inserts go through the stored procedure; the catalog endpoint calls the scalar function.

## Running locally

Requires Docker and Azure Functions Core Tools v4.

```bash
docker compose up -d
# run sql/*.sql against localhost,14331 (database ProductsDb)
cp src/Products.Functions/local.settings.sample.json src/Products.Functions/local.settings.json
cd src/Products.Functions && func start
```

## Tests

```bash
dotnet test
```

- `Products.UnitTests` – validation, service and HTTP mapping
- `Products.IntegrationTests` – runs the real SQL scripts in a throwaway SQL Server container (Testcontainers) and tests the repository, the procedure and the scalar function
