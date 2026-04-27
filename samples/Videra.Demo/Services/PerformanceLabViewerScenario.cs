using System;
using System.Collections.Generic;
using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics;

namespace Videra.Demo.Services;

public enum PerformanceLabDatasetSize
{
    Small,
    Medium,
    Large
}

public sealed record PerformanceLabViewerScenario(
    string Id,
    string DisplayName,
    PerformanceLabDatasetSize Size,
    int ObjectCount,
    int Seed,
    bool Pickable,
    string Description);

public static class PerformanceLabViewerScenarios
{
    private static readonly PerformanceLabViewerScenario[] Scenarios =
    [
        new(
            "viewer-instance-small",
            "Viewer instance batch - small",
            PerformanceLabDatasetSize.Small,
            1000,
            1101,
            true,
            "Small deterministic instance-batch dataset for quick interaction checks."),
        new(
            "viewer-instance-medium",
            "Viewer instance batch - medium",
            PerformanceLabDatasetSize.Medium,
            5000,
            1102,
            true,
            "Medium deterministic instance-batch dataset for retained-instance diagnostics."),
        new(
            "viewer-instance-large",
            "Viewer instance batch - large",
            PerformanceLabDatasetSize.Large,
            10000,
            1103,
            false,
            "Large deterministic instance-batch dataset with picking disabled by default.")
    ];

    public static IReadOnlyList<PerformanceLabViewerScenario> All => Scenarios;

    public static PerformanceLabViewerScenario Get(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        foreach (var scenario in Scenarios)
        {
            if (StringComparer.Ordinal.Equals(scenario.Id, id))
            {
                return scenario;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown Performance Lab viewer scenario.");
    }

    public static Matrix4x4[] CreateTransforms(PerformanceLabViewerScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        var transforms = new Matrix4x4[scenario.ObjectCount];
        if (transforms.Length == 0)
        {
            return transforms;
        }

        transforms[0] = Matrix4x4.Identity;
        var columns = Math.Max(1, (int)MathF.Ceiling(MathF.Sqrt(scenario.ObjectCount)));
        for (var i = 1; i < transforms.Length; i++)
        {
            var x = (i % columns) * 1.4f;
            var z = (i / columns) * 1.4f;
            transforms[i] = Matrix4x4.CreateTranslation(x, 0f, z);
        }

        return transforms;
    }

    public static Guid[] CreateObjectIds(PerformanceLabViewerScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        var objectIds = new Guid[scenario.ObjectCount];
        for (var i = 0; i < objectIds.Length; i++)
        {
            objectIds[i] = CreateObjectId(scenario.Seed, i);
        }

        return objectIds;
    }

    public static RgbaFloat[] CreateColors(PerformanceLabViewerScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        var colors = new RgbaFloat[scenario.ObjectCount];
        for (var i = 0; i < colors.Length; i++)
        {
            var band = i % 3;
            colors[i] = band switch
            {
                0 => new RgbaFloat(0.20f, 0.72f, 0.92f, 1f),
                1 => new RgbaFloat(0.18f, 0.80f, 0.44f, 1f),
                _ => new RgbaFloat(0.96f, 0.62f, 0.16f, 1f)
            };
        }

        return colors;
    }

    private static Guid CreateObjectId(int seed, int index)
    {
        return new Guid(
            seed,
            (short)(index & 0x7FFF),
            (short)((index >> 15) & 0x7FFF),
            0x56,
            0x44,
            0x52,
            0x41,
            (byte)((index >> 24) & 0xFF),
            (byte)((index >> 16) & 0xFF),
            (byte)((index >> 8) & 0xFF),
            (byte)(index & 0xFF));
    }
}
