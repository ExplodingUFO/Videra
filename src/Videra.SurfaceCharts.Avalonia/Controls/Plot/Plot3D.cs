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
    private Func<SurfaceDataWindow>? _getDataWindow;
    private Action<SurfaceDataWindow>? _setDataWindow;
    private Action? _fitToData;

    internal Plot3D(Action changed)
    {
        Changed = changed ?? throw new ArgumentNullException(nameof(changed));
        _seriesView = new ReadOnlyCollection<Plot3DSeries>(_series);
        Add = new Plot3DAddApi(this);
        Axes = new PlotAxes3D(this);
    }

    /// <summary>
    /// Gets the series authoring API.
    /// </summary>
    public Plot3DAddApi Add { get; }

    /// <summary>
    /// Gets the axis facade for X, Y, and Z labels, units, limits, and autoscale.
    /// </summary>
    public PlotAxes3D Axes { get; }

    /// <summary>
    /// Gets the series attached to this plot in draw order.
    /// </summary>
    public IReadOnlyList<Plot3DSeries> Series => _seriesView;

    /// <summary>
    /// Gets a typed snapshot of the series attached to this plot in draw order.
    /// </summary>
    public IReadOnlyList<TSeries> GetSeries<TSeries>()
        where TSeries : Plot3DSeries
    {
        var matches = new List<TSeries>();
        foreach (var series in _series)
        {
            if (series is TSeries typedSeries)
            {
                matches.Add(typedSeries);
            }
        }

        return matches.AsReadOnly();
    }

    /// <summary>
    /// Gets the series currently driving the chart view, or <c>null</c> when the plot is empty.
    /// </summary>
    public Plot3DSeries? ActiveSeries
    {
        get
        {
            for (var index = _series.Count - 1; index >= 0; index--)
            {
                var series = _series[index];
                if (series.IsVisible)
                {
                    return series;
                }
            }

            return null;
        }
    }

    internal Plot3DSeries? ActiveSurfaceSeries
    {
        get
        {
            var activeSeries = ActiveSeries;
            return activeSeries?.Kind is Plot3DSeriesKind.Surface or Plot3DSeriesKind.Waterfall ? activeSeries : null;
        }
    }

    internal IReadOnlyList<Plot3DSeries> ActiveComposedSeries
    {
        get
        {
            var activeSeries = ActiveSeries;
            return activeSeries is null ? [] : GetVisibleSeries(activeSeries.Kind);
        }
    }

    internal ISurfaceTileSource? ActiveSurfaceSource
    {
        get
        {
            var activeSeries = ActiveSeries;
            return activeSeries?.Kind is Plot3DSeriesKind.Surface or Plot3DSeriesKind.Waterfall
                ? Plot3DSeriesComposition.CreateSurfaceSource(GetVisibleSeries(activeSeries.Kind))
                : null;
        }
    }

    internal ScatterChartData? ActiveScatterData =>
        ActiveSeries?.Kind == Plot3DSeriesKind.Scatter
            ? Plot3DSeriesComposition.CreateScatterData(GetVisibleSeries(Plot3DSeriesKind.Scatter))
            : null;

    internal BarChartData? ActiveBarData =>
        ActiveSeries?.Kind == Plot3DSeriesKind.Bar
            ? Plot3DSeriesComposition.CreateBarData(GetVisibleSeries(Plot3DSeriesKind.Bar))
            : null;

    internal IReadOnlyList<ContourChartData> GetActiveContourData()
    {
        return ActiveSeries?.Kind == Plot3DSeriesKind.Contour
            ? GetVisibleSeries(Plot3DSeriesKind.Contour)
                .Select(static series => series.ContourData)
                .OfType<ContourChartData>()
                .ToArray()
            : [];
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

    internal LineChartData? ActiveLineData =>
        ActiveSeries?.Kind == Plot3DSeriesKind.Line
            ? Plot3DSeriesComposition.CreateLineData(GetVisibleSeries(Plot3DSeriesKind.Line))
            : null;

    internal Plot3DSeries? ActiveLineSeries
    {
        get
        {
            var activeSeries = ActiveSeries;
            return activeSeries?.Kind == Plot3DSeriesKind.Line ? activeSeries : null;
        }
    }

    internal RibbonChartData? ActiveRibbonData =>
        ActiveSeries?.Kind == Plot3DSeriesKind.Ribbon
            ? Plot3DSeriesComposition.CreateRibbonData(GetVisibleSeries(Plot3DSeriesKind.Ribbon))
            : null;

    internal Plot3DSeries? ActiveRibbonSeries
    {
        get
        {
            var activeSeries = ActiveSeries;
            return activeSeries?.Kind == Plot3DSeriesKind.Ribbon ? activeSeries : null;
        }
    }

    internal VectorFieldChartData? ActiveVectorFieldData =>
        ActiveSeries?.Kind == Plot3DSeriesKind.VectorField
            ? Plot3DSeriesComposition.CreateVectorFieldData(GetVisibleSeries(Plot3DSeriesKind.VectorField))
            : null;

    internal Plot3DSeries? ActiveVectorFieldSeries
    {
        get
        {
            var activeSeries = ActiveSeries;
            return activeSeries?.Kind == Plot3DSeriesKind.VectorField ? activeSeries : null;
        }
    }

    internal HeatmapSliceData? ActiveHeatmapSliceData =>
        ActiveSeries?.Kind == Plot3DSeriesKind.HeatmapSlice
            ? Plot3DSeriesComposition.CreateHeatmapSliceData(GetVisibleSeries(Plot3DSeriesKind.HeatmapSlice))
            : null;

    internal Plot3DSeries? ActiveHeatmapSliceSeries
    {
        get
        {
            var activeSeries = ActiveSeries;
            return activeSeries?.Kind == Plot3DSeriesKind.HeatmapSlice ? activeSeries : null;
        }
    }

    internal BoxPlotData? ActiveBoxPlotData =>
        ActiveSeries?.Kind == Plot3DSeriesKind.BoxPlot
            ? Plot3DSeriesComposition.CreateBoxPlotData(GetVisibleSeries(Plot3DSeriesKind.BoxPlot))
            : null;

    internal Plot3DSeries? ActiveBoxPlotSeries
    {
        get
        {
            var activeSeries = ActiveSeries;
            return activeSeries?.Kind == Plot3DSeriesKind.BoxPlot ? activeSeries : null;
        }
    }

    internal HistogramData? ActiveHistogramData =>
        ActiveSeries?.Kind == Plot3DSeriesKind.Histogram ? ActiveSeries.HistogramData : null;

    internal Plot3DSeries? ActiveHistogramSeries
    {
        get
        {
            var activeSeries = ActiveSeries;
            return activeSeries?.Kind == Plot3DSeriesKind.Histogram ? activeSeries : null;
        }
    }

    internal FunctionPlotData? ActiveFunctionPlotData =>
        ActiveSeries?.Kind == Plot3DSeriesKind.FunctionPlot ? ActiveSeries.FunctionPlotData : null;

    internal Plot3DSeries? ActiveFunctionPlotSeries
    {
        get
        {
            var activeSeries = ActiveSeries;
            return activeSeries?.Kind == Plot3DSeriesKind.FunctionPlot ? activeSeries : null;
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
        return Plot3DDatasetEvidence.Create(Revision, _series, _overlayOptions, ActiveComposedSeries);
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
    /// Moves an attached series to a draw-order index.
    /// </summary>
    /// <returns><c>true</c> when the series was attached; otherwise, <c>false</c>.</returns>
    public bool Move(Plot3DSeries series, int index)
    {
        ArgumentNullException.ThrowIfNull(series);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _series.Count);

        var currentIndex = _series.IndexOf(series);
        if (currentIndex < 0)
        {
            return false;
        }

        if (currentIndex == index)
        {
            return true;
        }

        _series.RemoveAt(currentIndex);
        _series.Insert(index, series);
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
        return CreateOutputEvidence(renderingStatus, scatterRenderingStatus, barRenderingStatus, contourRenderingStatus, lineRenderingStatus: null, ribbonRenderingStatus: null);
    }

    /// <summary>
    /// Creates deterministic chart-local output evidence for the current plot model and public rendering status projections.
    /// </summary>
    /// <param name="renderingStatus">The latest surface or waterfall rendering status, when available.</param>
    /// <param name="scatterRenderingStatus">The latest scatter rendering status, when available.</param>
    /// <param name="barRenderingStatus">The latest bar chart rendering status, when available.</param>
    /// <param name="contourRenderingStatus">The latest contour rendering status, when available.</param>
    /// <param name="lineRenderingStatus">The latest line rendering status, when available.</param>
    /// <param name="ribbonRenderingStatus">The latest ribbon rendering status, when available.</param>
    /// <returns>The chart-local output evidence.</returns>
    public Plot3DOutputEvidence CreateOutputEvidence(
        SurfaceChartRenderingStatus? renderingStatus,
        ScatterChartRenderingStatus? scatterRenderingStatus,
        BarChartRenderingStatus? barRenderingStatus,
        ContourChartRenderingStatus? contourRenderingStatus,
        LineChartRenderingStatus? lineRenderingStatus,
        RibbonChartRenderingStatus? ribbonRenderingStatus,
        VectorFieldChartRenderingStatus? vectorFieldRenderingStatus = null,
        HeatmapSliceChartRenderingStatus? heatmapSliceRenderingStatus = null,
        BoxPlotChartRenderingStatus? boxPlotRenderingStatus = null,
        HistogramChartRenderingStatus? histogramRenderingStatus = null,
        FunctionPlotChartRenderingStatus? functionPlotRenderingStatus = null)
    {
        var activeSeries = ActiveSeries;
        var activeSeriesIndex = activeSeries is null ? -1 : _series.IndexOf(activeSeries);
        var composedSeries = ActiveComposedSeries;
        var composedSeriesIdentities = CreateSeriesIdentities(composedSeries);
        var colorMapEvidence = CreateColorMapEvidence(activeSeries);
        var renderingEvidence = CreateRenderingEvidence(activeSeries, renderingStatus, scatterRenderingStatus, barRenderingStatus, contourRenderingStatus, lineRenderingStatus, ribbonRenderingStatus, vectorFieldRenderingStatus, heatmapSliceRenderingStatus, boxPlotRenderingStatus, histogramRenderingStatus, functionPlotRenderingStatus);

        return new Plot3DOutputEvidence(
            seriesCount: _series.Count,
            activeSeriesIndex: activeSeriesIndex,
            activeSeriesName: activeSeries?.Name,
            activeSeriesKind: activeSeries?.Kind,
            activeSeriesIdentity: CreateActiveOutputIdentity(activeSeries, activeSeriesIndex, composedSeriesIdentities),
            composedSeriesCount: composedSeriesIdentities.Count,
            composedSeriesIdentities: composedSeriesIdentities,
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
    /// Sets the internal view-state bridge used by <see cref="Axes"/> to reuse chart-view ownership.
    /// </summary>
    internal void SetViewStateBridge(
        Func<SurfaceDataWindow> getDataWindow,
        Action<SurfaceDataWindow> setDataWindow,
        Action fitToData)
    {
        _getDataWindow = getDataWindow ?? throw new ArgumentNullException(nameof(getDataWindow));
        _setDataWindow = setDataWindow ?? throw new ArgumentNullException(nameof(setDataWindow));
        _fitToData = fitToData ?? throw new ArgumentNullException(nameof(fitToData));
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
            var seriesIdentity = outputEvidence.ActiveSeriesIdentity ?? $"{activeSeries.Kind}:{activeSeries.Name ?? "<unnamed>"}:{_series.IndexOf(activeSeries)}";

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

    /// <summary>
    /// Captures a chart-local PNG snapshot and writes it to the caller-selected path.
    /// </summary>
    /// <param name="path">The caller-selected PNG output path.</param>
    /// <param name="width">The target snapshot width in pixels.</param>
    /// <param name="height">The target snapshot height in pixels.</param>
    /// <param name="scale">The DPI scale factor.</param>
    /// <param name="background">The background behavior for the snapshot.</param>
    /// <returns>A result containing the caller-selected artifact path and manifest on success, or a diagnostic on failure.</returns>
    public async Task<PlotSnapshotResult> SavePngAsync(
        string path,
        int width,
        int height,
        double scale = 1d,
        PlotSnapshotBackground background = PlotSnapshotBackground.Transparent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var request = new PlotSnapshotRequest(width, height, scale, background, PlotSnapshotFormat.Png);
        var result = await CaptureSnapshotAsync(request).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            return result;
        }

        var directory = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        try
        {
            System.IO.File.Copy(result.Path!, path, overwrite: true);
        }
        finally
        {
            System.IO.File.Delete(result.Path!);
        }

        return PlotSnapshotResult.Success(path, result.Manifest!, result.Duration);
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

        series.Attach(NotifyChanged);
        _series.Add(series);
        NotifyChanged();
        return series;
    }

    internal void SetOverlayOptions(SurfaceChartOverlayOptions overlayOptions)
    {
        OverlayOptions = overlayOptions;
    }

    internal PlotAxisLimits? GetHorizontalAxisLimits()
    {
        var dataWindow = _getDataWindow?.Invoke();
        return dataWindow is null
            ? null
            : new PlotAxisLimits(dataWindow.Value.StartX, dataWindow.Value.EndXExclusive);
    }

    internal PlotAxisLimits? GetDepthAxisLimits()
    {
        var dataWindow = _getDataWindow?.Invoke();
        return dataWindow is null
            ? null
            : new PlotAxisLimits(dataWindow.Value.StartY, dataWindow.Value.EndYExclusive);
    }

    internal PlotAxisLimits? GetValueAxisLimits()
    {
        var range = ColorMap?.Range ?? ActiveValueRange;
        return range is null ? null : new PlotAxisLimits(range.Value.Minimum, range.Value.Maximum);
    }

    internal void SetHorizontalAxisLimits(double minimum, double maximum)
    {
        if (_getDataWindow is null || _setDataWindow is null)
        {
            return;
        }

        var current = _getDataWindow();
        _setDataWindow(new SurfaceDataWindow(minimum, current.StartY, maximum - minimum, current.Height));
    }

    internal void SetDepthAxisLimits(double minimum, double maximum)
    {
        if (_getDataWindow is null || _setDataWindow is null)
        {
            return;
        }

        var current = _getDataWindow();
        _setDataWindow(new SurfaceDataWindow(current.StartX, minimum, current.Width, maximum - minimum));
    }

    internal void SetValueAxisLimits(double minimum, double maximum)
    {
        var palette = ColorMap?.Palette ?? SurfaceColorMapPresets.CreateDefault();
        ColorMap = new SurfaceColorMap(new SurfaceValueRange(minimum, maximum), palette);
    }

    internal void AutoScaleHorizontalAxis()
    {
        AutoScaleDataWindowAxis(updateHorizontal: true);
    }

    internal void AutoScaleDepthAxis()
    {
        AutoScaleDataWindowAxis(updateHorizontal: false);
    }

    internal void AutoScaleValueAxis()
    {
        var range = ActiveValueRange;
        if (range is null)
        {
            return;
        }

        var palette = ColorMap?.Palette ?? SurfaceColorMapPresets.CreateDefault();
        ColorMap = new SurfaceColorMap(range.Value, palette);
    }

    private void AutoScaleDataWindowAxis(bool updateHorizontal)
    {
        var metadata = ActiveSurfaceSource?.Metadata;
        if (metadata is null)
        {
            return;
        }

        if (_getDataWindow is null || _setDataWindow is null)
        {
            _fitToData?.Invoke();
            return;
        }

        var current = _getDataWindow();
        var next = updateHorizontal
            ? new SurfaceDataWindow(0d, current.StartY, metadata.Width, current.Height)
            : new SurfaceDataWindow(current.StartX, 0d, current.Width, metadata.Height);
        _setDataWindow(next);
    }

    internal PlotAxisLimits? GetNaturalHorizontalAxisLimits()
    {
        var metadata = ActiveSurfaceSource?.Metadata;
        return metadata is null ? null : new PlotAxisLimits(0d, metadata.Width);
    }

    internal PlotAxisLimits? GetNaturalDepthAxisLimits()
    {
        var metadata = ActiveSurfaceSource?.Metadata;
        return metadata is null ? null : new PlotAxisLimits(0d, metadata.Height);
    }

    internal PlotAxisLimits? GetNaturalValueAxisLimits()
    {
        var range = ActiveValueRange;
        return range is null ? null : new PlotAxisLimits(range.Value.Minimum, range.Value.Maximum);
    }

    private SurfaceValueRange? ActiveValueRange
    {
        get
        {
            var activeSeries = ActiveSeries;
            return activeSeries?.Kind switch
            {
                Plot3DSeriesKind.Surface or Plot3DSeriesKind.Waterfall => ActiveSurfaceSource?.Metadata.ValueRange,
                Plot3DSeriesKind.Scatter => ActiveScatterData?.Metadata.ValueRange,
                Plot3DSeriesKind.Histogram when ActiveHistogramData is not null =>
                    new SurfaceValueRange(ActiveHistogramData.RangeMin, ActiveHistogramData.RangeMax),
                Plot3DSeriesKind.FunctionPlot when ActiveFunctionPlotData is not null =>
                    new SurfaceValueRange(ActiveFunctionPlotData.Ys.Min(), ActiveFunctionPlotData.Ys.Max()),
                _ => null,
            };
        }
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
        ContourChartRenderingStatus? contourRenderingStatus,
        LineChartRenderingStatus? lineRenderingStatus,
        RibbonChartRenderingStatus? ribbonRenderingStatus,
        VectorFieldChartRenderingStatus? vectorFieldRenderingStatus,
        HeatmapSliceChartRenderingStatus? heatmapSliceRenderingStatus,
        BoxPlotChartRenderingStatus? boxPlotRenderingStatus,
        HistogramChartRenderingStatus? histogramRenderingStatus,
        FunctionPlotChartRenderingStatus? functionPlotRenderingStatus)
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
            Plot3DSeriesKind.Line when lineRenderingStatus is not null =>
                Plot3DRenderingEvidence.FromLineStatus(lineRenderingStatus),
            Plot3DSeriesKind.Ribbon when ribbonRenderingStatus is not null =>
                Plot3DRenderingEvidence.FromRibbonStatus(ribbonRenderingStatus),
            Plot3DSeriesKind.VectorField when vectorFieldRenderingStatus is not null =>
                Plot3DRenderingEvidence.FromVectorFieldStatus(vectorFieldRenderingStatus),
            Plot3DSeriesKind.HeatmapSlice when heatmapSliceRenderingStatus is not null =>
                Plot3DRenderingEvidence.FromHeatmapSliceStatus(heatmapSliceRenderingStatus),
            Plot3DSeriesKind.BoxPlot when boxPlotRenderingStatus is not null =>
                Plot3DRenderingEvidence.FromBoxPlotStatus(boxPlotRenderingStatus),
            Plot3DSeriesKind.Histogram when histogramRenderingStatus is not null =>
                Plot3DRenderingEvidence.FromHistogramStatus(histogramRenderingStatus),
            Plot3DSeriesKind.FunctionPlot when functionPlotRenderingStatus is not null =>
                Plot3DRenderingEvidence.FromFunctionPlotStatus(functionPlotRenderingStatus),
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

    internal string CreateSeriesDatasetIdentity(Plot3DSeries series)
    {
        var index = _series.IndexOf(series);
        return index < 0 ? string.Empty : CreateSeriesDatasetIdentity(index, series);
    }

    internal IReadOnlyList<Plot3DSeries> GetVisibleSeries(Plot3DSeriesKind kind)
    {
        var visible = new List<Plot3DSeries>();
        foreach (var series in _series)
        {
            if (series.Kind == kind && series.IsVisible)
            {
                visible.Add(series);
            }
        }

        return visible;
    }

    private IReadOnlyList<string> CreateSeriesIdentities(IReadOnlyList<Plot3DSeries> series)
    {
        if (series.Count == 0)
        {
            return [];
        }

        var identities = new List<string>(series.Count);
        foreach (var item in series)
        {
            var index = _series.IndexOf(item);
            if (index >= 0)
            {
                identities.Add(CreateSeriesDatasetIdentity(index, item));
            }
        }

        return identities;
    }

    private static string? CreateActiveOutputIdentity(
        Plot3DSeries? activeSeries,
        int activeSeriesIndex,
        IReadOnlyList<string> composedSeriesIdentities)
    {
        if (activeSeries is null || activeSeriesIndex < 0)
        {
            return null;
        }

        if (composedSeriesIdentities.Count > 1)
        {
            return $"Composed:{activeSeries.Kind}:{string.Join("|", composedSeriesIdentities)}";
        }

        return CreateSeriesIdentity(activeSeries, activeSeriesIndex);
    }

    internal static string CreateSeriesDatasetIdentity(int index, Plot3DSeries series)
    {
        var name = series.Name ?? "(unnamed)";
        return $"PlotSeries[{index}]:{series.Kind}:{name}";
    }

    private static Plot3DColorMapStatus CreateColorMapStatus(
        Plot3DSeries? activeSeries,
        SurfaceChartOutputEvidence? colorMapEvidence)
    {
        if (colorMapEvidence is not null)
        {
            return Plot3DColorMapStatus.Applied;
        }

        return activeSeries is null || activeSeries.Kind is Plot3DSeriesKind.Scatter or Plot3DSeriesKind.Contour or Plot3DSeriesKind.Bar or Plot3DSeriesKind.VectorField or Plot3DSeriesKind.HeatmapSlice or Plot3DSeriesKind.BoxPlot or Plot3DSeriesKind.Histogram or Plot3DSeriesKind.FunctionPlot
            ? Plot3DColorMapStatus.NotApplicable
            : Plot3DColorMapStatus.Unavailable;
    }

    private void NotifyChanged()
    {
        Revision++;
        Changed();
    }
}
