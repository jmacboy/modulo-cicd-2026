---
name: integration-generator
description: "Trigger: generate integration tests, integration test this controller, test this class/folder with integration tests. Apply project/integration-tests-rules.md; only for eligible HTTP controllers."
license: Apache-2.0
metadata:
  author: "jmacb"
  version: "1.0"
---

## Activation Contract

Activate when:

- User asks to generate, create, or write integration tests for a controller, class, or folder
- User says "integration test this", "generate integration tests", "test this controller end-to-end"
- User points to a WebApi controller and wants HTTP-level test coverage

Do NOT activate for Domain entities, Value Objects, Handlers, or Factories — route those to `test-generator` (unit tests) instead.

## Hard Rules

1. READ `project/integration-tests-rules.md` FRESH on every invocation before generating anything. It is the single source of truth — never hardcode its rules (naming, AAA structure, fake conventions, I-rules/F-rules/W-rules) into this skill file.
2. Run eligibility detection BEFORE writing or editing any file. Never generate tests for a target until eligibility is confirmed.
3. If zero eligible controllers are found in the resolved target, STOP. Create/edit nothing. Return only the eligibility report plus "no integration tests generated."
4. When `.codegraph/` exists and `codegraph_explore` is available, prefer it over raw Read/Grep/Glob to resolve controller → handler → repository chains and existing `Setup/` fakes. Fall back to Read/Grep/Glob otherwise.
5. Never invent a failure-path scenario the handler doesn't actually expose (e.g. don't assert a duplicate-key 500 if nothing in the handler/repository can throw one).
6. If a handler bypasses repositories and injects a `DbContext` directly, follow the EF Core InMemory Fakes rules (E-rules) in `project/integration-tests-rules.md` — do not invent an ad hoc swap. If applying them requires touching production source (`InternalsVisibleTo` for an internal `DbContext`/entity, or guarding a `Migrate()`-style call with `Database.IsRelational()`), call that out explicitly in the report as a production-code change, not a silent side effect.

## Decision Gates — Eligibility

| Source Type | Eligible? | Reason |
|---|---|---|
| Inherits `ControllerBase`/`Controller`, lives under a WebApi `Controllers/` folder, has `[Http*]` action methods | YES | In scope per `integration-tests-rules.md` (HTTP → Controller → MediatR → Handler wiring) |
| Domain Entity / Value Object | NO | Unit test scope — use `test-generator` |
| MediatR Handler (`IRequestHandler`) | NO | Unit test scope — use `test-generator` |
| Factory | NO | Unit test scope — use `test-generator` |
| Repository interface/implementation | NO | Faked as an in-memory double, not tested directly |
| `Program.cs` / DI / Startup / infrastructure file | NO | Not an HTTP-exposed endpoint |
| Folder mixing controllers with other types | PARTIAL | Generate only for eligible controllers; list every skipped file with its reason |

## Execution Steps

1. Read `project/integration-tests-rules.md` completely.
2. Resolve the target (file, class, or folder).
3. Classify every candidate `.cs` file against the Decision Gates table; build the eligibility report (eligible vs skipped + one-line reason each).
4. If eligible count is 0 → STOP per Hard Rule 3.
5. For each eligible controller:
   - Reuse an existing `{Controller}WebApplicationFactory` in `Inventory.IntTests/Setup/` if present; else create one per the rules file's W-rules.
   - Reuse existing `InMemory{Repository}`/`InMemory{UnitOfWork}` fakes if present; else create only the missing ones per the F-rules (ConcurrentDictionary-backed, reproduce duplicate-key/not-found behavior, no business logic).
   - Reuse an existing `{Feature}Response` DTO matching the endpoint's JSON shape if present; else create one.
   - Generate `Inventory.IntTests/Controllers/{Controller}Test.cs` following the I-rules (naming, AAA, `IClassFixture`, fresh `Guid` per test, HTTP status assertions before body/persisted-state assertions). Cover the happy path plus only the failure paths the handler actually exposes.
6. Run `dotnet test` to verify compilation and pass rate.

## Output Contract

Return:

- Eligibility report (always) — eligible vs skipped, with reasons.
- Files created or reused, with full paths.
- Which rule IDs from `integration-tests-rules.md` were applied (I-rules, F-rules, W-rules, E-rules when a DbContext-based handler was involved).
- Any production-source file touched to make the target testable (e.g. `InternalsVisibleTo`, a relational guard), with the exact reason.
- `dotnet test` results.
- On stop: eligibility report + explicit "no integration tests generated" statement — nothing else.

## References

- `project/integration-tests-rules.md` — full integration testing rules, examples, and anti-patterns (read at runtime, never duplicated here)
