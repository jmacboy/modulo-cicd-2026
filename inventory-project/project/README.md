# Store Inventory — Project Guide

## Project Structure

```
ms2025-store-inventory-main/
├── src/
│   ├── Inventory.Domain/          # Aggregates, Value Objects, Domain Events
│   ├── Inventory.Application/     # Handlers (CQRS), Commands, Queries
│   ├── Inventory.Infrastructure/  # EF Core, Repositories, DI
│   └── Inventory.WebApi/          # Controllers, Startup
├── Inventory.Tests/               # Unit tests (xUnit + Moq)
├── project/
│   ├── unit-testing-rules.md      # Testing conventions and rules
│   └── README.md                  # This file
└── Inventory.sln
```

---

## CI Integration

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Run specific layer
dotnet test --filter "FullyQualifiedName~Inventory.Tests.Domain"
dotnet test --filter "FullyQualifiedName~Inventory.Tests.Application"
```

---

## Checklist Before PR

- [ ] New code has corresponding unit tests
- [ ] All tests pass locally (`dotnet test`)
- [ ] No `UnitTest1.cs` placeholder remaining
- [ ] Test names follow `{Method}_{Scenario}` convention
- [ ] AAA comments present in every test
- [ ] Mocks verify interactions, not just existence
- [ ] No shared mutable state between tests
