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
  - Scope: the aggregate or area touched (`item`, `transaction`, `husky`, `client`).
- Messages are validated by **commitlint** (`commitlint.config.js`, extends
  `@commitlint/config-conventional`). A message that breaks an error rule is rejected:

| Rule | Requirement |
|---|---|
| `type-enum` | Type is one of `build`, `chore`, `ci`, `docs`, `feat`, `fix`, `perf`, `refactor`, `revert`, `style`, `test` |
| `type-case`, `type-empty` | Type is required and lower-case |
| `scope-empty` | Scope is required (`feat(item): ...`, not `feat: ...`); project rule added in `commitlint.config.js` |
| `subject-empty` | Summary is required |
| `subject-case` | Summary does not start in Sentence case, Start Case, PascalCase, or UPPER CASE (write `add item filter`, not `Add item filter`) |
| `subject-full-stop` | Summary does not end with `.` |
| `header-max-length` | Header (`<type>(<scope>): <summary>`) is at most 100 characters |
| `header-trim` | No leading or trailing whitespace in the header |
| `body-max-line-length`, `footer-max-line-length` | Body and footer lines are at most 100 characters |
| `body-leading-blank`, `footer-leading-blank` | Blank line before body and footer (warning only) |

- commitlint's default ignore list is off (`defaultIgnores: false`), so messages like `1.0.0`,
  `Merge branch ...`, or `Revert arbitrary text` are validated and rejected. Only two git-generated
  formats are exempt:
  - `fixup!`, `squash!`, `amend!` commits, consumed by `git rebase --autosquash`;
  - the message `git revert` writes: `Revert "<subject>"` (or `Reapply "<subject>"`) followed by
    `This reverts commit <sha>.`
- `git revert --reference` leaves the title for you to write, so give it a conventional title such
  as `revert(item): drop filter that broke the list`; a revert left with only Git's placeholder
  title is rejected.
- Check a message before committing: `echo "feat(item): add filter" | npx commitlint`.
- History also contains free-form Spanish messages (`agregado item service`); new commits use
  Conventional Commits.
- Keep tests and docs in the same commit as the behavior they cover.
- AI agents commit only when asked.
- **Before every push, scan the branch's changed files (`origin/main...HEAD`) against
  [code-rules.md](code-rules.md).** AI agents run the `code-rules-check` skill
  (`.claude/skills/code-rules-check/SKILL.md`): fixed violations are staged and committed as
  `fix(<scope>): apply code rules <IDs>`, and the push happens only on `PASS`. Humans review their
  branch diff against the same rules. The Husky hooks still run.

---

## Git Hooks

Hooks are managed by Husky 9 (`core.hooksPath = .husky/_`) and only work after `pnpm install` at
the root of the checkout or worktree (the `prepare` script runs `husky`).

| Hook | Runs | Config |
|---|---|---|
| `pre-commit` | `npx lint-staged` | `inventory-project/.lintstagedrc.mjs` (`dotnet format` on staged `*.cs`), `react-client/.lintstagedrc.json` (`prettier --write`) |
| `commit-msg` | `npx --no -- commitlint --edit "$1"` | `commitlint.config.js`; an invalid message blocks the commit |
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
