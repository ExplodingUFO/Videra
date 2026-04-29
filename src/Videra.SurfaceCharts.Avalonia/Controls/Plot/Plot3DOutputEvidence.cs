using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes deterministic chart-local output evidence for a <see cref="Plot3D"/>.
/// </summary>
public sealed class Plot3DOutputEvidence
{
    internal Plot3DOutputEvidence(
        int seriesCount,
        int activeSeriesIndex,
        string? activeSeriesName,
        Plot3DSeriesKind? activeSeriesKind,
        string? activeSeriesIdentity,
        int composedSeriesCount,
        IReadOnlyList<string> composedSeriesIdentities,
        Plot3DColorMapStatus colorMapStatus,
        SurfaceChartOutputEvidence? colorMapEvidence,
        string precisionProfile,
        Plot3DRenderingEvidence? renderingEvidence,
        IReadOnlyList<Plot3DOutputCapabilityDiagnostic> outputCapabilityDiagnostics)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(seriesCount);
        ArgumentOutOfRangeException.ThrowIfNegative(composedSeriesCount);
        ArgumentNullException.ThrowIfNull(composedSeriesIdentities);
        ArgumentException.ThrowIfNullOrWhiteSpace(precisionProfile);
        ArgumentNullException.ThrowIfNull(outputCapabilityDiagnostics);

        EvidenceKind = "plot-3d-output";
        SeriesCount = seriesCount;
        ActiveSeriesIndex = activeSeriesIndex;
        ActiveSeriesName = activeSeriesName;
        ActiveSeriesKind = activeSeriesKind;
        ActiveSeriesIdentity = activeSeriesIdentity;
        ComposedSeriesCount = composedSeriesCount;
        ComposedSeriesIdentities = composedSeriesIdentities.ToArray();
        ColorMapStatus = colorMapStatus;
        ColorMapEvidence = colorMapEvidence;
        PrecisionProfile = precisionProfile;
        RenderingEvidence = renderingEvidence;
        OutputCapabilityDiagnostics = outputCapabilityDiagnostics.ToArray();
    }

    /// <summary>
    /// Gets the stable evidence kind.
    /// </summary>
    public string EvidenceKind { get; }

    /// <summary>
    /// Gets the number of series currently attached to the plot.
    /// </summary>
    public int SeriesCount { get; }

    /// <summary>
    /// Gets the draw-order index of the active series, or <c>-1</c> when the plot is empty.
    /// </summary>
    public int ActiveSeriesIndex { get; }

    /// <summary>
    /// Gets the optional host-facing active series name.
    /// </summary>
    public string? ActiveSeriesName { get; }

    /// <summary>
    /// Gets the active chart family, or <c>null</c> when the plot is empty.
    /// </summary>
    public Plot3DSeriesKind? ActiveSeriesKind { get; }

    /// <summary>
    /// Gets a deterministic active-series identity composed from kind, name, and draw-order index.
    /// </summary>
    public string? ActiveSeriesIdentity { get; }

    /// <summary>
    /// Gets the number of visible same-kind series composing the active output.
    /// </summary>
    public int ComposedSeriesCount { get; }

    /// <summary>
    /// Gets deterministic Plot-local identities for the visible series composing the active output.
    /// </summary>
    public IReadOnlyList<string> ComposedSeriesIdentities { get; }

    /// <summary>
    /// Gets whether color-map evidence applies to the active output.
    /// </summary>
    public Plot3DColorMapStatus ColorMapStatus { get; }

    /// <summary>
    /// Gets surface or waterfall color-map evidence when the active output uses a color map.
    /// </summary>
    public SurfaceChartOutputEvidence? ColorMapEvidence { get; }

    /// <summary>
    /// Gets the chart-local numeric precision profile used for output labels.
    /// </summary>
    public string PrecisionProfile { get; }

    /// <summary>
    /// Gets public rendering-status evidence for the active output when supplied by the caller.
    /// </summary>
    public Plot3DRenderingEvidence? RenderingEvidence { get; }

    /// <summary>
    /// Gets explicit diagnostics for bounded output capabilities.
    /// </summary>
    public IReadOnlyList<Plot3DOutputCapabilityDiagnostic> OutputCapabilityDiagnostics { get; }
}

/// <summary>
/// Describes whether color-map evidence applies to a plot output.
/// </summary>
public enum Plot3DColorMapStatus
{
    /// <summary>Color-map evidence was produced for the active output.</summary>
    Applied,

    /// <summary>The active output does not use a color map.</summary>
    NotApplicable,

    /// <summary>The active output should use a color map, but evidence could not be produced.</summary>
    Unavailable,
}

/// <summary>
/// Describes a chart-local output capability diagnostic.
/// </summary>
public sealed class Plot3DOutputCapabilityDiagnostic
{
    internal Plot3DOutputCapabilityDiagnostic(
        string capability,
        bool isSupported,
        string diagnosticCode,
        string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(capability);
        ArgumentException.ThrowIfNullOrWhiteSpace(diagnosticCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Capability = capability;
        IsSupported = isSupported;
        DiagnosticCode = diagnosticCode;
        Message = message;
    }

    /// <summary>
    /// Gets the output capability name.
    /// </summary>
    public string Capability { get; }

    /// <summary>
    /// Gets whether the capability is supported by this bounded report contract.
    /// </summary>
    public bool IsSupported { get; }

    /// <summary>
    /// Gets a deterministic diagnostic code.
    /// </summary>
    public string DiagnosticCode { get; }

    /// <summary>
    /// Gets a host-facing diagnostic message.
    /// </summary>
    public string Message { get; }

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

    private static Plot3DOutputCapabilityDiagnostic Unsupported(
        string capability,
        string diagnosticCode,
        string message)
    {
        return new Plot3DOutputCapabilityDiagnostic(capability, isSupported: false, diagnosticCode, message);
    }
}

/// <summary>
/// Describes public rendering-status evidence for the active plot output.
/// </summary>
public sealed class Plot3DRenderingEvidence
{
    private Plot3DRenderingEvidence(
        string renderingKind,
        SurfaceChartRenderBackendKind backendKind,
        bool isReady,
        bool isFallback,
        string? fallbackReason,
        double viewWidth,
        double viewHeight)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(renderingKind);

        RenderingKind = renderingKind;
        BackendKind = backendKind;
        IsReady = isReady;
        IsFallback = isFallback;
        FallbackReason = fallbackReason;
        ViewWidth = viewWidth;
        ViewHeight = viewHeight;
    }

    /// <summary>
    /// Gets the rendering evidence family.
    /// </summary>
    public string RenderingKind { get; }

    /// <summary>
    /// Gets the public backend kind used by the active output.
    /// </summary>
    public SurfaceChartRenderBackendKind BackendKind { get; }

    /// <summary>
    /// Gets whether the active output is ready to render.
    /// </summary>
    public bool IsReady { get; }

    /// <summary>
    /// Gets whether the public rendering status reports fallback behavior.
    /// </summary>
    public bool IsFallback { get; }

    /// <summary>
    /// Gets the public fallback reason when one is reported.
    /// </summary>
    public string? FallbackReason { get; }

    /// <summary>
    /// Gets the public arranged output width when available.
    /// </summary>
    public double ViewWidth { get; }

    /// <summary>
    /// Gets the public arranged output height when available.
    /// </summary>
    public double ViewHeight { get; }

    internal static Plot3DRenderingEvidence FromSurfaceStatus(SurfaceChartRenderingStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);

        return new Plot3DRenderingEvidence(
            "surface-rendering-status",
            status.ActiveBackend,
            status.IsReady,
            status.IsFallback,
            status.FallbackReason,
            viewWidth: 0d,
            viewHeight: 0d);
    }

    internal static Plot3DRenderingEvidence FromScatterStatus(ScatterChartRenderingStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);

        return new Plot3DRenderingEvidence(
            "scatter-rendering-status",
            status.BackendKind,
            status.IsReady,
            isFallback: false,
            fallbackReason: null,
            viewWidth: status.ViewSize.Width,
            viewHeight: status.ViewSize.Height);
    }

    internal static Plot3DRenderingEvidence FromBarStatus(BarChartRenderingStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);

        return new Plot3DRenderingEvidence(
            "bar-rendering-status",
            status.BackendKind,
            status.IsReady,
            isFallback: false,
            fallbackReason: null,
            viewWidth: status.ViewSize.Width,
            viewHeight: status.ViewSize.Height);
    }

    internal static Plot3DRenderingEvidence FromContourStatus(ContourChartRenderingStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);

        return new Plot3DRenderingEvidence(
            "contour-rendering-status",
            SurfaceChartRenderBackendKind.Software,
            status.IsReady,
            isFallback: false,
            fallbackReason: null,
            viewWidth: 0d,
            viewHeight: 0d);
    }
}
