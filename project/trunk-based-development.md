# Trunk-Based Development — Repo-wide Rule

## Rule

**This repository uses trunk-based development. `main` is the trunk.**

- `main` must always be **green and releasable**: it builds, its hooks pass, and its tests pass.
- The merge gate is local: the Husky hooks plus the relevant tests run locally before merging.
- All work happens on **short-lived branches off `main`** and is integrated back into `main`
  quickly.
- There are **no long-lived branches**: no long-running feature branches, no `develop`, no
  `release/*` branches.

---

## Short-lived Branches

| Type | Branch | Use |
|---|---|---|
| Feature | `feat/<feature-name>` | New behavior |
| Bug fix | `fix/<feature-name>` | Bug fix |
| Tooling / config | `chore/<feature-name>` | Tooling, hooks, config |
| Documentation | `docs/<feature-name>` | Documentation only |

- Every branch is created from `origin/main` and lives in its own worktree
  (see [worktree-workflow.md](worktree-workflow.md)).
- **Integrate into `main` at least daily; a branch must never live longer than 1–2 days.**
- If a change cannot be finished in that window, split it into smaller changes or land the
  unfinished part hidden (see [Incomplete Work](#incomplete-work)). Do not keep the branch open.
- Naming and commit format: [git-conventions.md](git-conventions.md).

---

## Small Pull Requests

- One PR = one small, coherent change with its tests and docs.
- Prefer several small PRs merged in sequence over one large PR.
- A PR that keeps growing is a sign the branch is living too long: merge what is ready and
  continue in a new short-lived branch.

---

## Stay Close to the Trunk

Rebase on the latest `main` frequently (at least before pushing and before merging):

```bash
git fetch origin && git rebase origin/main
```

- Resolve conflicts on the branch, re-run the hooks and tests, then push.
- After a rebase of an already pushed branch, push with `git push --force-with-lease`
  (never plain `--force`).
- Do not merge `main` into the branch; rebase instead, to keep history linear.

---

## Merging into the Trunk

- Merge strategy: **squash merge** or **rebase merge** into `main`. Keep `main` history
  linear; do not create merge commits on `main`.
- Never commit or push directly to `main`; every change goes through a PR.
- Merging is a human decision.

### Before merging

- [ ] Branch rebased on the latest `origin/main`
- [ ] Git hooks passed (`pre-commit` runs lint-staged, `pre-push` runs the unit tests); never
      bypassed
- [ ] Relevant tests pass locally on the rebased branch (`dotnet test` in `inventory-project/`;
      `yarn test:pact` in `react-client/` when the client changed)
- [ ] Incomplete behavior is hidden (see below)

### Right after merging

Delete the branch and remove its worktree immediately
(commands: [worktree-workflow.md](worktree-workflow.md#remove-after-merge)).

---

## Incomplete Work

Work that must land on `main` before it is finished is integrated **hidden**, never kept on a
long branch:

- **Feature flags**: the new behavior is merged but disabled until it is complete.
- **Dark launching**: the new code path is deployed and exercised without being exposed to users.
- **Branch by abstraction**: introduce an abstraction over the code being replaced, move callers
  to it, build the new implementation behind it, then switch and remove the old one — each step
  a small PR on `main`.

The repository does not currently ship a feature-flag library. Choose the mechanism per change,
keep it as simple as possible, and remove the flag or abstraction once the work is complete.

---

## Broken Trunk

- A red `main` is the top priority: **fix it or revert the offending commit immediately**.
- Prefer reverting (`git revert <sha>` through a PR) when the fix is not obvious and quick.
- Do not merge other changes onto a broken `main`.

---

## Relationship with Worktrees

The worktree rule ([worktree-workflow.md](worktree-workflow.md)) and trunk-based development work
together: **a worktree lives exactly as long as its branch**. Create it from `origin/main` when the
branch starts, and remove it right after the branch is merged. Long-lived worktrees mean
long-lived branches, which this rule forbids.
