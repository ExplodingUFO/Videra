# Videra.Demo

[English](../../../samples/Videra.Demo/README.md) | [中文](demo.md)

`Videra.Demo` 是用于演示 Videra 在真实 Avalonia 桌面流程中如何接入的示例应用。

## 它展示了什么

- 在 Avalonia 窗口中集成 `VideraView`
- 通过 `DemoSceneBootstrapper` 等待后端就绪后再初始化导入能力
- 在 backend ready 后自动创建并取景默认演示立方体
- 使用高层场景 API：`LoadModelAsync` / `LoadModelsAsync`
- 在成功导入或默认场景加载后调用 `FrameAll()`
- 提供 `Frame All` / `Reset Camera` 快捷操作
- 通过 `BackendDiagnostics` 展示当前请求后端、实际解析后端、native host 绑定状态和 fallback 信息
- 切换渲染风格、线框模式、网格可见性和对象变换

## 运行时行为

Demo 启动后会等待 `VideraView` 完成后端初始化，然后：

1. 绑定导入服务
2. 通过高层场景 API 加载默认演示立方体
3. 调用 `FrameAll()` 让默认场景进入视野
4. 在右侧面板显示当前 backend diagnostics

如果默认演示立方体创建失败，Demo 仍会保持 backend ready，并在状态区域明确提示失败原因；此时仍可继续导入模型。

`PreferredBackend="Auto"` 会按当前平台优先选择原生后端：

- Windows: Direct3D 11
- Linux: Vulkan
- macOS: Metal

`Program.cs` 不再隐式覆盖 `VIDERA_BACKEND`，因此 Demo 与公开文档使用同一套后端选择路径。

## 高层导入流程

当前 Demo 的模型导入走的是 `VideraView` 自带的高层 API，而不是直接手动操作 `Engine`：

```csharp
var result = await View3D.LoadModelsAsync(paths);
if (result.LoadedObjects.Count > 0)
{
    View3D.FrameAll();
}

var diagnostics = View3D.BackendDiagnostics;
```

如果导入中有失败项，Demo 会保留成功加载的模型，并把失败信息汇总到状态栏中。
导入结果与默认场景失败信息都会进入状态区域，避免只写日志而用户不可见。

## 界面说明

- 左侧：`VideraView` 渲染区域
- 右侧：状态、环境设置、渲染风格、线框、场景列表、网格和变换面板
- `Import Model`、`Frame All`、`Reset Camera` 会随命令/能力状态启用；Demo 不再把 raw `IsBackendReady` 当作唯一对外叙事

## 运行方式

```bash
dotnet run --project samples/Videra.Demo/Videra.Demo.csproj
```

## 验证

仓库级验证：

```bash
./verify.sh --configuration Release
pwsh -File ./verify.ps1 -Configuration Release
```

仅验证 Demo 构建：

```bash
dotnet build samples/Videra.Demo/Videra.Demo.csproj -c Release
```

## 主要文件

```text
Videra.Demo/
├── Assets/
├── Converters/
├── Services/
│   ├── AvaloniaModelImporter.cs
│   ├── DemoSceneBootstrapper.cs
│   └── DemoMeshFactory.cs
├── ViewModels/
│   ├── CameraViewModel.cs
│   └── MainWindowViewModel.cs
├── Views/
│   ├── MainWindow.axaml
│   └── MainWindow.axaml.cs
├── App.axaml
├── App.axaml.cs
└── Program.cs
```
