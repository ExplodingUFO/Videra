# Phase 223 Context: Doctor Evidence Packet Validation

## Goal

Validate that repo-only `Videra Doctor` produces an actionable evidence packet before and after opt-in validators run.

## Assumptions

- Doctor remains repository-only and non-mutating.
- Doctor should reference existing validators and artifacts, not duplicate package, benchmark, consumer smoke, native validation, or release dry-run logic.
- Missing release-readiness artifacts are reportable evidence state, not a reason for default Doctor to fail.

## Relevant Files

- `scripts/Invoke-VideraDoctor.ps1`
- `docs/videra-doctor.md`
- `tests/Videra.Core.Tests/Repository/VideraDoctorRepositoryTests.cs`

## Success Criteria

1. Default Doctor output records repository, machine, package-contract, validation-script, and support-artifact availability.
2. Opt-in validation entries expose `pass`, `fail`, `skip`, and `unavailable` states with exact scripts, prerequisites, logs, and artifacts.
3. Doctor correlates release dry-run, package validation, benchmark, consumer-smoke, native-validation, and demo-support artifacts without reimplementing validators.
4. Repository tests cover the evidence packet shape.
