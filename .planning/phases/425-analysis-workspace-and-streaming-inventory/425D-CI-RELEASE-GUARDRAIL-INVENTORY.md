---
phase: 425
title: "Phase 425D CI Release-Readiness and Guardrail Inventory"
bead: Videra-7tqx.1.4
status: inventory
created_at: 2026-04-30
scope: "Read-only CI, release-readiness, consumer smoke, roadmap, scope guardrail, and repository-test inventory for v2.64"
---

# Phase 425D CI Release-Readiness and Guardrail Inventory

This file inventories the current CI, release-readiness, consumer smoke,
generated-roadmap, scope-guardrail, and repository-test truth surfaces that
Phase 430 can reuse for v2.64. It is evidence only: no code, workflows, scripts,
tests, Beads state, generated roadmap, or other inventory files are changed.

## Owners and Files

| Surface | Current owner files | Current truth | v2.64 relevance |
|---|---|---|---|
| Broad repository CI | `.github/workflows/ci.yml`, `scripts/verify.ps1`, `scripts/verify.sh` | Windows `verify` runs build/test/demo builds; `sample-contract-evidence`, `performance-lab-visual-evidence`, and `quality-gate-evidence` add focused SurfaceCharts and package smoke gates. | Phase 430 should extend focused filters rather than relying only on broad `verify`. |
| SurfaceCharts CI truth tests | `tests/Videra.Core.Tests/Repository/SurfaceChartsCiTruthTests.cs` | Asserts CI has SurfaceCharts sample evidence, generated roadmap/scope evidence, runtime evidence, and packaged SurfaceCharts smoke without `continue-on-error`, `|| true`, or `if: always()` on validation steps. | Best existing anti-fake CI test owner for new workspace, linked interaction, streaming, and package-template filters. |
| Release dry run workflow | `.github/workflows/release-dry-run.yml`, `scripts/Invoke-ReleaseReadinessValidation.ps1`, `scripts/Invoke-ReleaseDryRun.ps1` | Pull request/manual dry run builds packages, validates package contracts, runs SurfaceCharts packaged consumer smoke, runs focused SurfaceCharts repository tests, and records skipped publish/tag actions explicitly. | Phase 430 should add v2.64 package-template evidence here, not just docs/tests. |
| Package contract validation | `scripts/Validate-Packages.ps1`, `eng/public-api-contract.json`, `eng/package-size-budgets.json`, `tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs` | Validates canonical public packages, metadata, dependencies, symbols, README/icon presence, size budgets, and package boundary docs/workflows. | Reusable for package-template validation, but does not currently prove a v2.64 template app workflow. |
| Consumer smoke | `.github/workflows/consumer-smoke.yml`, `scripts/Invoke-ConsumerSmoke.ps1`, `smoke/Videra.ConsumerSmoke`, `smoke/Videra.SurfaceCharts.ConsumerSmoke` | Runs packaged viewer and SurfaceCharts scenarios across host workflows; SurfaceCharts success requires real support summary, PNG snapshot, ready rendering status, nonzero resident tiles, and no unsupported scatter/toolbar claims. | Strong release truth surface, but currently has one SurfaceCharts first-chart scenario rather than workspace/linked/streaming/template scenarios. |
| Public publish gates | `.github/workflows/publish-public.yml`, `.github/workflows/publish-existing-public-release.yml`, `.github/workflows/publish-github-packages.yml`, `scripts/Invoke-PublicReleasePreflight.ps1`, `scripts/Invoke-FinalReleaseSimulation.ps1` | Public publish is manual dispatch with explicit tag/version/expected commit, native validation, consumer smoke, package validation, and manual-gated release actions. Preflight fails missing/stale evidence. | Phase 430 can reuse the fail-closed release-action model; v2.64 should not claim publish readiness from skipped/unavailable evidence. |
| Generated roadmap | `scripts/Export-BeadsRoadmap.ps1`, `docs/ROADMAP.generated.md`, `tests/Videra.Core.Tests/Repository/BeadsPublicRoadmapTests.cs`, `.beads/issues.jsonl` | Roadmap is deterministically generated from exported Beads snapshot; test regenerates and requires byte stability. | Phase 430 should include generated-roadmap checks only through the script/test, never hand edits. |
| Snapshot/export scope | `scripts/Test-SnapshotExportScope.ps1`, `tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotContractTests.cs`, `tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotCaptureTests.cs`, `AGENTS.md` | Rejects old chart controls, direct public `Source`, PDF/vector export patterns, viewer export coupling, and hidden fallback/downshift patterns; tests pin PNG/image-only snapshot diagnostics. | Guardrail remains directly reusable; Phase 430 may need text/repository tests for v2.64 workspace/workbench and compatibility claims. |
| Release/support documentation truth | `docs/releasing.md`, `docs/release-policy.md`, `docs/support-matrix.md`, `docs/alpha-feedback.md`, `docs/surfacecharts-release-cutover.md`, `README.md` | Repository tests pin release readiness sequence, support artifact routing, package/public boundary, SurfaceCharts package truth, and no-publication dry-run language. | Needs v2.64 wording only after real workspace/template surfaces exist; avoid ahead-of-implementation claims. |
| Current v2.64 planning truth | `.planning/REQUIREMENTS.md`, `.planning/ROADMAP.md`, `.planning/STATE.md` | Phase 430 requires CI filters for workspace, linked interaction, streaming, cookbook, package templates, generated roadmap, and release-readiness truth. | This inventory maps what is missing before Phase 430 writes. |

## Reusable Truth Surfaces

- `.github/workflows/ci.yml` already has a focused SurfaceCharts sample gate for
  cookbook/demo/high-performance/streaming evidence and a generated-roadmap plus
  snapshot-scope gate.
- `SurfaceChartsCiTruthTests` is the best place to pin workflow intent because
  it validates the actual YAML and rejects fake green patterns on validation
  steps.
- `scripts/Invoke-ReleaseReadinessValidation.ps1` is the current composed local
  release-readiness entry point. It delegates to `Invoke-ReleaseDryRun.ps1`,
  runs packaged SurfaceCharts consumer smoke, runs focused repository tests, and
  runs `Test-SnapshotExportScope.ps1`.
- `scripts/Invoke-ReleaseDryRun.ps1` and `scripts/Validate-Packages.ps1` are the
  package-contract truth path. They create package evidence, validate metadata
  and dependency boundaries, write size-budget evidence, and keep publish/tag
  steps as explicit manual gates.
- `scripts/Invoke-ConsumerSmoke.ps1` provides the strongest anti-fake packaged
  runtime proof. For SurfaceCharts it fails if support summary, PNG snapshot,
  ready rendering status, resident tiles, or manifest-backed snapshot truth are
  absent.
- `Invoke-PublicReleasePreflight.ps1` already models release evidence as
  fail-closed: missing release dry-run, benchmark, native-validation, consumer
  smoke, or manual gate evidence makes preflight fail instead of silently
  passing.
- `BeadsPublicRoadmapTests` and `Export-BeadsRoadmap.ps1` are the generated
  roadmap truth. Any v2.64 roadmap synchronization should flow through those
  owners, not manual edits to `docs/ROADMAP.generated.md`.
- `Test-SnapshotExportScope.ps1` remains a useful pattern guardrail for blocked
  chart scope: old view controls, direct `Source`, PDF/vector export, hidden
  fallback/downshift, and viewer-export coupling.

## v2.64 Gaps

- **Workspace validation gap**: no current CI/release-readiness filter names a
  v2.64 multi-chart analysis workspace, active panel identity, workspace
  evidence, panel catalog, or layout/support-summary contract.
- **Linked interaction gap**: existing linked-view coverage is mainly cookbook
  and chart-state oriented. There is no Phase 430-ready CI gate for linked
  camera/axis/probe/selection propagation across workspace panels with
  host-owned selection truth.
- **Streaming validation gap**: CI already includes
  `ScatterStreamingScenarioEvidenceTests` and `SurfaceChartsPerformanceTruthTests`,
  and README documents streaming benchmark evidence as non-threshold evidence.
  It does not yet prove a v2.64 high-density/streaming workspace scenario with
  ingestion, visible-window, retention, cache, or no-substitution evidence.
- **Package-template gap**: `Invoke-ConsumerSmoke.ps1` proves packaged viewer and
  first-chart SurfaceCharts paths. It does not currently validate a copyable
  package-consumer template for workspace, linked interaction, or streaming.
- **Release-readiness composition gap**: `Invoke-ReleaseReadinessValidation.ps1`
  still runs the older focused SurfaceCharts repository filter set. Phase 430
  needs to add the v2.64 tests only after Phases 426-429 create stable test
  names and artifacts.
- **Guardrail prose gap**: `Test-SnapshotExportScope.ps1` catches known code
  patterns, but it cannot catch docs or template prose that implies a generic
  workbench, compatibility/parity layer, hidden fallback, backend expansion, or
  package publication readiness.
- **Generated roadmap ownership gap**: roadmap generation is covered, but this
  slice is not allowed to touch Beads state or generated roadmap files. Phase
  431 or a sync owner must reconcile `.beads/issues.jsonl` and
  `docs/ROADMAP.generated.md`.

## Risks and Non-Goals

- Do not count `skip`, `skipped`, `unavailable`, missing artifact, or build-only
  paths as success for v2.64 release readiness.
- Do not create fake CI success by adding `continue-on-error`, `|| true`,
  widened filters that run no tests, or `if: always()` around validation logic.
  Reserve `if: always()` for artifact uploads.
- Do not claim compatibility, parity, migration wrappers, or external plotting
  library equivalence. ScottPlot-style workflow language can remain an
  ergonomics reference only.
- Do not claim hidden fallback/downshift behavior. Unsupported output, missing
  render hosts, unavailable data paths, or backend/display issues must remain
  explicit diagnostics or failures.
- Do not broaden Phase 430 into a generic workbench, plugin editor, backend
  expansion, PDF/vector export, or public package publication phase.
- Do not hand-edit `docs/ROADMAP.generated.md` or `.beads/issues.jsonl` as part
  of CI truth work unless the task explicitly owns Beads synchronization.

## Candidate Phase 430 Write Scopes

| Slice | Candidate write scope | Safe parallelization notes | Validation focus |
|---|---|---|---|
| CI workflow truth | `.github/workflows/ci.yml`, `tests/Videra.Core.Tests/Repository/SurfaceChartsCiTruthTests.cs` | Can run after Phases 426-429 settle test names. Keep separate from consumer-smoke script edits. | Verify new focused filters and no fake-green workflow patterns. |
| Release-readiness composition | `scripts/Invoke-ReleaseReadinessValidation.ps1`, `tests/Videra.Core.Tests/Repository/ReleaseDryRunRepositoryTests.cs`, `RepositoryReleaseReadinessTests.cs` | Can proceed in parallel with CI YAML if test names are agreed first; avoid editing package validator unless package contract changes. | Ensure v2.64 package-template evidence is included and skipped/unavailable checks are not pass. |
| Package-template smoke | `smoke/`, `samples/`, docs under `samples/Videra.SurfaceCharts.Demo/Recipes/`, template-specific repository tests | Should be owned by Phase 429 first; Phase 430 only wires stable commands into release/CI. | Build/run packaged template path from local packages, with artifact assertions. |
| Guardrail hardening | `scripts/Test-SnapshotExportScope.ps1`, repository tests for docs/prose boundaries, `docs/*` only if claims exist | Can be independent if it only adds guardrail tests; coordinate with docs workers to avoid conflicting prose edits. | Reject old controls, direct `Source`, broad workbench, compatibility/fallback claims, backend expansion, fake evidence. |
| Generated roadmap truth | `scripts/Export-BeadsRoadmap.ps1`, `tests/Videra.Core.Tests/Repository/BeadsPublicRoadmapTests.cs`, `docs/ROADMAP.generated.md`, `.beads/issues.jsonl` | Keep for sync/finalization owner because Beads state is shared. This inventory task must not modify it. | Regenerate, assert byte-stable output, and keep Beads as source of truth. |

## Suggested Focused Validation Commands

Targeted inventory searches used for this file:

```powershell
rg --files -g '.github/**' -g 'scripts/**' -g 'tests/**' -g '*.sln*' -g '*.csproj' -g '*.props' -g '*.targets'
rg -n "release|readiness|preflight|dry run|dry-run|publish|package|consumer smoke|ConsumerSmoke|roadmap|scope|guardrail|snapshot|unavailable|skipped|skip|compat|fallback|workflow_dispatch|pull_request|dotnet test|dotnet pack" .github scripts tests docs README.md CHANGELOG.md Directory.Build.props Videra.slnx
rg -n "v2\.64|2\.64|workspace|linked|streaming|DataLogger3D|package template|template|cookbook|ScatterStreaming|Linked|PackageTemplate|Workspaces|workspace" .planning docs samples smoke scripts tests src .github README.md CHANGELOG.md
```

Focused Phase 430 candidate gates after implementation:

```powershell
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartsCiTruthTests|FullyQualifiedName~RepositoryReleaseReadinessTests|FullyQualifiedName~ReleaseDryRunRepositoryTests|FullyQualifiedName~BeadsPublicRoadmapTests"
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\Test-SnapshotExportScope.ps1
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\Invoke-ReleaseReadinessValidation.ps1 -ExpectedVersion 0.1.0-alpha.7 -Configuration Release -OutputRoot artifacts/release-readiness-validation
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\Invoke-ConsumerSmoke.ps1 -Configuration Release -Scenario SurfaceCharts -OutputRoot artifacts/surfacecharts-consumer-smoke-quality -TreatWarningsAsErrors
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\Validate-Packages.ps1 -PackageRoot artifacts/surfacecharts-consumer-smoke-quality/packages -ExpectedVersion <resolved-consumer-smoke-version>
```

The first two commands are suitable as focused local checks for CI/guardrail
edits. The release-readiness and consumer-smoke commands are heavier and should
be used once v2.64 template/runtime evidence exists and the worktree has a
stable restore/build state.

## Handoff Notes

- Phase 430 should wait for Phases 426-429 to produce stable workspace,
  linked-interaction, streaming, and package-template tests/artifacts before
  wiring CI/release-readiness filters.
- Prefer extending existing repository truth tests over adding unverified prose
  promises. Every new workflow/script claim should have a test that reads the
  actual owner file.
- Keep Phase 430 write sets disjoint: CI YAML/test truth, release-readiness
  scripts/tests, package-template smoke, and generated-roadmap sync can be
  separate worktrees if test names and artifact paths are agreed first.
- The final v2.64 sync owner, not this inventory slice, should update Beads
  export, generated roadmap, phase archive, and push state.
