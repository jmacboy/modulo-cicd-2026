# Feature: item-add-stock

## Objective

Let users add stock to an item from the react-client through a new `ItemController` endpoint.

## Problem / Why

Stock can only be increased today by calling the generic `TransactionController` twice (create + complete).
There is no item-centric endpoint or UI for it.

## Design decisions

- `POST /api/Item/{id}/stock` creates an **Entry** `Transaction` with the item and completes it in the same
  use case. `UpdateStockWhenTransactionCompleted` applies `Item.AddStock` during `CommitAsync`. Item stock
  is never mutated directly (keeps the transaction audit trail).
- The creator user comes from the request body (`userCreatorId`), consistent with `TransactionController`.
  The UI asks for the user GUID.
- `GetItemsHandler` must map `UnitaryCost` (currently always 0) so the UI shows the averaged cost.

## Scope

- Backend: `Inventory.Application/Items/AddStock/*`, `ItemController`, `GetItemsHandler` mapping, unit + integration tests.
- Frontend: `ItemService.addStock`, add-stock UI in the item list, Stock/Available/Unitary cost columns, Pact interaction.

Out of scope: auth, user lookup UI, routing.

## Constraints

- Handlers return `Result<T>`; domain failures map to 400, missing item to 404.
- Tests are written by the `test-writer` agent (.NET) and `pact-workflow` agent (react-client).
- IntTests `InMemoryUnitOfWork` does not publish domain events, so integration tests assert HTTP contract only.

## Tasks

- [x] T1 Backend: `AddStockCommand` + `AddStockHandler`, `POST api/Item/{id}/stock`, map `UnitaryCost` in `GetItemsHandler`, unit + integration tests. Route: delegated (writer trigger: 3+ non-trivial files).
- [x] T2 Frontend: `ItemService.addStock`, add-stock form in `ItemList.jsx`, stock columns, Pact consumer interaction. Route: delegated (writer trigger: 3+ non-trivial files).
- [x] T3 Frontend: add-stock form opens in a modal dialog (user request). Route: delegated (writer trigger: 2+ non-trivial files).
- [x] T4 Frontend: user-facing errors are client-owned messages mapped from API error `code` / HTTP status; backend descriptions never reach the UI (user request). Route: inline (one mechanical file).
- [x] T5 Backend: unhandled exceptions return a generic ProblemDetails 500 without internal details (user request). Route: delegated (writer trigger: 2+ non-trivial files incl. tests).

## Acceptance criteria

- Valid request returns 200 `{ value: <transactionId> }` and the item's stock and unitary cost update.
- Missing item returns 404; quantity <= 0, negative cost, or empty user returns 400 and nothing is committed.
- UI shows stock, available and unitary cost and refreshes after adding stock; errors appear in `role="alert"`.

## Checks

- `dotnet build`, `dotnet test Inventory.Tests`, `dotnet test Inventory.IntTests` (from `inventory-project/`).
- `yarn lint`, `yarn build`, `yarn test:pact` (from `react-client/`).

## Delivery

Strategy: `ask-on-risk`. Forecast: about 350 authored changed lines, so one PR.

## Progress

- Worktree `../repo-worktrees/item-add-stock` on `feat/item-add-stock` created and bootstrapped.
- T1 done (delegated writer). RED: 9/9 `AddStockHandlerTest` failed against a `NotImplementedException` stub; `GetItems_WithSeededItem_ReturnsUnitaryCost` failed (expected 7.25, actual 0) without the mapping fix. GREEN: `dotnet build` succeeded (0 errors, pre-existing warnings only); `dotnet test Inventory.Tests` 31/31 passed; `dotnet test Inventory.IntTests` 8/8 passed. Added `ItemErrors.NonNegativeCost`, new `TransactionErrors.CreatorIsRequired`, `WebApi/Contracts/AddStockRequest`, IntTests `InMemoryTransactionRepository` + `AddStockResponse`. Commit: `464aa34` (rebased; originally `439609c`) (feat(item): add stock endpoint via entry transaction). Native review: medium risk, declined by user for this candidate.
- T2 done (delegated writer). RED: `yarn test:pact` 3 passing, 1 failing (`TypeError: itemService.addStock is not a function`). GREEN: `yarn test:pact` 4/4 passing, pact file regenerated with the `agregar stock a un item` interaction; `yarn build` succeeded; `npx eslint src` clean. The pre-existing `yarn lint` failure in `react-client/.claude/hooks/pact-prompt-router.js` (Node globals) was fixed in a separate commit `10c764b` (rebased; originally `50951c0`) (chore, authorized by the user); `yarn lint` now passes. No component tests exist in the repo, so the UI is checked structurally (lint + build) only. Added `AddStockForm.jsx` (quantity/unitary cost/user GUID with labels, `role="alert"` errors, list re-fetch on success) and Stock/Available/Unitary cost columns in `ItemList.jsx`. 404 interaction skipped: the backend returns a plain string body whose content type was not verified. Commit: `b8caa2e` (rebased; originally `b99427e`) (feat(react-client): add stock form and contract).
- Rebased onto `origin/main` at `e984ea3` (adds `project/code-rules.md` and item-update: `PUT /api/Item/{id}`, inline rename, `getErrorMessage`). Conflicts resolved keeping both features: `ItemController` has both `UpdateItem` and `AddStock`; `ItemControllerTest` keeps the update and add-stock tests; `ItemList.jsx` keeps main's rename UI, `[]` list state and load-error alert plus the stock columns (the edit row also renders them); Pact spec/fixtures keep both describe blocks; the pact JSON was regenerated with `yarn test:pact` (7 interactions). `AddStockForm` reuses main's `getErrorMessage` instead of its own copy. Each replayed commit was checked: `dotnet build` (0 errors) for T1; `yarn lint`, `yarn build`, `yarn test:pact` (7 passing) for T2.
- T3 done (delegated writer). One native `<dialog>` (`AddStockDialog.jsx`) rendered once in `ItemList`, opened with `showModal()` from an "Add stock" button per row; title "Add stock: <name>"; focus moves to Quantity on open (code-rules JS-4, no `autoFocus`); Escape, Cancel and backdrop click close it; every close resets the form; submit is disabled in flight; errors show in `role="alert"` inside the dialog; success closes it and re-fetches the list. `AddStockForm.jsx` removed. Service and Pact contract unchanged. No component tests exist, so it was verified structurally: `yarn lint` clean, `yarn build` succeeded, `yarn test:pact` 7 passing.
- T4 done (inline). `getErrorMessage` maps known API codes and 404 to client messages, otherwise the per-action fallback; call sites unchanged. No unit test runner for utils (mocha only runs Pact specs), so checked structurally: `yarn lint` ok, `yarn build` ok, `yarn test:pact` 7 passing.
- T5 done (delegated writer). `WebApi/ExceptionHandling/GlobalExceptionHandler` (`IExceptionHandler`) logs the exception with `ILogger` and writes a generic `application/problem+json` 500 (`type`, `title` "An unexpected error occurred.", `status`, `traceId`); no message, type name or stack trace in any environment. Registered with `AddExceptionHandler` + `AddProblemDetails`; `app.UseExceptionHandler()` is the first middleware, so it replaces the automatic Developer Exception Page. CORS headers still reach the client on a 500 (asserted). RED: `CreateItem_WithDuplicateRequest_ReturnsGenericProblemDetails` failed (content type `text/plain`, the developer exception page). GREEN: `dotnet build` 0 errors (alternate `BaseOutputPath`: a running `Inventory.WebApi` locks `bin/`); `dotnet test Inventory.Tests` 39/39; `dotnet test Inventory.IntTests` 12/12. Added IntTests `Setup/ProblemDetailsResponse`.

## Next step

Human: live test, push and open PR.
