namespace Videra.SurfaceCharts.Demo.Services;

internal enum DataLoggerUpdateMode
{
    Replace,
    Append,
    FifoTrim
}

internal sealed record DataLoggerStreamingScenario(
    string Id,
    string DisplayName,
    string ChartType,
    DataLoggerUpdateMode UpdateMode,
    string Description)
{
    public override string ToString() => DisplayName;
}

internal static class DataLoggerStreamingScenarios
{
    private static readonly DataLoggerStreamingScenario[] Scenarios =
    [
        new(
            "surface-streaming",
            "Surface matrix stream",
            "Surface",
            DataLoggerUpdateMode.Append,
            "SurfaceDataLogger3D appending new rows with FIFO trimming."),
        new(
            "waterfall-streaming",
            "Waterfall matrix stream",
            "Waterfall",
            DataLoggerUpdateMode.Append,
            "WaterfallDataLogger3D delegating to SurfaceDataLogger3D for row streaming."),
        new(
            "bar-streaming",
            "Bar series stream",
            "Bar",
            DataLoggerUpdateMode.Append,
            "BarDataLogger3D appending new series to the bar chart."),
    ];

    public static IReadOnlyList<DataLoggerStreamingScenario> All => Scenarios;

    public static DataLoggerStreamingScenario Get(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return Scenarios.SingleOrDefault(s => string.Equals(s.Id, id, StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Unknown DataLogger streaming scenario '{id}'.");
    }
}
