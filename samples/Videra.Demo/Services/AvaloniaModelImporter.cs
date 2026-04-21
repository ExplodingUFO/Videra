using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.Logging;
using Videra.Avalonia.Controls;
using Videra.Core.Graphics;
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
            return new ModelLoadBatchResult(Array.Empty<Object3D>(), Array.Empty<ModelLoadFailure>(), TimeSpan.Zero);
        }

        var result = await _view.LoadModelsAsync(paths).ConfigureAwait(true);

        foreach (var failure in result.Failures)
        {
            Log.ImportFailed(_logger, failure.Path, failure.Exception);
        }

        if (result.Succeeded && result.LoadedObjects.Count > 0)
        {
            _view.FrameAll();
        }

        return result;
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Failed to import model: {FilePath}")]
        public static partial void ImportFailed(ILogger logger, string filePath, Exception exception);
    }
}
