# Verify Script Relocation Design

## Goal

Move the repository PowerShell verification entrypoint from the repository root into `scripts/verify.ps1`, update all first-party callers to the new location, and improve failure diagnostics so CI logs identify the failing test project and emit machine-readable results for later inspection.

## Approved Decisions

- Do not keep a root-level `verify.ps1` shim.
- Treat the relocation as an allowed breaking change and update every first-party caller in one pass.
- Keep the existing repository-level behavior: build, full test run, demo builds, and optional native validation switches.
- Improve diagnostics in the PowerShell path without turning `verify.ps1` into a god script. The script remains an orchestrator around discrete checks.

## Architecture

### Entrypoint Boundary

`scripts/verify.ps1` becomes the only PowerShell verification entrypoint. The script computes the repository root from `PSScriptRoot`, so moving it under `scripts/` does not require hard-coded absolute paths. All callers, repository guard tests, and public docs must point at `scripts/verify.ps1`.

### Diagnostics Boundary

The script should keep the existing high-level steps, but test execution must become more diagnosable:

- write `trx` files into a deterministic artifacts directory under the repository root;
- run test commands with enough console detail to surface the failing test assembly and failing test name in CI;
- print the `trx` location when tests fail so a human or follow-up automation can inspect exact failures.

This keeps failure analysis in the verification layer instead of leaking ad hoc logging into workflows.

### Repository Guarding

Repository tests that assert script paths and workflow conventions must move with the new contract. The repository architecture tests should guard:

- `scripts/verify.ps1` exists and contains the expected demo build targets;
- workflows and native validation wrappers call `scripts/verify.ps1`;
- public docs and contribution templates point to the new path.

## Error Handling

- Unknown arguments must still fail fast.
- Build, test, demo, and native validation failures must still contribute to the final non-zero exit code.
- On test failure, the script should emit a clear summary including the step name and the `trx` output location.

## Testing Strategy

- TDD on repository guard tests first so the old root path fails immediately.
- Targeted verification for repository tests after each task.
- Full `pwsh -File .\scripts\verify.ps1 -Configuration Release` verification at the end.

## Risks

- Missed doc or workflow references will leave the repo inconsistent.
- CI-only flaky tests are not fixed by the relocation itself, so diagnostics must make the next failure attributable.
