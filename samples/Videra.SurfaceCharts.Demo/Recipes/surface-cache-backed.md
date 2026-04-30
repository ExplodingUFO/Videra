# Cache-Backed Surface Recipe

This recipe loads a tiled surface cache and renders it through the same `VideraChartView` surface path used by the demo's `Explore next: Cache-backed streaming` source. The chart receives a `SurfaceCacheTileSource`, so tile reads stay owned by the cache layer while the control owns interaction, `ViewState`, tile scheduling, and rendering evidence.

## Project References

Use the Avalonia chart package and the processing package:

```xml
<ProjectReference Include="..\..\src\Videra.SurfaceCharts.Avalonia\Videra.SurfaceCharts.Avalonia.csproj" />
<ProjectReference Include="..\..\src\Videra.SurfaceCharts.Processing\Videra.SurfaceCharts.Processing.csproj" />
```

## Load The Demo Cache

The demo ships a manifest and payload sidecar:

- `Assets/sample-surface-cache/sample.surfacecache.json`
- `Assets/sample-surface-cache/sample.surfacecache.json.bin`

The manifest path is passed to `SurfaceCacheReader`. The reader validates metadata and tile records, while `SurfaceCacheTileSource` adapts the cache to the chart's tile-source contract.

```csharp
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Processing;

var chart = new VideraChartView();
var cachePath = Path.Combine(
    AppContext.BaseDirectory,
    "Assets",
    "sample-surface-cache",
    "sample.surfacecache.json");

var reader = await SurfaceCacheReader.ReadAsync(cachePath);
await using var source = new SurfaceCacheTileSource(reader);

chart.Plot.Clear();
chart.Plot.Add.Surface(source, "Cache-backed surface");
chart.Plot.Axes.X.Label = reader.Metadata.HorizontalAxis.Label;
chart.Plot.Axes.X.Unit = reader.Metadata.HorizontalAxis.Unit;
chart.Plot.Axes.Z.Label = reader.Metadata.VerticalAxis.Label;
chart.Plot.Axes.Z.Unit = reader.Metadata.VerticalAxis.Unit;
chart.Plot.Axes.Y.Label = "Value";
chart.FitToData();
```

Keep the `SurfaceCacheTileSource` alive for as long as the chart can request tiles. In an Avalonia window, make it a field and dispose it when the window closes:

```csharp
private SurfaceCacheTileSource? _surfaceCacheSource;

private async Task LoadCacheBackedSurfaceAsync(string cachePath)
{
    var reader = await SurfaceCacheReader.ReadAsync(cachePath);
    _surfaceCacheSource = new SurfaceCacheTileSource(reader);

    ChartView.Plot.Clear();
    ChartView.Plot.Add.Surface(_surfaceCacheSource, "Cache-backed surface");
    ChartView.FitToData();
}

protected override async void OnClosed(EventArgs e)
{
    if (_surfaceCacheSource is not null)
    {
        await _surfaceCacheSource.DisposeAsync();
    }

    base.OnClosed(e);
}
```

## Build A Cache For Your Own Matrix

If the source data starts as an in-memory `SurfaceMatrix`, build a tile source first and persist the tiles you want in the cache:

```csharp
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Processing;

var pyramid = new SurfacePyramidBuilder(maxTileWidth: 64, maxTileHeight: 64).Build(matrix);
var tileKeys = new[]
{
    new SurfaceTileKey(levelX: 0, levelY: 0, tileX: 0, tileY: 0),
    new SurfaceTileKey(levelX: 1, levelY: 1, tileX: 0, tileY: 0),
};

await SurfaceCacheWriter.WriteAsync(cachePath, pyramid, tileKeys);

var reader = await SurfaceCacheReader.ReadAsync(cachePath);
await using var cachedSource = new SurfaceCacheTileSource(reader);
chart.Plot.Add.Surface(cachedSource, "Cached matrix");
chart.FitToData();
```

The cache writer currently expects regular-grid, linear-axis metadata. Use the first-chart recipe for a small direct matrix, and use this cache-backed recipe when the chart should read committed tile files.

## ViewState And Support Evidence

`FitToData()` updates `chart.ViewState` from the loaded cache metadata. If the host stores camera state, store the full `SurfaceViewState` after the cache-backed surface has been added:

```csharp
SurfaceViewState savedViewState = chart.ViewState;
chart.ViewState = savedViewState;
```

For cache-backed support evidence, include:

- the cache manifest path
- `reader.Metadata.Width` and `reader.Metadata.Height`
- `reader.Metadata.HorizontalAxis`
- `reader.Metadata.VerticalAxis`
- `chart.ViewState.DataWindow`
- `chart.RenderingStatus.ActiveBackend`
- `chart.RenderingStatus.IsReady`
- `chart.RenderingStatus.ResidentTileCount`
- `chart.RenderingStatus.VisibleTileCount`
- any exception message from `SurfaceCacheReader.ReadAsync`

That evidence is enough to identify the Videra cache file, tile-source path, camera window, and active renderer state without adding a separate chart control.
