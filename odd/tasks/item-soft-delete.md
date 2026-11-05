# Feature: Item soft delete

## Objective
Replace the hard delete of `Item` with a soft delete: deleted items stay in the database but are hidden from every read.

## Problem / Why
`DeleteItemHandler` calls `ItemRepository.DeleteAsync`, which runs `DbContext.Remove` and physically deletes the row. That loses history and can break references from transactions.

## Scope
- Domain: `Item.IsDeleted`, `Item.DeletedAt`, `Item.Delete()` (fails if already deleted).
- Application: `DeleteItemHandler` loads the tracked item, calls `Delete()`, persists via `UpdateAsync`.
- Repository: remove hard `DeleteAsync` from `IItemRepository` / `ItemRepository`.
- Infrastructure: map new columns (`isDeleted`, `deletedAt`) in `ItemConfig` and `ItemPersistenceModel`; global query filter `!IsDeleted` in both `DomainDbContext` and `PersistenceDbContext`; EF migration `AddItemSoftDelete`.
- Tests (via `test-writer` subagent, per project CLAUDE.md): domain + handler unit tests, in-memory repo update.

## Constraints
- Clean Architecture layers stay intact (domain has no EF knowledge).
- Test files are only written by the `test-writer` subagent.

## Acceptance criteria
- `DELETE /item/{id}` returns success and the row remains with `isDeleted = true`, `deletedAt` set.
- Deleted items are not returned by `GetItems` nor `GetByIdAsync`; deleting again returns NotFound.
- Solution builds and all tests pass.

## Tasks
- [x] T1 Production code (domain, handler, repository, EF config, query filters, migration) — route: delegated (2+ non-trivial files, writer trigger); reserved-stock rule + front error message added inline
- [x] T2 Tests for soft delete — route: delegated (general-purpose agent following `test-writer.md`, since the agent type was not registered at repo root). Verifier: PASS. +4 domain facts, +3 handler facts, InMemoryItemRepository hides deleted items.
- [x] T3 Build + full test run, work-unit commit. `dotnet test Inventory.sln`: 22/22 unit, 4/4 integration (parent spot check). Feature commit: `feat(item): soft delete items...`. Hook fixed first in `30761a1` (per-package lint-staged configs).

## Progress / Evidence
- Branch: `feat/item-soft-delete`
- T1 code written (not committed). `dotnet build Inventory.sln`: 0 errors (parent spot check). Migration `20261031154640_AddItemSoftDelete` adds `isDeleted` (bool, default false) and `deletedAt` (nullable) only.
- Decision (user, 2026-10-31): block deletion when `Reserved > 0` (`ItemErrors.CannotDeleteWithReservedStock`); `DeleteItemHandler` maps the `DomainException` to a failed Result (400). EF warning 10622 left as is: the domain `TransactionItem` has no navigation to `Item`, so runtime transaction queries are unaffected; only the persistence (migration) model has the navigation.
- Scope addition (user): front-end delete button. It already existed in `react-client/src/ItemList.jsx`; added an error message (`role="alert"`) when the API rejects the delete. eslint OK, `vite build` OK.
- T2 note: `InMemoryItemRepository` still has a leftover `DeleteAsync` and does not filter out deleted items.

## Next step
T2 (tests via test-writer), then T3.
