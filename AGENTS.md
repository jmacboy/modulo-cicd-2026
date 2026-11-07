# AGENTS.md

Rules for AI coding agents working in this repository. Read this first, then the repo-wide rules
in [`project/`](project/README.md), then the nested instructions for the package you touch.

## MUST: trunk-based development

- **`main` is the trunk and must always be green and releasable.** Full rules:
  [`project/trunk-based-development.md`](project/trunk-based-development.md).
- Only short-lived branches off `origin/main` (`feat/`, `fix/`, `chore/`, `docs/`), each in its
  own worktree, integrated into `main` at least daily (1–2 days max). Never create long-lived
  feature, `develop`, or release branches.
- Keep PRs small; rebase often with `git fetch origin && git rebase origin/main`. Merge into
  `main` by squash or rebase merge (linear history), then delete the branch and remove its
  worktree right away.
- Unfinished work that must land early goes behind a feature flag, dark launch, or branch by
  abstraction, never on a long branch.
- Before merging, the Husky hooks (`pre-commit`, `pre-push`) must pass and the relevant tests
  must pass locally; a broken `main` is fixed or reverted immediately.

## MUST: code rules scan before every push

- **Before every `git push`, run the `code-rules-check` skill**
  (`.claude/skills/code-rules-check/SKILL.md`): it reads [`project/code-rules.md`](project/code-rules.md)
  and scans the files changed in `origin/main...HEAD`. Fixed violations are staged and committed
  (`fix(<scope>): apply code rules <IDs>`). Push only on `PASS`, or after the user explicitly
  accepts a reported violation.

## MUST: one worktree per feature

- **Before starting any feature, create a worktree per
  [`project/worktree-workflow.md`](project/worktree-workflow.md); never develop features in the
  main checkout.**
- Location: `../repo-worktrees/<feature-name>` (sibling of the repo root), branch
  `feat/<feature-name>` from `origin/main`. Never inside the repo, never in a temp directory.
- Bootstrap every new worktree (`pnpm install` at its root, `dotnet restore` in
  `inventory-project/`, `yarn install` in `react-client/`); without `pnpm install` the Git hooks
  do not run.
- Worktrees are short-lived: remove the worktree as soon as its branch is merged.
- Host ports `5432` (PostgreSQL) and `5094` (API) are shared across worktrees: run them from one
  worktree at a time. The database itself is likely separate per worktree (Compose project named
  after the checkout folder; unverified), so run migrations in each worktree.

## Repo map

| Path | What it is |
|---|---|
| `inventory-project/` | .NET 8 inventory service (`Inventory.sln`), Clean Architecture + DDD |
| `inventory-project/src/` | `Inventory.Domain`, `Inventory.Application`, `Inventory.Infrastructure`, `Inventory.WebApi` |
| `inventory-project/Inventory.Tests/` | Unit tests (xUnit + Moq) and the coverage script |
| `inventory-project/Inventory.IntTests/` | Integration tests (`WebApplicationFactory`, in-memory fakes in `Setup/`) |
| `inventory-project/project/` | Testing rules (`unit-testing-rules.md`, `integration-tests-rules.md`) |
| `react-client/` | React 19 + Vite client with Pact consumer contract tests |
| `react-client/project/pact-rules.md` | Pact test rules |
| `odd/tasks/` | Feature task documents (one file per feature) |
| `.husky/` | Git hooks (pre-commit, pre-push) |
| `docker-compose.yml` | PostgreSQL 16 for local development (run Compose from the repo root) |
| `project/` | Repo-wide rules: trunk-based development, worktree workflow, Git conventions |

## Nested instructions (read before working in a package)

- `inventory-project/CLAUDE.md`: all test work MUST be delegated to the `test-writer` subagent
  (`inventory-project/.claude/agents/test-writer.md`). Do not write or edit test files in the
  main thread.
- `inventory-project/.claude/skills/`: `test-generator` / `test-verifier` (unit) and
  `integration-generator` / `integration-verifier` (integration).
- `inventory-project/opencode.json`: denies edits to `Inventory.Tests/**` for OpenCode agents.
- `react-client/.claude/agents/pact-workflow.md`: use it for any Pact/contract test request; it
  runs the `pact-generator` + `pact-checker` skills.
- `react-client/.claude/skills/vercel-react-best-practices/`: React performance guidance.

## Architecture (inventory-project)

Dependency direction points inward. Never add a reference that points outward.

```
WebApi ──► Application ──► Domain
   └─────► Infrastructure ─┘ (Infrastructure ──► Application ──► Domain)
```

- **Domain**: aggregates (`Item`, `Transaction`, `User` extend `AggregateRoot`), value objects
  (`Shared/CostValue`, `Shared/QuantityValue`), domain events (`Transactions/Events/*` extend
  `DomainEvent`), repository interfaces, factories. Base types come from `Joseco.DDD.Core`.
  Domain has no EF Core or ASP.NET knowledge.
- **Application**: CQRS with MediatR. One folder per use case:
  `<Aggregate>/<UseCase>/<UseCase>Command.cs` + `<UseCase>Handler.cs`. Commands are records
  implementing `IRequest<Result<T>>`. Domain event handlers live in `<Aggregate>/EventHandlers/`
  and implement `INotificationHandler<TEvent>`.
- **Infrastructure**: EF Core 9 + Npgsql. Two contexts: `DomainDbContext` (write side) and
  `PersistenceDbContext` (read models / migrations). Repositories, `UnitOfWork`, and query
  handlers (`Queries/`) are `internal`.
- **WebApi**: controllers, `Program.cs`, Swagger. In Development it applies migrations on startup.

## Coding conventions

- Target `net8.0` with `Nullable` enabled in every project.
- Return `Result` / `Result<T>` (`Joseco.DDD.Core.Results`) from handlers; do not throw for
  expected failures. Define domain errors as static factories in `<Aggregate>Errors`
  (e.g. `ItemErrors`) returning `Error` with an `ErrorType`.
- Persist changes through `IUnitOfWork.CommitAsync`, not by calling the DbContext from handlers.
- Formatting is enforced by `.editorconfig` in each `src/*` project (4 spaces, CRLF for `*.cs`)
  and by `dotnet format` on commit. Do not reformat unrelated code.
- react-client: Prettier (`react-client/.prettierrc.json`: single quotes, 4 spaces, width 100,
  LF) and ESLint (`eslint.config.js`).
- Follow [`project/code-rules.md`](project/code-rules.md) (rule IDs such as `JS-1`, sourced from
  SonarQube findings); do not restate the rules here.

## Commands

Run .NET commands from `inventory-project/` unless noted. Docker Compose commands run from the
repo root, where `docker-compose.yml` lives.

| Task | Command |
|---|---|
| Start PostgreSQL | from the repo root: `docker compose up -d postgres` |
| Apply migrations | `dotnet ef database update --project src/Inventory.Infrastructure --startup-project src/Inventory.WebApi --context PersistenceDbContext` |
| Restore / build | `dotnet restore` then `dotnet build --no-restore` |
| All tests | `dotnet test` |
| Unit tests only | `dotnet test Inventory.Tests` |
| Integration tests only | `dotnet test Inventory.IntTests` |
| One layer | `dotnet test --filter "FullyQualifiedName~Inventory.Tests.Domain"` |
| Coverage (raw) | `dotnet test --collect:"XPlat Code Coverage"` |
| Coverage HTML report | in PowerShell, from `Inventory.Tests/`: `./ExecCodeCoverage.ps1` |
| Client install | from `react-client/`: `yarn install` (Yarn 1.22.22, see `packageManager`) |
| Client dev / build / lint | `yarn dev`, `yarn build`, `yarn lint` |
| Pact consumer tests | `yarn test:pact` (writes to `react-client/pacts/`) |

## Testing rules

- Follow `inventory-project/project/unit-testing-rules.md` and
  `inventory-project/project/integration-tests-rules.md`; follow
  `react-client/project/pact-rules.md` for contract tests. Do not restate or override them.
- Delegate test generation/fixes to `test-writer` (.NET) or `pact-workflow` (react-client).
- Add or update tests together with every behavior change.
- Unit tests never touch EF Core, HTTP, or a database. Integration tests swap real
  repositories for the in-memory fakes in `Inventory.IntTests/Setup/`.

## Git, hooks, and task docs

Full rules: [`project/git-conventions.md`](project/git-conventions.md). Summary:

- Never commit to `main`. Short-lived branches: `feat/|fix/|chore/|docs/<feature-name>`
  (kebab-case), each in its own worktree (see trunk-based development above).
- Conventional Commits (`feat(item): ...`, `chore(husky): ...`). Commit only when asked. Push,
  PRs, and merges are human decisions.
- Hooks: `pre-commit` runs `npx lint-staged` (`dotnet format`, `prettier`); `pre-push` runs the
  unit tests.
- For multi-step features, keep `odd/tasks/<feature-name>.md` updated in the feature worktree.

## Never

- Never bypass hooks (`--no-verify`, `HUSKY=0`) or weaken `.husky/*` / lint-staged configs.
- Never hand-edit `Inventory.Tests/coveragereport/` or `Inventory.Tests/CoverageHistory/`;
  regenerate them with `ExecCodeCoverage.ps1` only when asked.
- Never edit existing EF migrations or `PersistenceDbContextModelSnapshot.cs` by hand; add a new
  migration instead.
- Never edit `react-client/pacts/*.json` by hand; regenerate with `yarn test:pact`.
- Never create long-lived branches (`develop`, `release/*`, multi-day feature branches).
- Never commit secrets, `.env` files, `bin/`, `obj/`, `node_modules/`, `.atl/`, or `.codegraph/`.
- Never let Domain reference Application, Infrastructure, WebApi, or EF Core.
- Never write tests in the main thread in `inventory-project` (see `CLAUDE.md`).
