# Videra.Platform.Linux - Vulkan 后端

[English](../../../src/Videra.Platform.Linux/README.md) | [中文](platform-linux.md)

Linux 平台的 Vulkan 图形后端实现。

> 中文镜像用于快速查阅，英文版为准。

## 安装前置

当前 `alpha` 包通过 GitHub Packages 提供。先配置包源：

```bash
dotnet nuget add source "https://nuget.pkg.github.com/ExplodingUFO/index.json" \
  --name github-ExplodingUFO \
  --username YOUR_GITHUB_USER \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text
```

Linux 应用通常与 `Videra.Avalonia` 搭配安装：

```bash
dotnet add package Videra.Avalonia --version 0.1.0-alpha.1 --source github-ExplodingUFO
dotnet add package Videra.Platform.Linux --version 0.1.0-alpha.1 --source github-ExplodingUFO
```

当前 Linux 原生渲染链路仍基于 X11 句柄；如果运行在 Wayland 会话中，则依赖 `XWayland` 兼容路径。matching-host 原生验证仍需要真实 Linux 宿主。

## 模块架构

```mermaid
graph TB
    subgraph "Vulkan Backend"
        Backend[VulkanBackend<br/>后端实现]
        Factory[VulkanResourceFactory<br/>资源工厂]
        Executor[VulkanCommandExecutor<br/>命令执行器]
        Buffer[VulkanBuffer<br/>缓冲区]
        Pipeline[VulkanPipeline<br/>渲染管线]
        Shader[VulkanShader<br/>着色器]
    end

    subgraph "Silk.NET"
        Vk[Silk.NET.Vulkan]
        KHR[KHR Extensions]
    end

    subgraph "X11"
        Display[X11 Display]
        Window[X11 Window]
        Surface[VkSurfaceKHR]
    end

    Backend --> Factory
    Backend --> Executor
    Factory --> Buffer
    Factory --> Pipeline
    Factory --> Shader
    Backend --> Vk
    Backend --> KHR
    Vk --> Display
    Display --> Window
    Window --> Surface
```

## Vulkan 初始化流程

```mermaid
sequenceDiagram
    participant App as 应用程序
    participant Backend as VulkanBackend
    participant Vk as Vulkan API
    participant X11 as X11

    App->>Backend: Initialize(window, w, h)
    Backend->>Vk: CreateInstance()
    Backend->>X11: XOpenDisplay()
    Backend->>Vk: CreateXlibSurface()
    Backend->>Vk: EnumeratePhysicalDevices()
    Backend->>Vk: CreateDevice()
    Backend->>Vk: GetDeviceQueue()
    Backend->>Vk: CreateSwapchain()
    Backend->>Vk: CreateImageViews()
    Backend->>Vk: CreateRenderPass()
    Backend->>Vk: CreateDepthResources()
    Backend->>Vk: CreateFramebuffers()
    Backend->>Vk: CreateCommandPool()
    Backend->>Vk: CreateCommandBuffers()
    Backend->>Vk: CreateSyncObjects()
    Backend-->>App: IsInitialized = true
```

## 渲染流程

```mermaid
sequenceDiagram
    participant Engine as VideraEngine
    participant Backend as VulkanBackend
    participant Vk as Vulkan
    participant Queue as Graphics Queue

    Engine->>Backend: BeginFrame()
    Backend->>Vk: WaitForFences()
    Backend->>Vk: ResetFences()
    Backend->>Vk: AcquireNextImage()
    Backend->>Vk: ResetCommandBuffer()
    Backend->>Vk: BeginCommandBuffer()
    Backend->>Vk: CmdBeginRenderPass()

    loop 绘制对象
        Engine->>Backend: Draw()
        Backend->>Vk: CmdBindPipeline()
        Backend->>Vk: CmdBindVertexBuffers()
        Backend->>Vk: CmdBindIndexBuffer()
        Backend->>Vk: CmdBindDescriptorSets()
        Backend->>Vk: CmdDrawIndexed()
    end

    Engine->>Backend: EndFrame()
    Backend->>Vk: CmdEndRenderPass()
    Backend->>Vk: EndCommandBuffer()
    Backend->>Queue: QueueSubmit()
    Backend->>Vk: QueuePresent()
```

## Vulkan 对象层次

```mermaid
graph TB
    Instance[VkInstance] --> PhysicalDevice[VkPhysicalDevice]
    PhysicalDevice --> Device[VkDevice]
    Device --> Queue[VkQueue]
    Device --> CommandPool[VkCommandPool]
    CommandPool --> CommandBuffer[VkCommandBuffer]
    Device --> Swapchain[VkSwapchainKHR]
    Swapchain --> Images[VkImage[]]
    Images --> ImageViews[VkImageView[]]
    Device --> RenderPass[VkRenderPass]
    ImageViews --> Framebuffers[VkFramebuffer[]]
    RenderPass --> Framebuffers
```

## 同步机制

```mermaid
sequenceDiagram
    participant CPU as CPU
    participant Semaphore1 as ImageAvailable<br/>Semaphore
    participant Semaphore2 as RenderFinished<br/>Semaphore
    participant Fence as InFlight<br/>Fence
    participant GPU as GPU

    CPU->>Fence: WaitForFences()
    Fence-->>CPU: 上一帧完成
    CPU->>Fence: ResetFences()
    CPU->>Semaphore1: AcquireNextImage()
    GPU-->>Semaphore1: 图像可用
    CPU->>GPU: QueueSubmit()
    Note over GPU: 等待 Semaphore1
    Note over GPU: 执行渲染
    GPU->>Semaphore2: 信号
    GPU->>Fence: 信号
    CPU->>GPU: QueuePresent()
    Note over GPU: 等待 Semaphore2
    Note over GPU: 呈现图像
```

## 核心类

### VulkanBackend

实现 `IGraphicsBackend` 接口的 Vulkan 后端。

```csharp
public unsafe class VulkanBackend : IGraphicsBackend
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

## 深度缓冲配置

- 深度格式: `VK_FORMAT_D32_SFLOAT`
- 比较函数: `VK_COMPARE_OP_LESS_OR_EQUAL`
- 深度写入: 启用

## 文件结构

```
Videra.Platform.Linux/
├── VulkanBackend.cs           # 后端实现
├── VulkanBuffer.cs            # 缓冲区实现
├── VulkanCommandExecutor.cs   # 命令执行器
├── VulkanPipeline.cs          # 渲染管线
├── VulkanResourceFactory.cs   # 资源工厂
└── VulkanShader.cs            # 着色器
```

## 依赖

- .NET 8.0
- Silk.NET.Vulkan
- Silk.NET.Vulkan.Extensions.KHR
- Silk.NET.Shaderc
- Videra.Core

## 系统要求

- Linux（X11 窗口系统，或 Wayland 会话中的 `XWayland` 兼容路径）
- Vulkan 1.2+ 兼容显卡
- `libX11.so.6`（仓库验证脚本支持回退到 `libX11.so` / `libX11`）
- Vulkan 驱动程序

## 原生验证

在 Linux 原生主机上，可通过仓库统一验证入口执行 Vulkan 原生验证包：

```bash
# Unix shell
./verify.sh --configuration Release --include-native-linux
./verify.sh --configuration Release --include-native-linux-xwayland

# PowerShell
pwsh -File ./scripts/verify.ps1 -Configuration Release -IncludeNativeLinux
pwsh -File ./scripts/verify.ps1 -Configuration Release -IncludeNativeLinuxXWayland
```

这一步用于执行 `tests/Videra.Platform.Linux.Tests` 中的真实 X11-backed lifecycle/render-path 验证；如果在 Wayland 会话中运行，则同时验证 `XWayland` 兼容路径，而不仅仅是跨平台构建或非原生主机上的常规测试。
