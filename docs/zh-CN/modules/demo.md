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
- 一个收窄的 `Scene Pipeline Lab` 面板，用来说明 deferred upload、原子 scene replace 与 backend rebind 真相

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
if (result.Succeeded && result.Entries.Count > 0)
{
    View3D.FrameAll();
}

var diagnostics = View3D.BackendDiagnostics;
```

如果导入中有失败项，Demo 不会替换当前 active scene，而是保留原场景，并把最后一个失败信息连同成功计数一起汇总到状态栏中。`LoadModelsAsync` 现在以 `Entries` 作为公开结果成员，示例也按场景条目来描述导入结果。
导入结果与默认场景失败信息都会进入状态区域，避免只写日志而用户不可见。

右侧 `Scene Pipeline Lab` 文案会把三件事直接投到可见界面里：

- `SceneDocument` 才是运行时场景真相
- imported asset 会先保持 CPU / backend-neutral 状态，等 resource factory ready 后再上传
- retained scene/material runtime truth 还包括 per-primitive non-Blend material participation、occlusion texture binding/strength，以及 `KHR_texture_transform` 的 offset/scale/rotation 和 texture-coordinate override
- 这里描述的是 imported-asset/runtime truth，不宣称 renderer/shader/backend 消费这些 metadata
- mixed Blend/non-Blend imports 会继续被 guard，直到 transparent primitives 可独立排序
- backend fallback / rebind 发生时，scene truth 会被保留，而不是依赖 steady-state software staging path

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
./scripts/verify.sh --configuration Release
pwsh -File ./scripts/verify.ps1 -Configuration Release
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

