using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Videra.Avalonia.Controls;
using Videra.Core.Graphics.RenderPipeline.Extensibility;
using Videra.Core.Graphics.Wireframe;
using Videra.Core.Scene;
using Videra.Core.Styles.Presets;
using Videra.Import.Gltf;
using Videra.Import.Obj;

namespace Videra.Demo.Services;

public sealed record DemoSupportSettings(
    RenderStylePreset RenderStyle,
    WireframeMode WireframeMode,
    bool IsGridVisible,
    decimal GridHeight,
    string GridColor,
    string BackgroundColor,
    bool CameraInvertX,
    bool CameraInvertY,
    string? SelectedObjectName);

public static class DemoSupportReportBuilder
{
    public static string FormatImportReport(ModelLoadBatchResult? loadResult)
    {
        if (loadResult is null)
        {
            return "No import attempts yet.";
        }

        if (loadResult.Results.Count == 0)
        {
            return "No models were selected.";
        }

        var builder = new StringBuilder();
        builder.AppendLine("Import report");
        builder.AppendLine($"BatchSucceeded: {loadResult.Succeeded}");
        builder.AppendLine($"BatchDurationMs: {loadResult.Duration.TotalMilliseconds:N1}");
        builder.AppendLine($"AppliedEntries: {loadResult.Entries.Count}");
        builder.AppendLine($"Failures: {loadResult.Failures.Count}");

        foreach (var result in loadResult.Results)
        {
            var status = FormatFileStatus(result);
            builder.AppendLine($"- Path: {result.Path}");
            builder.AppendLine($"  Status: {status}");
            builder.AppendLine($"  ImportDurationMs: {result.ImportDuration.TotalMilliseconds:N1}");

            if (result.AssetMetrics is not null)
            {
                builder.AppendLine(
                    $"  Asset: vertices={result.AssetMetrics.VertexCount}; indices={result.AssetMetrics.IndexCount}; approximateGpuBytes={result.AssetMetrics.ApproximateGpuBytes}");
            }
            else
            {
                builder.AppendLine("  Asset: unavailable");
            }

            if (result.Failure is not null)
            {
                builder.AppendLine($"  Error: {result.Failure.ErrorMessage}");
            }

            AppendDiagnostics(builder, result.Diagnostics, indent: "  ");
        }

        return builder.ToString().TrimEnd();
    }

    private static string FormatFileStatus(ModelLoadFileResult result)
    {
        if (result.Failure is not null)
        {
            return "Failed";
        }

        return result.Applied ? "ImportedApplied" : "ImportedNotApplied";
    }

    public static string BuildDiagnosticsBundle(
        VideraBackendDiagnostics? diagnostics,
        RenderCapabilitySnapshot? renderCapabilities,
        int loadedModelCount,
        ModelLoadBatchResult? lastImportResult,
        DemoSupportSettings settings)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Videra demo diagnostics bundle");
        builder.AppendLine($"GeneratedUtc: {DateTimeOffset.UtcNow:O}");
        AppendEnvironment(builder);
        AppendPackageVersions(builder);
        builder.AppendLine($"LoadedModelCount: {loadedModelCount}");
        AppendSettings(builder, settings);
        AppendRenderCapabilities(builder, renderCapabilities);
        builder.AppendLine();
        builder.AppendLine("Backend diagnostics");
        builder.AppendLine(diagnostics is null
            ? "Unavailable"
            : VideraDiagnosticsSnapshotFormatter.Format(diagnostics));
        builder.AppendLine();
        builder.AppendLine(FormatImportReport(lastImportResult));
        return builder.ToString().TrimEnd();
    }

    public static string BuildMinimalReproduction(
        VideraBackendDiagnostics? diagnostics,
        ModelLoadBatchResult? lastImportResult,
        DemoSupportSettings settings)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Videra minimal reproduction metadata");
        builder.AppendLine($"GeneratedUtc: {DateTimeOffset.UtcNow:O}");
        AppendEnvironment(builder);
        AppendSettings(builder, settings);
        builder.AppendLine("ScenePaths:");

        var paths = lastImportResult?.Results
            .Select(static result => result.Path)
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.Ordinal)
            .ToArray() ?? [];

        if (paths.Length == 0)
        {
            builder.AppendLine("- <none>");
        }
        else
        {
            foreach (var path in paths)
            {
                builder.AppendLine($"- {path}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("DiagnosticsSnapshot:");
        builder.AppendLine(diagnostics is null
            ? "Unavailable"
            : VideraDiagnosticsSnapshotFormatter.Format(diagnostics));
        return builder.ToString().TrimEnd();
    }

    private static void AppendEnvironment(StringBuilder builder)
    {
        builder.AppendLine("Environment:");
        builder.AppendLine($"  OS: {RuntimeInformation.OSDescription}");
        builder.AppendLine($"  ProcessArchitecture: {RuntimeInformation.ProcessArchitecture}");
        builder.AppendLine($"  Framework: {RuntimeInformation.FrameworkDescription}");
        builder.AppendLine($"  RuntimeVersion: {Environment.Version}");
    }

    private static void AppendPackageVersions(StringBuilder builder)
    {
        builder.AppendLine("Packages:");
        AppendAssemblyVersion(builder, typeof(VideraView).Assembly);
        AppendAssemblyVersion(builder, typeof(ObjModelImporter).Assembly);
        AppendAssemblyVersion(builder, typeof(GltfModelImporter).Assembly);
    }

    private static void AppendAssemblyVersion(StringBuilder builder, Assembly assembly)
    {
        var name = assembly.GetName();
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;
        builder.AppendLine($"  {name.Name}: {informationalVersion ?? name.Version?.ToString() ?? "unknown"}");
    }

    private static void AppendSettings(StringBuilder builder, DemoSupportSettings settings)
    {
        builder.AppendLine("Settings:");
        builder.AppendLine($"  RenderStyle: {settings.RenderStyle}");
        builder.AppendLine($"  WireframeMode: {settings.WireframeMode}");
        builder.AppendLine($"  IsGridVisible: {settings.IsGridVisible}");
        builder.AppendLine($"  GridHeight: {settings.GridHeight}");
        builder.AppendLine($"  GridColor: {settings.GridColor}");
        builder.AppendLine($"  BackgroundColor: {settings.BackgroundColor}");
        builder.AppendLine($"  CameraInvertX: {settings.CameraInvertX}");
        builder.AppendLine($"  CameraInvertY: {settings.CameraInvertY}");
        builder.AppendLine($"  SelectedObject: {settings.SelectedObjectName ?? "<none>"}");
    }

    private static void AppendRenderCapabilities(StringBuilder builder, RenderCapabilitySnapshot? capabilities)
    {
        builder.AppendLine("RenderCapabilities:");
        if (capabilities is null)
        {
            builder.AppendLine("  Unavailable");
            return;
        }

        builder.AppendLine($"  IsInitialized: {capabilities.IsInitialized}");
        builder.AppendLine($"  ActiveBackendPreference: {capabilities.ActiveBackendPreference?.ToString() ?? "Unknown"}");
        builder.AppendLine($"  SupportsPassContributors: {capabilities.SupportsPassContributors}");
        builder.AppendLine($"  SupportsPassReplacement: {capabilities.SupportsPassReplacement}");
        builder.AppendLine($"  SupportsFrameHooks: {capabilities.SupportsFrameHooks}");
        builder.AppendLine($"  SupportsPipelineSnapshots: {capabilities.SupportsPipelineSnapshots}");
        builder.AppendLine($"  SupportsShaderCreation: {capabilities.SupportsShaderCreation}");
        builder.AppendLine($"  SupportsResourceSetCreation: {capabilities.SupportsResourceSetCreation}");
        builder.AppendLine($"  SupportsResourceSetBinding: {capabilities.SupportsResourceSetBinding}");
        builder.AppendLine($"  SupportedFeatureNames: {FormatList(capabilities.SupportedFeatureNames)}");
        builder.AppendLine($"  LastPipelineProfile: {capabilities.LastPipelineSnapshot?.Profile.ToString() ?? "Unavailable"}");
    }

    private static void AppendDiagnostics(
        StringBuilder builder,
        IReadOnlyList<ModelImportDiagnostic> diagnostics,
        string indent)
    {
        if (diagnostics.Count == 0)
        {
            builder.AppendLine($"{indent}Diagnostics: none");
            return;
        }

        builder.AppendLine($"{indent}Diagnostics:");
        foreach (var diagnostic in diagnostics)
        {
            var code = string.IsNullOrWhiteSpace(diagnostic.Code) ? string.Empty : $" [{diagnostic.Code}]";
            builder.AppendLine($"{indent}- {diagnostic.Severity}{code}: {diagnostic.Message}");
        }
    }

    private static string FormatList(IReadOnlyList<string> values)
    {
        return values.Count == 0 ? "None" : string.Join(", ", values);
    }
}
