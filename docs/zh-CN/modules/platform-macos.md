# Videra.Platform.macOS - Metal 后端

[English](../../../src/Videra.Platform.macOS/README.md) | [中文](platform-macos.md)

macOS 平台的 Metal 图形后端实现。

> 中文镜像用于快速查阅，英文版为准。

## 安装前置

公开消费者默认从 `nuget.org` 安装：

```bash
dotnet add package Videra.Avalonia
dotnet add package Videra.Platform.macOS
```

当前 `alpha` 的 `preview` 验证仍可使用 `GitHub Packages`，但那不是默认公开安装路径：

```bash
dotnet nuget add source "https://nuget.pkg.github.com/ExplodingUFO/index.json" \
  --name github-ExplodingUFO \
  --username YOUR_GITHUB_USER \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text

dotnet add package Videra.Avalonia --version 0.1.0-alpha.5 --source github-ExplodingUFO
dotnet add package Videra.Platform.macOS --version 0.1.0-alpha.5 --source github-ExplodingUFO
```

当前原生路径依赖 `NSView` 和 `CAMetalLayer`，matching-host 原生验证仍需要真实 macOS 宿主。

## 模块架构

```mermaid
graph TB
    subgraph "Metal Backend"
        Backend[MetalBackend<br/>后端实现]
        Factory[MetalResourceFactory<br/>资源工厂]
        Executor[MetalCommandExecutor<br/>命令执行器]
        Buffer[MetalBuffer<br/>缓冲区]
        Pipeline[MetalPipeline<br/>渲染管线]
    end

    subgraph "Objective-C Runtime"
        ObjC[libobjc.dylib]
        Metal[Metal Framework]
        QuartzCore[QuartzCore]
    end

    subgraph "macOS"
        NSView[NSView]
        CAMetalLayer[CAMetalLayer]
        MTLDevice[MTLDevice]
        CommandQueue[MTLCommandQueue]
    end

    Backend --> Factory
    Backend --> Executor
    Factory --> Buffer
    Factory --> Pipeline
    Backend --> ObjC
    ObjC --> Metal
    ObjC --> QuartzCore
    NSView --> CAMetalLayer
    CAMetalLayer --> MTLDevice
    MTLDevice --> CommandQueue
```

## Metal 初始化流程

```mermaid
sequenceDiagram
    participant App as 应用程序
    participant Backend as MetalBackend
    participant ObjC as Objective-C Runtime
    participant Metal as Metal

    App->>Backend: Initialize(nsView, w, h)
    Backend->>Backend: 保存 NSView
    Backend->>Backend: GetBackingScaleFactor()
    Backend->>Metal: MTLCreateSystemDefaultDevice()
    Backend->>ObjC: [device newCommandQueue]
    Backend->>ObjC: GetOrCreateMetalLayer()
    Backend->>ObjC: [view setWantsLayer:YES]
    Backend->>ObjC: [view setLayer:metalLayer]
    Backend->>ObjC: SetLayerDevice()
    Backend->>ObjC: SetLayerPixelFormat()
    Backend->>ObjC: SetLayerDrawableSize()
    Backend->>Backend: CreateDepthStencilState()
    Backend->>Backend: CreateResourceFactory()
    Backend->>Backend: CreateCommandExecutor()
    Backend-->>App: IsInitialized = true
```

## 渲染流程

```mermaid
sequenceDiagram
    participant Engine as VideraEngine
    participant Backend as MetalBackend
    participant Layer as CAMetalLayer
    participant Encoder as RenderCommandEncoder

    Engine->>Backend: BeginFrame()
    Backend->>Layer: nextDrawable()
    Backend->>Backend: CreateCommandBuffer()
    Backend->>Backend: CreateRenderPassDescriptor()
    Backend->>Backend: CreateRenderCommandEncoder()

    loop 绘制对象
        Engine->>Backend: Draw()
        Backend->>Encoder: setRenderPipelineState()
        Backend->>Encoder: setVertexBuffer()
        Backend->>Encoder: setVertexBuffer(uniform)
        Backend->>Encoder: drawIndexedPrimitives()
    end

    Engine->>Backend: EndFrame()
    Backend->>Encoder: endEncoding()
    Backend->>Backend: presentDrawable()
    Backend->>Backend: commit()
```

## Metal 对象层次

```mermaid
graph TB
    Device[MTLDevice] --> CommandQueue[MTLCommandQueue]
    CommandQueue --> CommandBuffer[MTLCommandBuffer]
    CommandBuffer --> RenderEncoder[MTLRenderCommandEncoder]

    Device --> Library[MTLLibrary]
    Library --> VertexFunc[MTLFunction<br/>Vertex]
    Library --> FragmentFunc[MTLFunction<br/>Fragment]

    Device --> PipelineState[MTLRenderPipelineState]
    Device --> DepthState[MTLDepthStencilState]

    NSView[NSView] --> CAMetalLayer[CAMetalLayer]
    CAMetalLayer --> Drawable[CAMetalDrawable]
    Drawable --> Texture[MTLTexture]
```

## Retina 显示支持

```mermaid
flowchart LR
    subgraph "逻辑坐标"
        LogicalSize[800 x 600 点]
    end

    subgraph "缩放因子"
        Scale[backingScaleFactor<br/>2.0x]
    end

    subgraph "物理像素"
        PhysicalSize[1600 x 1200 像素]
    end

    LogicalSize --> Scale
    Scale --> PhysicalSize
```

- 自动检测 `backingScaleFactor`
- 设置 `contentsScale` 匹配缩放因子
- `drawableSize` 使用物理像素尺寸

## 核心类

### MetalBackend

实现 `IGraphicsBackend` 接口的 Metal 后端。

```csharp
public class MetalBackend : IGraphicsBackend
{
    public void Initialize(IntPtr windowHandle, int width, int height);
    public void Resize(int width, int height);
    public void BeginFrame();
    public void EndFrame();
    public void SetClearColor(Vector4 color);
    public IResourceFactory GetResourceFactory();
    public ICommandExecutor GetCommandExecutor();
}
```

## Objective-C 互操作

通过 P/Invoke 调用 Objective-C Runtime：

```csharp
[DllImport("/usr/lib/libobjc.dylib")]
static extern IntPtr objc_getClass(string name);

[DllImport("/usr/lib/libobjc.dylib")]
static extern IntPtr sel_registerName(string name);

[DllImport("/usr/lib/libobjc.dylib")]
static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);
```

## 深度缓冲配置

- 深度格式: `MTLPixelFormatDepth32Float`
- 比较函数: `MTLCompareFunctionLessEqual`
- 深度写入: 启用
- 深度范围: [0, 1] (Metal 约定)

## 文件结构

```
Videra.Platform.macOS/
├── MetalBackend.cs           # 后端实现
├── MetalBuffer.cs            # 缓冲区实现
├── MetalCommandExecutor.cs   # 命令执行器
├── MetalPipeline.cs          # 渲染管线
└── MetalResourceFactory.cs   # 资源工厂
```

## 依赖

- .NET 8.0
- Videra.Core
- macOS 系���框架 (通过 P/Invoke)

## 系统要求

- macOS 10.15 (Catalina) 或更高版本
- Metal 兼容显卡
- 支持 Apple Silicon (M1/M2/M3) 和 Intel Mac

## 原生验证

在 macOS 原生主机上，可通过仓库统一验证入口执行 Metal 原生验证包：

```bash
# Unix shell
./scripts/verify.sh --configuration Release --include-native-macos

# PowerShell
pwsh -File ./scripts/verify.ps1 -Configuration Release -IncludeNativeMacOS
```

这一步用于执行 `tests/Videra.Platform.macOS.Tests` 中的真实 NSView-backed lifecycle/render-path 验证，而不仅仅是当前非 macOS 主机上的构建级验证。

