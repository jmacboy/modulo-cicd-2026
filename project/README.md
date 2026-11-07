# Project Rules — Repo-wide Index

Rules and conventions that apply to the whole repository. Package-specific rules live next to
each package and are linked below.

## Repo-wide Rules

| Document | Covers |
|---|---|
| [trunk-based-development.md](trunk-based-development.md) | **Mandatory**: branching model — `main` as trunk, short-lived branches, small PRs, rebase, merge strategy, feature flags, broken trunk |
| [worktree-workflow.md](worktree-workflow.md) | **Mandatory**: one Git worktree per feature — location, naming, commands, bootstrap, shared resources, lifecycle |
| [git-conventions.md](git-conventions.md) | Branches, Conventional Commits, Git hooks, pull request flow, task documents |
| [code-rules.md](code-rules.md) | **Mandatory**: code rules with stable IDs (JS-1 promise handling, S9383; JS-2 optional chaining, S6582; JS-3 null dereference, S2259; JS-4 no autoFocus, S9379; JS-5 no listeners on non-interactive elements, S6847; JS-6 keyboard equivalent for click handlers, S1082) |

## Package Rules

| Package | Document | Covers |
|---|---|---|
| `inventory-project` | [project/README.md](../inventory-project/project/README.md) | Project guide and PR checklist |
| `inventory-project` | [unit-testing-rules.md](../inventory-project/project/unit-testing-rules.md) | Unit tests (xUnit + Moq) |
| `inventory-project` | [integration-tests-rules.md](../inventory-project/project/integration-tests-rules.md) | Integration tests (`WebApplicationFactory`) |
| `react-client` | [pact-rules.md](../react-client/project/pact-rules.md) | Pact consumer contract tests |

## Precedence

Package rules refine repo-wide rules for their package; they never override the trunk-based development,
worktree, or Git conventions above.
