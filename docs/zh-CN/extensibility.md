# 扩展合同

[English](../extensibility.md) | [中文](extensibility.md)

本页是 Videra 对外渲染扩展面的中文镜像。英文版 [docs/extensibility.md](../extensibility.md) 仍然是 source of truth；当你需要可直接照抄的参考流程时，先看 [samples/Videra.ExtensibilitySample](../../samples/Videra.ExtensibilitySample/README.md)，再回到本页确认生命周期、回退和边界合同。

## 公开入口

- `VideraView.Engine` 是公开扩展根。
- `RegisterPassContributor(...)` 用来在稳定 pass slot 上追加工作，而不是接管整个 pipeline。
- `RegisterFrameHook(...)` 用来在 `FrameBegin`、`SceneSubmit`、`FrameEnd` 上挂接确定性的回调。
- `RenderCapabilities` 暴露 Core 侧 capability snapshot 与最近一次 pipeline snapshot。
- `BackendDiagnostics` 暴露 readiness、resolved backend、fallback 状态与 pipeline diagnostics。
- `LoadModelAsync(...)` 与 `FrameAll()` 是窄 sample 的场景加载与 framing 步骤。

## 推荐流程

受支持的公开流程如下：

1. 创建或取得一个 `VideraView`。
2. 配置 backend 选项，包含是否启用 `AllowSoftwareFallback`。
3. 通过 `VideraView.Engine` 调用 `RegisterPassContributor(...)`。
4. 通过 `VideraView.Engine` 调用 `RegisterFrameHook(...)`。
5. 等待 `BackendDiagnostics.IsReady` 或 `BackendReady` 后再加载内容。
6. 调用 `LoadModelAsync(...)`。
7. 调用 `FrameAll()`。
8. 检查 `RenderCapabilities` 与 `BackendDiagnostics`。

`LoadModelsAsync(...)` 仍属于同一条公开入口，但 scene 语义更严格：导入任务可以部分成功，active scene 只有在全部导入成功时才会被替换。

```csharp
using Videra.Avalonia.Controls;
using Videra.Core.Graphics;
using Videra.Core.Graphics.RenderPipeline.Extensibility;

view.Options = new VideraViewOptions
{
    Backend =
    {
        PreferredBackend = GraphicsBackendPreference.Auto,
        AllowSoftwareFallback = true
    }
};

view.Engine.RegisterPassContributor(RenderPassSlot.SolidGeometry, contributor);
view.Engine.RegisterFrameHook(RenderFrameHookPoint.FrameEnd, context =>
{
    Console.WriteLine(context.HookPoint);
});

if (view.BackendDiagnostics.IsReady)
{
    var loadResult = await view.LoadModelAsync("Assets/reference-cube.obj");
    if (loadResult.Succeeded)
    {
        view.FrameAll();
    }
}

var capabilities = view.RenderCapabilities;
var diagnostics = view.BackendDiagnostics;
```

实际可运行的参考项目位于 `samples/Videra.ExtensibilitySample`，主流程实现位于 `samples/Videra.ExtensibilitySample/Views/MainWindow.axaml.cs`。

## 行为矩阵

| 场景 | 注册合同 | capability / diagnostics 合同 | 加载与 framing 指南 |
| --- | --- | --- | --- |
| Ready | `RegisterPassContributor(...)` 与 `RegisterFrameHook(...)` 正常注册，并参与下一帧。 | `RenderCapabilities.IsInitialized` 为 `true`。`BackendDiagnostics.IsReady` 为 `true`，backend/profile 字段描述当前运行时。 | `LoadModelAsync(...)` 与 `FrameAll()` 是 view ready 之后的预期公开流程。 |
| Pre-initialization | 在第一帧 backend-ready 之前允许注册，并会保留到渲染真正开始。 | `RenderCapabilities` 可查询，但 `IsInitialized = false`。`BackendDiagnostics.IsReady` 为 `false`，此时还没有 native fallback reason。 | 在调用 `LoadModelAsync(...)` 与 `FrameAll()` 之前，先等待 `BackendReady` 或 `BackendDiagnostics.IsReady`。 |
| `disposed` engine | 后续的 `RegisterPassContributor(...)`、`ReplacePassContributor(...)` 与 `RegisterFrameHook(...)` 调用都会被忽略，保持 `no-op`。 | `RenderCapabilities` 仍可查询，并报告 `IsInitialized = false`。如果前面已经完成过一帧，最近一次 pipeline snapshot 仍可能在 disposal 后被保留。此时应把 `BackendDiagnostics` 视为 last-known view/session state，而不是重新激活机制。 | 不要期待 `disposed` 过的 engine 或 view 再产生新的渲染工作；如果需要新的 session，请创建新的 view/engine。 |
| Native unavailable with software fallback | 注册接口不变；engine 会切到 software backend，而不是改掉 API 形状。 | 当 `AllowSoftwareFallback = true` 时，`BackendDiagnostics.IsUsingSoftwareFallback` 为 `true`，`ResolvedBackend` 会变成 `Software`，`FallbackReason` 会解释 native backend 为什么不可用。`RenderCapabilities` 继续暴露相同的 contributor/hook support 标志。 | 在 ready 之后，窄 sample 仍可调用 `LoadModelAsync(...)` 与 `FrameAll()`，但宿主应用应把 fallback 状态显式展示给用户。 |
| Native unavailable without software fallback | 注册合同不变，但不会创建 ready session。同一份 unavailable 原因会以 failure 的形式暴露，而不是 fallback。 | 当 `AllowSoftwareFallback = false` 时，backend resolution 会失败，而不是走带 `FallbackReason` 的 software 路径。在 Core-first 流程中，`GraphicsBackendFactory.ResolveBackend(...)` 会抛出同一份 unavailable reason；在 Avalonia 流程中，view 会保持 not ready，并通过 `InitializationFailed` / `LastInitializationError` 暴露失败。 | 在缺失 package/runtime 问题修复并且 view 能正常初始化之前，不要调用 `LoadModelAsync(...)` 或 `FrameAll()`。 |

## 场景加载真相

- `SceneDocument` 是公开加载 helper 背后的 authoritative viewer-scene contract。
- `LoadModelAsync(...)` 会先得到 backend-neutral imported asset，只有在 resource factory ready 时才真正上传。
- `LoadModelsAsync(...)` 使用有界并发导入，并且只有在所有请求都成功时才替换 active scene。
- backend rebind 或 fallback 恢复时，会从保留的 imported asset 和 scene object 恢复场景，而不是长期依赖 software staging path。

## 范围边界

- `VideraEngine` 是唯一的公开扩展根。
- `VideraViewRuntime`、`RenderSessionOrchestrator`、`RenderSession` 与 `VideraViewSessionBridge` 仍然是 internal orchestration seam。
- `SceneDocument`、`SceneUploadCoordinator`、`IGraphicsDevice`、`IRenderSurface` 与 `LegacyGraphicsBackendAdapter` 仍属于 internal contract，不是公开扩展点。
- 公开合同刻意保持 C#-first、in-process。
- `package discovery` 与 `plugin loading` 继续保持 out of scope。
- 公开 sample 与文档不应依赖 `SoftwareBackend` 这类 internal-only 类型。

## 相关文档

- [项目首页](README.md)
- [文档导航](index.md)
- [架构说明](ARCHITECTURE.md)
- [Videra.Core](modules/videra-core.md)
- [Videra.Avalonia](modules/videra-avalonia.md)
- [Videra.ExtensibilitySample](../../samples/Videra.ExtensibilitySample/README.md)
