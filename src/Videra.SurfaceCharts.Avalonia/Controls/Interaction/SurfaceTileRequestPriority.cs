namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal readonly record struct SurfaceTileRequestPriority(
    int Bucket,
    int CenterDistanceBucket,
    int DepthBucket,
    int ScreenAreaPenalty,
    int LevelPenalty,
    int Sequence);
