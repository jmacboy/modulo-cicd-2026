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
- T1 done (delegated writer). RED: 9/9 `AddStockHandlerTest` failed against a `NotImplementedException` stub; `GetItems_WithSeededItem_ReturnsUnitaryCost` failed (expected 7.25, actual 0) without the mapping fix. GREEN: `dotnet build` succeeded (0 errors, pre-existing warnings only); `dotnet test Inventory.Tests` 31/31 passed; `dotnet test Inventory.IntTests` 8/8 passed. Added `ItemErrors.NonNegativeCost`, new `TransactionErrors.CreatorIsRequired`, `WebApi/Contracts/AddStockRequest`, IntTests `InMemoryTransactionRepository` + `AddStockResponse`. Commit: `439609c` (feat(item): add stock endpoint via entry transaction). Native review: medium risk, declined by user for this candidate.
- T2 done (delegated writer). RED: `yarn test:pact` 3 passing, 1 failing (`TypeError: itemService.addStock is not a function`). GREEN: `yarn test:pact` 4/4 passing, pact file regenerated with the `agregar stock a un item` interaction; `yarn build` succeeded; `npx eslint src` clean. The pre-existing `yarn lint` failure in `react-client/.claude/hooks/pact-prompt-router.js` (Node globals) was fixed in a separate commit `50951c0` (chore, authorized by the user); `yarn lint` now passes. No component tests exist in the repo, so the UI is checked structurally (lint + build) only. Added `AddStockForm.jsx` (quantity/unitary cost/user GUID with labels, `role="alert"` errors, list re-fetch on success) and Stock/Available/Unitary cost columns in `ItemList.jsx`. 404 interaction skipped: the backend returns a plain string body whose content type was not verified.

## Next step

Human: live test, push and open PR.
