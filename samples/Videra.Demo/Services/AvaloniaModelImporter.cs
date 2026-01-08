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

    // 构造函数注入资源工厂
    public AvaloniaModelImporter(TopLevel topLevel, IResourceFactory factory)
    {
        _topLevel = topLevel;
        _factory = factory;
    }

    public async Task<IEnumerable<Object3D>> ImportModelsAsync()
    {
        // 1. 调用 Avalonia 文件选择器
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

        // 2. 加载模型
        foreach (var file in files)
            try
            {
                var path = file.Path.LocalPath;
                // 在后台线程加载，避免卡顿 UI
                var obj = await Task.Run(() => ModelImporter.Load(path, _factory));
                results.Add(obj);
            }
            catch (Exception ex)
            {
                // 实际项目中这里可以弹窗提示错误
                Console.WriteLine($"Load Error: {ex.Message}");
            }

        return results;
    }
}