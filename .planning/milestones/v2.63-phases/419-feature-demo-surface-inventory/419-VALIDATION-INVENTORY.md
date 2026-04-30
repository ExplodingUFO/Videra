# Phase 419C Validation Inventory

Owner: `Videra-i8zb`
Scope: docs-only inventory for Phase 423/424 planning. No product code changes.
Worktree: `.worktrees/v263-phase419-validation`

## Current Truth Map

| Owner | File or surface | Current truth |
| --- | --- | --- |
| CI owner | `.github/workflows/ci.yml` | Windows `verify` runs `scripts/verify.ps1 -Configuration Release`. `sample-contract-evidence` runs focused SurfaceCharts sample tests, generated-roadmap/scope evidence, and SurfaceCharts runtime integration filters. `quality-gate-evidence` runs packaged viewer and SurfaceCharts consumer smoke with warnings as errors, then validates package artifacts. SurfaceCharts evidence steps do not carry `continue-on-error`, `|| true`, or `if: always()` success masking; artifact upload steps may use `if: always()` only to preserve diagnostics after a failure. |
| CI owner | `.github/workflows/release-dry-run.yml` | Read-only release-candidate review path. It calls `scripts/Invoke-ReleaseReadinessValidation.ps1`, uploads `release-readiness-validation-evidence`, and is tested to avoid package/feed writes, release creation, tags, `NUGET_API_KEY`, and `GITHUB_TOKEN`. |
| Release owner | `scripts/Invoke-ReleaseReadinessValidation.ps1` | Composes the local v2.63-ready release gate: release dry run, packaged SurfaceCharts consumer smoke, focused SurfaceCharts/script-facing repository tests, and `scripts/Test-SnapshotExportScope.ps1`. It records environment warnings and explicitly skipped publish/tag steps. Skipped public publish/tag steps are informational only, not validation success. |
| Release owner | `scripts/Invoke-ReleaseDryRun.ps1` | Packs the canonical public package set from `eng/public-api-contract.json`, validates packages, emits dry-run summary/evidence index files, and records public publish/tag/GitHub release actions as `manual-gate` with `failClosedDefault = true` and `actionTaken = false`. It is non-publishing by design. |
| Scope owner | `scripts/Test-SnapshotExportScope.ps1` | Read-only source scan for old chart controls, direct public `Source` API, PDF/vector export code, chart-to-viewer export-service coupling, and hidden fallback/downshift identifiers in chart cleanup paths. Current coverage is snapshot/export and old-code scope, not a general v2.63 feature API inventory. |
| Roadmap owner | `docs/ROADMAP.generated.md` and `tests/Videra.Core.Tests/Repository/BeadsPublicRoadmapTests.cs` | Roadmap is generated from `.beads/issues.jsonl`; test reruns `scripts/Export-BeadsRoadmap.ps1` and asserts the generated file is unchanged. Current roadmap shows v2.63 active, Phase 419 ready, and Phases 420-424 blocked in order. |
| Cookbook owner | `samples/Videra.SurfaceCharts.Demo/README.md` and `samples/Videra.SurfaceCharts.Demo/Recipes/*.md` | Current cookbook covers first chart, surface/cache-backed data, waterfall, axes/linked views, scatter/live data, bar, contour, support evidence, and PNG snapshot. It frames ScottPlot 5 as recipe ergonomics inspiration only and keeps the demo repository-only. |
| Cookbook test owner | `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookCoverageMatrixTests.cs` | Pins cookbook rows across root README, docs index, demo README, cutover doc, XAML labels, code-behind recipe groups, visible proof labels, and detailed recipe file existence. Current rows are First Chart, Styling, Interactions, Live Data, Linked Axes, Bar, Contour, Export. |
| Cookbook test owner | `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookFirstSurfaceRecipeTests.cs` | Pins first-chart and surface/cache-backed recipes to `VideraChartView`, `Plot.Add.Surface`, `SurfaceMatrix`, `SurfacePyramidBuilder`, `FitToData`, `ViewState`, and support-evidence language while rejecting old controls, source API, compatibility, fallback/downshift, generic benchmark, and god-code terms. |
| Cookbook test owner | `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookWaterfallLinkedRecipeTests.cs` | Pins waterfall, axes, overlay, view-state, and disposable linked-view recipe semantics. Also rejects old controls, `.Source`, compatibility/adapters/shims, PDF/vector, OpenGL/WebGL, hidden fallback/downshift, generic plotting/editor, and benchmark guarantee terms. |
| Cookbook test owner | `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookScatterLiveRecipeTests.cs` | Pins `Plot.Add.Scatter`, `DataLogger3D`, columnar/FIFO/live-window diagnostics, autoscale decisions, interaction quality, and support-summary terms. Rejects performance guarantees, zero-copy, GPU-driven culling, fallback renderer, automatic downshift, generic plotting/workbench claims. |
| Cookbook test owner | `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbookBarContourSnapshotRecipeTests.cs` | Pins bounded bar, contour, support-summary, and PNG snapshot recipes, including manifest fields and PNG-only bitmap boundary. Rejects PDF/SVG/vector methods, old chart-view construction, OpenGL/WebGL backend names, compatibility wrappers, automatic downshift, silent fallback, and fake performance evidence. |
| Demo validation owner | `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs` and `SurfaceChartsDemoViewportBehaviorTests.cs` | Current tests guard demo configuration, labels, status/support summary behavior, and viewport behavior for existing paths. They are not yet the owner for new Phase 420-422 feature/demo additions until those additions land. |
| Consumer smoke owner | `tests/Videra.Core.Tests/Samples/SurfaceChartsConsumerSmokeConfigurationTests.cs` and `scripts/Invoke-ConsumerSmoke.ps1` | Packaged SurfaceCharts smoke is the package-path proof and emits `consumer-smoke-result.json`, `diagnostics-snapshot.txt`, `surfacecharts-support-summary.txt`, `chart-snapshot.png`, logs, and environment data. CI/release readiness run it with `-Scenario SurfaceCharts`; quality-gate CI also passes `-TreatWarningsAsErrors`. |
| Release truth owner | `tests/Videra.Core.Tests/Repository/ReleaseDryRunRepositoryTests.cs` | Pins dry-run/readiness/preflight/final-simulation behavior, manual-gate statuses, evidence-only optional visual evidence, fail-closed missing artifact behavior, and separation between dry-run and feed-push workflows. |
| SurfaceCharts release owner | `tests/Videra.Core.Tests/Repository/SurfaceChartsReleaseTruthRepositoryTests.cs` | Pins public publish workflow dependency on SurfaceCharts smoke, repository-only/demo-vs-packaged split, cutover doc support boundary, explicit maintainer approval, no compatibility/parity claims, no hidden fallback, and no PDF/vector/backend expansion. |
| CI truth owner | `tests/Videra.Core.Tests/Repository/SurfaceChartsCiTruthTests.cs` | Pins CI filters for current SurfaceCharts sample evidence, roadmap/scope evidence, runtime evidence, and packaged consumer smoke/package validation without fake-green patterns in those steps. |
| Support docs owner | `docs/surfacecharts-release-cutover.md`, `docs/support-matrix.md`, `docs/alpha-feedback.md`, `docs/releasing.md`, `docs/release-policy.md` | Current support truth says `Videra.SurfaceCharts.Avalonia` is the public entry package, `Videra.SurfaceCharts.Processing` is only for surface/cache-backed use, demo and smoke apps are repository-only, support summaries are evidence-only, and missing/unavailable optional visual evidence is review context rather than a publish blocker. |

## Gaps For New v2.63 Feature/Demo Validation

1. Phase 420/421/422 feature additions do not have final cookbook rows yet. `SurfaceChartsCookbookCoverageMatrixTests.cs` must add rows only after the real new demo labels, code-behind groups, snippets, and docs exist.
2. The release-readiness focused test filter is hard-coded in `scripts/Invoke-ReleaseReadinessValidation.ps1`. New v2.63 cookbook/demo/CI truth tests will not run in the composed local release gate unless Phase 423 explicitly updates that filter.
3. `SurfaceChartsCiTruthTests.cs` pins the existing CI filters. It must be expanded when new Phase 423 validation tests are added so CI cannot silently skip the new validation.
4. `scripts/Test-SnapshotExportScope.ps1` is a v2.52/v2.62 scope guard. It should remain in the final gate, but it does not prove new v2.63 feature/demo behavior.
5. Current cookbook tests mostly assert token presence and forbidden terms. New features should add concrete recipe/demo/support-summary assertions, not just broad keyword checks.
6. Packaged consumer smoke currently proves the packaged SurfaceCharts happy path and support artifacts. If Phase 420-422 add package-visible behavior, Phase 423 must decide whether the smoke artifact should expose it or whether it remains demo-only.
7. Demo viewport/config tests are current-path guards. New gallery navigation, copy-snippet, support summary, and scenario-state claims need targeted tests owned by Phase 423 rather than relying on existing broad demo tests.
8. Roadmap truth depends on `.beads/issues.jsonl` export. Phase 424 must regenerate and test the roadmap after any child beads are created/closed; stale roadmap output should not be accepted as documentation truth.

## Recommended Child Beads

### Phase 423: Cookbook Matrix And Recipe Truth

Type: `task`
Parent/dependency: discovered from `Videra-64g`; blocked by the Phase 420/421/422 implementation beads that introduce the real public/demo surfaces.
Write scope:

- `samples/Videra.SurfaceCharts.Demo/README.md`
- `samples/Videra.SurfaceCharts.Demo/Recipes/*.md`
- `README.md`
- `docs/surfacecharts-release-cutover.md`
- `docs/support-matrix.md`
- `tests/Videra.Core.Tests/Samples/SurfaceChartsCookbook*Tests.cs`

Validation commands:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartsCookbook"
git diff --check
```

Handoff notes: add one cookbook row per real demo/workflow surface. Require visible proof label, code-behind group/snippet token, README/cutover text, and forbidden-scope assertions. Do not add recipe rows for planned-only features.

### Phase 423: Demo Support Summary And CI Truth

Type: `task`
Parent/dependency: discovered from `Videra-64g`; depends on the final Phase 422 demo gallery workflow.
Write scope:

- `.github/workflows/ci.yml`
- `samples/Videra.SurfaceCharts.Demo/**`
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemo*Tests.cs`
- `tests/Videra.Core.Tests/Repository/SurfaceChartsCiTruthTests.cs`

Validation commands:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartsDemo|FullyQualifiedName~SurfaceChartsCiTruthTests"
git diff --check
```

Handoff notes: CI must run new focused demo tests without `continue-on-error`, `|| true`, or success-masking `if: always()`. Artifact upload may stay `if: always()` only when the preceding validation step still fails normally.

### Phase 423: Release Readiness Focused Filter Update

Type: `task`
Parent/dependency: discovered from `Videra-64g`; depends on the cookbook/demo/CI truth tests above.
Write scope:

- `scripts/Invoke-ReleaseReadinessValidation.ps1`
- `tests/Videra.Core.Tests/Repository/ReleaseDryRunRepositoryTests.cs`
- `docs/releasing.md` only if the command list or evidence wording changes

Validation commands:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ReleaseDryRunRepositoryTests|FullyQualifiedName~SurfaceChartsCiTruthTests"
git diff --check
```

Handoff notes: keep the composed gate non-publishing. New skipped/manual-gated steps must remain explicit evidence states, not implied pass states.

### Phase 424: Final Roadmap And Scope Evidence Closeout

Type: `task`
Parent/dependency: discovered from `Videra-7ip`; blocked by `Videra-64g` and all Phase 423 child beads.
Write scope:

- `docs/ROADMAP.generated.md`
- `.planning/phases/419-feature-demo-surface-inventory/**` or the active final-verification phase summary
- Beads state through `bd`, not manual edits to `.beads/issues.jsonl`

Validation commands:

```powershell
pwsh -File ./scripts/Test-SnapshotExportScope.ps1
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~BeadsPublicRoadmapTests|FullyQualifiedName~SurfaceChartsCiTruthTests|FullyQualifiedName~ReleaseDryRunRepositoryTests"
git diff --check
```

Handoff notes: regenerate roadmap through `scripts/Export-BeadsRoadmap.ps1` or the tested Beads export path after Beads state changes. If hooks export `.beads/issues.jsonl`, restore it before committing unless the bead export is explicitly part of the Phase 424 closeout.

### Phase 424: Final Release-Readiness Evidence Run

Type: `task`
Parent/dependency: discovered from `Videra-7ip`; blocked by final roadmap/scope closeout.
Write scope:

- final verification summary only
- generated artifacts under `artifacts/` if the phase explicitly chooses to retain them

Validation commands:

```powershell
pwsh -File ./scripts/Invoke-ReleaseReadinessValidation.ps1 -ExpectedVersion <current-version> -Configuration Release -OutputRoot artifacts/v263-release-readiness-final
git diff --check
```

Handoff notes: record pass/fail/warning/skipped/manual-gate statuses exactly as emitted. Do not translate skipped publish/tag steps, unavailable optional visual evidence, missing optional support context, or environment warnings into success.

## No Fake Evidence Rule

No skipped check, missing artifact, unavailable optional evidence, manual-gated release action, warning-only environment condition, or artifact upload after failure should be treated as validation success. Success requires the owning command or test step to exit successfully and produce the expected evidence. If evidence is not run, skipped, unavailable, or missing, the Phase 423/424 handoff must say that explicitly and keep the affected claim open.
