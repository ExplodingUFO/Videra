using System.Text;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes chart-local probe evidence for a surface chart.
/// </summary>
public sealed class SurfaceChartProbeEvidence
{
    internal SurfaceChartProbeEvidence(
        SurfaceChartProbeEvidenceStatus probeStatus,
        string? hoveredProbeReadout,
        IReadOnlyList<string> pinnedProbeReadouts,
        string? deltaVsFirstPinReadout)
    {
        ArgumentNullException.ThrowIfNull(pinnedProbeReadouts);

        ProbeStatus = probeStatus;
        HoveredProbeReadout = hoveredProbeReadout;
        PinnedProbeReadouts = pinnedProbeReadouts;
        PinnedProbeCount = pinnedProbeReadouts.Count;
        DeltaVsFirstPinReadout = deltaVsFirstPinReadout;
    }

    /// <summary>
    /// Gets the stable evidence kind for surface-chart probe evidence.
    /// </summary>
    public string EvidenceKind { get; } = "surface-chart-probe";

    /// <summary>
    /// Gets the resolved probe evidence status.
    /// </summary>
    public SurfaceChartProbeEvidenceStatus ProbeStatus { get; }

    /// <summary>
    /// Gets the hovered probe readout, when a hovered probe is resolved.
    /// </summary>
    public string? HoveredProbeReadout { get; }

    /// <summary>
    /// Gets the number of resolved pinned probes.
    /// </summary>
    public int PinnedProbeCount { get; }

    /// <summary>
    /// Gets formatted readouts for each resolved pinned probe.
    /// </summary>
    public IReadOnlyList<string> PinnedProbeReadouts { get; }

    /// <summary>
    /// Gets the hovered-probe delta against the first pinned probe, when both are available.
    /// </summary>
    public string? DeltaVsFirstPinReadout { get; }
}

/// <summary>
/// Describes which probe evidence is present for a surface chart.
/// </summary>
public enum SurfaceChartProbeEvidenceStatus
{
    /// <summary>No hovered or pinned probe evidence is available.</summary>
    Empty,

    /// <summary>A hovered probe is available.</summary>
    Hovered,

    /// <summary>One or more pinned probes are available.</summary>
    Pinned,

    /// <summary>A hovered probe and one or more pinned probes are available.</summary>
    HoveredAndPinned,
}

/// <summary>
/// Creates deterministic chart-local output evidence from overlay presentation options.
/// </summary>
public static class SurfaceChartOverlayEvidenceFormatter
{
    /// <summary>
    /// Creates output evidence for a color map using legend label semantics from the overlay options.
    /// </summary>
    /// <param name="paletteName">The palette name.</param>
    /// <param name="colorMap">The color map that supplies palette and range semantics.</param>
    /// <param name="overlayOptions">The chart-local overlay formatting options.</param>
    /// <returns>The formatted output evidence.</returns>
    public static SurfaceChartOutputEvidence Create(
        string paletteName,
        SurfaceColorMap colorMap,
        SurfaceChartOverlayOptions? overlayOptions = null)
    {
        overlayOptions ??= SurfaceChartOverlayOptions.Default;

        return SurfaceChartEvidenceFormatter.Create(
            paletteName,
            colorMap,
            DescribePrecisionProfile(overlayOptions),
            value => overlayOptions.FormatLabel("Legend", value));
    }

    /// <summary>
    /// Creates output evidence for a palette and explicit sample label values using legend label semantics from the overlay options.
    /// </summary>
    /// <param name="paletteName">The palette name.</param>
    /// <param name="palette">The palette that supplies output color stops.</param>
    /// <param name="overlayOptions">The chart-local overlay formatting options.</param>
    /// <param name="sampleValues">Representative numeric values to format as labels.</param>
    /// <returns>The formatted output evidence.</returns>
    public static SurfaceChartOutputEvidence Create(
        string paletteName,
        SurfaceColorMapPalette palette,
        SurfaceChartOverlayOptions? overlayOptions = null,
        params double[] sampleValues)
    {
        overlayOptions ??= SurfaceChartOverlayOptions.Default;

        return SurfaceChartEvidenceFormatter.Create(
            paletteName,
            palette,
            DescribePrecisionProfile(overlayOptions),
            value => overlayOptions.FormatLabel("Legend", value),
            sampleValues);
    }

    /// <summary>
    /// Creates a deterministic precision profile description for overlay-backed output evidence.
    /// </summary>
    /// <param name="overlayOptions">The chart-local overlay formatting options.</param>
    /// <returns>The precision profile description.</returns>
    public static string DescribePrecisionProfile(SurfaceChartOverlayOptions? overlayOptions = null)
    {
        overlayOptions ??= SurfaceChartOverlayOptions.Default;

        var formatterProfile = overlayOptions.LabelFormatter is null ? "Default" : "Custom";
        return $"SurfaceChartOverlayOptions:" +
            $"Tick={overlayOptions.TickLabelFormat}({SurfaceChartOverlayOptions.NormalizePrecision(overlayOptions.TickLabelPrecision)});" +
            $"Legend={overlayOptions.LegendLabelFormat}({SurfaceChartOverlayOptions.NormalizePrecision(overlayOptions.LegendLabelPrecision)});" +
            $"Formatter={formatterProfile}";
    }
}

/// <summary>
/// Creates deterministic chart-local probe evidence readouts.
/// </summary>
public static class SurfaceChartProbeEvidenceFormatter
{
    /// <summary>
    /// Creates a chart-local probe evidence model from resolved hover and pinned probes.
    /// </summary>
    /// <param name="hoveredProbe">The currently hovered probe, if one is resolved.</param>
    /// <param name="pinnedProbes">The currently resolved pinned probes.</param>
    /// <param name="overlayOptions">The chart-local overlay formatting options.</param>
    /// <returns>The formatted probe evidence model.</returns>
    public static SurfaceChartProbeEvidence Create(
        SurfaceProbeInfo? hoveredProbe,
        IReadOnlyList<SurfaceProbeInfo> pinnedProbes,
        SurfaceChartOverlayOptions? overlayOptions = null)
    {
        ArgumentNullException.ThrowIfNull(pinnedProbes);
        overlayOptions ??= SurfaceChartOverlayOptions.Default;

        var pinnedProbeReadouts = new string[pinnedProbes.Count];
        for (var index = 0; index < pinnedProbes.Count; index++)
        {
            pinnedProbeReadouts[index] = CreatePinnedReadout(index + 1, pinnedProbes[index], overlayOptions);
        }

        var hoveredProbeReadout = hoveredProbe is SurfaceProbeInfo probe
            ? CreateProbeReadout(probe, overlayOptions, separator: ", ")
            : null;
        var deltaVsFirstPinReadout = hoveredProbe is SurfaceProbeInfo hovered && pinnedProbes.Count > 0
            ? CreateDeltaReadout(hovered, pinnedProbes[0], overlayOptions)
            : null;

        return new SurfaceChartProbeEvidence(
            CreateStatus(hoveredProbe, pinnedProbes.Count),
            hoveredProbeReadout,
            Array.AsReadOnly(pinnedProbeReadouts),
            deltaVsFirstPinReadout);
    }

    /// <summary>
    /// Formats a chart-local probe evidence model as a deterministic text block.
    /// </summary>
    /// <param name="evidence">The probe evidence model.</param>
    /// <returns>The formatted evidence text.</returns>
    public static string Format(SurfaceChartProbeEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        StringBuilder builder = new();
        builder.Append("EvidenceKind: ").Append(evidence.EvidenceKind).Append('\n');
        builder.Append("ProbeStatus: ").Append(evidence.ProbeStatus.ToString()).Append('\n');
        builder.Append("PinnedCount: ").Append(evidence.PinnedProbeCount.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append('\n');

        if (!string.IsNullOrWhiteSpace(evidence.HoveredProbeReadout))
        {
            builder.Append("Hovered: ").Append(evidence.HoveredProbeReadout).Append('\n');
        }

        foreach (var pinnedReadout in evidence.PinnedProbeReadouts)
        {
            builder.Append("Pinned: ").Append(pinnedReadout).Append('\n');
        }

        if (!string.IsNullOrWhiteSpace(evidence.DeltaVsFirstPinReadout))
        {
            builder.Append("DeltaVsFirstPin: ").Append(evidence.DeltaVsFirstPinReadout);
        }

        return builder.ToString().TrimEnd();
    }

    private static SurfaceChartProbeEvidenceStatus CreateStatus(SurfaceProbeInfo? hoveredProbe, int pinnedProbeCount)
    {
        if (hoveredProbe is not null && pinnedProbeCount > 0)
        {
            return SurfaceChartProbeEvidenceStatus.HoveredAndPinned;
        }

        if (hoveredProbe is not null)
        {
            return SurfaceChartProbeEvidenceStatus.Hovered;
        }

        return pinnedProbeCount > 0
            ? SurfaceChartProbeEvidenceStatus.Pinned
            : SurfaceChartProbeEvidenceStatus.Empty;
    }

    private static string CreateProbeReadout(
        SurfaceProbeInfo probe,
        SurfaceChartOverlayOptions overlayOptions,
        string separator)
    {
        return $"X {overlayOptions.FormatProbeAxisX(probe.AxisX)} (sample {overlayOptions.FormatProbeAxisX(probe.SampleX)}){separator}" +
            $"Y {overlayOptions.FormatProbeAxisY(probe.AxisY)} (sample {overlayOptions.FormatProbeAxisY(probe.SampleY)}){separator}" +
            $"Value {overlayOptions.FormatProbeValue(probe.Value)} {(probe.IsApproximate ? "Approx" : "Exact")}";
    }

    internal static string CreateHoveredOverlayReadout(
        SurfaceProbeInfo probe,
        IReadOnlyList<SurfaceProbeInfo> pinnedProbes,
        SurfaceChartOverlayOptions overlayOptions)
    {
        var readout = CreateProbeReadout(probe, overlayOptions, separator: "\n");
        if (pinnedProbes.Count == 0)
        {
            return readout;
        }

        return $"{readout}\n{CreateDeltaReadout(probe, pinnedProbes[0], overlayOptions)}";
    }

    internal static string CreatePinnedOverlayReadout(
        int pinnedIndex,
        SurfaceProbeInfo probe,
        SurfaceChartOverlayOptions overlayOptions)
    {
        return CreatePinnedReadout(pinnedIndex, probe, overlayOptions);
    }

    private static string CreatePinnedReadout(
        int pinnedIndex,
        SurfaceProbeInfo probe,
        SurfaceChartOverlayOptions overlayOptions)
    {
        return $"Pin {pinnedIndex} {(probe.IsApproximate ? "Approx" : "Exact")}\n" +
            $"X {overlayOptions.FormatProbeAxisX(probe.AxisX)} (sample {overlayOptions.FormatProbeAxisX(probe.SampleX)})\n" +
            $"Y {overlayOptions.FormatProbeAxisY(probe.AxisY)} (sample {overlayOptions.FormatProbeAxisY(probe.SampleY)})\n" +
            $"Value {overlayOptions.FormatProbeValue(probe.Value)}";
    }

    private static string CreateDeltaReadout(
        SurfaceProbeInfo probe,
        SurfaceProbeInfo referenceProbe,
        SurfaceChartOverlayOptions overlayOptions)
    {
        var deltaX = probe.AxisX - referenceProbe.AxisX;
        var deltaY = probe.AxisY - referenceProbe.AxisY;
        var deltaValue = probe.Value - referenceProbe.Value;

        return $"Delta vs Pin 1\nX {overlayOptions.FormatProbeDelta("X", deltaX)}\nY {overlayOptions.FormatProbeDelta("Z", deltaY)}\nValue {overlayOptions.FormatProbeDelta("Y", deltaValue)}";
    }
}
