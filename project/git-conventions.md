# Git Conventions — Repo-wide Rules

## Branches

- Branching model: **trunk-based development**. `main` is the trunk; branches are short-lived
  (integrated within 1–2 days) and there are no long-lived feature, `develop`, or release
  branches. Full rules: [trunk-based-development.md](trunk-based-development.md).
- Never commit to `main`. Every change goes through a branch and a pull request.
- Every branch is developed in its own worktree. See [worktree-workflow.md](worktree-workflow.md).
- Branch names: `<type>/<feature-name>`, kebab-case.

| Type | Use |
|---|---|
| `feat/` | New behavior (observed: `feat/item-soft-delete`) |
| `fix/` | Bug fix |
| `chore/` | Tooling, hooks, config |
| `docs/` | Documentation only |

---

## Commits

- Use **Conventional Commits**: `<type>(<scope>): <summary>`.
  - Examples from history: `feat(item): soft delete items and block deletion with reserved stock`,
    `chore(husky): run lint-staged per package in the monorepo`.
  - Types: `feat`, `fix`, `chore`, `docs`, `test`, `refactor`.
  - Scope: the aggregate or area touched (`item`, `transaction`, `husky`, `client`).
- History also contains free-form Spanish messages (`agregado item service`); new commits use
  Conventional Commits.
- Keep tests and docs in the same commit as the behavior they cover.
- AI agents commit only when asked.

---

## Git Hooks

Hooks are managed by Husky 9 (`core.hooksPath = .husky/_`) and only work after `pnpm install` at
the root of the checkout or worktree (the `prepare` script runs `husky`).

| Hook | Runs | Config |
|---|---|---|
| `pre-commit` | `npx lint-staged` | `inventory-project/.lintstagedrc.mjs` (`dotnet format` on staged `*.cs`), `react-client/.lintstagedrc.json` (`prettier --write`) |
| `pre-push` | `dotnet test inventory-project/Inventory.Tests` | A failing unit test blocks the push |

- Never bypass hooks (`--no-verify`, `HUSKY=0`).
- Never weaken `.husky/*` or the lint-staged configs to make a commit or push pass. Fix the code.

---

## Pull Requests

1. Rebase on the trunk (`git fetch origin && git rebase origin/main`), then push the branch:
   `git push -u origin <type>/<feature-name>`.
2. Open a small PR to `main`.
3. Before merging, the hooks must have passed and the relevant tests must pass locally
   (`dotnet test` in `inventory-project/`, `yarn test:pact` in `react-client/` when the client
   changed).
4. A human reviews and merges (squash or rebase merge, linear history).
   Push, PR creation, and merge are human decisions.
5. Right after merge, remove the worktree and delete the branch
   (see [worktree-workflow.md](worktree-workflow.md#remove-after-merge)).

Merge strategy, rebase cadence, and broken-trunk handling:
[trunk-based-development.md](trunk-based-development.md).

---

## Task Documents

- Multi-step features keep `odd/tasks/<feature-name>.md` (objective, scope, checklist, evidence)
  on the feature branch, inside the feature worktree.
