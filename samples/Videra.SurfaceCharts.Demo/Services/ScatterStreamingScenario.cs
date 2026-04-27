using System;
using System.Collections.Generic;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Demo.Services;

public enum ScatterStreamingUpdateMode
{
    Replace,
    Append,
    FifoTrim
}

public sealed record ScatterStreamingScenario(
    string Id,
    string DisplayName,
    ScatterStreamingUpdateMode UpdateMode,
    int InitialPointCount,
    int UpdatePointCount,
    int? FifoCapacity,
    bool Pickable,
    string Description)
{
    public override string ToString() => DisplayName;
}

public static class ScatterStreamingScenarios
{
    private static readonly ScatterStreamingScenario[] Scenarios =
    [
        new(
            "scatter-replace-100k",
            "Scatter replace stream",
            ScatterStreamingUpdateMode.Replace,
            100_000,
            100_000,
            null,
            false,
            "Columnar replacement path for high-volume scatter data."),
        new(
            "scatter-append-100k",
            "Scatter append stream",
            ScatterStreamingUpdateMode.Append,
            100_000,
            25_000,
            null,
            false,
            "Columnar append path without FIFO trimming."),
        new(
            "scatter-fifo-trim-100k",
            "Scatter FIFO trim stream",
            ScatterStreamingUpdateMode.FifoTrim,
            100_000,
            50_000,
            100_000,
            false,
            "Columnar append path that intentionally trims old points through FIFO capacity.")
    ];

    public static IReadOnlyList<ScatterStreamingScenario> All => Scenarios;

    public static ScatterStreamingScenario Get(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return Array.Find(Scenarios, scenario => StringComparer.Ordinal.Equals(scenario.Id, id))
            ?? throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown scatter streaming scenario.");
    }

    public static ScatterColumnarSeries CreateSeries(ScatterStreamingScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        var series = new ScatterColumnarSeries(
            0xFF22C55Eu,
            scenario.DisplayName,
            isSortedX: true,
            pickable: scenario.Pickable,
            fifoCapacity: scenario.FifoCapacity);
        series.ReplaceRange(CreateData(0, scenario.InitialPointCount));
        ApplyUpdate(series, scenario);
        return series;
    }

    public static ScatterColumnarData CreateData(int startIndex, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        var x = new float[count];
        var y = new float[count];
        var z = new float[count];
        var size = new float[count];
        for (var i = 0; i < count; i++)
        {
            var sample = startIndex + i;
            x[i] = sample * 0.01f;
            y[i] = MathF.Sin(sample * 0.017f) * 20f;
            z[i] = MathF.Cos(sample * 0.013f) * 12f;
            size[i] = 0.75f + ((sample % 5) * 0.1f);
        }

        return new ScatterColumnarData(x, y, z, size);
    }

    private static void ApplyUpdate(ScatterColumnarSeries series, ScatterStreamingScenario scenario)
    {
        var update = CreateData(scenario.InitialPointCount, scenario.UpdatePointCount);
        if (scenario.UpdateMode == ScatterStreamingUpdateMode.Replace)
        {
            series.ReplaceRange(update);
            return;
        }

        series.AppendRange(update);
    }
}
