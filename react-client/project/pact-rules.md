# Contract test rules (Pact) — react-client

Based on the pattern established in `src/tests/ItemService.spec.js` and
`src/tests/PactResponses.js`. Use this document as a checklist when writing
new consumer pacts in this project.

## 1. Test file structure

- One test file per consumer service (`<Service>.spec.js`), located in
  `src/tests/`.
- A single `PactV3` instance per file, created once in the root `describe`
  with explicit `consumer` and `provider`:

    ```js
    const provider = new PactV3({ consumer: 'react-client', provider: 'inventory-service' });
    ```

- Nest `describe` blocks by feature (e.g. "get item list", "Add an item")
  and one `it` per concrete HTTP interaction. Never mix two different HTTP
  calls in the same `it`.

## 2. Defining the interaction

Every interaction follows the same fixed order:

1. `provider.given(<state>, <optional parameters>)`
2. `.uponReceiving(<interaction description>)`
3. `.withRequest({...})`
4. `.willRespondWith({...})`

Rules:

- `given()` describes a **provider state**, not the request. If the state
  needs concrete data (e.g. a fixed id), pass it as the second argument —
  never hardcode that data into the text description:

    ```js
    provider.given('create item', { id: crearItemRequestBody.id });
    ```

- `withRequest` must declare `method`, `path`, and `headers` whenever the
  request uses them (e.g. `Content-Type` on a POST with a body). Don't omit
  headers that the real service sends.
- `willRespondWith` always declares `status` and `headers` for the response,
  not just the `body`.

## 3. Fixtures and matchers — never bare literals

- Request/response bodies live in a separate file (`PactResponses.js`), not
  inline in the test. The test imports already-built constants.
- Every expected response is built with `MatchersV3`
  (`like`, `eachLike`, `uuid`, `integer`, `string`, etc.), never with bare
  literal values, unless that literal is a real, stable part of the contract
  (e.g. a request id the consumer itself controls).
- For lists, use `eachLike(template, min)` instead of a fixed array. This
  tells the provider "at least one element with this shape," not "exactly
  these elements."
- The body of a **request** the consumer builds can be a literal
  (`crearItemRequestBody`), because the consumer controls those values. The
  body of a **response** almost never should be, because the provider decides
  those values.

## 4. Assertions — verify shape, not provider values

This is the most important rule, and the easiest one to break:

- Verify **type and shape** (`to.be.a('string')`, `to.be.a('number')`,
  `to.have.all.keys(...)`), not specific values the provider could change
  without breaking the contract.
- `to.have.all.keys(...)` is mandatory on the first element of any returned
  collection, to catch schema drift (added or removed fields) as soon as it
  appears.
- Never assume equality between a value you sent in the request and a value
  the response returns, unless the contract explicitly guarantees that
  equality. Real example from this file: an id returned by a POST is
  validated with a GUID-format regex, not by comparing it against the id that
  was sent — that equality was a coincidence of the fixture, not a real
  guarantee.
- Don't assert exact list cardinality (`.to.have.lengthOf(3)`) when the
  contract only promises "not empty." Use
  `.to.be.an('array').that.is.not.empty`.
- Keep any assertion comment that explains a non-obvious decision (why
  something is deliberately NOT asserted) — that's the kind of comment worth
  having.

## 5. Running the test

- Always `return provider.executeTest(async mockServer => {...})` — the
  `return` is mandatory so Mocha waits for the promise; forgetting it lets
  the test pass even when it actually fails.
- Instantiate the real service (`ItemService(mockServer.url)`) inside the
  `executeTest` callback, pointing at the mock server, never at a hardcoded
  URL.
- Only one `await`/`.then()` on the service call per test.

## General best practices for new pacts

- **One contract, one intent**: each interaction tests a single thing the
  consumer needs from the provider. If you need two scenarios (ok / error),
  that's two separate interactions, each with its own `given`.
- **Descriptive, reusable provider states**: name the state in business terms
  ("an item with stock exists", not "test1"), so the provider team can map it
  to real verification data.
- **Don't test provider logic**: the pact test verifies that the consumer
  knows how to parse the response, not that the provider's business logic is
  correct. That's covered by provider-side verification.
- **Minimize coupling to implementation details**: don't assert headers,
  field order, or internal formatting the client code doesn't actually
  consume.
- **One central fixtures file per domain**: if more entities appear besides
  `Item`, consider splitting `PactResponses.js` into per-entity files instead
  of one ever-growing file.
- **Version and publish generated pacts** (`pacts/*.json`) to the Pact
  Broker as part of the pipeline, so the provider verifies them in CI before
  deploying.
- **Never hand-edit the generated pact JSON**: if the contract needs to
  change, the change happens in the test, never in the resulting
  `pacts/*.json` file.
- **Name `it()` blocks in business language**, not technical language:
  describe what the contract guarantees for the consumer, not how it's
  implemented.
