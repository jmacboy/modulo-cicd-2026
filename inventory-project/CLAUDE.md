# Project Instructions

## Test delegation

Any request to generate, write, create, fix, or verify tests — unit or integration, for a
file, class, or folder — MUST be delegated to the `test-writer` subagent via the Agent tool.
Do not write or edit test files directly in the main thread. `test-writer` decides which
track applies (unit vs integration) from the request wording and the target's type, then
runs the matching generator + verifier skill pair (`test-generator`/`test-verifier` for
unit, `integration-generator`/`integration-verifier` for integration).

Trigger phrases include (not limited to): "generate tests", "write tests", "create unit
tests", "create integration tests", "test this file/class/folder/controller", "add tests
for X", "haz tests de X", "genera tests", "escribe tests", "genera tests de integración".
