using System.Collections.ObjectModel;
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
        var activeSeries = ActiveSeries;
        var activeSeriesIndex = activeSeries is null ? -1 : _series.IndexOf(activeSeries);
        var colorMapEvidence = CreateColorMapEvidence(activeSeries);
        var renderingEvidence = CreateRenderingEvidence(activeSeries, renderingStatus, scatterRenderingStatus);

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
        ScatterChartRenderingStatus? scatterRenderingStatus)
    {
        return activeSeries?.Kind switch
        {
            Plot3DSeriesKind.Surface or Plot3DSeriesKind.Waterfall when renderingStatus is not null =>
                Plot3DRenderingEvidence.FromSurfaceStatus(renderingStatus),
            Plot3DSeriesKind.Scatter when scatterRenderingStatus is not null =>
                Plot3DRenderingEvidence.FromScatterStatus(scatterRenderingStatus),
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

        return activeSeries is null || activeSeries.Kind == Plot3DSeriesKind.Scatter
            ? Plot3DColorMapStatus.NotApplicable
            : Plot3DColorMapStatus.Unavailable;
    }

    private void NotifyChanged()
    {
        Revision++;
        Changed();
    }
}
