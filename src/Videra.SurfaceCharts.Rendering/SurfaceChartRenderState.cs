using System.Collections.ObjectModel;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Rendering;

public sealed class SurfaceChartRenderState
{
    private const double DoubleTolerance = 1e-6;
    private const float FloatTolerance = 1e-5f;

    private static readonly Comparison<SurfaceTileKey> TileKeyComparison =
        static (left, right) =>
        {
            var comparison = left.LevelX.CompareTo(right.LevelX);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = left.LevelY.CompareTo(right.LevelY);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = left.TileX.CompareTo(right.TileX);
            if (comparison != 0)
            {
                return comparison;
            }

            return left.TileY.CompareTo(right.TileY);
        };

    private readonly SurfaceRenderer _renderer;
    private readonly Dictionary<SurfaceTileKey, SurfaceChartResidentTile> _residentTiles = [];
    private SurfaceMetadata? _metadata;
    private SurfaceColorMap? _colorMap;
    private SurfaceViewState _viewState;
    private double _viewWidth;
    private double _viewHeight;
    private float _renderScale = 1f;
    private bool _hasViewState;

    public SurfaceChartRenderState(SurfaceRenderer? renderer = null)
    {
        _renderer = renderer ?? new SurfaceRenderer();
    }

    public SurfaceMetadata? Metadata => _metadata;

    public int ResidentTileCount => _residentTiles.Count;

    public IReadOnlyList<SurfaceChartResidentTile> ResidentTiles
    {
        get
        {
            var ordered = _residentTiles.Values.ToArray();
            Array.Sort(ordered, static (left, right) => TileKeyComparison(left.Key, right.Key));
            return Array.AsReadOnly(ordered);
        }
    }

    public bool TryGetResidentTile(SurfaceTileKey key, out SurfaceChartResidentTile residentTile)
    {
        return _residentTiles.TryGetValue(key, out residentTile!);
    }

    public SurfaceChartRenderChangeSet Update(SurfaceChartRenderInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var fullResetRequired = RequiresFullReset(inputs);
        var projectionDirty = !fullResetRequired && ViewConfigurationChanged(inputs);

        SurfaceChartRenderChangeSet changeSet;
        if (fullResetRequired)
        {
            changeSet = ApplyFullReset(inputs);
        }
        else
        {
            changeSet = ApplyIncrementalChanges(inputs, projectionDirty);
        }

        _metadata = inputs.Metadata;
        _colorMap = inputs.ColorMap;
        _viewState = inputs.ViewState;
        _viewWidth = inputs.ViewWidth;
        _viewHeight = inputs.ViewHeight;
        _renderScale = inputs.RenderScale;
        _hasViewState = true;

        return changeSet;
    }

    private bool RequiresFullReset(SurfaceChartRenderInputs inputs)
    {
        if (_metadata is null)
        {
            return inputs.Metadata is not null;
        }

        if (inputs.Metadata is null)
        {
            return true;
        }

        return !ReferenceEquals(_metadata, inputs.Metadata);
    }

    private bool ViewConfigurationChanged(SurfaceChartRenderInputs inputs)
    {
        if (!_hasViewState)
        {
            return false;
        }

        return _viewState != inputs.ViewState
            || !NearlyEqual(_viewWidth, inputs.ViewWidth)
            || !NearlyEqual(_viewHeight, inputs.ViewHeight)
            || !NearlyEqual(_renderScale, inputs.RenderScale);
    }

    private SurfaceChartRenderChangeSet ApplyFullReset(SurfaceChartRenderInputs inputs)
    {
        var removedKeys = _residentTiles.Keys.OrderBy(static key => key, Comparer<SurfaceTileKey>.Create(TileKeyComparison)).ToArray();
        _residentTiles.Clear();

        if (!CanBuildResidents(inputs))
        {
            return new SurfaceChartRenderChangeSet
            {
                FullResetRequired = true,
                AddedResidentKeys = Array.Empty<SurfaceTileKey>(),
                RemovedResidentKeys = removedKeys,
            };
        }

        var addedKeys = new List<SurfaceTileKey>();
        foreach (var tile in inputs.LoadedTiles)
        {
            var residentTile = CreateResidentTile(inputs.Metadata!, tile, inputs.ColorMap!);
            _residentTiles[tile.Key] = residentTile;
            addedKeys.Add(tile.Key);
        }

        addedKeys.Sort(TileKeyComparison);
        return new SurfaceChartRenderChangeSet
        {
            FullResetRequired = true,
            AddedResidentKeys = Array.AsReadOnly(addedKeys.ToArray()),
            RemovedResidentKeys = removedKeys,
        };
    }

    private SurfaceChartRenderChangeSet ApplyIncrementalChanges(SurfaceChartRenderInputs inputs, bool projectionDirty)
    {
        if (!CanBuildResidents(inputs))
        {
            var clearedKeys = _residentTiles.Keys.OrderBy(static key => key, Comparer<SurfaceTileKey>.Create(TileKeyComparison)).ToArray();
            _residentTiles.Clear();
            return new SurfaceChartRenderChangeSet
            {
                FullResetRequired = _metadata is not null || clearedKeys.Length > 0,
                RemovedResidentKeys = clearedKeys,
            };
        }

        var incomingTiles = inputs.LoadedTiles.ToDictionary(static tile => tile.Key);
        var removedKeys = new List<SurfaceTileKey>();
        var addedKeys = new List<SurfaceTileKey>();
        var colorMapChanged = !ReferenceEquals(_colorMap, inputs.ColorMap);

        foreach (var existingKey in _residentTiles.Keys.ToArray())
        {
            if (!incomingTiles.ContainsKey(existingKey))
            {
                _residentTiles.Remove(existingKey);
                removedKeys.Add(existingKey);
            }
        }

        foreach (var tile in inputs.LoadedTiles)
        {
            if (!_residentTiles.TryGetValue(tile.Key, out var residentTile))
            {
                _residentTiles[tile.Key] = CreateResidentTile(inputs.Metadata!, tile, inputs.ColorMap!);
                addedKeys.Add(tile.Key);
                continue;
            }

            if (!ReferenceEquals(residentTile.SourceTile, tile))
            {
                _residentTiles[tile.Key] = CreateResidentTile(inputs.Metadata!, tile, inputs.ColorMap!);
                addedKeys.Add(tile.Key);
                continue;
            }

            _ = residentTile;
        }

        removedKeys.Sort(TileKeyComparison);
        addedKeys.Sort(TileKeyComparison);

        return new SurfaceChartRenderChangeSet
        {
            FullResetRequired = false,
            ResidencyDirty = addedKeys.Count > 0 || removedKeys.Count > 0,
            ColorDirty = colorMapChanged,
            ProjectionDirty = projectionDirty,
            AddedResidentKeys = Array.AsReadOnly(addedKeys.ToArray()),
            RemovedResidentKeys = Array.AsReadOnly(removedKeys.ToArray()),
        };
    }

    private static bool CanBuildResidents(SurfaceChartRenderInputs inputs)
    {
        return inputs.Metadata is not null
            && inputs.ColorMap is not null
            && inputs.LoadedTiles.Count > 0;
    }

    private static bool NearlyEqual(double left, double right)
    {
        return Math.Abs(left - right) <= DoubleTolerance;
    }

    private static bool NearlyEqual(float left, float right)
    {
        return Math.Abs(left - right) <= FloatTolerance;
    }

    private SurfaceChartResidentTile CreateResidentTile(
        SurfaceMetadata metadata,
        SurfaceTile sourceTile,
        SurfaceColorMap colorMap)
    {
        var renderTile = _renderer.BuildTile(metadata, sourceTile, colorMap);

        return new SurfaceChartResidentTile(
            sourceTile,
            renderTile);
    }
}
