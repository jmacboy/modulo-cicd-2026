---
name: pact-checker
description: 'Trigger: verify pact, check pact test, pact diagnostic, audit contract test. Verify generated Pact consumer tests against project/pact-rules.md and report a diagnostic, without editing files.'
license: Apache-2.0
metadata:
    author: 'jose-alvarez'
    version: '1.0'
---

## Activation Contract

Load when the user asks to verify, check, audit, or diagnose whether existing Pact consumer tests comply with the project's pact rules or cover the intended service/feature — typically right after `pact-generator` produced or extended a spec.

## Hard Rules

- Read `project/pact-rules.md` in full at the start of every run, every time — never rely on memory, a summary, or a previous read. If it's missing, stop and tell the user.
- Evaluate compliance strictly against that file as written there; do not add or invent rules of your own.
- Read every target spec and fixtures file in full before judging it — never diagnose from a partial read or from what you assume the file contains.
- Never proceed without knowing the target service or feature. If not given, ask exactly one question, then wait.
- Read-only: never edit, fix, or regenerate any spec or fixtures file. Report findings only; fixing is a separate, explicit request.
- Every finding must cite concrete `file:line` evidence. No bare pass/fail claims without a quoted line or code excerpt backing it.

## Decision Gates

| Condition                            | Action                                                                                                                                                         |
| ------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `.codegraph/` exists at project root | Use `codegraph_explore` (or CLI: `codegraph query`/`explore`/`node`/`files`/`callers`) to find the target service's client code and its existing spec/fixtures |
| `.codegraph/` missing                | Use Glob (`src/services/**`, `src/tests/**`) + Grep + Read instead                                                                                             |
| No spec file found for the target    | Report "no pact test exists for this service/feature" — don't fabricate a diagnostic                                                                           |

## Execution Steps

1. Read `project/pact-rules.md` in full.
2. Confirm or ask which service/feature to verify; wait for the reply if not given.
3. Locate the service's client code and its spec/fixtures files per the Decision Gates table.
4. Check rule compliance per interaction: `given/uponReceiving/withRequest/willRespondWith` order, matchers vs. bare literals, shape-only assertions, fixtures kept in a separate file, `return provider.executeTest(...)`.
5. Check feature coverage: list every HTTP call the service's client code exposes and confirm each has at least one matching interaction in the spec; flag any uncovered call.
6. Assemble the diagnostic: one row per finding, each with `file:line` evidence and a rule reference from `project/pact-rules.md`.

## Output Contract

Return a diagnostic report with: overall verdict (compliant / needs fixes), a rule-compliance table (finding, file:line, rule violated or satisfied), and a coverage table (client HTTP call vs. spec interaction, covered/missing). Never modify files.

## References

- `project/pact-rules.md`
- `.claude/skills/pact-generator/SKILL.md`
