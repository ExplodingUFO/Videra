using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Owns the series model authored through <see cref="VideraChartView.Plot"/>.
/// </summary>
public sealed class Plot3D
{
    private readonly List<Plot3DSeries> _series = [];
    private readonly ReadOnlyCollection<Plot3DSeries> _seriesView;
    private SurfaceColorMap? _colorMap;
    private SurfaceChartOverlayOptions _overlayOptions = SurfaceChartOverlayOptions.Default;
    private Func<int, int, double, Task<RenderTargetBitmap>>? _renderOffscreen;
    private Action<bool>? _setSnapshotMode;

    internal Plot3D(Action changed)
    {
        Changed = changed ?? throw new ArgumentNullException(nameof(changed));
        _seriesView = new ReadOnlyCollection<Plot3DSeries>(_series);
        Add = new Plot3DAddApi(this);
    }

    /// <summary>
    /// Gets the series authoring API.
    /// </summary>
    public Plot3DAddApi Add { get; }

    /// <summary>
    /// Gets the series attached to this plot in draw order.
    /// </summary>
    public IReadOnlyList<Plot3DSeries> Series => _seriesView;

    /// <summary>
    /// Gets the series currently driving the chart view, or <c>null</c> when the plot is empty.
    /// </summary>
    public Plot3DSeries? ActiveSeries => _series.Count == 0 ? null : _series[^1];

    internal Plot3DSeries? ActiveSurfaceSeries
    {
        get
        {
            var activeSeries = ActiveSeries;
            return activeSeries?.Kind is Plot3DSeriesKind.Surface or Plot3DSeriesKind.Waterfall ? activeSeries : null;
        }
    }

    internal Plot3DSeries? ActiveScatterSeries
    {
        get
        {
            var activeSeries = ActiveSeries;
            return activeSeries?.Kind == Plot3DSeriesKind.Scatter ? activeSeries : null;
        }
    }

    internal Plot3DSeries? ActiveContourSeries
    {
        get
        {
            var activeSeries = ActiveSeries;
            return activeSeries?.Kind == Plot3DSeriesKind.Contour ? activeSeries : null;
        }
    }

    internal Plot3DSeries? ActiveBarSeries
    {
        get
        {
            var activeSeries = ActiveSeries;
            return activeSeries?.Kind == Plot3DSeriesKind.Bar ? activeSeries : null;
        }
    }

    /// <summary>
    /// Gets or sets the optional color map used by surface and waterfall series.
    /// </summary>
    public SurfaceColorMap? ColorMap
    {
        get => _colorMap;
        set
        {
            if (ReferenceEquals(_colorMap, value))
            {
                return;
            }

            _colorMap = value;
            NotifyChanged();
        }
    }

    /// <summary>
    /// Gets or sets plot-level axis, grid, legend, probe, and numeric-formatting options for
    /// formatter, title/unit override, minor ticks, grid plane, and axis-side selection.
    /// </summary>
    public SurfaceChartOverlayOptions OverlayOptions
    {
        get => _overlayOptions;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (ReferenceEquals(_overlayOptions, value))
            {
                return;
            }

            _overlayOptions = value;
            NotifyChanged();
        }
    }

    /// <summary>
    /// Gets a monotonically increasing revision that changes when the plot model changes.
    /// </summary>
    public int Revision { get; private set; }

    private Action Changed { get; }

    /// <summary>
    /// Creates deterministic dataset evidence from the current Plot-authored series.
    /// </summary>
    /// <returns>A snapshot of the current dataset evidence.</returns>
    public Plot3DDatasetEvidence CreateDatasetEvidence()
    {
        return Plot3DDatasetEvidence.Create(Revision, _series, _overlayOptions);
    }

    /// <summary>
    /// Removes all series from the plot.
    /// </summary>
    public void Clear()
    {
        if (_series.Count == 0)
        {
            return;
        }

        _series.Clear();
        NotifyChanged();
    }

    /// <summary>
    /// Removes a series from the plot.
    /// </summary>
    /// <returns><c>true</c> when the series was present and removed; otherwise, <c>false</c>.</returns>
    public bool Remove(Plot3DSeries series)
    {
        ArgumentNullException.ThrowIfNull(series);

        var removed = _series.Remove(series);
        if (!removed)
        {
            return false;
        }

        NotifyChanged();
        return true;
    }

    /// <summary>
    /// Gets the draw-order index of a series, or <c>-1</c> when the series is not attached to this plot.
    /// </summary>
    public int IndexOf(Plot3DSeries series)
    {
        ArgumentNullException.ThrowIfNull(series);
        return _series.IndexOf(series);
    }

    /// <summary>
    /// Creates deterministic chart-local output evidence for the current plot model.
    /// </summary>
    /// <returns>The chart-local output evidence.</returns>
    public Plot3DOutputEvidence CreateOutputEvidence()
    {
        return CreateOutputEvidence(renderingStatus: null, scatterRenderingStatus: null);
    }

    /// <summary>
    /// Creates deterministic chart-local output evidence for the current plot model and public rendering status projections.
    /// </summary>
    /// <param name="renderingStatus">The latest surface or waterfall rendering status, when available.</param>
    /// <param name="scatterRenderingStatus">The latest scatter rendering status, when available.</param>
    /// <returns>The chart-local output evidence.</returns>
    public Plot3DOutputEvidence CreateOutputEvidence(
        SurfaceChartRenderingStatus? renderingStatus,
        ScatterChartRenderingStatus? scatterRenderingStatus)
    {
        return CreateOutputEvidence(renderingStatus, scatterRenderingStatus, barRenderingStatus: null, contourRenderingStatus: null);
    }

    /// <summary>
    /// Creates deterministic chart-local output evidence for the current plot model and public rendering status projections.
    /// </summary>
    /// <param name="renderingStatus">The latest surface or waterfall rendering status, when available.</param>
    /// <param name="scatterRenderingStatus">The latest scatter rendering status, when available.</param>
    /// <param name="barRenderingStatus">The latest bar chart rendering status, when available.</param>
    /// <param name="contourRenderingStatus">The latest contour rendering status, when available.</param>
    /// <returns>The chart-local output evidence.</returns>
    public Plot3DOutputEvidence CreateOutputEvidence(
        SurfaceChartRenderingStatus? renderingStatus,
        ScatterChartRenderingStatus? scatterRenderingStatus,
        BarChartRenderingStatus? barRenderingStatus,
        ContourChartRenderingStatus? contourRenderingStatus)
    {
        var activeSeries = ActiveSeries;
        var activeSeriesIndex = activeSeries is null ? -1 : _series.IndexOf(activeSeries);
        var colorMapEvidence = CreateColorMapEvidence(activeSeries);
        var renderingEvidence = CreateRenderingEvidence(activeSeries, renderingStatus, scatterRenderingStatus, barRenderingStatus, contourRenderingStatus);

        return new Plot3DOutputEvidence(
            seriesCount: _series.Count,
            activeSeriesIndex: activeSeriesIndex,
            activeSeriesName: activeSeries?.Name,
            activeSeriesKind: activeSeries?.Kind,
            activeSeriesIdentity: CreateSeriesIdentity(activeSeries, activeSeriesIndex),
            colorMapStatus: CreateColorMapStatus(activeSeries, colorMapEvidence),
            colorMapEvidence: colorMapEvidence,
            precisionProfile: SurfaceChartOverlayEvidenceFormatter.DescribePrecisionProfile(OverlayOptions),
            renderingEvidence: renderingEvidence,
            outputCapabilityDiagnostics: Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics());
    }

    /// <summary>
    /// Sets the internal render offscreen delegate used by <see cref="CaptureSnapshotAsync"/> to render the chart.
    /// </summary>
    /// <param name="renderOffscreen">The delegate that renders the chart to a bitmap.</param>
    internal void SetRenderOffscreen(Func<int, int, double, Task<RenderTargetBitmap>> renderOffscreen)
    {
        _renderOffscreen = renderOffscreen;
    }

    /// <summary>
    /// Sets the snapshot mode callback used to suppress interaction chrome during snapshot capture.
    /// </summary>
    /// <param name="setSnapshotMode">The delegate that sets snapshot mode on the overlay coordinator.</param>
    internal void SetSnapshotModeCallback(Action<bool> setSnapshotMode)
    {
        _setSnapshotMode = setSnapshotMode;
    }

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

        // Validate dimensions (defense-in-depth; PlotSnapshotRequest constructor also validates)
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
            // Enable snapshot mode to suppress interaction chrome (crosshair, hovered probe, toolbar)
            _setSnapshotMode?.Invoke(true);

            // Render offscreen via bridge
            var bitmap = await _renderOffscreen(request.Width, request.Height, request.Scale).ConfigureAwait(false);

            // Restore normal mode
            _setSnapshotMode?.Invoke(false);

            // Encode to PNG and save
            var outputPath = System.IO.Path.ChangeExtension(System.IO.Path.GetRandomFileName(), ".png");
            await EncodeAndSavePng(bitmap, outputPath).ConfigureAwait(false);

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
            // Ensure snapshot mode is restored even on failure
            _setSnapshotMode?.Invoke(false);

            return PlotSnapshotResult.Failed(
                PlotSnapshotDiagnostic.Create("snapshot.capture.failed", $"Snapshot capture failed: {ex.Message}"),
                stopwatch.Elapsed);
        }
    }

    private static async Task EncodeAndSavePng(RenderTargetBitmap bitmap, string path)
    {
        var directory = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        using var outputStream = System.IO.File.Create(path);
        bitmap.Save(outputStream);
    }

    internal Plot3DSeries AddSeries(Plot3DSeries series)
    {
        ArgumentNullException.ThrowIfNull(series);

        _series.Add(series);
        NotifyChanged();
        return series;
    }

    private SurfaceChartOutputEvidence? CreateColorMapEvidence(Plot3DSeries? activeSeries)
    {
        if (activeSeries?.Kind is not (Plot3DSeriesKind.Surface or Plot3DSeriesKind.Waterfall))
        {
            return null;
        }

        var source = activeSeries.SurfaceSource;
        if (source is null)
        {
            return null;
        }

        var colorMap = ColorMap ?? new SurfaceColorMap(source.Metadata.ValueRange, SurfaceColorMapPresets.CreateDefault());
        var paletteName = ColorMap is null ? "Default" : "Plot.ColorMap";
        return SurfaceChartOverlayEvidenceFormatter.Create(paletteName, colorMap, OverlayOptions);
    }

    private static Plot3DRenderingEvidence? CreateRenderingEvidence(
        Plot3DSeries? activeSeries,
        SurfaceChartRenderingStatus? renderingStatus,
        ScatterChartRenderingStatus? scatterRenderingStatus,
        BarChartRenderingStatus? barRenderingStatus,
        ContourChartRenderingStatus? contourRenderingStatus)
    {
        return activeSeries?.Kind switch
        {
            Plot3DSeriesKind.Surface or Plot3DSeriesKind.Waterfall when renderingStatus is not null =>
                Plot3DRenderingEvidence.FromSurfaceStatus(renderingStatus),
            Plot3DSeriesKind.Scatter when scatterRenderingStatus is not null =>
                Plot3DRenderingEvidence.FromScatterStatus(scatterRenderingStatus),
            Plot3DSeriesKind.Bar when barRenderingStatus is not null =>
                Plot3DRenderingEvidence.FromBarStatus(barRenderingStatus),
            Plot3DSeriesKind.Contour when contourRenderingStatus is not null =>
                Plot3DRenderingEvidence.FromContourStatus(contourRenderingStatus),
            _ => null,
        };
    }

    private static string? CreateSeriesIdentity(Plot3DSeries? activeSeries, int activeSeriesIndex)
    {
        if (activeSeries is null || activeSeriesIndex < 0)
        {
            return null;
        }

        var name = activeSeries.Name ?? "<unnamed>";
        return $"{activeSeries.Kind}:{name}:{activeSeriesIndex}";
    }

    private static Plot3DColorMapStatus CreateColorMapStatus(
        Plot3DSeries? activeSeries,
        SurfaceChartOutputEvidence? colorMapEvidence)
    {
        if (colorMapEvidence is not null)
        {
            return Plot3DColorMapStatus.Applied;
        }

        return activeSeries is null || activeSeries.Kind is Plot3DSeriesKind.Scatter or Plot3DSeriesKind.Contour or Plot3DSeriesKind.Bar
            ? Plot3DColorMapStatus.NotApplicable
            : Plot3DColorMapStatus.Unavailable;
    }

    private void NotifyChanged()
    {
        Revision++;
        Changed();
    }
}
