# PNG Snapshot Recipe

This recipe covers the bounded bitmap export path for SurfaceCharts. The shipped
path is chart-local PNG snapshot export through `Plot.SavePngAsync(...)`, which
wraps `CaptureSnapshotAsync(PlotSnapshotRequest)`.

## Save A PNG

```csharp
var result = await chart.Plot.SavePngAsync(
    "artifacts/surfacecharts/first-chart.png",
    width: 1920,
    height: 1080);

if (!result.Succeeded)
{
    throw new InvalidOperationException(result.Failure?.Message);
}
```

`SavePngAsync` creates a `PlotSnapshotRequest` with
`PlotSnapshotFormat.Png` and delegates to `CaptureSnapshotAsync`. A successful
`PlotSnapshotResult` contains both the PNG path and a `PlotSnapshotManifest`.

## Capture With An Explicit Request

Use `CaptureSnapshotAsync` directly when the host needs to choose scale or
background behavior:

```csharp
var request = new PlotSnapshotRequest(
    width: 1920,
    height: 1080,
    scale: 1.0,
    background: PlotSnapshotBackground.Transparent,
    format: PlotSnapshotFormat.Png);

var result = await chart.Plot.CaptureSnapshotAsync(request);
```

When the export succeeds, read these manifest fields:

- `Manifest.Width`
- `Manifest.Height`
- `Manifest.OutputEvidenceKind`
- `Manifest.DatasetEvidenceKind`
- `Manifest.ActiveSeriesIdentity`
- `Manifest.Format`
- `Manifest.Background`
- `Manifest.CreatedUtc`

The demo projects the same truth into the support summary as `SnapshotStatus`,
`SnapshotPath`, `SnapshotWidth`, `SnapshotHeight`, `SnapshotFormat`,
`SnapshotBackground`, `SnapshotOutputEvidenceKind`,
`SnapshotDatasetEvidenceKind`, `SnapshotActiveSeriesIdentity`, and
`SnapshotCreatedUtc`.

## Failure Truth

`CaptureSnapshotAsync` returns a failed `PlotSnapshotResult` with a diagnostic
instead of inventing an export. Expected failure codes include
`snapshot.chart.no-active-series`, `snapshot.format.unsupported`, and
`snapshot.render.no-host`.

`Plot3DOutputCapabilityDiagnostic` reports `ImageExport` as supported for the
PNG path. `PdfExport` and `VectorExport` remain unsupported capability
diagnostics; they are not alternate export modes in this milestone.

## Boundary

This recipe is PNG-only bitmap export via Avalonia `RenderTargetBitmap`. Do not
add PDF export, vector export, SVG export, backend expansion, OpenGL/WebGL
promises, compatibility wrappers, hidden fallback/downshift, or a generic export
workbench.
