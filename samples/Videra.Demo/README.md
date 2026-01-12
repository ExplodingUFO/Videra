# Videra Demo 运行指南

## 🎯 当前状态

项目已成功完成从 Veldrid 到 Silk.NET 的**基础架构迁移**，所有编译错误已修复。

### ✅ 已完成
- 抽象接口层实现
- Windows/macOS/Linux 三个平台后端框架
- VideraView 控件创建
- 项目编译成功

### ⚠️ 临时措施
为了让 Demo 程序能够编译和运行，我们做了以下临时调整：

1. **3D 视图占位符**: MainWindow.axaml 中暂时用 TextBlock 替代了 VideraView
2. **禁用模型导入**: ImportCommand 会显示警告而不会崩溃
3. **可选依赖注入**: MainWindowViewModel 接受 nullable IModelImporter

## 🚀 运行 Demo

### macOS
```bash
cd /Users/superdragon/RiderProjects/Videra
dotnet build samples/Videra.Demo/Videra.Demo.csproj
dotnet run --project samples/Videra.Demo/Videra.Demo.csproj
```

### Windows
```bash
cd C:\Path\To\Videra
dotnet build samples\Videra.Demo\Videra.Demo.csproj
dotnet run --project samples\Videra.Demo\Videra.Demo.csproj
```

### Linux
```bash
cd ~/Videra
dotnet build samples/Videra.Demo/Videra.Demo.csproj
dotnet run --project samples/Videra.Demo/Videra.Demo.csproj
```

## 📋 预期行为

运行后你会看到：
- ✅ 窗口正常打开
- ✅ 右侧面板显示（环境设置、场景图等）
- ⚠️ 左侧 3D 视图显示 "3D View (Under Migration to Silk.NET)"
- ⚠️ 点击 "Import Model" 按钮不会有响应（功能已禁用）

## 🔧 恢复完整功能的步骤

要恢复 3D 渲染功能，需要完成以下工作：

### 1. 实现 Shader 编译系统
```csharp
// src/Videra.Platform.Windows/D3D11ShaderCompiler.cs
// src/Videra.Platform.macOS/MetalShaderCompiler.cs
// src/Videra.Platform.Linux/VulkanShaderCompiler.cs
```

### 2. 完成 Pipeline 创建
- D3D11: Input Layout + Rasterizer State
- Metal: Render Pipeline State Object
- Vulkan: Graphics Pipeline 完整配置

### 3. 重写 VideraEngine 渲染循环
使用新的抽象接口替代 Veldrid API

### 4. 恢复 MainWindow.axaml 中的 VideraView
取消注释以下代码：
```xml
<controls:VideraView Name="View3D" 
             Grid.Column="0"
             Items="{Binding SceneObjects}"
             BackgroundColor="{Binding BgColor}"
             .../>
```

### 5. 恢复 MainWindow.axaml.cs 中的集成
```csharp
var engine = View3D.Engine;
var importerService = new AvaloniaModelImporter(topLevel, engine);
DataContext = new MainWindowViewModel(importerService);
```

## 📚 相关文档

- [ARCHITECTURE.md](../../ARCHITECTURE.md) - 完整架构设计
- [MIGRATION_GUIDE.md](../../MIGRATION_GUIDE.md) - 详细迁移指南
- [IMPLEMENTATION_STATUS.md](../../IMPLEMENTATION_STATUS.md) - 实施状态和后续计划

## 🐛 故障排除

### 编译错误
如果遇到编译错误，确保：
- .NET 8.0 SDK 已安装
- 所有 NuGet 包已恢复：`dotnet restore`
- 平台特定项目的 RuntimeIdentifier 正确

### 运行时错误
当前版本不应该有运行时错误，因为 3D 渲染功能已临时禁用。

## 💡 开发建议

建议按照以下顺序进行开发：

1. **Week 1-2**: 实现 Shader 编译和 Pipeline 创建
2. **Week 3**: 重写 VideraEngine 渲染循环
3. **Week 4**: 测试和优化
4. **Week 5**: 恢复 Demo 的完整功能

## 📞 需要帮助？

参考以下资源：
- Silk.NET 文档: https://dotnet.github.io/Silk.NET/
- Direct3D 11 文档: https://docs.microsoft.com/en-us/windows/win32/direct3d11/
- Metal 文档: https://developer.apple.com/metal/
- Vulkan 教程: https://vulkan-tutorial.com/

---

**最后更新**: 2026年1月8日  
**状态**: 可编译运行，3D 渲染功能待实现
