using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal sealed class SurfaceTileCache
{
    private readonly object _sync = new();
    private readonly Dictionary<SurfaceTileKey, long> _requestedKeys = [];
    private readonly Dictionary<SurfaceTileKey, SurfaceTile> _loadedTiles = [];

    public void Clear()
    {
        lock (_sync)
        {
            _requestedKeys.Clear();
            _loadedTiles.Clear();
        }
    }

    public bool TryMarkRequested(SurfaceTileKey key, long requestGeneration)
    {
        lock (_sync)
        {
            if (_loadedTiles.ContainsKey(key))
            {
                return false;
            }

            if (_requestedKeys.TryGetValue(key, out var currentGeneration) && currentGeneration >= requestGeneration)
            {
                return false;
            }

            _requestedKeys[key] = requestGeneration;
            return true;
        }
    }

    public void ReleaseRequested(SurfaceTileKey key, long requestGeneration)
    {
        lock (_sync)
        {
            if (_requestedKeys.TryGetValue(key, out var currentGeneration) && currentGeneration == requestGeneration)
            {
                _requestedKeys.Remove(key);
            }
        }
    }

    public bool TryStore(SurfaceTile tile, long requestGeneration)
    {
        ArgumentNullException.ThrowIfNull(tile);

        lock (_sync)
        {
            if (!_requestedKeys.TryGetValue(tile.Key, out var currentGeneration) || currentGeneration != requestGeneration)
            {
                return false;
            }

            _requestedKeys.Remove(tile.Key);
            _loadedTiles[tile.Key] = tile;
            return true;
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
