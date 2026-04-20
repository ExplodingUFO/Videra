using System.Numerics;
using Videra.SurfaceCharts.Core.Rendering;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Performs camera-ray picking against loaded surface tiles.
/// </summary>
public static class SurfaceHeightfieldPicker
{
    private const float IntersectionEpsilon = 0.0001f;
    // Exact screen->ray round-trips can drift slightly across runtimes/platforms when the
    // ray lands on a shared peak vertex. Keep the vertex-snap fallback comfortably above
    // that cross-platform projection noise while staying far below the tile's sample spacing.
    private const float VertexSnapDistanceEpsilon = 0.05f;
    private const float VertexSnapDistanceTieEpsilon = 0.05f;

    /// <summary>
    /// Creates a world-space pick ray from one screen-space pointer position.
    /// </summary>
    /// <param name="screenPosition">The screen-space pointer position in pixels.</param>
    /// <param name="cameraFrame">The active camera frame.</param>
    /// <returns>The normalized world-space pick ray.</returns>
    public static SurfacePickRay CreatePickRay(Vector2 screenPosition, SurfaceCameraFrame cameraFrame)
    {
        if (!float.IsFinite(screenPosition.X) || !float.IsFinite(screenPosition.Y))
        {
            throw new ArgumentOutOfRangeException(nameof(screenPosition), "Screen-space pick positions must be finite.");
        }

        var farPoint = SurfaceProjectionMath.UnprojectFromScreen(new Vector3(screenPosition, 1f), cameraFrame);
        return new SurfacePickRay(cameraFrame.Position, farPoint - cameraFrame.Position);
    }

    /// <summary>
    /// Picks the highest-detail loaded tile intersected by the supplied ray.
    /// </summary>
    /// <param name="metadata">The dataset metadata.</param>
    /// <param name="loadedTiles">The currently loaded tiles.</param>
    /// <param name="pickRay">The world-space pick ray.</param>
    /// <returns>The resolved hit, or <see langword="null"/> when nothing is intersected.</returns>
    public static SurfacePickHit? Pick(
        SurfaceMetadata metadata,
        IReadOnlyList<SurfaceTile> loadedTiles,
        SurfacePickRay pickRay)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(loadedTiles);

        foreach (var tile in loadedTiles
                     .OrderByDescending(static candidate => candidate.Key.LevelX + candidate.Key.LevelY)
                     .ThenByDescending(static candidate => candidate.Key.LevelX)
                     .ThenByDescending(static candidate => candidate.Key.LevelY))
        {
            if (!IntersectsTileBounds(metadata, tile, pickRay))
            {
                continue;
            }

            if (TryPickTile(metadata, tile, pickRay, out var hit))
            {
                return hit;
            }
        }

        return null;
    }

    private static bool TryPickTile(
        SurfaceMetadata metadata,
        SurfaceTile tile,
        SurfacePickRay pickRay,
        out SurfacePickHit hit)
    {
        hit = default;

        if (tile.Width < 2 || tile.Height < 2)
        {
            return false;
        }

        PickCandidate? bestCandidate = null;
        for (var row = 0; row < tile.Height - 1; row++)
        {
            for (var column = 0; column < tile.Width - 1; column++)
            {
                var topLeft = CreateVertex(metadata, tile, column, row);
                var bottomLeft = CreateVertex(metadata, tile, column, row + 1);
                var topRight = CreateVertex(metadata, tile, column + 1, row);
                var bottomRight = CreateVertex(metadata, tile, column + 1, row + 1);

                bestCandidate = ChooseCloserCandidate(
                    bestCandidate,
                    TryCreateCandidate(tile, pickRay, topLeft, bottomLeft, topRight),
                    TryCreateCandidate(tile, pickRay, topRight, bottomLeft, bottomRight));
            }
        }

        var bestVertexSnap = TryCreateVertexSnapCandidate(metadata, tile, pickRay);
        if (bestVertexSnap is not null &&
            (bestCandidate is null || bestVertexSnap.Value.Distance <= bestCandidate.Value.Distance + VertexSnapDistanceTieEpsilon))
        {
            bestCandidate = bestVertexSnap;
        }

        if (bestCandidate is null)
        {
            return false;
        }

        hit = new SurfacePickHit(
            tile.Key,
            bestCandidate.Value.SampleX,
            bestCandidate.Value.SampleY,
            bestCandidate.Value.WorldPosition,
            bestCandidate.Value.Value,
            isApproximate: tile.Bounds.Width != tile.Width || tile.Bounds.Height != tile.Height,
            bestCandidate.Value.Distance);
        return true;
    }

    private static PickCandidate? ChooseCloserCandidate(
        PickCandidate? currentBest,
        PickCandidate? first,
        PickCandidate? second)
    {
        var best = currentBest;
        if (first is not null && (best is null || first.Value.Distance < best.Value.Distance))
        {
            best = first;
        }

        if (second is not null && (best is null || second.Value.Distance < best.Value.Distance))
        {
            best = second;
        }

        return best;
    }

    private static PickCandidate? TryCreateCandidate(
        SurfaceTile tile,
        SurfacePickRay pickRay,
        TileVertex first,
        TileVertex second,
        TileVertex third)
    {
        if (!TryIntersectTriangle(
                pickRay,
                first.WorldPosition,
                second.WorldPosition,
                third.WorldPosition,
                out var distance,
                out var weightSecond,
                out var weightThird))
        {
            return null;
        }

        var weightFirst = 1f - weightSecond - weightThird;
        var sampleX =
            (weightFirst * (float)first.SampleX) +
            (weightSecond * (float)second.SampleX) +
            (weightThird * (float)third.SampleX);
        var sampleY =
            (weightFirst * (float)first.SampleY) +
            (weightSecond * (float)second.SampleY) +
            (weightThird * (float)third.SampleY);
        var value =
            (weightFirst * first.Value) +
            (weightSecond * second.Value) +
            (weightThird * third.Value);

        return new PickCandidate(
            tile.Key,
            sampleX,
            sampleY,
            pickRay.GetPoint(distance),
            value,
            distance);
    }

    private static PickCandidate? TryCreateVertexSnapCandidate(
        SurfaceMetadata metadata,
        SurfaceTile tile,
        SurfacePickRay pickRay)
    {
        PickCandidate? bestCandidate = null;
        for (var row = 0; row < tile.Height; row++)
        {
            for (var column = 0; column < tile.Width; column++)
            {
                var vertex = CreateVertex(metadata, tile, column, row);
                var offset = vertex.WorldPosition - pickRay.Origin;
                var distanceAlongRay = Vector3.Dot(offset, pickRay.Direction);
                if (distanceAlongRay < 0f)
                {
                    continue;
                }

                var projectedPoint = pickRay.GetPoint(distanceAlongRay);
                var distanceToRay = Vector3.Distance(projectedPoint, vertex.WorldPosition);
                if (distanceToRay > VertexSnapDistanceEpsilon)
                {
                    continue;
                }

                var candidate = new PickCandidate(
                    tile.Key,
                    (float)vertex.SampleX,
                    (float)vertex.SampleY,
                    vertex.WorldPosition,
                    vertex.Value,
                    distanceAlongRay);
                if (bestCandidate is null || candidate.Distance < bestCandidate.Value.Distance)
                {
                    bestCandidate = candidate;
                }
            }
        }

        return bestCandidate;
    }

    private static bool TryIntersectTriangle(
        SurfacePickRay pickRay,
        Vector3 first,
        Vector3 second,
        Vector3 third,
        out float distance,
        out float weightSecond,
        out float weightThird)
    {
        var edgeOne = second - first;
        var edgeTwo = third - first;
        var p = Vector3.Cross(pickRay.Direction, edgeTwo);
        var determinant = Vector3.Dot(edgeOne, p);
        if (MathF.Abs(determinant) <= IntersectionEpsilon)
        {
            distance = 0f;
            weightSecond = 0f;
            weightThird = 0f;
            return false;
        }

        var inverseDeterminant = 1f / determinant;
        var t = pickRay.Origin - first;
        weightSecond = Vector3.Dot(t, p) * inverseDeterminant;
        if (weightSecond < -IntersectionEpsilon || weightSecond > 1f + IntersectionEpsilon)
        {
            distance = 0f;
            weightThird = 0f;
            return false;
        }

        var q = Vector3.Cross(t, edgeOne);
        weightThird = Vector3.Dot(pickRay.Direction, q) * inverseDeterminant;
        if (weightThird < -IntersectionEpsilon || (weightSecond + weightThird) > 1f + IntersectionEpsilon)
        {
            distance = 0f;
            return false;
        }

        distance = Vector3.Dot(edgeTwo, q) * inverseDeterminant;
        return distance >= IntersectionEpsilon;
    }

    private static bool IntersectsTileBounds(
        SurfaceMetadata metadata,
        SurfaceTile tile,
        SurfacePickRay pickRay)
    {
        var min = new Vector3(
            (float)metadata.MapHorizontalCoordinate(tile.Bounds.StartX),
            (float)tile.ValueRange.Minimum,
            (float)metadata.MapVerticalCoordinate(tile.Bounds.StartY));
        var max = new Vector3(
            (float)metadata.MapHorizontalCoordinate(tile.Bounds.EndXExclusive - 1d),
            (float)tile.ValueRange.Maximum,
            (float)metadata.MapVerticalCoordinate(tile.Bounds.EndYExclusive - 1d));

        return IntersectsAxisAlignedBounds(pickRay, min, max);
    }

    private static bool IntersectsAxisAlignedBounds(SurfacePickRay pickRay, Vector3 minimum, Vector3 maximum)
    {
        var minDistance = float.NegativeInfinity;
        var maxDistance = float.PositiveInfinity;

        if (!UpdateAxisInterval(pickRay.Origin.X, pickRay.Direction.X, minimum.X, maximum.X, ref minDistance, ref maxDistance) ||
            !UpdateAxisInterval(pickRay.Origin.Y, pickRay.Direction.Y, minimum.Y, maximum.Y, ref minDistance, ref maxDistance) ||
            !UpdateAxisInterval(pickRay.Origin.Z, pickRay.Direction.Z, minimum.Z, maximum.Z, ref minDistance, ref maxDistance))
        {
            return false;
        }

        return maxDistance >= Math.Max(0f, minDistance);
    }

    private static bool UpdateAxisInterval(
        float origin,
        float direction,
        float minimum,
        float maximum,
        ref float minDistance,
        ref float maxDistance)
    {
        if (MathF.Abs(direction) <= IntersectionEpsilon)
        {
            return origin >= minimum && origin <= maximum;
        }

        var inverseDirection = 1f / direction;
        var axisMin = (minimum - origin) * inverseDirection;
        var axisMax = (maximum - origin) * inverseDirection;
        if (axisMin > axisMax)
        {
            (axisMin, axisMax) = (axisMax, axisMin);
        }

        minDistance = Math.Max(minDistance, axisMin);
        maxDistance = Math.Min(maxDistance, axisMax);
        return maxDistance >= minDistance;
    }

    private static TileVertex CreateVertex(SurfaceMetadata metadata, SurfaceTile tile, int column, int row)
    {
        var sampleX = MapTileSampleCoordinate(tile.Bounds.StartX, tile.Bounds.Width, tile.Width, column);
        var sampleY = MapTileSampleCoordinate(tile.Bounds.StartY, tile.Bounds.Height, tile.Height, row);
        var value = tile.Values.Span[(row * tile.Width) + column];
        var worldPosition = new Vector3(
            (float)metadata.MapHorizontalCoordinate(sampleX),
            value,
            (float)metadata.MapVerticalCoordinate(sampleY));
        return new TileVertex(sampleX, sampleY, value, worldPosition);
    }

    private static double MapTileSampleCoordinate(int start, int span, int sampleCount, int sampleIndex)
    {
        if (sampleCount <= 1)
        {
            return start + ((span - 1d) * 0.5d);
        }

        return start + (sampleIndex * ((span - 1d) / (sampleCount - 1d)));
    }

    private readonly record struct TileVertex(double SampleX, double SampleY, float Value, Vector3 WorldPosition);

    private readonly record struct PickCandidate(
        SurfaceTileKey TileKey,
        float SampleX,
        float SampleY,
        Vector3 WorldPosition,
        float Value,
        float Distance);
}
