using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Videra.Core.Graphics;
using Videra.Core.IO;

namespace Videra.Demo.Services;

public class AvaloniaModelImporter : IModelImporter
{
    private readonly VideraEngine _engine;
    private readonly TopLevel _topLevel;

    // 构造函数注入 View 的关键组件
    public AvaloniaModelImporter(TopLevel topLevel, VideraEngine engine)
    {
        _topLevel = topLevel;
        _engine = engine;
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

        // 2. 加载模型 (使用 Engine 的 GraphicsDevice)
        foreach (var file in files)
            try
            {
                var path = file.Path.LocalPath;
                // 在后台线程加载，避免卡顿 UI
                var obj = await Task.Run(() => ModelImporter.Load(path, _engine.GraphicsDevice));
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