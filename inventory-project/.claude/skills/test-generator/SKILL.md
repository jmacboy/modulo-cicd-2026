---
name: test-generator
description: "Trigger: generate tests, create unit tests, write tests for file, test this class, test folder. Generate xUnit unit tests following project rules from project/unit-testing-rules.md."
license: Apache-2.0
metadata:
  author: "jmacb"
  version: "1.0"
---

## Activation Contract

Activate when:

- User asks to generate, create, or write unit tests for a file, class, or folder
- User says "test this", "generate tests", "write tests for"
- User points to a Domain entity, Value Object, Handler, or Factory and wants test coverage

## Hard Rules

1. READ `project/unit-testing-rules.md` FIRST before generating any test. It is the single source of truth.
2. When `.codegraph/` exists for the project and the `codegraph_explore` tool (or `codegraph` CLI) is available, prefer it over raw Read/Grep/Glob to understand the source file(s): resolve constructor dependencies, callers/callees, and related symbols in one call instead of a manual search. Fall back to Read/Grep/Glob only when CodeGraph is unavailable or unindexed for that path.

## Decision Gates

| Source Type               | Test Location                                                     | Layer Rules            |
| ------------------------- | ----------------------------------------------------------------- | ---------------------- |
| Domain Entity / Aggregate | `Inventory.Tests/Domain/{Aggregate}/`                             | D1–D10 from rules      |
| Value Object              | `Inventory.Tests/Domain/{ValueObject}/`                           | D1–D10, no mocking     |
| Handler (IRequestHandler) | `Inventory.Tests/Application/{Feature}/{UseCase}/`                | A1–A11 from rules      |
| Factory                   | `Inventory.Tests/Domain/{Aggregate}/` or `Application/{Feature}/` | F1–F3 from rules       |
| Folder of files           | Generate one test class per file, same layer rules                | Match source structure |

## Execution Steps

1. Read `project/unit-testing-rules.md` completely.
2. Identify the source file(s) or folder to test.
3. Determine the layer: Domain, Application, or Factory.
4. Read the source file(s) — understand public methods, constructor dependencies, guard clauses, state mutations. Use `codegraph_explore` when available to also pull in the exact shapes of injected interfaces/collaborators (e.g. repository, unit of work) so mocks match real signatures.
5. Generate test class(es) following the naming and structure rules.
6. Place tests in the correct `Inventory.Tests/` subfolder mirroring the source structure.
7. For folders: iterate every `.cs` file, skip interfaces (`I*.cs`), skip DI registration files, skip `Program.cs`.
8. Run `dotnet test` to verify compilation and pass rate.

## Output Contract

Return:

- List of files created with full paths
- Number of test methods generated per file
- Which rules from `unit-testing-rules.md` were applied (D1–D10, A1–A11, F1–F3)
- Any `[Theory]` consolidations made (3+ similar facts collapsed)
- `dotnet test` results if run

## References

- `project/unit-testing-rules.md` — full testing rules, examples, anti-patterns, and conventions
