namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes one chart to export in a batch operation.
/// </summary>
/// <param name="Plot">The plot to export.</param>
/// <param name="Name">A short name used in the output filename pattern.</param>
public sealed record PlotBatchItem(Plot3D Plot, string Name);

/// <summary>
/// Exports multiple charts in a single batch operation with a JSON manifest for CI integration.
/// </summary>
public static class PlotBatchExporter
{
    /// <summary>
    /// Exports multiple charts to the specified directory using a naming pattern.
    /// </summary>
    /// <param name="items">The charts to export.</param>
    /// <param name="outputDirectory">The directory to write exported files.</param>
    /// <param name="request">The snapshot request template applied to all charts.</param>
    /// <param name="namingPattern">
    /// The filename pattern. Supported tokens:
    /// <c>{name}</c> = chart name, <c>{format}</c> = format extension (png/svg), <c>{index}</c> = zero-based index.
    /// Default: <c>"{name}.{format}"</c>
    /// </param>
    /// <returns>A batch manifest describing the results of all exports.</returns>
    public static async Task<PlotBatchManifest> ExportAsync(
        IReadOnlyList<PlotBatchItem> items,
        string outputDirectory,
        PlotSnapshotRequest request,
        string namingPattern = "{name}.{format}")
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(namingPattern);

        Directory.CreateDirectory(outputDirectory);

        var startedUtc = DateTime.UtcNow;
        var entries = new List<PlotBatchManifestEntry>(items.Count);

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var entryStopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var extension = request.Format == PlotSnapshotFormat.Svg ? "svg" : "png";
                var fileName = namingPattern
                    .Replace("{name}", SanitizeFileName(item.Name))
                    .Replace("{format}", extension)
                    .Replace("{index}", index.ToString(System.Globalization.CultureInfo.InvariantCulture));

                var outputPath = Path.Combine(outputDirectory, fileName);

                PlotSnapshotResult result;
                if (request.Format == PlotSnapshotFormat.Svg)
                {
                    result = await item.Plot.SaveSvgAsync(
                        outputPath,
                        request.Width,
                        request.Height,
                        request.Scale,
                        request.Background).ConfigureAwait(false);
                }
                else
                {
                    result = await item.Plot.SavePngAsync(
                        outputPath,
                        request.Width,
                        request.Height,
                        request.Scale,
                        request.Background).ConfigureAwait(false);
                }

                entryStopwatch.Stop();

                if (result.Succeeded)
                {
                    entries.Add(new PlotBatchManifestEntry(
                        name: item.Name,
                        outputPath: outputPath,
                        format: request.Format.ToString(),
                        width: request.Width,
                        height: request.Height,
                        succeeded: true,
                        errorMessage: null,
                        activeSeriesIdentity: result.Manifest?.ActiveSeriesIdentity,
                        duration: entryStopwatch.Elapsed));
                }
                else
                {
                    entries.Add(new PlotBatchManifestEntry(
                        name: item.Name,
                        outputPath: null,
                        format: request.Format.ToString(),
                        width: request.Width,
                        height: request.Height,
                        succeeded: false,
                        errorMessage: result.Failure?.Message ?? "Unknown error",
                        activeSeriesIdentity: null,
                        duration: entryStopwatch.Elapsed));
                }
            }
            catch (Exception ex)
            {
                entryStopwatch.Stop();
                entries.Add(new PlotBatchManifestEntry(
                    name: item.Name,
                    outputPath: null,
                    format: request.Format.ToString(),
                    width: request.Width,
                    height: request.Height,
                    succeeded: false,
                    errorMessage: ex.Message,
                    activeSeriesIdentity: null,
                    duration: entryStopwatch.Elapsed));
            }
        }

        var completedUtc = DateTime.UtcNow;
        var manifest = new PlotBatchManifest(entries, startedUtc, completedUtc);

        // Write manifest alongside exports
        var manifestPath = Path.Combine(outputDirectory, "manifest.json");
        await manifest.SaveAsync(manifestPath).ConfigureAwait(false);

        return manifest;
    }

    /// <summary>
    /// Exports multiple charts with per-chart format selection.
    /// </summary>
    public static async Task<PlotBatchManifest> ExportAsync(
        IReadOnlyList<(PlotBatchItem Item, PlotSnapshotRequest Request)> items,
        string outputDirectory,
        string namingPattern = "{name}.{format}")
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(namingPattern);

        Directory.CreateDirectory(outputDirectory);

        var startedUtc = DateTime.UtcNow;
        var entries = new List<PlotBatchManifestEntry>(items.Count);

        for (var index = 0; index < items.Count; index++)
        {
            var (item, request) = items[index];
            var entryStopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var extension = request.Format == PlotSnapshotFormat.Svg ? "svg" : "png";
                var fileName = namingPattern
                    .Replace("{name}", SanitizeFileName(item.Name))
                    .Replace("{format}", extension)
                    .Replace("{index}", index.ToString(System.Globalization.CultureInfo.InvariantCulture));

                var outputPath = Path.Combine(outputDirectory, fileName);

                PlotSnapshotResult result;
                if (request.Format == PlotSnapshotFormat.Svg)
                {
                    result = await item.Plot.SaveSvgAsync(
                        outputPath,
                        request.Width,
                        request.Height,
                        request.Scale,
                        request.Background).ConfigureAwait(false);
                }
                else
                {
                    result = await item.Plot.SavePngAsync(
                        outputPath,
                        request.Width,
                        request.Height,
                        request.Scale,
                        request.Background).ConfigureAwait(false);
                }

                entryStopwatch.Stop();

                entries.Add(result.Succeeded
                    ? new PlotBatchManifestEntry(
                        item.Name, outputPath, request.Format.ToString(),
                        request.Width, request.Height, true, null,
                        result.Manifest?.ActiveSeriesIdentity, entryStopwatch.Elapsed)
                    : new PlotBatchManifestEntry(
                        item.Name, null, request.Format.ToString(),
                        request.Width, request.Height, false,
                        result.Failure?.Message ?? "Unknown error", null, entryStopwatch.Elapsed));
            }
            catch (Exception ex)
            {
                entryStopwatch.Stop();
                entries.Add(new PlotBatchManifestEntry(
                    item.Name, null, request.Format.ToString(),
                    request.Width, request.Height, false, ex.Message, null, entryStopwatch.Elapsed));
            }
        }

        var completedUtc = DateTime.UtcNow;
        var manifest = new PlotBatchManifest(entries, startedUtc, completedUtc);

        var manifestPath = Path.Combine(outputDirectory, "manifest.json");
        await manifest.SaveAsync(manifestPath).ConfigureAwait(false);

        return manifest;
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var result = new char[name.Length];
        for (var i = 0; i < name.Length; i++)
        {
            result[i] = invalid.Contains(name[i]) ? '_' : name[i];
        }

        return new string(result);
    }
}
