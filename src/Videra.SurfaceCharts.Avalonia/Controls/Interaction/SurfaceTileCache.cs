using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal sealed class SurfaceTileCache
{
    private readonly object _sync = new();
    private readonly HashSet<SurfaceTileKey> _requestedKeys = [];
    private readonly Dictionary<SurfaceTileKey, SurfaceTile> _loadedTiles = [];

    public void Clear()
    {
        lock (_sync)
        {
            _requestedKeys.Clear();
            _loadedTiles.Clear();
        }
    }

    public bool TryMarkRequested(SurfaceTileKey key)
    {
        lock (_sync)
        {
            return _requestedKeys.Add(key);
        }
    }

    public void Store(SurfaceTile tile)
    {
        ArgumentNullException.ThrowIfNull(tile);

        lock (_sync)
        {
            _requestedKeys.Add(tile.Key);
            _loadedTiles[tile.Key] = tile;
        }
    }

    public IReadOnlyList<SurfaceTile> GetLoadedTiles()
    {
        lock (_sync)
        {
            return _loadedTiles.Values
                .OrderBy(static tile => tile.Key.LevelY)
                .ThenBy(static tile => tile.Key.LevelX)
                .ThenBy(static tile => tile.Key.TileY)
                .ThenBy(static tile => tile.Key.TileX)
                .ToArray();
        }
    }
}
