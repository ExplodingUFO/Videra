using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.Logging;
using Videra.Avalonia.Controls;
using Videra.Core.Exceptions;
using Videra.Core.Graphics;
using Videra.Core.Scene;
using Videra.Import.Gltf;
using Videra.Import.Obj;

namespace Videra.Demo.Services;

public partial class AvaloniaModelImporter : IModelImporter
{
    private readonly TopLevel _topLevel;
    private readonly VideraView _view;
    private readonly ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<AvaloniaModelImporter>();

    public AvaloniaModelImporter(TopLevel topLevel, VideraView view)
    {
        _topLevel = topLevel;
        _view = view;
        _view.Options.ModelImporter ??= ImportModel;
    }

    public async Task<ModelLoadBatchResult> ImportModelsAsync()
    {
        var files = await _topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import 3D Models",
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("3D Models") { Patterns = GltfModelImporter.SupportedFormats.Concat(ObjModelImporter.SupportedFormats).ToArray() }
            }
        });

        var paths = files
            .Select(file => file.Path.LocalPath)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .ToArray();

        if (paths.Length == 0)
        {
            return new ModelLoadBatchResult(Array.Empty<SceneDocumentEntry>(), Array.Empty<ModelLoadFailure>(), TimeSpan.Zero);
        }

        var result = await _view.LoadModelsAsync(paths).ConfigureAwait(true);

        foreach (var failure in result.Failures)
        {
            Log.ImportFailed(_logger, failure.Path, failure.Exception);
        }

        if (result.Succeeded && result.Entries.Count > 0)
        {
            _view.FrameAll();
        }

        return result;
    }

    private static ImportedSceneAsset ImportModel(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".gltf" or ".glb" => GltfModelImporter.Import(path),
            ".obj" => ObjModelImporter.Import(path),
            _ => throw new InvalidModelInputException(
                $"File extension '{Path.GetExtension(path)}' is not supported.",
                "LoadModel",
                new Dictionary<string, string?> { ["Extension"] = Path.GetExtension(path) })
        };
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Failed to import model: {FilePath}")]
        public static partial void ImportFailed(ILogger logger, string filePath, Exception exception);
    }
}
