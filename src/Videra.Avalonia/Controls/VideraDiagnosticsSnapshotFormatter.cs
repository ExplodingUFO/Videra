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
        builder.AppendLine($"LastInitializationError: {FormatNullable(diagnostics.LastInitializationError)}");
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
        return builder.ToString();
    }

    private static string FormatNullable(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "Unavailable" : value;
}
