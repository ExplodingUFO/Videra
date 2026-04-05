using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.Logging;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.IO;

namespace Videra.Demo.Services;

public partial class AvaloniaModelImporter : IModelImporter
{
    private readonly IResourceFactory _factory;
    private readonly TopLevel _topLevel;
    private readonly ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<AvaloniaModelImporter>();

    public AvaloniaModelImporter(TopLevel topLevel, IResourceFactory factory)
    {
        _topLevel = topLevel;
        _factory = factory;
    }

    public async Task<IEnumerable<Object3D>> ImportModelsAsync()
    {
        var files = await _topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import 3D Models",
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("3D Models") { Patterns = ModelImporter.SupportedFormats }
            }
        });

        var results = new List<Object3D>();

        foreach (var file in files)
        {
            try
            {
                var path = file.Path.LocalPath;
                var obj = await Task.Run(() => ModelImporter.Load(path, _factory));
                results.Add(obj);
            }
            catch (Exception ex)
            {
                Log.ImportFailed(_logger, file.Path.LocalPath, ex);
            }
        }

        return results;
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Failed to import model: {FilePath}")]
        public static partial void ImportFailed(ILogger logger, string filePath, Exception exception);
    }
}
