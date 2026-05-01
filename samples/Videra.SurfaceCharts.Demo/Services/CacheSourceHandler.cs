using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Processing;

namespace Videra.SurfaceCharts.Demo.Services;

/// <summary>
/// Manages lazy loading of a cache-backed <see cref="ISurfaceTileSource"/>
/// and applies failure state when the load cannot complete.
/// </summary>
internal sealed class CacheSourceHandler
{
    private readonly string _cachePath;
    private readonly string _cachePayloadPath;
    private Task<ISurfaceTileSource>? _cacheSourceTask;
    private string? _lastCacheLoadFailureMessage;

    internal CacheSourceHandler(string cachePath, string cachePayloadPath)
    {
        _cachePath = cachePath;
        _cachePayloadPath = cachePayloadPath;
    }

    internal string? LastCacheLoadFailureMessage => _lastCacheLoadFailureMessage;

    internal string GetCacheAssetSummary()
        => $"Manifest {_cachePath}; Payload sidecar {_cachePayloadPath}";

    internal async Task LoadAndApplyCacheSourceAsync(
        SurfaceDemoScenario scenario,
        Func<string, bool> isSelectedScenario,
        Action<VideraChartView, ISurfaceTileSource, string, string, string, string> applySource,
        VideraChartView surfaceChartView,
        Action<string> setPlotPathHeading,
        Action<string> setPlotPathDetails,
        Action<string> setDatasetSummary,
        Action<string> setAssetSummary,
        Action<string> setDatasetText,
        Action refreshProofTexts)
    {
        try
        {
            var cacheSource = await GetOrCreateCacheSourceAsync().ConfigureAwait(true);
            if (!isSelectedScenario(scenario.Id))
            {
                return;
            }

            _lastCacheLoadFailureMessage = null;
            applySource(
                surfaceChartView,
                cacheSource,
                scenario.Label,
                $"Advanced follow-up after the first chart renders. Loads manifest metadata from {_cachePath} and uses lazy viewport tile streaming from {_cachePayloadPath}.",
                "The cache-backed path reads a committed manifest plus binary sidecar and only requests the tiles needed for the current view.",
                $"Manifest {_cachePath}; Payload sidecar {_cachePayloadPath}");
        }
        catch (Exception exception)
        {
            if (!isSelectedScenario(scenario.Id))
            {
                return;
            }

            _lastCacheLoadFailureMessage = $"{exception.GetType().Name}: {exception.Message}";
            ApplyCacheLoadFailure(
                scenario, exception,
                setPlotPathHeading, setPlotPathDetails,
                setDatasetSummary, setAssetSummary,
                setDatasetText, refreshProofTexts);
        }
    }

    private void ApplyCacheLoadFailure(
        SurfaceDemoScenario scenario,
        Exception exception,
        Action<string> setPlotPathHeading,
        Action<string> setPlotPathDetails,
        Action<string> setDatasetSummary,
        Action<string> setAssetSummary,
        Action<string> setDatasetText,
        Action refreshProofTexts)
    {
        setPlotPathHeading(scenario.Label);
        setPlotPathDetails($"Cache-backed streaming failed to load: {exception.Message}. No Plot path switch was performed.");
        setDatasetSummary("Cache-backed data path unavailable. The previous chart Plot path remains active and there was no scenario/data-path fallback.");
        setAssetSummary($"Manifest {_cachePath}; Payload sidecar {_cachePayloadPath}");
        setDatasetText("Cache-backed data path unavailable. The previous chart Plot path remains active and there was no scenario/data-path fallback.");
        refreshProofTexts();
    }

    private Task<ISurfaceTileSource> GetOrCreateCacheSourceAsync()
        => _cacheSourceTask ??= LoadCacheSourceAsync();

    private async Task<ISurfaceTileSource> LoadCacheSourceAsync()
    {
        var reader = await SurfaceCacheReader.ReadAsync(_cachePath).ConfigureAwait(false);
        return new SurfaceCacheTileSource(reader);
    }
}
