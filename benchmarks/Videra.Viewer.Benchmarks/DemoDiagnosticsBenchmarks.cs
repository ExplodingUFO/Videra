using System.Numerics;
using BenchmarkDotNet.Attributes;
using Videra.Avalonia.Controls;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.RenderPipeline.Extensibility;
using Videra.Core.Graphics.Wireframe;
using Videra.Core.Scene;
using Videra.Core.Styles.Presets;
using Videra.Demo.Services;

namespace Videra.Viewer.Benchmarks;

[MemoryDiagnoser]
public class DemoDiagnosticsBenchmarks
{
    private ModelLoadBatchResult _loadResult = null!;
    private VideraBackendDiagnostics _diagnostics = null!;
    private RenderCapabilitySnapshot _capabilities = null!;
    private DemoSupportSettings _settings = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _loadResult = new ModelLoadBatchResult(
            Array.Empty<SceneDocumentEntry>(),
            [new ModelLoadFailure("missing.obj", new InvalidOperationException("missing file"))],
            TimeSpan.FromMilliseconds(7),
            [
                ModelLoadFileResult.Success(
                    "ok.obj",
                    entry: null,
                    [new ModelImportDiagnostic(ModelImportDiagnosticSeverity.Info, "parsed")],
                    TimeSpan.FromMilliseconds(4),
                    new SceneAssetMetrics(3, 3, 120, new BoundingBox3(Vector3.Zero, Vector3.One))),
                ModelLoadFileResult.Failed(
                    "missing.obj",
                    new ModelLoadFailure("missing.obj", new InvalidOperationException("missing file")),
                    [new ModelImportDiagnostic(ModelImportDiagnosticSeverity.Error, "missing file", "OBJ404")],
                    TimeSpan.FromMilliseconds(1))
            ]);

        _diagnostics = new VideraBackendDiagnostics
        {
            RequestedBackend = GraphicsBackendPreference.Auto,
            ResolvedBackend = GraphicsBackendPreference.Software,
            IsReady = true,
            IsUsingSoftwareFallback = true,
            FallbackReason = "native backend unavailable",
            SceneDocumentVersion = 2,
            LastFrameObjectCount = 3,
            SupportedRenderFeatureNames = ["Opaque"]
        };

        _capabilities = new RenderCapabilitySnapshot
        {
            IsInitialized = true,
            ActiveBackendPreference = GraphicsBackendPreference.Software,
            SupportedFeatureNames = ["Opaque"]
        };

        _settings = new DemoSupportSettings(
            RenderStylePreset.Tech,
            WireframeMode.Overlay,
            IsGridVisible: true,
            GridHeight: 0m,
            GridColor: "#556677",
            BackgroundColor: "#101820",
            CameraInvertX: false,
            CameraInvertY: true,
            SelectedObjectName: "demo cube");
    }

    [Benchmark]
    public int FormatImportReport()
    {
        return DemoSupportReportBuilder.FormatImportReport(_loadResult).Length;
    }

    [Benchmark]
    public int BuildDiagnosticsBundle()
    {
        return DemoSupportReportBuilder.BuildDiagnosticsBundle(
            _diagnostics,
            _capabilities,
            loadedModelCount: 2,
            _loadResult,
            _settings).Length;
    }
}
