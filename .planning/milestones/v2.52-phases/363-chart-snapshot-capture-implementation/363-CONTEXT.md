# Phase 363: Chart Snapshot Capture Implementation - Context

**Gathered:** 2026-04-29
**Status:** Ready for planning
**Bead:** Videra-lu9.3

<domain>
## Phase Boundary

Implement a minimal Avalonia-local PNG/bitmap snapshot path for `VideraChartView.Plot`. The capture produces a PNG artifact through the Plot-owned contract created in Phase 362. Validates dimensions, target, and chart readiness with explicit errors. Implementation stays chart-local and does not widen backend/runtime contracts.

</domain>

<decisions>
## Implementation Decisions

### Capture Implementation
- Rendering: Avalonia `RenderTargetBitmap` — renders VideraChartView offscreen, chart-local approach
- PNG encoding: SkiaSharp `SKImage`/`SKData` — already a project dependency (used by VideraSnapshotExportService)
- Thread model: Marshal to UI thread for render via `Dispatcher.UIThread.InvokeAsync`, async for file I/O
- Validation: Validate dimensions > 0, Plot has series, format is Png — return `PlotSnapshotResult` with diagnostic on failure

### the agent's Discretion
- Method `CaptureSnapshotAsync` lives on `Plot3D` but needs access to VideraChartView's visual tree — use internal bridge pattern (Plot3D calls back to VideraChartView for render)
- Update `Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics()` to report ImageExport as supported
- PNG save path: caller provides path in request, result includes the path on success
- Follow existing SkiaSharp patterns from `VideraSnapshotExportService` for pixel capture and PNG encoding

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `VideraSnapshotExportService` — reference for SkiaSharp PNG encoding pattern (SKImage, SKData, SKEncodedImageFormat)
- `PlotSnapshotRequest/Result/Manifest/Diagnostic` — contract types from Phase 362
- `VideraChartView.Rendering.cs` — `Render(DrawingContext)` method shows how the chart renders (SurfaceScenePainter.DrawScene)
- `Plot3D` — owns series model, needs CaptureSnapshotAsync method
- `VideraChartView` — has `Bounds.Size`, `_renderScene`, `_chartProjection` for rendering state

### Established Patterns
- SkiaSharp usage: `SKImage.FromBitmap()`, `SKData.Encode(SKEncodedImageFormat.Png, 100)`, `data.SaveTo(stream)`
- UI thread dispatching: `Dispatcher.UIThread.InvokeAsync()` / `Dispatcher.UIThread.CheckAccess()`
- Validation: `ArgumentOutOfRangeException.ThrowIfNegative()`, `ArgumentException.ThrowIfNullOrWhiteSpace()`
- Chart-local types in `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/`

### Integration Points
- `Plot3D.CaptureSnapshotAsync(PlotSnapshotRequest)` — the new method
- `VideraChartView` — needs internal method to render offscreen to RenderTargetBitmap
- `Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics()` — must update ImageExport to supported
- Test project: `tests/Videra.SurfaceCharts.Core.Tests/` — add capture tests

</code_context>

<specifics>
## Specific Ideas

- Phase 362 created the contract types; Phase 363 implements the actual capture
- The capture should use Avalonia's `RenderTargetBitmap` to render the control offscreen at the requested dimensions
- After rendering, extract pixel data and encode to PNG via SkiaSharp
- Save to the path specified in the request
- Build the manifest from the captured state (dimensions, evidence kinds, active series)
- Update `CreateUnsupportedExportDiagnostics()` to remove the ImageExport unsupported entry

</specifics>

<deferred>
## Deferred Ideas

- GPU-accelerated offscreen rendering (future)
- Stream-based result (return bytes instead of file path) (future)
- Custom DPI scaling beyond 1x/2x (future)

</deferred>
