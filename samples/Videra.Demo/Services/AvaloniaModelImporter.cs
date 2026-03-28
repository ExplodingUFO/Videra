using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.IO;

namespace Videra.Demo.Services;

public class AvaloniaModelImporter : IModelImporter
{
    private readonly IResourceFactory _factory;
    private readonly TopLevel _topLevel;

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
            catch (Exception)
            {
                // 导入错误由调用方通过返回结果数量和状态消息处理
            }
        }

        return results;
    }
}
