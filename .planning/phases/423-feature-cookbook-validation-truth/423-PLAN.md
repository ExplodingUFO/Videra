# Phase 423 Plan: Feature Cookbook and Validation Truth

## Goal

Align the v2.63 cookbook, demo support evidence, CI truth, and release-readiness
filters with the feature/demo behavior that actually shipped in Phases 420-422.

## Scope

- Cookbook docs and tests must cover only implemented native SurfaceCharts
  features.
- Demo support and CI truth tests must fail on stale filters, success masking,
  or overstated evidence.
- Release-readiness validation must include the new focused checks after the
  cookbook/CI truth checks exist.
- Related docs must avoid compatibility, parity, migration-adapter, old-control,
  hidden fallback/downshift, backend-expansion, or fake-validation claims.

## Beads

### 423A: Cookbook Matrix Recipe Truth

Bead: `Videra-64g.1`

Owner: main workspace.

Write scope:

- `samples/Videra.SurfaceCharts.Demo/README.md`
- `samples/Videra.SurfaceCharts.Demo/Recipes/*.md`
- `docs/surfacecharts-release-cutover.md`
- `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbook*Tests.cs`

Validation:

```powershell
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartsCookbook" --no-restore
git diff --check
```

Handoff:

- Keep rows tied to visible demo paths, concrete recipe files, and focused test
  owners.
- Do not add rows for planned-only features.
- Do not add external-library compatibility or parity wording.

### 423B: Demo Support and CI Truth

Bead: `Videra-64g.2`

Owner: isolated worktree `agents/v263-phase423-ci-truth`.

Write scope:

- `.github/workflows/ci.yml`
- `tests/Videra.Core.Tests/Repository/SurfaceChartsCiTruthTests.cs`
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemo*Tests.cs` only if needed
  for real demo/support behavior
- demo support/service files only if needed for real support-summary truth

Validation:

```powershell
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartsDemo|FullyQualifiedName~SurfaceChartsCiTruthTests" --no-restore
git diff --check
```

Handoff:

- CI must run focused demo/support checks without `continue-on-error`, `|| true`,
  or success-masking `if: always()` in validation steps.
- Artifact upload steps may preserve diagnostics after failures, but must not
  make validation look green.

### 423C: Release Readiness Focused Filter

Bead: `Videra-64g.3`

Owner: main workspace after 423A and 423B merge.

Write scope:

- `scripts/Invoke-ReleaseReadinessValidation.ps1`
- `tests/Videra.Core.Tests/Repository/ReleaseDryRunRepositoryTests.cs`
- `docs/releasing.md` only if command/evidence wording changes

Validation:

```powershell
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ReleaseDryRunRepositoryTests|FullyQualifiedName~SurfaceChartsCiTruthTests" --no-restore
git diff --check
```

Handoff:

- Keep the composed gate non-publishing.
- Skipped/manual-gated release actions remain explicit evidence states, not
  validation success.

## Phase Verification

After all child beads are merged:

```powershell
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartsCookbook|FullyQualifiedName~SurfaceChartsDemo|FullyQualifiedName~SurfaceChartsCiTruthTests|FullyQualifiedName~ReleaseDryRunRepositoryTests" --no-restore
git diff --check
bd dep cycles --json
```

## Stop Conditions

- Stop and report if a validation command cannot be run honestly.
- Stop and report if a child worktree cannot be cleaned safely.
- Stop and report if Beads/Dolt state cannot be pushed or reconciled safely.
