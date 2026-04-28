using System.Globalization;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Describes deterministic chart-local evidence for rendered surface-chart output.
/// </summary>
public sealed class SurfaceChartOutputEvidence
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceChartOutputEvidence"/> class.
    /// </summary>
    /// <param name="evidenceKind">The evidence kind.</param>
    /// <param name="paletteName">The palette name.</param>
    /// <param name="colorStops">The palette color stops in ARGB hex notation.</param>
    /// <param name="precisionProfile">The numeric precision profile used for labels.</param>
    /// <param name="sampleFormattedLabels">Representative labels formatted with the precision profile.</param>
    /// <exception cref="ArgumentException">Thrown when a string argument is blank.</exception>
    /// <exception cref="ArgumentNullException">Thrown when a collection argument is <c>null</c>.</exception>
    public SurfaceChartOutputEvidence(
        string evidenceKind,
        string paletteName,
        IReadOnlyList<string> colorStops,
        string precisionProfile,
        IReadOnlyList<string> sampleFormattedLabels)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(evidenceKind);
        ArgumentException.ThrowIfNullOrWhiteSpace(paletteName);
        ArgumentNullException.ThrowIfNull(colorStops);
        ArgumentException.ThrowIfNullOrWhiteSpace(precisionProfile);
        ArgumentNullException.ThrowIfNull(sampleFormattedLabels);

        EvidenceKind = evidenceKind;
        PaletteName = paletteName;
        ColorStops = colorStops.ToArray();
        PrecisionProfile = precisionProfile;
        SampleFormattedLabels = sampleFormattedLabels.ToArray();
    }

    /// <summary>
    /// Gets the evidence kind.
    /// </summary>
    public string EvidenceKind { get; }

    /// <summary>
    /// Gets the palette name.
    /// </summary>
    public string PaletteName { get; }

    /// <summary>
    /// Gets the palette color stops in ARGB hex notation.
    /// </summary>
    public IReadOnlyList<string> ColorStops { get; }

    /// <summary>
    /// Gets the numeric precision profile used for labels.
    /// </summary>
    public string PrecisionProfile { get; }

    /// <summary>
    /// Gets representative labels formatted with the precision profile.
    /// </summary>
    public IReadOnlyList<string> SampleFormattedLabels { get; }
}

/// <summary>
/// Formats deterministic SurfaceCharts output evidence without coupling to a renderer or file format.
/// </summary>
public static class SurfaceChartEvidenceFormatter
{
    private const string EvidenceKind = "SurfaceChartOutputEvidence";
    private const string PrecisionProfile = "InvariantCulture:G6";

    /// <summary>
    /// Creates output evidence for a color map using its range minimum, midpoint, and maximum as sample labels.
    /// </summary>
    /// <param name="paletteName">The palette name.</param>
    /// <param name="colorMap">The color map that supplies palette and range semantics.</param>
    /// <returns>The formatted output evidence.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="paletteName"/> is blank.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="colorMap"/> is <c>null</c>.</exception>
    public static SurfaceChartOutputEvidence Create(string paletteName, SurfaceColorMap colorMap)
    {
        ArgumentNullException.ThrowIfNull(colorMap);

        return Create(
            paletteName,
            colorMap.Palette,
            colorMap.Range.Minimum,
            GetMidpoint(colorMap.Range),
            colorMap.Range.Maximum);
    }

    /// <summary>
    /// Creates output evidence for a color map using the supplied precision profile and label formatter.
    /// </summary>
    /// <param name="paletteName">The palette name.</param>
    /// <param name="colorMap">The color map that supplies palette and range semantics.</param>
    /// <param name="precisionProfile">The deterministic precision profile description.</param>
    /// <param name="sampleLabelFormatter">The formatter used for sample labels.</param>
    /// <returns>The formatted output evidence.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="paletteName"/> or <paramref name="precisionProfile"/> is blank.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="colorMap"/> or <paramref name="sampleLabelFormatter"/> is <c>null</c>.</exception>
    public static SurfaceChartOutputEvidence Create(
        string paletteName,
        SurfaceColorMap colorMap,
        string precisionProfile,
        Func<double, string> sampleLabelFormatter)
    {
        ArgumentNullException.ThrowIfNull(colorMap);

        return Create(
            paletteName,
            colorMap.Palette,
            precisionProfile,
            sampleLabelFormatter,
            colorMap.Range.Minimum,
            GetMidpoint(colorMap.Range),
            colorMap.Range.Maximum);
    }

    /// <summary>
    /// Creates output evidence for a palette and explicit sample label values.
    /// </summary>
    /// <param name="paletteName">The palette name.</param>
    /// <param name="palette">The palette that supplies output color stops.</param>
    /// <param name="sampleValues">Representative numeric values to format as labels.</param>
    /// <returns>The formatted output evidence.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="paletteName"/> is blank.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="palette"/> or <paramref name="sampleValues"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a sample value is not finite.</exception>
    public static SurfaceChartOutputEvidence Create(
        string paletteName,
        SurfaceColorMapPalette palette,
        params double[] sampleValues)
    {
        return Create(paletteName, palette, PrecisionProfile, FormatSampleLabel, sampleValues);
    }

    /// <summary>
    /// Creates output evidence for a palette and explicit sample label values using the supplied precision profile.
    /// </summary>
    /// <param name="paletteName">The palette name.</param>
    /// <param name="palette">The palette that supplies output color stops.</param>
    /// <param name="precisionProfile">The deterministic precision profile description.</param>
    /// <param name="sampleLabelFormatter">The formatter used for sample labels.</param>
    /// <param name="sampleValues">Representative numeric values to format as labels.</param>
    /// <returns>The formatted output evidence.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="paletteName"/> or <paramref name="precisionProfile"/> is blank.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="palette"/>, <paramref name="sampleLabelFormatter"/>, or <paramref name="sampleValues"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a sample value is not finite.</exception>
    public static SurfaceChartOutputEvidence Create(
        string paletteName,
        SurfaceColorMapPalette palette,
        string precisionProfile,
        Func<double, string> sampleLabelFormatter,
        params double[] sampleValues)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paletteName);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentException.ThrowIfNullOrWhiteSpace(precisionProfile);
        ArgumentNullException.ThrowIfNull(sampleLabelFormatter);
        ArgumentNullException.ThrowIfNull(sampleValues);

        var colorStops = new string[palette.Count];
        for (var index = 0; index < colorStops.Length; index++)
        {
            colorStops[index] = FormatColorStop(palette[index]);
        }

        var sampleFormattedLabels = new string[sampleValues.Length];
        for (var index = 0; index < sampleValues.Length; index++)
        {
            sampleFormattedLabels[index] = FormatSampleLabel(sampleValues[index], sampleLabelFormatter);
        }

        return new SurfaceChartOutputEvidence(
            EvidenceKind,
            paletteName,
            colorStops,
            precisionProfile,
            sampleFormattedLabels);
    }

    private static string FormatColorStop(uint color)
    {
        return string.Create(CultureInfo.InvariantCulture, $"#{color:X8}");
    }

    private static string FormatSampleLabel(double value)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Surface chart evidence sample labels must be finite.");
        }

        return value.ToString("G6", CultureInfo.InvariantCulture);
    }

    private static string FormatSampleLabel(double value, Func<double, string> sampleLabelFormatter)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Surface chart evidence sample labels must be finite.");
        }

        return sampleLabelFormatter(value);
    }

    private static double GetMidpoint(SurfaceValueRange range)
    {
        return (range.Minimum / 2.0) + (range.Maximum / 2.0);
    }
}
