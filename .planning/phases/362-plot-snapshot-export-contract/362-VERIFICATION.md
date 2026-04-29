# Phase 362: Plot Snapshot Export Contract — Verification

**Verified:** 2026-04-29T16:05:00Z
**Status:** passed

## Must-Have Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | PlotSnapshotRequest can be constructed with valid Width, Height, Scale, Background, Format | ✅ passed | `Request_ConstructsWithValidParameters` test passes; public ctor with 5 validated parameters |
| 2 | PlotSnapshotResult can represent success (with Path + Manifest) and failure (with Diagnostic) | ✅ passed | `Result_Success_FactoryCreatesSuccessResult` and `Result_Failed_FactoryCreatesFailedResult` tests pass |
| 3 | PlotSnapshotManifest carries deterministic metadata: Width, Height, OutputEvidenceKind, DatasetEvidenceKind, ActiveSeriesIdentity, Format, Background, CreatedUtc | ✅ passed | `Manifest_ConstructsWithValidParameters` test verifies all 8 properties |
| 4 | Invalid requests (zero dimensions, null path, unsupported format) produce PlotSnapshotDiagnostic with explicit diagnostic code | ✅ passed | `Request_RejectsZeroWidth`, `Request_RejectsZeroHeight`, `Request_RejectsZeroScale`, `Request_RejectsNegativeWidth`, `Result_Success_RequiresPath`, `Result_Success_RequiresManifest`, `Result_Failed_RequiresDiagnostic` tests pass |
| 5 | Types live in Videra.SurfaceCharts.Avalonia.Controls namespace with no dependency on Videra.Avalonia or Videra.Core | ✅ passed | All 6 files use `namespace Videra.SurfaceCharts.Avalonia.Controls;`; csproj references only SurfaceCharts.Core and SurfaceCharts.Rendering |

## Must-Have Artifacts

| Path | Provides | Contains | Status |
|------|----------|----------|--------|
| `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotFormat.cs` | Format enum with Png value | `PlotSnapshotFormat` | ✅ exists |
| `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotBackground.cs` | Background enum with Transparent and Opaque values | `PlotSnapshotBackground` | ✅ exists |
| `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotDiagnostic.cs` | Diagnostic type for explicit error reporting | `PlotSnapshotDiagnostic` | ✅ exists |
| `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotRequest.cs` | Request type capturing dimensions, scale, background, format | `PlotSnapshotRequest` | ✅ exists |
| `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotResult.cs` | Result type with Path, Manifest, Succeeded, Failure, Duration | `PlotSnapshotResult` | ✅ exists |
| `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotManifest.cs` | Manifest type with deterministic metadata | `PlotSnapshotManifest` | ✅ exists |
| `tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotContractTests.cs` | Unit tests for contract construction, validation, diagnostics | `PlotSnapshotContractTests` (20 tests, 232 lines) | ✅ exists |

## Key Links

| From | To | Via | Pattern | Status |
|------|----|-----|---------|--------|
| PlotSnapshotRequest | PlotSnapshotFormat | Format property | `PlotSnapshotFormat.Png` | ✅ verified |
| PlotSnapshotRequest | PlotSnapshotBackground | Background property | `PlotSnapshotBackground.` | ✅ verified |
| PlotSnapshotResult | PlotSnapshotManifest | Manifest property | `PlotSnapshotManifest` | ✅ verified |
| PlotSnapshotResult | PlotSnapshotDiagnostic | Failure property on failed result | `PlotSnapshotDiagnostic` | ✅ verified |

## Build & Test Results

- **Build:** `dotnet build src/Videra.SurfaceCharts.Avalonia/Videra.SurfaceCharts.Avalonia.csproj --no-restore` — 0 errors, 0 warnings
- **Tests:** `dotnet test tests/Videra.SurfaceCharts.Core.Tests/ --filter "PlotSnapshotContract" --no-restore` — 20 passed, 0 failed

## Commits

| Hash | Message |
|------|---------|
| `925ed74` | `feat(362-01): create snapshot contract types` |
| `791beba` | `test(362-01): add snapshot contract unit tests` |
