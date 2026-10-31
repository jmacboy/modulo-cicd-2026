---
name: integration-verifier
description: "Trigger: verify integration tests, integration test verification, check integration test rules, validate integration test compliance. Verify generated integration tests against project/integration-tests-rules.md — static analysis + execution report."
license: Apache-2.0
metadata:
  author: "jmacb"
  version: "1.0"
---

## Activation Contract

Activate when:

- User asks to verify, validate, or check integration tests against project rules
- User says "verify integration tests", "check integration test rules", "validate integration test compliance"
- User points to `Inventory.IntTests` files or folders and wants a compliance report
- Typically runs after `integration-generator`

Do NOT activate for `Inventory.Tests` unit test files — route those to `test-verifier` instead.

## Hard Rules

1. ALWAYS read `project/integration-tests-rules.md` first — it is the single source of truth. Never hardcode its rules (I-rules, F-rules, W-rules, E-rules) into this skill file.
2. Run static analysis BEFORE test execution.
3. Report violations as `{rule_id}: {description} → {file}:{line}`.
4. NEVER modify files — read-only operation.
5. E-rules (EF Core InMemory) apply ONLY to fakes/factories backing a `DbContext`-based handler. Do not flag a repository-backed fake for missing E-rules, and do not flag an EF Core fake for missing F-rules.
6. When `.codegraph/` exists for the project and the `codegraph_explore` tool (or `codegraph` CLI) is available, prefer it over raw Read/Grep/Glob to resolve the controller under test, its handler chain, and whether the handler injects a repository interface or a `DbContext` directly. Fall back to Read/Grep/Glob only when CodeGraph is unavailable or unindexed for that path.

## Decision Gates

| Input | Action |
|-------|--------|
| Single `{Controller}Test.cs` | Analyze that file, plus its paired `Setup/{Controller}WebApplicationFactory.cs` and any `Setup/InMemory*.cs`/`Setup/{Feature}Response.cs` it references |
| Folder | Glob `**/*Test.cs` under `Inventory.IntTests/Controllers/` and analyze all matches, plus their referenced `Setup/` files |
| A referenced `Setup/` file is missing | Report it as a violation of I1/W1/F1 (whichever applies) — do not skip silently |
| `integration-tests-rules.md` missing | Stop and report — cannot verify without rules |

## Execution Steps

1. Read `project/integration-tests-rules.md`. Extract I-rules, F-rules, W-rules, E-rules, naming conventions, and anti-patterns.
2. Resolve input path. If folder, collect all `*Test.cs` files under `Inventory.IntTests/Controllers/`. Use `codegraph_explore` when available to trace each test class to its `WebApplicationFactory`, fakes, and the real handler being exercised.
3. For each test class, resolve its companion files: the `{Controller}WebApplicationFactory`, every `InMemory{Repository}`/`InMemory{UnitOfWork}` it registers, and the `{Feature}Response` DTO(s) it deserializes into.
4. Detect whether the underlying handler injects a repository interface or a `DbContext`-derived type directly (constructor signature via `codegraph_explore` or Read). This decides whether F-rules or E-rules apply to that controller's fakes/factory.
5. **Static analysis** — for each test file, check:
   - Naming: class `{Controller}Test`, method `{Endpoint}_{Scenario}` (I1, I5)
   - `IClassFixture<{Controller}WebApplicationFactory>`, factory injected via constructor into a `private readonly` field, never re-instantiated per test (I1, I2)
   - `HttpClient` obtained with `_factory.CreateClient()` at the start of every test, never shared/cached across tests (I3)
   - AAA pattern with explicit `//Arrange`, `//Act`, `//Assert` comments (I4)
   - No real database, external API, or filesystem call — every outbound dependency is an in-memory fake (I6)
   - Request bodies use anonymous objects unless a shared request DTO already exists (I8)
   - Response deserialized via `ReadFromJsonAsync<T>()` into a dedicated `{Feature}Response` in `Setup/` — no manual `JsonDocument` parsing (I9)
   - HTTP status asserted first (`EnsureSuccessStatusCode()` or `Assert.Equal(HttpStatusCode.X, ...)`) before body/persisted-state assertions (I10, I11)
   - Persisted state asserted through a DI scope: `using var scope = _factory.Services.CreateScope()` (I12)
   - Every test uses a fresh `Guid.NewGuid()` — no hardcoded IDs (I13)
   - No assertions on exception messages/stack traces for unhandled-exception failure paths — only `HttpStatusCode` (I14)
   - For the paired factory: name `{Controller}WebApplicationFactory : WebApplicationFactory<{Controller}>` (W1); `ConfigureWebHost`/`ConfigureServices` removes the real registration before adding the fake (W2); fakes registered `Singleton` (W3); no routing/middleware/controller overrides (W4)
   - If repository-backed: fake named `InMemory{Repository}` implementing the real interface (F1); `ConcurrentDictionary`-backed (F2); `AddAsync` reproduces duplicate-key throw (F3); `IUnitOfWork` fake is a no-op (F4); no business logic beyond uniqueness/not-found (F5)
   - If `DbContext`-backed: constructor takes the `DbContext`-derived type (E1); if internal, `InternalsVisibleTo("Inventory.IntTests")` is declared and called out as a production-code touch (E2); factory uses `RemoveAll<DbContextOptions<T>>()`, not a plain `Remove` (E3); re-registered via `AddSingleton(new DbContextOptionsBuilder<T>()...Options)` — flag as a violation if `AddDbContext<T>(...)` is called a second time instead (E4); one fixed `_dbName` field set at construction (E5); seed/assert through the same `DbContext` type resolved from a DI scope (E6); no business logic in seeded data (E7); any unconditional `Database.Migrate()`-style call is guarded with `Database.IsRelational()` rather than dodged via `UseEnvironment(...)` (E8)
   - Anti-patterns: new factory per test method, hardcoded IDs, manual JSON parsing, missing HTTP-layer assertion before body, business logic in fakes, re-testing business rules already covered by unit tests, uncleaned shared mutable state across test classes reusing a factory
6. **Execution** — run `dotnet test Inventory.IntTests` (optionally with `--filter` for a targeted class). Capture stdout/stderr.
7. Parse results: passed / failed / skipped counts + error messages for failures.
8. Generate consolidated report (see Output Contract).

## Output Contract

Return a report with these sections:

**Static Analysis** — table of rule checks with Status (✅/❌), File, Line, and Detail. Group by controller (test class + its factory/fakes/DTO).

**Execution Results** — tests run, passed, failed, skipped counts. List failed tests with error messages.

**Recommendations** (non-blocking) — e.g., "business rule re-tested at HTTP layer, already covered by Inventory.Tests" or a production-code touch (E2/E8) worth double-checking.

## References

- `project/integration-tests-rules.md` — project-specific integration testing rules (read at runtime, never duplicated)
