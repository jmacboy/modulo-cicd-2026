# Feature: Item update

## Objective
Allow updating an existing `Item` from the API and from the React client.

## Problem / Why
Items can be created, listed, and soft-deleted, but a typo in the name can only be fixed by deleting and recreating the item, which loses its stock history.

## Scope
- Domain: `Item.Rename(string newName)` (name required; fails on a deleted item).
- Application: `Items/UpdateItem/UpdateItemCommand` + `UpdateItemHandler` (NotFound when missing, maps `DomainException` to a failed Result, persists via `UpdateAsync` + `CommitAsync`).
- WebApi: `PUT /api/Item/{id}` with body `{ "name": "..." }`.
- Client: `ItemService.updateItem(id, item)` and an inline edit (Edit / Save / Cancel) in `ItemList.jsx` with an error message on failure.
- Tests: unit (domain + handler) and integration via `test-writer`; Pact consumer test via `pact-workflow`.

## Constraints
- Only the name is editable. Stock, reserved, and unitary cost are driven by transactions.
- Clean Architecture layers stay intact; test files are only written by the designated subagents.

## Acceptance criteria
- `PUT /api/Item/{id}` with a valid name returns 200 and the new name is returned by `GET /api/Item`.
- Empty name returns 400; unknown or deleted id returns 404.
- The client lets the user rename an item in the list and shows the API error when it fails.
- Solution builds; unit, integration, and Pact tests pass; lint passes.

## Tasks
- [x] T1 Backend production code (domain, command/handler, controller) — route: delegated (writer trigger, 3+ non-trivial files). `dotnet build`: 0 errors (parent spot check).
- [x] T2 Backend tests (unit + integration) — route: delegated (general-purpose agent following `test-writer.md`). +3 domain, +3 handler, +3 integration (10 cases with theory). `dotnet test Inventory.Tests`: 30/30 (parent spot check); `Inventory.IntTests`: 7/7. Integration rename test reads back through the fake repo because GET uses the separate EF InMemory `PersistenceDbContext`.
- [x] T3 Client: `updateItem` service + inline edit UI — route: delegated (same writer as T1). eslint on changed files OK, `yarn build` OK, prettier OK. Note: full `yarn lint` already fails on `react-client/.claude/hooks/pact-prompt-router.js` (`process` undefined), unrelated.
- [x] T4 Pact consumer test for `updateItem` — route: delegated (general-purpose agent following `pact-workflow.md`). 3 PUT interactions (200/400/404). `yarn test:pact`: 6 passing (parent spot check). Exception to pact-rules §3: 404 text/plain body is a literal because PactV3 serializes `like()` on non-JSON bodies.
- [x] T5 Full verification + work-unit commits. Commits: `e01a3e2` feat(item), `e54a1f6` feat(client), `a97cf99` docs(project). Native review: assessed high, consent declined by user (candidate-scoped). code-rules-check: PASS (JS-1..JS-4, 6 files). Pre-push hook: unit 30/30. Pushed `feat/item-update` to origin (2026-11-07).

- [x] T6 (user request) Repo-wide `project/code-rules.md` with rule JS-1 (SonarQube `javascript:S9383`, handle every promise), indexed in `project/README.md`; fixed `ItemList.jsx` `getItemList()` missing `.catch` — route: inline (1 doc + 1 mechanical edit). prettier/eslint/`yarn build` OK.
- [x] T7 (user request) Fixed `App.jsx` `addItem` missing `.catch` (JS-1, shows `role="alert"` error); extracted shared `src/utils/getErrorMessage.js` used by `App.jsx` and `ItemList.jsx`. Added rule JS-2 (SonarQube `javascript:S6582`, optional chaining) and fixed `ItemList.jsx` `itemList && itemList.map` → `itemList?.map`. eslint/prettier/`yarn build` OK, `yarn test:pact` 6 passing. No other `a && a.b` guards in `src/`.
- [x] T8 (user request) Added rule JS-3 (SonarQube `javascript:S2259`, null dereference). Fixed `ItemList.jsx` at the source: `useState([])` instead of `useState()`, `setItemList(items ?? [])`, so `curr.filter`/`curr.map` (lines 63 and 90) are safe; `itemList?.map` back to `itemList.map`. eslint/prettier/`yarn build` OK; no `useState()` left in `src/`.
- [x] T9 (user request) Added rule JS-4 (SonarQube `javascript:S9379`, no `autoFocus`). `ItemList.jsx`: removed `autoFocus`, focus moves via `editInputRef` in a `useEffect` on `editingId` (only after the user clicks Edit). eslint/prettier/`yarn build` OK; no `autoFocus` left in `src/`.
- [x] T10 (user request) Pre-commit code rules scan: new project skill `.claude/skills/code-rules-check/SKILL.md` (reads `project/code-rules.md` fresh every run, scans changed files, PASS/FAIL report, blocks commit on open violations). Registered as a MUST in `AGENTS.md` and in `project/git-conventions.md` (Commits). Dry run on this branch: PASS (6 JS files, JS-1..JS-4). JS-1 clarified to accept promises returned to a handling caller (Pact tests). Not enforced by Husky: the check is LLM-judged.
  - Revised (user): runs before every **push**, not every commit; scans `origin/main...HEAD`; fixes are staged and committed as `fix(<scope>): apply code rules <IDs>` (skill v1.1).

## Delivery
Strategy: `ask-on-risk`. Forecast: ~300 authored changed lines (single PR).

## Progress / Evidence
- Branch: `feat/item-update`, worktree `../repo-worktrees/item-update`.

- Contract: `PUT /api/Item/{id}` body `{ "name" }` → 200 `Result<bool>`; 400 `Error` (`NameIsRequired`, `CannotUpdateDeletedItem`); 404 string when unknown or soft-deleted (global query filter in `ItemConfig` applies to `FindAsync`).

## Next step
Open a PR to `main` (human decision), merge by squash/rebase, then remove the worktree.
