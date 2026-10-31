# Unit Testing Rules — Store Inventory

## Stack

- **Framework**: .NET 8.0 / C#
- **Test runner**: xUnit 2.5.3
- **Mocking**: Moq 4.20.72
- **Coverage**: coverlet.collector 6.0.0
- **Test project**: `Inventory.Tests`

---

## Architecture & Test Pyramid

```
        ┌──────────────────┐
        │   Integration    │   ← Infrastructure + DB (future)
        ├──────────────────┤
        │   Application    │   ← Handlers + MediatR pipeline
        ├──────────────────┤
        │     Domain       │   ← Entities, Value Objects, Rules
        └──────────────────┘
```

**Unit tests cover Domain and Application layers ONLY.**
Integration and infrastructure tests are out of scope for this file.

---

## Layer 1 — Domain Tests

**Location**: `Inventory.Tests/Domain/{Aggregate}/`

### What to test

- Aggregate construction and initial state
- Every public method's happy path, and alternative paths (invalid input, edge cases)
- Every Exception thrown
- Value Object equality and arithmetic
- State transitions (e.g., `ReserveStock` → `Available` decreases)

### Rules

| # | Rule |
|---|------|
| D1 | One test class per aggregate or value object. File: `{Aggregate}Test.cs` |
| D2 | Test method naming: `{Method}_{Scenario}` — e.g., `AddStock_NegativeQuantity` |
| D3 | Use AAA (Arrange / Act / Assert) pattern with explicit `//Arrange`, `//Act`, `//Assert` comments |
| D4 | Do NOT mock the aggregate under test. Domain entities are pure logic — instantiate them directly |
| D5 | Do NOT mock value objects (`CostValue`, `QuantityValue`). They are structs — test them as-is |
| D6 | Mock only external dependencies injected via constructors (e.g., `ICostStrategy` in `AddStock`) |
| D7 | Every `DomainException` must be tested with both `Assert.Throws` AND the error message assertion |
| D8 | Test state invariants after mutations: if `AddStock(10)` → assert `Stock`, `Available`, and `UnitaryCost` |
| D9 | One assertion concern per test. If testing cost calculation AND stock update, use two tests |
| D10 | **Use `[Theory]` with `[InlineData]` when testing the same method with different inputs.** If 3+ `[Fact]` methods test the same logic with only data changes, collapse them into one `[Theory]` |

### Example — Single test with [Fact]

```csharp
[Fact]
public void AddStock_PositiveQuantity_UpdatesStockAndCost()
{
    //Arrange
    var item = new Item(Guid.NewGuid(), "Widget");
    var strategy = new AverageCostStrategy();

    //Act
    item.AddStock(20, 15.50m, strategy);

    //Assert
    Assert.Equal(20, item.Stock.Value);
    Assert.Equal(15.50m, item.UnitaryCost.Value);
    Assert.Equal(20, item.Available.Value);
}
```

### Example — Same method, different inputs with [Theory]

```csharp
[Theory]
[InlineData(10, 10.0, 10, 10.0)]   // qty, cost, expectedStock, expectedCost
[InlineData(20, 15.5, 20, 15.5)]
[InlineData(5, 0.0, 5, 0.0)]
public void AddStock_SingleBatch_SetsCorrectValues(int qty, decimal cost,
    int expectedStock, decimal expectedCost)
{
    //Arrange
    var item = new Item(Guid.NewGuid(), "Widget");
    var strategy = new AverageCostStrategy();

    //Act
    item.AddStock(qty, cost, strategy);

    //Assert
    Assert.Equal(expectedStock, item.Stock.Value);
    Assert.Equal(expectedCost, item.UnitaryCost.Value);
}
```

### Example — Guard clause validation with [Theory]

```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(-100)]
public void AddStock_ZeroOrNegativeQuantity_ThrowsDomainException(int qty)
{
    //Arrange
    var item = new Item(Guid.NewGuid(), "Widget");
    var strategy = new AverageCostStrategy();

    //Act + Assert
    var ex = Assert.Throws<DomainException>(() => item.AddStock(qty, 10.0m, strategy));
    Assert.Equal(ItemErrors.NonNegativeStock().Description, ex.Error.Description);
}
```

---

## Layer 2 — Application Tests (Handlers)

**Location**: `Inventory.Tests/Application/{Feature}/{UseCase}/`

### What to test

- Handler returns `Result.Success` with correct value on valid input
- Handler returns `Result.Failure` with correct error on invalid input
- Repository methods are called with correct arguments
- `UnitOfWork.CommitAsync` is called exactly once on success
- Handler does NOT call `CommitAsync` on failure
- All dependencies are mocked and verified (repositories, factories, UnitOfWork)
- Dependency interactions (verify mock calls)

### Rules

| # | Rule |
|---|------|
| A1 | One test class per handler. File: `{HandlerName}Test.cs` |
| A2 | Test method naming: `{Method}_{Scenario}` — e.g., `Handle_ItemNotFound_ReturnsFailure` |
| A3 | Use AAA pattern with explicit comments |
| A4 | Mock ALL dependencies from the handler constructor: repositories, UnitOfWork, factories |
| A5 | Use `Mock<T>` for interfaces (`IItemRepository`, `IUnitOfWork`, `ITransactionFactory`) |
| A6 | Verify repository calls with `It.Is<T>(predicate)` for argument validation |
| A7 | Use `Times.Once` for commit verification on success, `Times.Never` on failure |
| A8 | Test the **Result envelope**: `result.IsSuccess`, `result.Value`, `result.Error` |
| A9 | Do NOT test the domain layer inside handler tests. Domain logic is already covered by domain tests |
| A10 | One test per scenario: success path, each failure path, edge case |
| A11 | **Use `[Theory]` with `[InlineData]` when testing the same handler with different inputs or error codes.** If 3+ scenarios differ only in data, collapse into one `[Theory]` |

### Example — Handler with [Fact]

```csharp
[Fact]
public async Task Handle_ValidCommand_ReturnsSuccessWithId()
{
    //Arrange
    var repositoryMock = new Mock<IItemRepository>();
    var unitOfWorkMock = new Mock<IUnitOfWork>();
    var handler = new CreateItemHandler(repositoryMock.Object, unitOfWorkMock.Object);
    var command = new CreateItemCommand(Guid.NewGuid(), "Test Item");

    //Act
    var result = await handler.Handle(command, CancellationToken.None);

    //Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(command.Id, result.Value);
    repositoryMock.Verify(x =>
        x.AddAsync(It.Is<Item>(i => i.Id == command.Id && i.Name == command.ItemName)),
        Times.Once);
    unitOfWorkMock.Verify(x => x.CommitAsync(CancellationToken.None), Times.Once);
}
```

### Example — Handler validation with [Theory]

```csharp
[Theory]
[InlineData("")]
[InlineData(null)]
[InlineData("   ")]
public async Task Handle_InvalidName_ReturnsFailure(string? name)
{
    //Arrange
    var repositoryMock = new Mock<IItemRepository>();
    var unitOfWorkMock = new Mock<IUnitOfWork>();
    var handler = new CreateItemHandler(repositoryMock.Object, unitOfWorkMock.Object);
    var command = new CreateItemCommand(Guid.NewGuid(), name);

    //Act
    var result = await handler.Handle(command, CancellationToken.None);

    //Assert
    Assert.False(result.IsSuccess);
    unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
}
```

---

## Layer 3 — Factory Tests

**Location**: `Inventory.Tests/Domain/{Aggregate}/` or `Inventory.Tests/Application/{Feature}/`

### What to test

- Factory creates correct transaction type (Entry vs Exit)
- Factory sets correct initial state
- Factory applies items with correct quantities
- Factory throws on invalid input

### Rules

| # | Rule |
|---|------|
| F1 | Mock repositories when the factory depends on them for item lookup |
| F2 | Assert the created object's type, status, and item count |
| F3 | Test that domain events are queued correctly after factory creation |

---

## Test Data & Fixtures

### Rules

| # | Rule |
|---|------|
| T1 | Use `Guid.NewGuid()` for IDs unless the test requires a specific ID |
| T2 | Use meaningful test data names: `"Widget"`, `"Gadget"` — not `"test"`, `"abc"` |
| T3 | Use `CancellationToken.None` for all handler calls |
| T4 | Create helper methods in the test class for repeated setups (e.g., `CreateDefaultItem()`) |
| T5 | Do NOT use shared mutable state between tests. Each test is independent |

### Recommended Fixtures (when tests grow)

```csharp
public static class TestData
{
    public static Item CreateDefaultItem() =>
        new Item(Guid.NewGuid(), "Default Widget");

    public static Transaction CreateDefaultEntryTransaction() =>
        TransactionFactory.CreateEntryTransaction(
            Guid.NewGuid(),
            [(Guid.NewGuid(), 10, 10.0m)]);
}
```

---

## Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Test class | `{ClassUnderTest}Test` | `ItemTest`, `CreateTransactionHandlerTest` |
| Test method | `{Method}_{Scenario}` | `AddStock_NegativeQuantity`, `Handle_ItemNotFound_ReturnsFailure` |
| Test file | `{ClassUnderTest}Test.cs` | `ItemTest.cs` |
| Namespace | `Inventory.Tests.{Layer}.{Feature}` | `Inventory.Tests.Domain.Items` |

---

## Coverage Targets

| Layer | Minimum Coverage |
|-------|-----------------|
| Domain | 90% — every public method + guard clause |
| Application | 80% — every handler success + failure path |
| Value Objects | 95% — arithmetic + equality |

Run coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## Anti-Patterns (DO NOT)

1. **DO NOT** test private methods directly. Test through public API.
2. **DO NOT** write tests that depend on execution order. Each test must pass in isolation.
3. **DO NOT** use `Assert.True(result.Value == expected)`. Use `Assert.Equal(expected, result.Value)`.
4. **DO NOT** mock the System Under Test. Only mock its collaborators.
5. **DO NOT** catch exceptions with try/catch in tests. Use `Assert.Throws<T>()`.
6. **DO NOT** test infrastructure (EF Core, HTTP, DB) in unit tests.
7. **DO NOT** leave `UnitTest1.cs` — delete it once real tests exist.
8. **DO NOT** use `[Theory]` without `[InlineData]` or `[MemberData]`. Data-driven tests must have data.


