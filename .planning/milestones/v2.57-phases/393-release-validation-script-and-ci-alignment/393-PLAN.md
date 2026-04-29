# Phase 393 Plan: Release Validation Script and CI Alignment

## Goal

Provide a single v2.57 release-readiness validation command that composes the existing release dry run, SurfaceCharts packaged consumer smoke, focused tests, and snapshot scope guardrails while clearly separating pass/fail checks, local environment warnings, and skipped public publish/tag steps.

## Tasks

1. Add a minimal orchestration script that runs existing validation commands instead of duplicating package/smoke logic.
2. Align release dry-run CI to call the new readiness command and upload its readiness artifact root.
3. Add focused repository tests for the new script contract and CI entrypoint.
4. Verify the script wiring in build-only mode, run focused tests, and run snapshot scope guardrails.

## Success Criteria

- One command exists: `scripts/Invoke-ReleaseReadinessValidation.ps1`.
- The command runs package build/validation, SurfaceCharts consumer smoke, focused tests, and snapshot guardrails by default.
- `-ConsumerSmokeBuildOnly` is available for cheap local wiring proof without changing the full default path.
- Summary artifacts separate pass/fail checks, local environment warnings, and skipped public publish/tag steps.
- No public publish, release tag, feed push, or hidden fallback/downshift path is introduced.