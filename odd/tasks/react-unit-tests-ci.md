# React unit tests + unified CI pipeline

## Objective

Add unit tests with coverage to `react-client`, and replace the two separate workflows
(`dotnet.yml`, `node.js.yml`) with one pipeline that has two jobs: `frontend` first, then
`backend`. Each job produces its coverage report; the backend job sends both coverages to
SonarCloud in a single analysis, and coverage reports are uploaded as GitHub artifacts.

## Why

One SonarCloud project (`jmacboy_modulo-cicd-2026`) should show coverage for both the .NET
service and the React client. Running the frontend first lets the backend job's Sonar scan
reuse the frontend LCOV report.

## Scope / constraints

- Vitest + Testing Library + jsdom, coverage via `@vitest/coverage-v8` (lcov + html).
- Vitest must not pick up the Pact specs in `src/tests/**` (Mocha keeps running them).
- Do not change application behavior; follow `project/code-rules.md`.
- Workflow: `frontend` job uploads LCOV + HTML report; `backend` job `needs: frontend`,
  downloads the LCOV, passes it to `dotnet sonarscanner begin`, and uploads its HTML report.
- Route: delegated direct (2+ non-trivial files: test setup + several test files + workflow).

## Tasks

- [x] T1 Vitest setup + unit tests for `getErrorMessage`, `ItemService`, `App`, `ItemList`,
      `AddStockDialog`; `yarn test` / `yarn test:coverage` scripts.
- [x] T2 Unified workflow `.github/workflows/ci.yml`; remove the old two workflows.

## Acceptance criteria

- `yarn test:coverage` passes locally and writes `react-client/coverage/lcov.info`.
- `yarn test:pact`, `yarn lint`, `yarn build` still pass.
- `ci.yml` parses as valid YAML; frontend runs before backend; artifacts uploaded for both.

## Progress / evidence

### T1 (route: delegated writer)

- Vitest 5.1 + jsdom 31 + Testing Library (react 16, dom 10, user-event 14, jest-dom 7).
  Config in `react-client/vitest.config.js` (merges `vite.config.js`); setup in
  `react-client/vitest.setup.js` (jest-dom matchers plus a test-only `<dialog>` polyfill,
  because jsdom has no `showModal`/`close`). Tests are co-located as `src/**/*.test.{js,jsx}`;
  the Mocha Pact specs in `src/tests/**` are not picked up.
- Evidence: `yarn test:coverage` 5 files / 56 tests passed; Statements 100% (155/155),
  Branches 100% (38/38), Functions 100% (71/71), Lines 100% (149/149). `yarn test:pact`
  7 passing; `yarn lint` clean; `yarn build` ok.
- No application code changed.
- Commit: `475e083`.

### T2 (route: delegated writer)

- `.github/workflows/ci.yml` replaces `dotnet.yml` and `node.js.yml`. Job `frontend` runs lint,
  build, `test:coverage`, and `test:pact`, then uploads `frontend-coverage-lcov` and
  `frontend-coverage-report`. Job `backend` (`needs: frontend`) downloads the LCOV, rewrites
  `SF:src/...` to `SF:react-client/src/...`, and passes it to Sonar via
  `sonar.javascript.lcov.reportPaths`. It also extends the exclusions and uploads
  `backend-coverage-report`.
- Rebased on main after #10 (root `.config/dotnet-tools.json` pins `dotnet-sonarscanner` and
  `reportgenerator`): `ci.yml` runs `dotnet tool restore` at the repo root before Sonar begin and
  uses `dotnet sonarscanner` / `dotnet tool run reportgenerator`. The scanner cache and
  `--tool-path` install are dropped; the Sonar packages cache uses the Linux path `~/.sonar/cache`.
- Evidence: `npx yaml-lint .github/workflows/ci.yml` succeeded; the SF rewrite was checked on a
  sample LCOV. The pipeline has not run on GitHub yet.
