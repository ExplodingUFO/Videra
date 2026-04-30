# Phase 304 Summary: No-Downshift Contract Inventory

## Result

Completed the repo-wide inventory and separated intentional fallback from stale/downshift risk.

## Default No-Downshift Contract

- Core defaults keep software fallback disabled:
  - `src/Videra.Core/Graphics/GraphicsBackendResolution.cs`
  - `src/Videra.Core/Graphics/GraphicsBackendFactory.cs`
  - `src/Videra.Core/Rendering/RenderSessionOrchestrator.cs`
- Avalonia defaults keep software fallback disabled:
  - `src/Videra.Avalonia/Controls/VideraViewOptions.cs`
- Canonical docs already mention explicit opt-in in several places:
  - `README.md`
  - `src/Videra.Avalonia/README.md`
  - `src/Videra.Core/README.md`
  - `docs/extensibility.md`
  - `docs/zh-CN/extensibility.md`

## Explicit Opt-In Fallback

- Packaged smoke app intentionally opts in for support capture:
  - `smoke/Videra.ConsumerSmoke/Views/MainWindow.axaml.cs`
- Backend factory and integration tests include explicit fallback cases:
  - `tests/Videra.Core.Tests/Graphics/GraphicsBackendFactoryTests.cs`
  - `tests/Videra.Core.IntegrationTests/Rendering/*`

## Stale or Risky Viewer Wording

Phase 305 owns the viewer/backend docs and guardrails:

- `ARCHITECTURE.md`
- `docs/capability-matrix.md`
- `docs/index.md`
- `docs/package-matrix.md`
- `docs/troubleshooting.md`
- `src/Videra.Core/README.md`
- focused repository tests under `tests/Videra.Core.Tests/Repository`

Main risk: wording such as "software-fallback-oriented integration" or broad "software fallback" capability descriptions can imply fallback is a default recovery path.

## Support Evidence Alignment

Phase 306 owns support/readiness evidence wording:

- `docs/alpha-feedback.md`
- `docs/videra-doctor.md`
- `scripts/Invoke-VideraDoctor.ps1` only if output semantics need tightening
- `smoke/Videra.ConsumerSmoke/Views/MainWindow.axaml.cs` only for support-report wording, not default behavior
- focused Doctor/support repository tests

Main risk: support text should use failure/not-ready/initialization wording by default and reserve `FallbackReason` for actual explicit fallback.

## SurfaceCharts Boundary

Phase 307 owns chart-local fallback wording and tests:

- `src/Videra.SurfaceCharts.Avalonia/README.md`
- `src/Videra.SurfaceCharts.Rendering/README.md`
- `samples/Videra.SurfaceCharts.Demo/README.md`
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs` for cache-load wording only
- `docs/zh-CN/modules/videra-surfacecharts-avalonia.md`
- `tests/Videra.Core.Tests/Repository/SurfaceChartsDocumentationTerms.cs`

Main risk: SurfaceCharts chart-local software fallback is intentional, but docs must not blur it with viewer backend downshift. Demo cache-load failure wording should say the prior source remains active, not that a hidden fallback path was selected.

## Deferred Findings

Snapshot export has an internal live-readback-to-software export path in `src/Videra.Avalonia/Runtime/VideraSnapshotExportService.cs`. It is outside this milestone's viewer backend resolution contract and should be evaluated separately if the no-downshift policy is later extended to export pipelines.
