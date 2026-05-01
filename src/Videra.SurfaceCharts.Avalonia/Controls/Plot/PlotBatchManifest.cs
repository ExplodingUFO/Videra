using System.Text.Json;
using System.Text.Json.Serialization;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// JSON manifest describing the results of a batch export operation.
/// Designed for CI integration: includes per-chart metadata, timestamps, and pass/fail status.
/// </summary>
public sealed class PlotBatchManifest
{
    /// <summary>
    /// Initializes a new <see cref="PlotBatchManifest"/> with the specified batch results.
    /// </summary>
    public PlotBatchManifest(
        IReadOnlyList<PlotBatchManifestEntry> entries,
        DateTime startedUtc,
        DateTime completedUtc)
    {
        ArgumentNullException.ThrowIfNull(entries);

        Entries = entries;
        StartedUtc = startedUtc;
        CompletedUtc = completedUtc;
        TotalCount = entries.Count;
        SuccessCount = entries.Count(static e => e.Succeeded);
        FailureCount = entries.Count(static e => !e.Succeeded);
    }

    /// <summary>
    /// Gets the per-chart export entries.
    /// </summary>
    public IReadOnlyList<PlotBatchManifestEntry> Entries { get; }

    /// <summary>
    /// Gets the UTC timestamp when the batch export started.
    /// </summary>
    public DateTime StartedUtc { get; }

    /// <summary>
    /// Gets the UTC timestamp when the batch export completed.
    /// </summary>
    public DateTime CompletedUtc { get; }

    /// <summary>
    /// Gets the total number of charts in the batch.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Gets the number of successfully exported charts.
    /// </summary>
    public int SuccessCount { get; }

    /// <summary>
    /// Gets the number of failed chart exports.
    /// </summary>
    public int FailureCount { get; }

    /// <summary>
    /// Gets whether all charts in the batch exported successfully.
    /// </summary>
    public bool AllSucceeded => FailureCount == 0;

    /// <summary>
    /// Serializes this manifest to JSON.
    /// </summary>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, PlotBatchManifestSerializerContext.Default.PlotBatchManifest);
    }

    /// <summary>
    /// Deserializes a manifest from JSON.
    /// </summary>
    public static PlotBatchManifest? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        return JsonSerializer.Deserialize(json, PlotBatchManifestSerializerContext.Default.PlotBatchManifest);
    }

    /// <summary>
    /// Saves this manifest to a file as formatted JSON.
    /// </summary>
    public async Task SaveAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(this, PlotBatchManifestSerializerContext.Default.PlotBatchManifest);
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        var formattedJson = JsonSerializer.Serialize(
            JsonSerializer.Deserialize(json, PlotBatchManifestSerializerContext.Default.PlotBatchManifest),
            new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, formattedJson).ConfigureAwait(false);
    }
}

/// <summary>
/// A single entry in a batch export manifest describing one chart export result.
/// </summary>
public sealed class PlotBatchManifestEntry
{
    /// <summary>
    /// Initializes a new <see cref="PlotBatchManifestEntry"/>.
    /// </summary>
    public PlotBatchManifestEntry(
        string name,
        string? outputPath,
        string? format,
        int width,
        int height,
        bool succeeded,
        string? errorMessage,
        string? activeSeriesIdentity,
        TimeSpan duration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        OutputPath = outputPath;
        Format = format;
        Width = width;
        Height = height;
        Succeeded = succeeded;
        ErrorMessage = errorMessage;
        ActiveSeriesIdentity = activeSeriesIdentity;
        Duration = duration;
    }

    /// <summary>
    /// Gets the chart name used in the naming pattern.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the output file path, or <c>null</c> when the export failed.
    /// </summary>
    public string? OutputPath { get; }

    /// <summary>
    /// Gets the export format (e.g., "Png", "Svg").
    /// </summary>
    public string? Format { get; }

    /// <summary>
    /// Gets the export width in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the export height in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets whether this chart exported successfully.
    /// </summary>
    public bool Succeeded { get; }

    /// <summary>
    /// Gets the error message when the export failed, or <c>null</c> on success.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Gets the active series identity at the time of export.
    /// </summary>
    public string? ActiveSeriesIdentity { get; }

    /// <summary>
    /// Gets the time taken to export this chart.
    /// </summary>
    public TimeSpan Duration { get; }
}

/// <summary>
/// JSON source generator context for AOT-compatible serialization.
/// </summary>
[JsonSerializable(typeof(PlotBatchManifest))]
[JsonSerializable(typeof(PlotBatchManifestEntry))]
internal partial class PlotBatchManifestSerializerContext : JsonSerializerContext;
