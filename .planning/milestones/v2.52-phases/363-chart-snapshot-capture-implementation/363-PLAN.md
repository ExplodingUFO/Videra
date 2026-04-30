---
phase: 363-chart-snapshot-capture-implementation
plan: 01
type: execute
wave: 1
depends_on:
  - 362-plot-snapshot-export-contract
files_modified:
  - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs
  - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DOutputEvidence.cs
  - src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Rendering.cs
  - src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs
  - tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotCaptureTests.cs
autonomous: true
requirements: [CAP-01, CAP-02, CAP-03, CAP-04, VER-01, VER-02, VER-03]

must_haves:
  truths:
    - "Plot3D.CaptureSnapshotAsync produces a PNG file at the caller-specified path"
    - "Capture rejects invalid dimensions (<=0) with explicit PlotSnapshotDiagnostic"
    - "Capture rejects empty plot (no series) with explicit PlotSnapshotDiagnostic"
    - "Capture rejects unsupported format with explicit PlotSnapshotDiagnostic"
    - "Plot3DOutputCapabilityDiagnostic reports ImageExport as supported after capture path exists"
    - "Manifest contains deterministic output/dataset evidence kind and active series identity"
  artifacts:
    - path: "src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs"
      provides: "CaptureSnapshotAsync method with validation, render bridge, PNG encoding"
      contains: "CaptureSnapshotAsync"
    - path: "src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DOutputEvidence.cs"
      provides: "Updated CreateUnsupportedExportDiagnostics marking ImageExport as supported"
      contains: "ImageExport"
    - path: "src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Rendering.cs"
      provides: "Internal offscreen render method using RenderTargetBitmap"
      contains: "RenderTargetBitmap"
    - path: "tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotCaptureTests.cs"
      provides: "Capture integration tests covering success, invalid, unsupported, manifest"
      min_lines: 80
  key_links:
    - from: "Plot3D.CaptureSnapshotAsync"
      to: "VideraChartView render bridge"
      via: "internal Action<RenderTargetBitmap, Size> delegate"
      pattern: "RenderOffscreen"
    - from: "Plot3D.CaptureSnapshotAsync"
      to: "PlotSnapshotResult.Success/Failed"
      via: "factory methods from Phase 362 contract"
      pattern: "PlotSnapshotResult\\.(Success|Failed)"
    - from: "Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics"
      to: "ImageExport supported entry"
      via: "isSupported: true for ImageExport"
      pattern: "ImageExport.*isSupported.*true"
---

<objective>
Implement the minimal Avalonia-local PNG/bitmap snapshot capture path for VideraChartView.Plot.

Purpose: Phase 362 created the contract types (request/result/manifest/diagnostic). Phase 363 wires them to produce actual PNG artifacts through Avalonia's RenderTargetBitmap + SkiaSharp encoding, completing the chart-local snapshot vertical slice.

Output: Plot3D.CaptureSnapshotAsync method, offscreen render bridge on VideraChartView, updated capability diagnostics, and focused integration tests.
</objective>

<execution_context>
@$HOME/.config/opencode/get-shit-done/workflows/execute-plan.md
@$HOME/.config/opencode/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/STATE.md
@.planning/phases/363-chart-snapshot-capture-implementation/363-CONTEXT.md
@.planning/phases/362-plot-snapshot-export-contract/362-01-SUMMARY.md

<interfaces>
<!-- Key types and contracts the executor needs. Extracted from Phase 362 contract types. -->

From src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotRequest.cs:
```csharp
public sealed class PlotSnapshotRequest
{
    public PlotSnapshotRequest(int width, int height, double scale, PlotSnapshotBackground background, PlotSnapshotFormat format);
    public int Width { get; }
    public int Height { get; }
    public double Scale { get; }
    public PlotSnapshotBackground Background { get; }
    public PlotSnapshotFormat Format { get; }
}
```

From src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotResult.cs:
```csharp
public sealed class PlotSnapshotResult
{
    public bool Succeeded { get; }
    public string? Path { get; }
    public PlotSnapshotManifest? Manifest { get; }
    public PlotSnapshotDiagnostic? Failure { get; }
    public TimeSpan Duration { get; }
    internal static PlotSnapshotResult Success(string path, PlotSnapshotManifest manifest, TimeSpan duration);
    internal static PlotSnapshotResult Failed(PlotSnapshotDiagnostic failure, TimeSpan duration);
}
```

From src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotManifest.cs:
```csharp
public sealed class PlotSnapshotManifest
{
    internal PlotSnapshotManifest(int width, int height, string outputEvidenceKind, string datasetEvidenceKind,
        string activeSeriesIdentity, PlotSnapshotFormat format, PlotSnapshotBackground background, DateTime createdUtc);
    public int Width { get; }
    public int Height { get; }
    public string OutputEvidenceKind { get; }
    public string DatasetEvidenceKind { get; }
    public string ActiveSeriesIdentity { get; }
    public PlotSnapshotFormat Format { get; }
    public PlotSnapshotBackground Background { get; }
    public DateTime CreatedUtc { get; }
}
```

From src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotDiagnostic.cs:
```csharp
public sealed class PlotSnapshotDiagnostic
{
    public string DiagnosticCode { get; }
    public string Message { get; }
    internal static PlotSnapshotDiagnostic Create(string diagnosticCode, string message);
}
```

From src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs (current state):
```csharp
public sealed class Plot3D
{
    public IReadOnlyList<Plot3DSeries> Series { get; }
    public Plot3DSeries? ActiveSeries { get; }
    public Plot3DDatasetEvidence CreateDatasetEvidence();
    public Plot3DOutputEvidence CreateOutputEvidence(SurfaceChartRenderingStatus?, ScatterChartRenderingStatus?);
}
```

From src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DOutputEvidence.cs (current state):
```csharp
// Line 152-168: CreateUnsupportedExportDiagnostics() currently returns ImageExport as isSupported: false
internal static IReadOnlyList<Plot3DOutputCapabilityDiagnostic> CreateUnsupportedExportDiagnostics()
{
    return
    [
        Unsupported("ImageExport", "plot-output.export.image.unsupported", "Plot3D output evidence does not implement image export."),
        Unsupported("PdfExport", "plot-output.export.pdf.unsupported", "Plot3D output evidence does not implement PDF export."),
        Unsupported("VectorExport", "plot-output.export.vector.unsupported", "Plot3D output evidence does not implement vector export."),
    ];
}
```

From src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs:
```csharp
public partial class VideraChartView : Decorator
{
    public Plot3D Plot { get; }
    public SurfaceChartRenderingStatus RenderingStatus { get; }
    public ScatterChartRenderingStatus ScatterRenderingStatus { get; }
    // Plot = new Plot3D(OnPlotChanged) in constructor
}
```

From src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Rendering.cs:
```csharp
public partial class VideraChartView
{
    private SurfaceRenderScene? _renderScene;
    public override void Render(DrawingContext context) { ... SurfaceScenePainter.DrawScene(context, _renderHost.SoftwareScene, projection); }
}
```

SkiaSharp PNG encoding pattern (from VideraSnapshotExportService.cs):
```csharp
using var image = SKImage.FromBitmap(bitmap);
using var data = image.Encode(SKEncodedImageFormat.Png, 100);
using var output = File.Create(path);
data.SaveTo(output);
```
</interfaces>
</context>

<tasks>

<task type="auto" tdd="true">
  <name>Task 1: Add capture bridge, CaptureSnapshotAsync, and update diagnostics</name>
  <files>
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DOutputEvidence.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Rendering.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs
  </files>
  <read_first>
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DOutputEvidence.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Rendering.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotRequest.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotResult.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotManifest.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotDiagnostic.cs
  </read_first>
  <behavior>
    - CaptureSnapshotAsync with valid request on plot with series produces PlotSnapshotResult.Succeeded == true
    - CaptureSnapshotAsync with width <= 0 returns PlotSnapshotResult.Succeeded == false with diagnostic code "snapshot.request.invalid-dimensions"
    - CaptureSnapshotAsync with height <= 0 returns PlotSnapshotResult.Succeeded == false with diagnostic code "snapshot.request.invalid-dimensions"
    - CaptureSnapshotAsync with empty plot (no series) returns PlotSnapshotResult.Succeeded == false with diagnostic code "snapshot.chart.no-active-series"
    - CaptureSnapshotAsync with unsupported format returns PlotSnapshotResult.Succeeded == false with diagnostic code "snapshot.format.unsupported"
    - CreateUnsupportedExportDiagnostics returns ImageExport with IsSupported == true
    - CreateUnsupportedExportDiagnostics returns PdfExport with IsSupported == false (unchanged)
    - CreateUnsupportedExportDiagnostics returns VectorExport with IsSupported == false (unchanged)
  </behavior>
  <action>
**Step 1: Add internal render bridge delegate type to VideraChartView.Rendering.cs**

Add a new internal delegate and method in `VideraChartView.Rendering.cs` for offscreen rendering:

```csharp
// Add at class level (inside partial class VideraChartView)
internal delegate void RenderOffscreenHandler(RenderTargetBitmap bitmap, Size size);

internal RenderOffscreenHandler? RenderOffscreenDelegate { get; private set; }
```

Add the offscreen render method:

```csharp
internal async Task<RenderTargetBitmap> RenderOffscreenAsync(int width, int height, double scale)
{
    var pixelSize = new PixelSize(width, height);
    var dpi = new Vector(96 * scale, 96 * scale);
    var bitmap = new RenderTargetBitmap(pixelSize, dpi);

    if (Dispatcher.UIThread.CheckAccess())
    {
        RenderToBitmap(bitmap, new Size(width, height));
    }
    else
    {
        await Dispatcher.UIThread.InvokeAsync(() => RenderToBitmap(bitmap, new Size(width, height)));
    }

    return bitmap;
}

private void RenderToBitmap(RenderTargetBitmap bitmap, Size size)
{
    // Ensure render host is synced before offscreen render
    SyncRenderHost();
    bitmap.Render(this);
}
```

Add required usings at top of file:
```csharp
using Avalonia.Media.Imaging;
using Avalonia.Platform;
```

**Step 2: Wire Plot3D to VideraChartView render bridge in VideraChartView.Core.cs**

In the VideraChartView constructor, after `Plot = new Plot3D(OnPlotChanged)`, set the render callback on Plot3D. This requires adding an internal Action<Func<int, int, double, Task<RenderTargetBitmap>>> setter on Plot3D:

In `Plot3D.cs`, add:
```csharp
private Func<int, int, double, Task<Avalonia.Media.Imaging.RenderTargetBitmap>>? _renderOffscreen;

internal void SetRenderOffscreen(Func<int, int, double, Task<Avalonia.Media.Imaging.RenderTargetBitmap>> renderOffscreen)
{
    _renderOffscreen = renderOffscreen;
}
```

In `VideraChartView.Core.cs` constructor, after `Plot = new Plot3D(OnPlotChanged)`:
```csharp
Plot.SetRenderOffscreen(RenderOffscreenAsync);
```

**Step 3: Add CaptureSnapshotAsync to Plot3D.cs**

Add the method after `CreateOutputEvidence`:

```csharp
/// <summary>
/// Captures a chart-local PNG/bitmap snapshot through the Plot-owned contract.
/// </summary>
/// <param name="request">The snapshot request specifying dimensions, format, and background.</param>
/// <returns>A result containing the artifact path and manifest on success, or a diagnostic on failure.</returns>
public async Task<PlotSnapshotResult> CaptureSnapshotAsync(PlotSnapshotRequest request)
{
    ArgumentNullException.ThrowIfNull(request);
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

    // Validate format
    if (request.Format != PlotSnapshotFormat.Png)
    {
        return PlotSnapshotResult.Failed(
            PlotSnapshotDiagnostic.Create("snapshot.format.unsupported", $"Format '{request.Format}' is not supported. Only Png is currently supported."),
            stopwatch.Elapsed);
    }

    // Validate dimensions
    if (request.Width <= 0 || request.Height <= 0)
    {
        return PlotSnapshotResult.Failed(
            PlotSnapshotDiagnostic.Create("snapshot.request.invalid-dimensions", $"Snapshot dimensions must be positive. Got {request.Width}x{request.Height}."),
            stopwatch.Elapsed);
    }

    // Validate chart readiness
    var activeSeries = ActiveSeries;
    if (activeSeries is null)
    {
        return PlotSnapshotResult.Failed(
            PlotSnapshotDiagnostic.Create("snapshot.chart.no-active-series", "Cannot capture snapshot: plot has no active series."),
            stopwatch.Elapsed);
    }

    // Validate render bridge
    if (_renderOffscreen is null)
    {
        return PlotSnapshotResult.Failed(
            PlotSnapshotDiagnostic.Create("snapshot.render.no-host", "Cannot capture snapshot: no render host is attached to this plot."),
            stopwatch.Elapsed);
    }

    try
    {
        // Render offscreen via bridge
        var bitmap = await _renderOffscreen(request.Width, request.Height, request.Scale);

        // Encode to PNG via SkiaSharp
        var outputPath = System.IO.Path.GetTempFileName() + ".png";
        await EncodeAndSavePng(bitmap, outputPath, request);

        // Build manifest
        var outputEvidence = CreateOutputEvidence();
        var datasetEvidence = CreateDatasetEvidence();
        var activeSeriesIndex = _series.IndexOf(activeSeries);
        var seriesIdentity = $"{activeSeries.Kind}:{activeSeries.Name ?? "<unnamed>"}:{activeSeriesIndex}";

        var manifest = new PlotSnapshotManifest(
            width: request.Width,
            height: request.Height,
            outputEvidenceKind: outputEvidence.EvidenceKind,
            datasetEvidenceKind: datasetEvidence.EvidenceKind,
            activeSeriesIdentity: seriesIdentity,
            format: request.Format,
            background: request.Background,
            createdUtc: DateTime.UtcNow);

        return PlotSnapshotResult.Success(outputPath, manifest, stopwatch.Elapsed);
    }
    catch (Exception ex)
    {
        return PlotSnapshotResult.Failed(
            PlotSnapshotDiagnostic.Create("snapshot.capture.failed", $"Snapshot capture failed: {ex.Message}"),
            stopwatch.Elapsed);
    }
}
```

Add the PNG encoding helper method:

```csharp
private static async Task EncodeAndSavePng(Avalonia.Media.Imaging.RenderTargetBitmap bitmap, string path, PlotSnapshotRequest request)
{
    using var stream = new MemoryStream();
    bitmap.Save(stream);
    stream.Position = 0;

    // Use SkiaSharp for PNG encoding
    using var skImage = SkiaSharp.SKImage.FromEncodedData(stream);
    using var skData = skImage.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);

    var directory = System.IO.Path.GetDirectoryName(path);
    if (!string.IsNullOrEmpty(directory))
    {
        System.IO.Directory.CreateDirectory(directory);
    }

    using var outputStream = System.IO.File.Create(path);
    skData.SaveTo(outputStream);
}
```

Add required usings to Plot3D.cs:
```csharp
using System.Diagnostics;
using Avalonia.Media.Imaging;
using SkiaSharp;
```

**Step 4: Update Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics**

In `Plot3DOutputEvidence.cs`, change the ImageExport entry from `isSupported: false` to `isSupported: true` and update the diagnostic code/message:

```csharp
internal static IReadOnlyList<Plot3DOutputCapabilityDiagnostic> CreateUnsupportedExportDiagnostics()
{
    return
    [
        Supported("ImageExport", "plot-output.export.image.supported", "Plot3D output evidence supports PNG image export via CaptureSnapshotAsync."),
        Unsupported("PdfExport", "plot-output.export.pdf.unsupported", "Plot3D output evidence does not implement PDF export."),
        Unsupported("VectorExport", "plot-output.export.vector.unsupported", "Plot3D output evidence does not implement vector export."),
    ];
}

private static Plot3DOutputCapabilityDiagnostic Supported(
    string capability,
    string diagnosticCode,
    string message)
{
    return new Plot3DOutputCapabilityDiagnostic(capability, isSupported: true, diagnosticCode, message);
}
```

**Step 5: Add SkiaSharp package reference**

Since SkiaSharp is available transitively through Avalonia 11.x but may not expose the needed types directly, add an explicit PackageReference if the build fails without it. Check `src/Videra.SurfaceCharts.Avalonia/Videra.SurfaceCharts.Avalonia.csproj` and add:

```xml
<PackageReference Include="SkiaSharp" Version="2.88.9" />
```

Note: Avalonia 11.x bundles SkiaSharp internally. If the types resolve without an explicit reference, skip this step. Test by building first.
  </action>
  <behavior>
    - CaptureSnapshotAsync validates request dimensions and returns diagnostic on failure
    - CaptureSnapshotAsync validates plot has active series and returns diagnostic if empty
    - CaptureSnapshotAsync validates format is Png and returns diagnostic for unsupported
    - CaptureSnapshotAsync produces a PNG file and returns success result with manifest
    - CreateUnsupportedExportDiagnostics reports ImageExport as supported
  </behavior>
  <verify>
    <automated>dotnet build src/Videra.SurfaceCharts.Avalonia/Videra.SurfaceCharts.Avalonia.csproj</automated>
  </verify>
  <done>
    - Plot3D.CaptureSnapshotAsync exists and validates request, chart readiness, format, and dimensions
    - VideraChartView has internal RenderOffscreenAsync method using RenderTargetBitmap
    - Plot3D is wired to VideraChartView render bridge via SetRenderOffscreen
    - CreateUnsupportedExportDiagnostics reports ImageExport as supported (IsSupported == true)
    - Project builds successfully
  </done>
</task>

<task type="auto" tdd="true">
  <name>Task 2: Write capture integration tests</name>
  <files>
    tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotCaptureTests.cs
  </files>
  <read_first>
    tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotContractTests.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotRequest.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotResult.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotManifest.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotDiagnostic.cs,
    src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DOutputEvidence.cs
  </read_first>
  <behavior>
    - CaptureSnapshotAsync_ValidRequest_ReturnsSuccessWithManifest: valid request on plot with mock render bridge returns Succeeded=true, non-null Path, non-null Manifest with correct dimensions
    - CaptureSnapshotAsync_ZeroWidth_ReturnsFailedWithDiagnostic: width=0 returns Succeeded=false, diagnostic code "snapshot.request.invalid-dimensions"
    - CaptureSnapshotAsync_NegativeHeight_ReturnsFailedWithDiagnostic: height=-1 returns Succeeded=false, diagnostic code "snapshot.request.invalid-dimensions"
    - CaptureSnapshotAsync_EmptyPlot_ReturnsFailedWithDiagnostic: plot with no series returns Succeeded=false, diagnostic code "snapshot.chart.no-active-series"
    - CaptureSnapshotAsync_UnsupportedFormat_ReturnsFailedWithDiagnostic: format != Png returns Succeeded=false, diagnostic code "snapshot.format.unsupported"
    - CaptureSnapshotAsync_ManifestContainsDeterministicMetadata: manifest OutputEvidenceKind=="plot-3d-output", DatasetEvidenceKind=="Plot3DDatasetEvidence", ActiveSeriesIdentity matches "{Kind}:{Name}:{Index}" pattern
    - CreateUnsupportedExportDiagnostics_ImageExportIsSupported: ImageExport diagnostic has IsSupported==true
    - CreateUnsupportedExportDiagnostics_PdfAndVectorRemainUnsupported: PdfExport and VectorExport have IsSupported==false
  </behavior>
  <action>
Create `tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotCaptureTests.cs` following the existing test patterns from `PlotSnapshotContractTests.cs` (xUnit + FluentAssertions).

Since Plot3D requires a render bridge (VideraChartView) which needs Avalonia UI thread, tests that exercise the full capture path need a mock/fake render delegate. For tests that only exercise validation, Plot3D can be constructed directly.

**Test structure:**

```csharp
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public class PlotSnapshotCaptureTests
{
    // ── Validation tests (no render bridge needed) ─────────────────

    [Fact]
    public async Task CaptureSnapshotAsync_ZeroWidth_ReturnsFailedWithDiagnostic()
    {
        var plot = CreatePlotWithSeries();
        var request = new PlotSnapshotRequest(0, 1080, 1.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Png);

        var result = await plot.CaptureSnapshotAsync(request);

        result.Succeeded.Should().BeFalse();
        result.Failure.Should().NotBeNull();
        result.Failure!.DiagnosticCode.Should().Be("snapshot.request.invalid-dimensions");
    }

    [Fact]
    public async Task CaptureSnapshotAsync_NegativeHeight_ReturnsFailedWithDiagnostic()
    {
        var plot = CreatePlotWithSeries();
        var request = new PlotSnapshotRequest(1920, -1, 1.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Png);

        var result = await plot.CaptureSnapshotAsync(request);

        result.Succeeded.Should().BeFalse();
        result.Failure.Should().NotBeNull();
        result.Failure!.DiagnosticCode.Should().Be("snapshot.request.invalid-dimensions");
    }

    [Fact]
    public async Task CaptureSnapshotAsync_EmptyPlot_ReturnsFailedWithDiagnostic()
    {
        var plot = CreateEmptyPlot();
        var request = new PlotSnapshotRequest(1920, 1080, 1.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Png);

        var result = await plot.CaptureSnapshotAsync(request);

        result.Succeeded.Should().BeFalse();
        result.Failure.Should().NotBeNull();
        result.Failure!.DiagnosticCode.Should().Be("snapshot.chart.no-active-series");
    }

    [Fact]
    public async Task CaptureSnapshotAsync_UnsupportedFormat_ReturnsFailedWithDiagnostic()
    {
        // PlotSnapshotFormat currently only has Png, so this test uses (PlotSnapshotFormat)(-1)
        // to simulate an unsupported enum value
        var plot = CreatePlotWithSeries();
        var request = new PlotSnapshotRequest(1920, 1080, 1.0, PlotSnapshotBackground.Transparent, (PlotSnapshotFormat)(-1));

        var result = await plot.CaptureSnapshotAsync(request);

        result.Succeeded.Should().BeFalse();
        result.Failure.Should().NotBeNull();
        result.Failure!.DiagnosticCode.Should().Be("snapshot.format.unsupported");
    }

    [Fact]
    public async Task CaptureSnapshotAsync_NoRenderHost_ReturnsFailedWithDiagnostic()
    {
        var plot = CreatePlotWithSeries();
        // Don't set render bridge — simulates Plot3D without VideraChartView
        var request = new PlotSnapshotRequest(1920, 1080, 1.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Png);

        var result = await plot.CaptureSnapshotAsync(request);

        result.Succeeded.Should().BeFalse();
        result.Failure.Should().NotBeNull();
        result.Failure!.DiagnosticCode.Should().Be("snapshot.render.no-host");
    }

    // ── Manifest determinism tests ─────────────────────────────────

    [Fact]
    public void Manifest_OutputEvidenceKind_IsPlot3DOutput()
    {
        var manifest = CreateManifest();

        manifest.OutputEvidenceKind.Should().Be("plot-3d-output");
    }

    [Fact]
    public void Manifest_DatasetEvidenceKind_IsPlot3DDatasetEvidence()
    {
        var manifest = CreateManifest();

        manifest.DatasetEvidenceKind.Should().Be("Plot3DDatasetEvidence");
    }

    [Fact]
    public void Manifest_ActiveSeriesIdentity_FollowsConvention()
    {
        var manifest = CreateManifest();

        manifest.ActiveSeriesIdentity.Should().MatchRegex(@"^.+:\S+:\d+$");
    }

    // ── Capability diagnostic tests ────────────────────────────────

    [Fact]
    public void CreateUnsupportedExportDiagnostics_ImageExportIsSupported()
    {
        var diagnostics = Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics();
        var imageExport = diagnostics.First(d => d.Capability == "ImageExport");

        imageExport.IsSupported.Should().BeTrue();
    }

    [Fact]
    public void CreateUnsupportedExportDiagnostics_PdfExportRemainsUnsupported()
    {
        var diagnostics = Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics();
        var pdfExport = diagnostics.First(d => d.Capability == "PdfExport");

        pdfExport.IsSupported.Should().BeFalse();
    }

    [Fact]
    public void CreateUnsupportedExportDiagnostics_VectorExportRemainsUnsupported()
    {
        var diagnostics = Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics();
        var vectorExport = diagnostics.First(d => d.Capability == "VectorExport");

        vectorExport.IsSupported.Should().BeFalse();
    }

    // ── Helpers ────────────────────────────────────────────────────

    private static Plot3D CreateEmptyPlot()
    {
        return new Plot3D(() => { });
    }

    private static Plot3D CreatePlotWithSeries()
    {
        var plot = new Plot3D(() => { });
        // Use ScatterAddApi (doesn't require tile source) to add a series
        // The exact API depends on Plot3DAddApi — use scatter as it's simplest
        plot.Add.Scatter("test-series");
        return plot;
    }

    private static PlotSnapshotManifest CreateManifest()
    {
        return new PlotSnapshotManifest(
            1920, 1080, "plot-3d-output", "Plot3DDatasetEvidence",
            "Scatter:test-series:0", PlotSnapshotFormat.Png,
            PlotSnapshotBackground.Transparent, DateTime.UtcNow);
    }
}
```

**Important notes:**
- The `CreatePlotWithSeries()` helper uses `plot.Add.Scatter("test-series")` — verify the exact API signature. If Scatter requires additional parameters, adjust accordingly. Read `Plot3DAddApi` to confirm.
- Validation tests don't need a render bridge since validation fails before the bridge is invoked.
- The "no render host" test covers the case where Plot3D is used standalone without VideraChartView.
- If `Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics()` is `internal`, the test project already has `InternalsVisibleTo` access (confirmed in csproj).
  </action>
  <behavior>
    - 8 tests covering: zero width, negative height, empty plot, unsupported format, no render host, manifest evidence kinds, manifest series identity pattern, capability diagnostics
    - All tests use FluentAssertions (.Should().Be(), .Should().BeFalse(), etc.)
    - Tests follow existing PlotSnapshotContractTests.cs patterns
  </behavior>
  <verify>
    <automated>dotnet test tests/Videra.SurfaceCharts.Core.Tests --filter "FullyQualifiedName~PlotSnapshotCaptureTests" --no-restore</automated>
  </verify>
  <done>
    - PlotSnapshotCaptureTests.cs exists with 8+ focused tests
    - Tests cover: invalid dimensions, empty plot, unsupported format, no render host, manifest determinism, capability diagnostics
    - All tests pass
  </done>
</task>

</tasks>

<verification>
1. `dotnet build src/Videra.SurfaceCharts.Avalonia/Videra.SurfaceCharts.Avalonia.csproj` succeeds
2. `dotnet test tests/Videra.SurfaceCharts.Core.Tests --filter "FullyQualifiedName~PlotSnapshotCaptureTests"` — all tests pass
3. `dotnet test tests/Videra.SurfaceCharts.Core.Tests` — existing tests still pass (no regression)
4. Grep for `ImageExport` in `Plot3DOutputEvidence.cs` shows `isSupported: true`
5. Grep for `CaptureSnapshotAsync` in `Plot3D.cs` shows the method exists
6. Grep for `RenderTargetBitmap` in `VideraChartView.Rendering.cs` shows the offscreen render method
</verification>

<success_criteria>
- Plot3D.CaptureSnapshotAsync validates dimensions, chart readiness, format, and render host availability with explicit diagnostics
- Capture produces a PNG file via Avalonia RenderTargetBitmap + SkiaSharp encoding on success
- Plot3DOutputCapabilityDiagnostic reports ImageExport as supported
- 8+ focused tests pass covering success validation, error paths, and manifest determinism
- Existing tests continue to pass (no regression)
- Implementation stays chart-local — no new project references, no backend expansion
</success_criteria>

<output>
After completion, create `.planning/phases/363-chart-snapshot-capture-implementation/363-01-SUMMARY.md`
</output>
