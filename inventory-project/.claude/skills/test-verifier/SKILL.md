---
name: test-verifier
description: "Trigger: verify tests, test verification, check test rules, validate tests, verify test compliance. Verify generated tests against unit-testing-rules.md — static analysis + execution report."
license: Apache-2.0
metadata:
  author: "jmacb"
  version: "1.0"
---

## Activation Contract

Activate when:

- User asks to verify, validate, or check tests against project rules
- User says "verify tests", "check test rules", "validate test compliance"
- User points to test files or folders and wants a compliance report
- Typically runs after `test-generator`

## Hard Rules

1. ALWAYS read `project/unit-testing-rules.md` first — it is the single source of truth.
2. Run static analysis BEFORE test execution.
3. Report violations as `{rule_id}: {description} → {file}:{line}`.
4. NEVER modify files — read-only operation.
5. `[Theory]` with 3+ similar `[Fact]`s is a recommendation, NOT a failure.
6. When `.codegraph/` exists for the project and the `codegraph_explore` tool (or `codegraph` CLI) is available, prefer it over raw Read/Grep/Glob to resolve the test file(s), the class under test, and its collaborators (e.g. confirming mocked interfaces match real signatures, checking for unmocked SUT calls). Fall back to Read/Grep/Glob only when CodeGraph is unavailable or unindexed for that path.

## Decision Gates

| Input | Action |
|-------|--------|
| Single `.cs` file | Analyze that file only |
| Folder | Glob `**/*Test.cs` and analyze all matches |
| Layer not detectable from path | Ask the user to specify (Domain / Application / Factory) |
| `unit-testing-rules.md` missing | Stop and report — cannot verify without rules |

## Execution Steps

1. Read `project/unit-testing-rules.md`. Extract rules per layer (D*, A*, F*, T*) and anti-patterns.
2. Resolve input path. If folder, collect all `*Test.cs` files. Use `codegraph_explore` when available to locate the class under test and its collaborators quickly.
3. Detect layer per file by path convention:
   - `Domain/` → Domain rules (D1–D10)
   - `Application/` → Application rules (A1–A11)
   - Factory files → Factory rules (F1–F3)
4. **Static analysis** — for each test file, check:
   - Naming: class `{ClassUnderTest}Test`, method `{Method}_{Scenario}`
   - Structure: AAA pattern with `//Arrange`, `//Act`, `//Assert` comments
   - Mocking: no mocking of SUT; mock only collaborators
   - Assertions: `Assert.Throws<DomainException>` + error message check (D7)
   - Anti-patterns: no `Assert.True(a == b)`, no try/catch, no `UnitTest1.cs`
   - Data-driven: `[Theory]` requires `[InlineData]` or `[MemberData]`
5. **Execution** — run `dotnet test` (optionally with `--filter` for targeted namespace). Capture stdout/stderr.
6. Parse results: passed / failed / skipped counts + error messages for failures.
7. Generate consolidated report (see Output Contract).

## Output Contract

Return a report with these sections:

**Static Analysis** — table of rule checks with Status (✅/❌), File, Line, and Detail.

**Execution Results** — tests run, passed, failed, skipped counts. List failed tests with error messages.

**Recommendations** (non-blocking) — e.g., "N Facts could collapse to 1 Theory with InlineData".

## References

- `project/unit-testing-rules.md` — project-specific rules (read at runtime, never duplicated)
