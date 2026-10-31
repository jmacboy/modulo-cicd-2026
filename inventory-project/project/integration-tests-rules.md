# Integration Testing Rules — Store Inventory

## Stack

- **Framework**: .NET 8.0 / C#
- **Test runner**: xUnit 2.5.3
- **Host**: `Microsoft.AspNetCore.Mvc.Testing` 8.0.30 (`WebApplicationFactory<TEntryPoint>`)
- **Coverage**: coverlet.collector 6.0.0
- **Test project**: `Inventory.IntTests`
- **EF Core InMemory provider**: `Microsoft.EntityFrameworkCore.InMemory` (matching the `Microsoft.EntityFrameworkCore` version used by `Inventory.Infrastructure`) — only needed when a handler injects a `DbContext` directly (see EF Core InMemory Fakes below)

---

## Scope

**Integration tests cover the WebApi layer end-to-end**: HTTP request → Controller → MediatR → Handler → Repository/UnitOfWork, using an **in-memory fake infrastructure** instead of a real database.

They do NOT replace unit tests. Domain and Application logic in isolation stays in `Inventory.Tests` (see `project/unit-testing-rules.md`). An integration test exists to prove the wiring (routing, DI, serialization, status codes) works — not to re-verify business rules already covered at the unit level.

---

## Architecture

```
Client (HttpClient from WebApplicationFactory)
        │  HTTP request (PostAsJsonAsync / GetAsync)
        ▼
   Controller (real, from Inventory.WebApi)
        ▼
   MediatR pipeline (real)
        ▼
   Handler (real, from Inventory.Application)
        ▼
   Repository / UnitOfWork  ← FAKE (in-memory, registered in the factory)
```

Only the persistence boundary is faked. Everything above it (controller, MediatR, handlers, domain) runs for real.

---

## Location & File Layout

| Folder | Purpose |
|---|---|
| `Inventory.IntTests/Controllers/{Controller}Test.cs` | One test class per WebApi controller |
| `Inventory.IntTests/Setup/{Controller}WebApplicationFactory.cs` | One factory per controller (or shared, if controllers share the same fake dependencies) |
| `Inventory.IntTests/Setup/InMemory{Repository}.cs` | One in-memory fake per repository interface used by the controller under test |
| `Inventory.IntTests/Setup/{Feature}Response.cs` | DTO mirroring the JSON shape returned by the endpoint, used to deserialize the response body |

---

## Rules

| # | Rule |
|---|------|
| I1 | One test class per controller. File: `{Controller}Test.cs`, implementing `IClassFixture<{Controller}WebApplicationFactory>` |
| I2 | Inject the factory via constructor and store it in a `private readonly` field. Do NOT create a new factory per test |
| I3 | Get the `HttpClient` with `_factory.CreateClient()` at the start of every test — never share a client across tests |
| I4 | Use AAA pattern with explicit `//Arrange`, `//Act`, `//Assert` comments |
| I5 | Test method naming: `{Endpoint}_{Scenario}` — e.g., `CreateItem_WithValidRequest`, `CreateItem_WithDuplicateRequest` |
| I6 | Never call a real database, external API, or filesystem. Replace every outbound dependency (`IItemRepository`, `IUnitOfWork`, etc.) with an in-memory fake registered in `ConfigureWebHost` |
| I7 | Register fakes by removing the existing `ServiceDescriptor` first (`SingleOrDefault` + `Remove`), then adding the fake with `AddSingleton`. Never leave both registrations in the container |
| I8 | Use anonymous objects (`new { Id = ..., ItemName = ... }`) for request bodies unless a shared request DTO already exists in `Inventory.Application` |
| I9 | Deserialize the response with `ReadFromJsonAsync<T>()` into a dedicated `{Feature}Response` class in `Setup/` that mirrors the real JSON shape — do NOT parse `JsonDocument` manually |
| I10 | Assert the HTTP layer first: `response.EnsureSuccessStatusCode()` for the happy path, or `Assert.Equal(HttpStatusCode.X, response.StatusCode)` for failure paths |
| I11 | Assert the response envelope (`IsSuccess`, `IsFailure`, `Value`) before asserting persisted state |
| I12 | After asserting the HTTP response, open a DI scope with `_factory.Services.CreateScope()` and resolve the fake repository to assert persisted state directly. Always `using var scope = ...` |
| I13 | Every test must use a fresh `Guid.NewGuid()` for entity IDs — never hardcode IDs, since the in-memory fakes are singletons shared across tests in the same class |
| I14 | Do NOT assert on exception messages or stack traces for failure paths driven by unhandled exceptions (e.g. duplicate key) — assert only the resulting `HttpStatusCode` |

---

## In-Memory Fakes (`Setup/`)

### Rules

| # | Rule |
|---|------|
| F1 | One fake class per repository interface: `InMemory{Repository}`, implementing the real interface from `Inventory.Domain` |
| F2 | Back the fake with a `ConcurrentDictionary<Guid, TEntity>` — the factory registers it as `Singleton`, so it must be thread-safe across parallel test execution |
| F3 | `AddAsync` must reproduce the real repository's duplicate-key behavior (e.g., throw `InvalidOperationException` on `TryAdd` failure) so failure-path integration tests are meaningful |
| F4 | `IUnitOfWork` fakes are no-ops (`Task.CompletedTask`) — integration tests do not verify commit semantics, that belongs in Application unit tests |
| F5 | Never add business logic to a fake beyond what's needed to reproduce infrastructure behavior (uniqueness, not-found). Business rules belong in the Domain layer, not in test doubles |

### Example — Fake repository

```csharp
public class InMemoryItemRepository : IItemRepository
{
    private readonly ConcurrentDictionary<Guid, Item> _items = new();

    public Task AddAsync(Item entity)
    {
        if (!_items.TryAdd(entity.Id, entity))
        {
            throw new InvalidOperationException($"Ya existe un item con id {entity.Id}.");
        }

        return Task.CompletedTask;
    }

    public Task<Item?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        _items.TryGetValue(id, out var item);
        return Task.FromResult(item);
    }
}
```

---

## EF Core InMemory Fakes (DbContext-based handlers)

Some handlers (typically CQRS queries under `Inventory.Infrastructure/Queries/`) bypass the repository abstraction and inject a `DbContext`-derived type directly (e.g. `PersistenceDbContext`). There is no repository interface to swap in that case — swap the `DbContext`'s underlying provider instead. **Never let such a handler hit the real Npgsql/SQL Server connection in a test.**

### Rules

| # | Rule |
|---|------|
| E1 | Detect this case when the handler's constructor takes a `DbContext`-derived type (e.g. `PersistenceDbContext dbContext`) instead of a repository interface |
| E2 | If the `DbContext` type (or the entity type in its `DbSet<T>`) is `internal` to its assembly (e.g. `Inventory.Infrastructure`), it is invisible to `Inventory.IntTests` by default. Make it visible with `[assembly: InternalsVisibleTo("Inventory.IntTests")]` in that assembly (e.g. an `AssemblyInfo.cs`) and add the matching `ProjectReference` to `Inventory.IntTests.csproj`. This touches production source — call it out explicitly in the output report rather than doing it silently |
| E3 | In the controller's `WebApplicationFactory`, remove the existing `DbContextOptions<T>` registration with `services.RemoveAll<DbContextOptions<T>>()`. Removing only `T` itself (as done for repositories, I7) is not enough, since `AddDbContext` also registers `DbContextOptions<T>` |
| E4 | Re-register by building the options directly and adding the built instance — `services.AddSingleton(new DbContextOptionsBuilder<T>().UseInMemoryDatabase(name).Options)`. **Do NOT call `services.AddDbContext<T>(...)` again**: `AddDbContext` is additive — calling it a second time layers the new provider configuration on top of the original one instead of replacing it, and EF Core throws `InvalidOperationException: ... multiple database providers ... have been registered` the first time the context is used. Add the `Microsoft.EntityFrameworkCore.InMemory` package to `Inventory.IntTests.csproj` if missing |
| E5 | Use ONE fixed database name per factory instance (e.g. a `private readonly string _dbName = $"IntTests_{Guid.NewGuid()}";` field set at factory construction) so state persists across requests within the same test class — mirrors the Singleton behavior of repository fakes (I13) |
| E6 | Seed and assert state through the SAME `DbContext` type, resolved from a DI scope (`scope.ServiceProvider.GetRequiredService<T>()`), exactly like I12 does for repositories |
| E7 | Never add business logic to the seeded data beyond what the test scenario needs (F5 applies here too) |
| E8 | If any startup code path calls `Database.Migrate()` (or another relational-only `DatabaseFacade` method) unconditionally against this same `DbContext` — e.g. a `Development`-only migration step in `Program.cs` — guard it with `Database.IsRelational()` in the production code (see `PersistenceDbContext.Migrate()`) instead of changing the test's hosting environment. Overriding the environment (`UseEnvironment(...)`) to dodge that call also silently disables ASP.NET Core's implicit Development-only `UseDeveloperExceptionPage()`, which breaks any failure-path test expecting unhandled exceptions to surface as a real HTTP 500 instead of being rethrown to the test client |

### Example — Factory swapping a DbContext

```csharp
public class ItemControllerWebApplicationFactory : WebApplicationFactory<ItemController>
{
    private readonly string _dbName = $"IntTests_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IItemRepository));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            services.AddSingleton<IItemRepository, InMemoryItemRepository>();
            services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();

            services.RemoveAll<DbContextOptions<PersistenceDbContext>>();
            services.AddSingleton(new DbContextOptionsBuilder<PersistenceDbContext>()
                .UseInMemoryDatabase(_dbName)
                .Options);
        });
    }
}
```

---

## WebApplicationFactory (`Setup/`)

### Rules

| # | Rule |
|---|------|
| W1 | Name the factory `{Controller}WebApplicationFactory`, extending `WebApplicationFactory<{Controller}>` |
| W2 | Override `ConfigureWebHost`, and inside `ConfigureServices` remove every real infrastructure registration before adding the fake |
| W3 | Register fakes as `Singleton` so state persists across requests within the same test (needed to assert persisted state after the HTTP call) |
| W4 | Do NOT override routing, middleware, or controllers — only the DI container for outbound dependencies |

### Example

```csharp
public class ItemControllerWebApplicationFactory : WebApplicationFactory<ItemController>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IItemRepository));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<IItemRepository, InMemoryItemRepository>();
            services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();
        });
    }
}
```

---

## Example — Full controller test

```csharp
public class ItemControllerTest : IClassFixture<ItemControllerWebApplicationFactory>
{
    private readonly ItemControllerWebApplicationFactory _factory;

    public ItemControllerTest(ItemControllerWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateItem_WithValidRequest()
    {
        //Arrange
        var client = _factory.CreateClient();
        var itemGuid = Guid.NewGuid();
        var itemName = "Test Item";

        //Act
        var response = await client.PostAsJsonAsync("/api/Item", new { Id = itemGuid, ItemName = itemName });

        //Assert
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ItemCreateResponse>();
        Assert.NotNull(body);
        Assert.True(body.IsSuccess);
        Assert.Equal(itemGuid, body.Value);

        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IItemRepository>();
        var item = await repository.GetByIdAsync(itemGuid);
        Assert.NotNull(item);
        Assert.Equal(itemName, item.Name);
    }

    [Fact]
    public async Task CreateItem_WithDuplicateRequest()
    {
        //Arrange
        var client = _factory.CreateClient();
        var itemGuid = Guid.NewGuid();
        var itemName = "Test Item";

        //Act
        var response = await client.PostAsJsonAsync("/api/Item", new { Id = itemGuid, ItemName = itemName });
        var response2 = await client.PostAsJsonAsync("/api/Item", new { Id = itemGuid, ItemName = "Name" });

        //Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.InternalServerError, response2.StatusCode);
    }
}
```

---

## Naming Conventions

| Element | Convention | Example |
|---|---|---|
| Test class | `{Controller}Test` | `ItemControllerTest` |
| Test method | `{Endpoint}_{Scenario}` | `CreateItem_WithValidRequest` |
| Factory | `{Controller}WebApplicationFactory` | `ItemControllerWebApplicationFactory` |
| Fake repository | `InMemory{Repository}` | `InMemoryItemRepository` |
| Response DTO | `{Feature}Response` | `ItemCreateResponse` |
| Namespace | `Inventory.IntTests.{Layer}` | `Inventory.IntTests.Controllers`, `Inventory.IntTests.Setup` |

---

## Anti-Patterns (DO NOT)

1. **DO NOT** point tests at a real database, container, or external service. Everything outbound is an in-memory fake.
2. **DO NOT** create a new `WebApplicationFactory` per test method — reuse the one injected via `IClassFixture`.
3. **DO NOT** hardcode entity IDs — fakes are singletons shared across every test in the class, so collisions produce false failures.
4. **DO NOT** re-test business rules already covered by Domain/Application unit tests. Integration tests verify wiring, not logic.
5. **DO NOT** parse response bodies manually with `JsonDocument`/`JsonSerializer` inline — define a `{Feature}Response` DTO in `Setup/`.
6. **DO NOT** skip the HTTP-layer assertion (`EnsureSuccessStatusCode` / `StatusCode`) before asserting the body or persisted state.
7. **DO NOT** add business logic to an in-memory fake beyond reproducing infrastructure behavior (uniqueness, not-found).
8. **DO NOT** leave shared mutable state uncleaned between test classes that reuse the same factory — prefer one factory per controller.
9. **DO NOT** let a handler that injects a `DbContext` directly hit the real database in a test. Swap its provider for EF Core InMemory in the factory (E-rules) — do not assume repository-only fakes cover it when no repository interface exists.
