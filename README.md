# Tax Calculator API

A SOLID, clean-layered .NET 8 Web API for configuring and calculating country taxes.

## Solution Structure

```
TaxCalculator.sln
├── src/
│   ├── TaxCalculator.Models        ← Class library: pure DTOs & enums (zero deps)
│   │   ├── Enums/TaxItemType.cs
│   │   ├── Requests/TaxRequests.cs
│   │   └── Responses/TaxResponses.cs
│   │
│   ├── TaxCalculator.Database      ← Class library: EF Core entities, config, DbContext
│   │   ├── Entities/TaxEntities.cs
│   │   ├── Configurations/EntityConfigurations.cs
│   │   └── Context/TaxDbContext.cs
│   │
│   ├── TaxCalculator.Domain        ← Class library: business logic
│   │   ├── Interfaces/             ← ITaxConfigurationService, ITaxCalculationService,
│   │   │                              ITaxCalculator, future stubs
│   │   ├── TaxCalculators/         ← FixedTaxCalculator, FlatRateTaxCalculator,
│   │   │                              ProgressiveTaxCalculator, TaxCalculatorResolver
│   │   ├── Services/               ← TaxConfigurationService, TaxCalculationService
│   │   ├
│   │   ├── Exceptions/TaxExceptions.cs
│   │   └── ServiceCollectionExtensions.cs
│   │
│   └── TaxCalculator.API           ← Web API: controllers, validators, middleware, Mapping
│       ├── Controllers/TaxController.cs
│       ├── Validators/TaxValidators.cs    ← FluentValidation
│       ├── Middleware/ExceptionHandlingMiddleware.cs
│       └── Program.cs
│
└── tests/
    └── TaxCalculator.Tests
        ├── TaxCalculators/         ← Unit tests per calculator type
        ├── Services/               ← Integration tests with in-memory DB
        └── Validators/             ← FluentValidation test helpers
```

## SOLID Applied

| Principle | Where |
|-----------|-------|
| **S** – SRP | `TaxConfigurationService` only persists. `TaxCalculationService` only calculates. Each `ITaxCalculator` handles exactly one tax type. |
| **O** – OCP | Adding a new tax type = new class implementing `ITaxCalculator` + one DI registration. Zero changes to existing code. |
| **L** – LSP | Any `ITaxCalculator` implementation is a valid substitute — the resolver and calculation service treat them identically. |
| **I** – ISP | `ITaxConfigurationService` and `ITaxCalculationService` are separate narrow interfaces. Controller depends on only what it uses. |
| **D** – DIP | `TaxCalculationService` depends on `ITaxCalculator` (abstraction), not concrete calculators. |

## Running

### Local
```bash
cd src/TaxCalculator.API
dotnet run
# Swagger UI: http://localhost:5000
```

### Docker
```bash
docker-compose up --build
# Swagger UI: http://localhost:8080
```

### Tests
```bash
dotnet test
```

## API

### PUT `/api/tax/configuration`
```json
{
  "countryCode": "DE",
  "taxItems": [
    { "name": "CommunityTax", "type": "Fixed",    "amount": 1500 },
    { "name": "RadioTax",     "type": "Fixed",    "amount": 500  },
    { "name": "PensionTax",   "type": "FlatRate", "rate": 20     },
    {
      "name": "IncomeTax",
      "type": "Progressive",
      "brackets": [
        { "threshold": 10000, "rate": 0  },
        { "threshold": 30000, "rate": 20 },
        { "threshold": null,  "rate": 40 }
      ]
    }
  ]
}
```

### POST `/api/tax/calculate`
```json
{ "countryCode": "DE", "grossSalary": 62000 }
```

```json
{
  "grossSalary": 62000,
  "taxableBase": 60000,
  "totalTaxes": 30000,
  "netSalary": 32000,
  "breakdown": [
    { "name": "CommunityTax", "type": "Fixed",       "amount": 1500  },
    { "name": "RadioTax",     "type": "Fixed",       "amount": 500   },
    { "name": "PensionTax",   "type": "FlatRate",    "amount": 12000 },
    { "name": "IncomeTax",    "type": "Progressive", "amount": 16000 }
  ]
}
```

## Future Extension Points

### New Tax Type
1. Create `MyNewTaxCalculator : ITaxCalculator` in `TaxCalculator.Service/TaxCalculators/`
2. Register: `services.AddSingleton<ITaxCalculator, MyNewTaxCalculator>()`
3. Done. Zero other changes.

### Tax Credits (future)
In `TaxCalculationService`, after totalling taxes:
```csharp
// var credits = await _taxCreditService.GetTotalCreditsAsync(employeeId, ct);
// totalTaxes -= credits;
```

### External Config Providers (future)
In `TaxCalculationService`, after the DB lookup:
```csharp
// config ??= await _externalProvider.GetConfigurationAsync(countryCode, ct);
```
