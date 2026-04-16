namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal readonly record struct SurfaceTileRequestPriority(
    int Bucket,
    int Distance,
    int LevelPenalty,
    int Sequence);
