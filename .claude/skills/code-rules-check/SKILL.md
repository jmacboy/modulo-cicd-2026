---
name: code-rules-check
description: "Trigger: before any git push, pre-push check, code rules scan, SonarQube rules. Scan files changed in unpushed commits against project/code-rules.md, fix and commit."
license: Apache-2.0
metadata:
  author: "jmacboy"
  version: "1.1"
---

## Activation Contract

Load this skill:
- before every `git push` in this repository, by any agent, with no exception;
- when the user asks to check changed code against the code rules.

## Hard Rules

- Read `project/code-rules.md` in full on every run. Never rely on a cached or remembered rule list: rules are added often.
- Scan only files changed on the branch versus `origin/main`; do not report pre-existing code in untouched files.
- Apply a rule only to the files its section targets (e.g. `JavaScript / React (react-client)` rules apply to `react-client/**/*.{js,jsx}`).
- Report every violation with `path:line`, the rule ID, and its SonarQube source.
- Fixed violations are staged and committed before the push: one Conventional Commit, `fix(<scope>): apply code rules <IDs>`, with no AI attribution. Stage only the files you fixed.
- Never push while a violation remains, unless the user explicitly accepts that exact violation.
- Fix only inside the branch's authorized scope; never weaken a rule or edit `code-rules.md` to make code pass.
- Test files under `inventory-project/` stay with `test-writer`; report their violations instead of editing them.
- Never bypass hooks (`--no-verify`, `HUSKY=0`) when committing fixes.

## Decision Gates

| Result | Action |
|--------|--------|
| No violations | Report `PASS` and continue to the push |
| Violations in files you are authorized to edit | Fix, run format/lint for those files, scan again, commit the fixes |
| Violations outside your scope or in protected test files | Report `FAIL` and stop for the user |
| Uncommitted changes in the worktree | Warn: they are not part of the push and are not scanned |
| No rule section matches a changed file | Report it as `not covered`; not a failure |

## Execution Steps

1. Read `project/code-rules.md`.
2. `git fetch origin`, then list files changed in the commits to push: `git diff --name-only origin/main...HEAD`. Drop deleted files.
3. Map each file to the rule sections that target it.
4. Read each matching file (changed hunks first, plus enough context) and check every applicable rule.
5. Apply the decision gate.
6. After fixes, repeat steps 2–5 once; if violations remain, report `FAIL`.
7. If anything was fixed, `git add <fixed files>` and commit (`fix(<scope>): apply code rules <IDs>`); the Husky `pre-commit` hook must pass.

## Output Contract

```
code-rules-check: PASS | FAIL
Rules read: JS-1..JS-n (from project/code-rules.md)
Range: origin/main...HEAD (<n> commits)
Files scanned: <n> (<n> not covered)
Violations:
- <path>:<line> <rule ID> (<SonarQube key>): <one-line reason> — fixed | open
Fix commit: <sha> <message> | none
```

## References

- `project/code-rules.md` — the rules (single source of truth).
- `project/git-conventions.md` — push flow that requires this check.
