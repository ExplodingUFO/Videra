using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Videra.Avalonia.Controls;

/// <summary>
/// Formats a copy-pasteable diagnostics snapshot for alpha support, issue reports, and consumer smoke artifacts.
/// </summary>
public static class VideraDiagnosticsSnapshotFormatter
{
    /// <summary>
    /// Formats the supplied diagnostics into a stable text snapshot suitable for issues, discussions, and smoke artifacts.
    /// </summary>
    /// <param name="diagnostics">The backend/runtime diagnostics snapshot to format.</param>
    /// <returns>A multi-line plain-text diagnostics snapshot.</returns>
    public static string Format(VideraBackendDiagnostics diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        var assembly = typeof(VideraView).Assembly;
        var packageVersion =
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "unknown";

        var builder = new StringBuilder();
        builder.AppendLine("Videra diagnostics snapshot");
        builder.AppendLine($"GeneratedUtc: {DateTimeOffset.UtcNow:O}");
        builder.AppendLine($"PackageVersion: {packageVersion}");
        builder.AppendLine($"FrameworkDescription: {RuntimeInformation.FrameworkDescription}");
        builder.AppendLine($"OperatingSystem: {RuntimeInformation.OSDescription}");
        builder.AppendLine($"ProcessArchitecture: {RuntimeInformation.ProcessArchitecture}");
        builder.AppendLine($"AppBaseDirectory: {AppContext.BaseDirectory}");
        builder.AppendLine($"RequestedBackend: {diagnostics.RequestedBackend}");
        builder.AppendLine($"ResolvedBackend: {diagnostics.ResolvedBackend}");
        builder.AppendLine($"IsReady: {diagnostics.IsReady}");
        builder.AppendLine($"IsUsingSoftwareFallback: {diagnostics.IsUsingSoftwareFallback}");
        builder.AppendLine($"FallbackReason: {FormatNullable(diagnostics.FallbackReason)}");
        builder.AppendLine($"NativeHostBound: {diagnostics.NativeHostBound}");
        builder.AppendLine($"ResolvedDisplayServer: {FormatNullable(diagnostics.ResolvedDisplayServer)}");
        builder.AppendLine($"DisplayServerFallbackUsed: {diagnostics.DisplayServerFallbackUsed}");
        builder.AppendLine($"DisplayServerFallbackReason: {FormatNullable(diagnostics.DisplayServerFallbackReason)}");
        builder.AppendLine($"DisplayServerCompatibility: {DescribeDisplayServerCompatibility(diagnostics)}");
        builder.AppendLine($"LastInitializationError: {FormatNullable(diagnostics.LastInitializationError)}");
        builder.AppendLine($"RenderPipelineProfile: {FormatNullable(diagnostics.RenderPipelineProfile)}");
        builder.AppendLine($"LastFrameStageNames: {FormatList(diagnostics.LastFrameStageNames)}");
        builder.AppendLine($"LastFrameFeatureNames: {FormatList(diagnostics.LastFrameFeatureNames)}");
        builder.AppendLine($"LastFrameObjectCount: {diagnostics.LastFrameObjectCount}");
        builder.AppendLine($"LastFrameOpaqueObjectCount: {diagnostics.LastFrameOpaqueObjectCount}");
        builder.AppendLine($"LastFrameTransparentObjectCount: {diagnostics.LastFrameTransparentObjectCount}");
        builder.AppendLine($"SupportedRenderFeatureNames: {FormatList(diagnostics.SupportedRenderFeatureNames)}");
        builder.AppendLine($"TransparentFeatureStatus: {FormatNullable(diagnostics.TransparentFeatureStatus)}");
        builder.AppendLine($"SceneDocumentVersion: {diagnostics.SceneDocumentVersion}");
        builder.AppendLine($"PendingSceneUploads: {diagnostics.PendingSceneUploads}");
        builder.AppendLine($"PendingSceneUploadBytes: {diagnostics.PendingSceneUploadBytes}");
        builder.AppendLine($"ResidentSceneObjects: {diagnostics.ResidentSceneObjects}");
        builder.AppendLine($"DirtySceneObjects: {diagnostics.DirtySceneObjects}");
        builder.AppendLine($"FailedSceneUploads: {diagnostics.FailedSceneUploads}");
        builder.AppendLine($"LastFrameUploadedObjects: {diagnostics.LastFrameUploadedObjects}");
        builder.AppendLine($"LastFrameUploadedBytes: {diagnostics.LastFrameUploadedBytes}");
        builder.AppendLine($"LastFrameUploadFailures: {diagnostics.LastFrameUploadFailures}");
        builder.AppendLine($"LastFrameUploadDuration: {diagnostics.LastFrameUploadDuration}");
        builder.AppendLine($"ResolvedUploadBudgetObjects: {diagnostics.ResolvedUploadBudgetObjects}");
        builder.AppendLine($"ResolvedUploadBudgetBytes: {diagnostics.ResolvedUploadBudgetBytes}");
        builder.AppendLine($"IsClippingActive: {diagnostics.IsClippingActive}");
        builder.AppendLine($"ActiveClippingPlaneCount: {diagnostics.ActiveClippingPlaneCount}");
        builder.AppendLine($"MeasurementCount: {diagnostics.MeasurementCount}");
        builder.AppendLine($"LastSnapshotExportPath: {FormatNullable(diagnostics.LastSnapshotExportPath)}");
        builder.AppendLine($"LastSnapshotExportStatus: {FormatNullable(diagnostics.LastSnapshotExportStatus)}");
        builder.AppendLine($"SupportsShaderCreation: {diagnostics.SupportsShaderCreation}");
        builder.AppendLine($"SupportsResourceSetCreation: {diagnostics.SupportsResourceSetCreation}");
        builder.AppendLine($"SupportsResourceSetBinding: {diagnostics.SupportsResourceSetBinding}");
        return builder.ToString();
    }

    /// <summary>
    /// Summarizes the current Linux display-server support boundary in one stable line suitable
    /// for diagnostics snapshots, support artifacts, and consumer smoke evidence.
    /// </summary>
    public static string DescribeDisplayServerCompatibility(VideraBackendDiagnostics diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        return diagnostics.ResolvedDisplayServer switch
        {
            "X11" => "Direct X11 native host path.",
            "XWayland" => "Wayland session using XWayland compatibility fallback; compositor-native Wayland embedding is not active.",
            "Wayland" => "Compositor-native Wayland host path.",
            _ => "Unavailable"
        };
    }

    private static string FormatNullable(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "Unavailable" : value;

    private static string FormatList(IReadOnlyList<string>? values) =>
        values is { Count: > 0 } ? string.Join(", ", values) : "Unavailable";
}
