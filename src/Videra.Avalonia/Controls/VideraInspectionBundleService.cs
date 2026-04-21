using System.Numerics;
using System.Text.Json;
using Avalonia.Media;
using Videra.Avalonia.Controls.Interaction;
using Videra.Core.Inspection;
using Videra.Core.Selection.Annotations;

namespace Videra.Avalonia.Controls;

public static class VideraInspectionBundleService
{
    private const string AssetsDirectoryName = "assets";
    public const string InspectionStateFileName = "inspection-state.json";
    public const string DiagnosticsFileName = "diagnostics.txt";
    public const string SnapshotFileName = "snapshot.png";
    public const string AssetManifestFileName = "asset-manifest.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static async Task<VideraInspectionBundleExportResult> ExportAsync(
        VideraView view,
        string directoryPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Directory.CreateDirectory(directoryPath);

            var inspectionState = CreateInspectionStateDocument(view.CaptureInspectionState());
            var sourceAssetManifest = view.CaptureInspectionBundleAssetManifestForRuntime();
            var assetManifest = CreateBundledAssetManifest(sourceAssetManifest, directoryPath);
            var diagnostics = VideraDiagnosticsSnapshotFormatter.Format(view.BackendDiagnostics);

            await WriteJsonAsync(Path.Combine(directoryPath, InspectionStateFileName), inspectionState, cancellationToken).ConfigureAwait(true);
            await WriteJsonAsync(Path.Combine(directoryPath, AssetManifestFileName), assetManifest, cancellationToken).ConfigureAwait(true);
            await File.WriteAllTextAsync(Path.Combine(directoryPath, DiagnosticsFileName), diagnostics, cancellationToken).ConfigureAwait(true);

            var snapshotPath = Path.Combine(directoryPath, SnapshotFileName);
            var snapshot = await view.ExportSnapshotAsync(snapshotPath, cancellationToken).ConfigureAwait(true);
            if (!snapshot.Succeeded)
            {
                return VideraInspectionBundleExportResult.Failed(
                    directoryPath,
                    snapshot.Failure?.Message ?? "Snapshot export failed.",
                    assetManifest.CanReplayScene,
                    assetManifest.ReplayLimitation);
            }

            return VideraInspectionBundleExportResult.Success(
                directoryPath,
                assetManifest.CanReplayScene,
                assetManifest.ReplayLimitation,
                assetManifest.Entries.Count,
                inspectionState.Annotations.Count);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return VideraInspectionBundleExportResult.Failed(
                directoryPath,
                ex.Message,
                canReplayScene: false,
                replayLimitation: null);
        }
    }

    public static async Task<VideraInspectionBundleImportResult> ImportAsync(
        VideraView view,
        string directoryPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var inspectionState = ReadJson<VideraInspectionBundleStateDocument>(directoryPath, InspectionStateFileName);
            var assetManifest = ReadJson<VideraInspectionBundleAssetManifest>(directoryPath, AssetManifestFileName);

            if (!assetManifest.CanReplayScene)
            {
                return VideraInspectionBundleImportResult.Failed(
                    directoryPath,
                    assetManifest.ReplayLimitation ?? "The inspection bundle cannot replay the original scene.");
            }

            Dictionary<Guid, Guid> objectIdMap = new();
            var sceneReloaded = false;

            if (assetManifest.Entries.Count > 0)
            {
                var filePaths = assetManifest.Entries
                    .Select(entry => ResolveBundledAssetPath(directoryPath, entry.FilePath))
                    .ToArray();
                var loadResult = await view.LoadModelsAsync(filePaths, cancellationToken).ConfigureAwait(true);
                if (!loadResult.Succeeded || loadResult.LoadedObjects.Count != assetManifest.Entries.Count)
                {
                    var failure = loadResult.Failures.Count == 0
                        ? "LoadModelsAsync did not recreate the bundle scene."
                        : string.Join(" | ", loadResult.Failures.Select(static failure => $"{failure.Path}: {failure.ErrorMessage}"));
                    return VideraInspectionBundleImportResult.Failed(directoryPath, failure);
                }

                for (var i = 0; i < assetManifest.Entries.Count; i++)
                {
                    var manifestEntry = assetManifest.Entries[i];
                    var loadedObject = loadResult.LoadedObjects[i];
                    objectIdMap[manifestEntry.OriginalObjectId] = loadedObject.Id;
                    loadedObject.Name = manifestEntry.Name;
                    loadedObject.Position = manifestEntry.Position.ToVector3();
                    loadedObject.Rotation = manifestEntry.Rotation.ToVector3();
                    loadedObject.Scale = manifestEntry.Scale.ToVector3();
                }

                sceneReloaded = true;
            }

            view.ApplyInspectionState(CreateInspectionState(inspectionState, objectIdMap));

            return VideraInspectionBundleImportResult.Success(
                directoryPath,
                sceneReloaded,
                assetManifest.Entries.Count,
                inspectionState.Annotations.Count);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return VideraInspectionBundleImportResult.Failed(directoryPath, ex.Message);
        }
    }

    private static Task WriteJsonAsync<T>(string path, T value, CancellationToken cancellationToken)
    {
        return File.WriteAllTextAsync(path, JsonSerializer.Serialize(value, JsonOptions), cancellationToken);
    }

    private static T ReadJson<T>(string directoryPath, string fileName)
    {
        var path = Path.Combine(directoryPath, fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Required inspection bundle file '{fileName}' was not found.", path);
        }

        return JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidDataException($"Inspection bundle file '{fileName}' could not be deserialized.");
    }

    private static VideraInspectionBundleAssetManifest CreateBundledAssetManifest(
        VideraInspectionBundleAssetManifest sourceManifest,
        string directoryPath)
    {
        if (sourceManifest.Entries.Count == 0)
        {
            return sourceManifest;
        }

        var copiedAssetPaths = new Dictionary<string, string>(StringComparer.Ordinal);
        var bundledEntries = new List<VideraInspectionBundleAssetEntry>(sourceManifest.Entries.Count);
        var canReplayScene = sourceManifest.CanReplayScene;
        var replayLimitation = sourceManifest.ReplayLimitation;

        Directory.CreateDirectory(Path.Combine(directoryPath, AssetsDirectoryName));

        foreach (var entry in sourceManifest.Entries)
        {
            var bundledRelativePath = TryCopyAssetIntoBundle(directoryPath, entry.FilePath, copiedAssetPaths, out var copyFailure);
            if (copyFailure is not null)
            {
                canReplayScene = false;
                replayLimitation = AppendReplayLimitation(replayLimitation, copyFailure);
            }

            bundledEntries.Add(new VideraInspectionBundleAssetEntry
            {
                OriginalObjectId = entry.OriginalObjectId,
                FilePath = bundledRelativePath ?? string.Empty,
                Name = entry.Name,
                Position = entry.Position,
                Rotation = entry.Rotation,
                Scale = entry.Scale
            });
        }

        return new VideraInspectionBundleAssetManifest
        {
            CanReplayScene = canReplayScene,
            ReplayLimitation = replayLimitation,
            Entries = bundledEntries
        };
    }

    private static string? TryCopyAssetIntoBundle(
        string directoryPath,
        string sourcePath,
        Dictionary<string, string> copiedAssetPaths,
        out string? failure)
    {
        failure = null;

        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            failure = "One or more imported assets did not expose a source file path, so the bundle could not package a replayable scene.";
            return null;
        }

        var normalizedSourcePath = Path.GetFullPath(sourcePath);
        if (!File.Exists(normalizedSourcePath))
        {
            failure = "One or more imported assets were no longer present on disk during bundle export, so the bundle could not package a replayable scene.";
            return null;
        }

        if (copiedAssetPaths.TryGetValue(normalizedSourcePath, out var bundledRelativePath))
        {
            return bundledRelativePath;
        }

        var assetIndex = copiedAssetPaths.Count;
        var bundledFileName = $"asset-{assetIndex:D3}{Path.GetExtension(normalizedSourcePath)}";
        bundledRelativePath = $"{AssetsDirectoryName}/{bundledFileName}";
        var destinationPath = Path.Combine(directoryPath, bundledRelativePath.Replace('/', Path.DirectorySeparatorChar));
        File.Copy(normalizedSourcePath, destinationPath, overwrite: true);
        copiedAssetPaths[normalizedSourcePath] = bundledRelativePath;
        return bundledRelativePath;
    }

    private static string AppendReplayLimitation(string? existing, string next)
    {
        if (string.IsNullOrWhiteSpace(existing))
        {
            return next;
        }

        return existing.Contains(next, StringComparison.Ordinal) ? existing : $"{existing} {next}";
    }

    private static string ResolveBundledAssetPath(string directoryPath, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new InvalidDataException("Inspection bundle asset entry is missing a bundled file path.");
        }

        if (Path.IsPathRooted(relativePath))
        {
            throw new InvalidDataException("Inspection bundle asset entries must use relative bundled paths.");
        }

        var bundleRoot = Path.GetFullPath(directoryPath);
        var resolvedPath = Path.GetFullPath(Path.Combine(bundleRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var bundleRootWithSeparator = bundleRoot.EndsWith(Path.DirectorySeparatorChar)
            ? bundleRoot
            : bundleRoot + Path.DirectorySeparatorChar;
        if (!resolvedPath.StartsWith(bundleRootWithSeparator, comparison))
        {
            throw new InvalidDataException("Inspection bundle asset entries must stay inside the bundle directory.");
        }

        if (!File.Exists(resolvedPath))
        {
            throw new FileNotFoundException("Inspection bundle asset file was not found inside the bundle.", resolvedPath);
        }

        return resolvedPath;
    }

    private static VideraInspectionBundleStateDocument CreateInspectionStateDocument(VideraInspectionState state)
    {
        return new VideraInspectionBundleStateDocument
        {
            CameraTarget = BundleVector3.From(state.CameraTarget),
            CameraRadius = state.CameraRadius,
            CameraYaw = state.CameraYaw,
            CameraPitch = state.CameraPitch,
            SelectedObjectIds = state.SelectedObjectIds.ToArray(),
            PrimarySelectedObjectId = state.PrimarySelectedObjectId,
            Annotations = CreateAnnotationEntries(state.Annotations),
            MeasurementSnapMode = state.MeasurementSnapMode,
            ClippingPlanes = state.ClippingPlanes
                .Select(static plane => new VideraInspectionBundleClipPlane
                {
                    Point = BundleVector3.From(plane.Point),
                    Normal = BundleVector3.From(plane.Normal),
                    IsEnabled = plane.IsEnabled
                })
                .ToArray(),
            Measurements = state.Measurements
                .Select(static measurement => new VideraInspectionBundleMeasurement
                {
                    Id = measurement.Id,
                    Label = measurement.Label,
                    IsVisible = measurement.IsVisible,
                    Start = CreateMeasurementAnchor(measurement.Start),
                    End = CreateMeasurementAnchor(measurement.End)
                })
                .ToArray()
        };
    }

    private static VideraInspectionState CreateInspectionState(
        VideraInspectionBundleStateDocument document,
        IReadOnlyDictionary<Guid, Guid> objectIdMap)
    {
        return new VideraInspectionState
        {
            CameraTarget = document.CameraTarget.ToVector3(),
            CameraRadius = document.CameraRadius,
            CameraYaw = document.CameraYaw,
            CameraPitch = document.CameraPitch,
            SelectedObjectIds = document.SelectedObjectIds
                .Select(objectId => RemapObjectId(objectId, objectIdMap))
                .ToArray(),
            PrimarySelectedObjectId = RemapObjectId(document.PrimarySelectedObjectId, objectIdMap),
            Annotations = CreateAnnotations(document.Annotations, objectIdMap),
            MeasurementSnapMode = document.MeasurementSnapMode,
            ClippingPlanes = document.ClippingPlanes
                .Select(static plane => VideraClipPlane.FromPointNormal(plane.Point.ToVector3(), plane.Normal.ToVector3(), plane.IsEnabled))
                .ToArray(),
            Measurements = document.Measurements
                .Select(measurement => new VideraMeasurement
                {
                    Id = measurement.Id,
                    Label = measurement.Label,
                    IsVisible = measurement.IsVisible,
                    Start = CreateMeasurementAnchor(measurement.Start, objectIdMap),
                    End = CreateMeasurementAnchor(measurement.End, objectIdMap)
                })
                .ToArray()
        };
    }

    private static IReadOnlyList<VideraInspectionBundleAnnotationEntry> CreateAnnotationEntries(IReadOnlyList<VideraAnnotation> annotations)
    {
        return annotations.Select(static annotation => annotation switch
        {
            VideraNodeAnnotation node => new VideraInspectionBundleAnnotationEntry
            {
                Kind = AnnotationAnchorKind.Object,
                Id = node.Id,
                Text = node.Text,
                Color = node.Color.ToString(),
                IsVisible = node.IsVisible,
                ObjectId = node.ObjectId
            },
            VideraWorldPointAnnotation world => new VideraInspectionBundleAnnotationEntry
            {
                Kind = AnnotationAnchorKind.WorldPoint,
                Id = world.Id,
                Text = world.Text,
                Color = world.Color.ToString(),
                IsVisible = world.IsVisible,
                WorldPoint = BundleVector3.From(world.WorldPoint)
            },
            _ => throw new InvalidOperationException($"Unsupported annotation type '{annotation.GetType().FullName}'.")
        }).ToArray();
    }

    private static VideraInspectionBundleMeasurementAnchor CreateMeasurementAnchor(VideraMeasurementAnchor anchor)
    {
        return new VideraInspectionBundleMeasurementAnchor
        {
            WorldPoint = BundleVector3.From(anchor.WorldPoint),
            ObjectId = anchor.ObjectId
        };
    }

    private static VideraMeasurementAnchor CreateMeasurementAnchor(
        VideraInspectionBundleMeasurementAnchor anchor,
        IReadOnlyDictionary<Guid, Guid> objectIdMap)
    {
        var objectId = RemapObjectId(anchor.ObjectId, objectIdMap);
        return objectId.HasValue
            ? VideraMeasurementAnchor.ForObjectPoint(objectId.Value, anchor.WorldPoint.ToVector3())
            : VideraMeasurementAnchor.ForWorldPoint(anchor.WorldPoint.ToVector3());
    }

    private static IReadOnlyList<VideraAnnotation> CreateAnnotations(
        IReadOnlyList<VideraInspectionBundleAnnotationEntry> entries,
        IReadOnlyDictionary<Guid, Guid> objectIdMap)
    {
        return entries.Select<VideraInspectionBundleAnnotationEntry, VideraAnnotation>(entry => entry.Kind switch
        {
            AnnotationAnchorKind.Object => new VideraNodeAnnotation
            {
                Id = entry.Id,
                Text = entry.Text,
                Color = Color.Parse(entry.Color),
                IsVisible = entry.IsVisible,
                ObjectId = RemapObjectId(entry.ObjectId, objectIdMap)
                    ?? throw new InvalidDataException("Object annotation entry is missing an object identifier.")
            },
            AnnotationAnchorKind.WorldPoint => new VideraWorldPointAnnotation
            {
                Id = entry.Id,
                Text = entry.Text,
                Color = Color.Parse(entry.Color),
                IsVisible = entry.IsVisible,
                WorldPoint = entry.WorldPoint?.ToVector3()
                    ?? throw new InvalidDataException("World-point annotation entry is missing a world point.")
            },
            _ => throw new InvalidDataException($"Unsupported annotation anchor kind '{entry.Kind}'.")
        }).ToArray();
    }

    private static Guid RemapObjectId(Guid objectId, IReadOnlyDictionary<Guid, Guid> objectIdMap)
    {
        return objectIdMap.TryGetValue(objectId, out var remapped) ? remapped : objectId;
    }

    private static Guid? RemapObjectId(Guid? objectId, IReadOnlyDictionary<Guid, Guid> objectIdMap)
    {
        return objectId.HasValue ? RemapObjectId(objectId.Value, objectIdMap) : null;
    }
}

public sealed class VideraInspectionBundleExportResult
{
    public bool Succeeded { get; init; }

    public required string DirectoryPath { get; init; }

    public bool CanReplayScene { get; init; }

    public string? ReplayLimitation { get; init; }

    public int AssetCount { get; init; }

    public int AnnotationCount { get; init; }

    public string? FailureMessage { get; init; }

    internal static VideraInspectionBundleExportResult Success(
        string directoryPath,
        bool canReplayScene,
        string? replayLimitation,
        int assetCount,
        int annotationCount)
    {
        return new VideraInspectionBundleExportResult
        {
            Succeeded = true,
            DirectoryPath = directoryPath,
            CanReplayScene = canReplayScene,
            ReplayLimitation = replayLimitation,
            AssetCount = assetCount,
            AnnotationCount = annotationCount
        };
    }

    internal static VideraInspectionBundleExportResult Failed(
        string directoryPath,
        string failureMessage,
        bool canReplayScene,
        string? replayLimitation)
    {
        return new VideraInspectionBundleExportResult
        {
            Succeeded = false,
            DirectoryPath = directoryPath,
            CanReplayScene = canReplayScene,
            ReplayLimitation = replayLimitation,
            FailureMessage = failureMessage
        };
    }
}

public sealed class VideraInspectionBundleImportResult
{
    public bool Succeeded { get; init; }

    public required string DirectoryPath { get; init; }

    public bool SceneReloaded { get; init; }

    public int AssetCount { get; init; }

    public int AnnotationCount { get; init; }

    public string? FailureMessage { get; init; }

    internal static VideraInspectionBundleImportResult Success(
        string directoryPath,
        bool sceneReloaded,
        int assetCount,
        int annotationCount)
    {
        return new VideraInspectionBundleImportResult
        {
            Succeeded = true,
            DirectoryPath = directoryPath,
            SceneReloaded = sceneReloaded,
            AssetCount = assetCount,
            AnnotationCount = annotationCount
        };
    }

    internal static VideraInspectionBundleImportResult Failed(string directoryPath, string failureMessage)
    {
        return new VideraInspectionBundleImportResult
        {
            Succeeded = false,
            DirectoryPath = directoryPath,
            FailureMessage = failureMessage
        };
    }
}

internal sealed class VideraInspectionBundleStateDocument
{
    public required BundleVector3 CameraTarget { get; init; }

    public float CameraRadius { get; init; }

    public float CameraYaw { get; init; }

    public float CameraPitch { get; init; }

    public IReadOnlyList<Guid> SelectedObjectIds { get; init; } = Array.Empty<Guid>();

    public Guid? PrimarySelectedObjectId { get; init; }

    public IReadOnlyList<VideraInspectionBundleAnnotationEntry> Annotations { get; init; } = Array.Empty<VideraInspectionBundleAnnotationEntry>();

    public VideraMeasurementSnapMode MeasurementSnapMode { get; init; } = VideraMeasurementSnapMode.Free;

    public IReadOnlyList<VideraInspectionBundleClipPlane> ClippingPlanes { get; init; } = Array.Empty<VideraInspectionBundleClipPlane>();

    public IReadOnlyList<VideraInspectionBundleMeasurement> Measurements { get; init; } = Array.Empty<VideraInspectionBundleMeasurement>();
}

internal sealed class VideraInspectionBundleClipPlane
{
    public required BundleVector3 Point { get; init; }

    public required BundleVector3 Normal { get; init; }

    public bool IsEnabled { get; init; } = true;
}

internal sealed class VideraInspectionBundleMeasurement
{
    public Guid Id { get; init; }

    public string? Label { get; init; }

    public bool IsVisible { get; init; } = true;

    public required VideraInspectionBundleMeasurementAnchor Start { get; init; }

    public required VideraInspectionBundleMeasurementAnchor End { get; init; }
}

internal sealed class VideraInspectionBundleMeasurementAnchor
{
    public required BundleVector3 WorldPoint { get; init; }

    public Guid? ObjectId { get; init; }
}

internal sealed class VideraInspectionBundleAnnotationEntry
{
    public AnnotationAnchorKind Kind { get; init; }

    public Guid Id { get; init; }

    public string Text { get; init; } = string.Empty;

    public string Color { get; init; } = Colors.White.ToString();

    public bool IsVisible { get; init; } = true;

    public Guid? ObjectId { get; init; }

    public BundleVector3? WorldPoint { get; init; }
}

internal sealed class VideraInspectionBundleAssetManifest
{
    public bool CanReplayScene { get; init; }

    public string? ReplayLimitation { get; init; }

    public IReadOnlyList<VideraInspectionBundleAssetEntry> Entries { get; init; } = Array.Empty<VideraInspectionBundleAssetEntry>();
}

internal sealed class VideraInspectionBundleAssetEntry
{
    public Guid OriginalObjectId { get; init; }

    public string FilePath { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public required BundleVector3 Position { get; init; }

    public required BundleVector3 Rotation { get; init; }

    public BundleVector3 Scale { get; init; } = BundleVector3.From(Vector3.One);
}

internal sealed record BundleVector3(float X, float Y, float Z)
{
    public static BundleVector3 From(Vector3 vector) => new(vector.X, vector.Y, vector.Z);

    public Vector3 ToVector3() => new(X, Y, Z);
}
