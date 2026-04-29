# Phase 393 Context: Release Validation Script and CI Alignment

## Scope

Implement one local v2.57 release-readiness validation command for package build, SurfaceCharts packaged consumer smoke, focused SurfaceCharts/demo-doc tests, and snapshot export scope guardrails.

## Boundaries

- Own script-facing validation files under `scripts/`, CI wiring, and focused repository tests that describe the script contract.
- Do not edit README/demo/migration/support docs owned by Phase 394.
- Do not edit runtime `src/` code unless validation cannot run without a tiny direct fix; no runtime fix was needed.
- Do not edit shared `.planning/STATE.md`, `.planning/ROADMAP.md`, `.planning/REQUIREMENTS.md`, `.beads/issues.jsonl`, or `docs/ROADMAP.generated.md`.

## Existing Surfaces

- `scripts/Invoke-ReleaseDryRun.ps1` already packs public packages and runs package validation.
- `scripts/Invoke-ConsumerSmoke.ps1 -Scenario SurfaceCharts` already proves packaged SurfaceCharts consumer smoke and support artifacts.
- `scripts/Test-SnapshotExportScope.ps1` already verifies snapshot export scope guardrails.
- `.github/workflows/release-dry-run.yml` was the CI entry for package-only dry run evidence.