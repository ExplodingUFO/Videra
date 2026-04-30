# Phase 417 Plan: No-Compat Guardrails and CI Truth

## Success Criteria

1. Scope script catches the old-control/direct-Source/fallback/downshift shapes
   identified by Phase 414 while preserving intentional negative wording.
2. CI and release-readiness truth tests require current SurfaceCharts cookbook,
   generated roadmap, scope guardrail, and packaged consumer-smoke checks.
3. Focused tests and scripts pass honestly without introducing compatibility or
   fake fallback behavior.

## Workstream A: `Videra-5j5`

Branch/worktree: `agents/v262-phase417-guardrails` in
`.worktrees/v262-phase417-guardrails`.

Write scope:

- `scripts/Test-SnapshotExportScope.ps1`
- focused tests that assert the script contract, if needed

Validation:

- `pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1`
- focused repository tests covering snapshot scope guardrails, if changed

Handoff:

- report forbidden patterns added to the script;
- report any intentional exclusions to avoid false positives on negative
  guardrail docs.

## Workstream B: `Videra-raj`

Branch/worktree: `agents/v262-phase417-ci-truth` in
`.worktrees/v262-phase417-ci-truth`.

Write scope:

- `.github/workflows/ci.yml`
- `scripts/Invoke-ReleaseReadinessValidation.ps1`
- `tests/Videra.Core.Tests/Repository/SurfaceChartsCiTruthTests.cs`
- `tests/Videra.Core.Tests/Repository/ReleaseDryRunRepositoryTests.cs`
- `tests/Videra.Core.Tests/Repository/SurfaceChartsReleaseTruthRepositoryTests.cs`
  if publish/release needs pinning is missing

Validation:

- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~SurfaceChartsCiTruthTests|FullyQualifiedName~ReleaseDryRunRepositoryTests|FullyQualifiedName~SurfaceChartsReleaseTruthRepositoryTests" --no-restore`

Handoff:

- report which CI/release gates are now pinned;
- report any skipped heavyweight runtime command as intentionally checked by
  repository truth tests instead of executed in this phase.

## Integration

After both workstreams land, integrate code changes into `master`, close
`Videra-5j5`, `Videra-raj`, and `Videra-r9q`, update roadmap/state, regenerate
`docs/ROADMAP.generated.md`, and run composed validation before advancing to
Phase 418.
