---
name: pact-generator
description: 'Trigger: pact test, generate pact, contract test, consumer pact, Pact.js. Generate a new Pact consumer test for a react-client service/feature following project/pact-rules.md.'
license: Apache-2.0
metadata:
    author: 'jose-alvarez'
    version: '1.0'
---

## Activation Contract

Load when the user asks to generate, create, or add a Pact consumer test, contract test, or pact coverage for a service/feature in this react-client project.

## Hard Rules

- Read `project/pact-rules.md` in full at the start of every run, every time — never rely on memory, a summary, or a previous read of it. If it's missing, stop and tell the user; never invent pact conventions.
- Follow every rule in `project/pact-rules.md` exactly as written there. Do not paraphrase, skip, or override it with generic Pact knowledge — that file is the single source of truth for structure, matchers, assertions, and fixtures.
- Never proceed without a target service or feature. If not given, ask exactly one question, then wait — do not guess.

## Decision Gates

| Condition                                   | Action                                                                                                                                                              |
| ------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `.codegraph/` exists at project root        | Use `codegraph_explore` (or CLI: `codegraph query`/`explore`/`node`/`files`/`callers`) to locate the target service, its HTTP calls, and any existing spec/fixtures |
| `.codegraph/` missing                       | Use Glob (`src/services/**`, `src/tests/**`) + Grep + Read instead                                                                                                  |
| A spec file for this service already exists | Extend it with new `describe`/`it` blocks, don't duplicate                                                                                                          |
| No spec file exists yet                     | Create `src/tests/<Service>.spec.js` mirroring the existing pattern referenced in `project/pact-rules.md`                                                           |

## Execution Steps

1. Read `project/pact-rules.md` in full.
2. Ask which service/feature to target (skip if already stated); wait for the reply.
3. Check for `.codegraph/`; locate the service's client code, its HTTP calls (method, path, headers, body shape), and existing spec/fixtures via the matching path in the Decision Gates table.
4. Draft the interaction(s) and fixtures strictly per `project/pact-rules.md`.
5. Write or extend the spec file.
6. Present the diff/summary before calling it done.

## Output Contract

Report: files created/modified, discovery method used (codegraph vs. fallback), and confirmation each added interaction was checked against `project/pact-rules.md`.

## References

- `project/pact-rules.md`
