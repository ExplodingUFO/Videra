# Videra - 基于 Silk.NET 的跨平台 3D 渲染引擎

## 🎯 项目概述

Videra 是一个为 AvaloniaUI 设计的 3D 模型查看器组件,采用分治策略为不同平台实现了原生图形 API 后端:

- **Windows**: Direct3D 11 (Silk.NET.Direct3D11)
- **macOS**: Metal (Silk.NET.Metal + Objective-C Runtime)
- **Linux**: Vulkan (Silk.NET.Vulkan)

## 🏗️ 架构设计

### 项目结构

```
Videra/
├── src/
│   ├── Videra.Core/                    # 核心渲染逻辑（平台无关）
│   │   ├── Graphics/
│   │   │   ├── Abstractions/          # 抽象接口层
│   │   │   │   ├── IGraphicsBackend.cs
│   │   │   │   ├── IResourceFactory.cs
│   │   │   │   ├── IBuffer.cs
│   │   │   │   ├── IPipeline.cs
│   │   │   │   └── ICommandExecutor.cs
│   │   │   ├── GraphicsBackendFactory.cs  # 平台后端工厂
│   │   │   ├── VideraEngine.cs
│   │   │   ├── Object3D.cs
│   │   │   ├── GridRenderer.cs
│   │   │   └── AxisRenderer.cs
│   │   ├── Cameras/
│   │   │   └── OrbitCamera.cs
│   │   └── IO/
│   │       └── ModelImporter.cs
│   │
│   ├── Videra.Platform.Windows/        # Windows D3D11 后端
│   │   ├── D3D11Backend.cs
│   │   ├── D3D11ResourceFactory.cs
│   │   ├── D3D11Buffer.cs
│   │   └── D3D11CommandExecutor.cs
│   │
│   ├── Videra.Platform.macOS/          # macOS Metal 后端
│   │   ├── MetalBackend.cs
│   │   ├── MetalResourceFactory.cs
│   │   ├── MetalBuffer.cs
│   │   └── MetalCommandExecutor.cs
│   │
│   ├── Videra.Platform.Linux/          # Linux Vulkan 后端
│   │   ├── VulkanBackend.cs
│   │   ├── VulkanResourceFactory.cs
│   │   └── VulkanCommandExecutor.cs
│   │
│   └── Videra.Avalonia/                # AvaloniaUI 集成
│       └── Controls/
│           ├── VideraView.cs        # 新的渲染控件
│           └── VideraView.Input.cs
│
└── samples/
    └── Videra.Demo/                    # 示例应用程序
```

### 抽象接口层

#### IGraphicsBackend
```csharp
public interface IGraphicsBackend : IDisposable
{
    void Initialize(IntPtr windowHandle, int width, int height);
    void Resize(int width, int height);
    void BeginFrame();
    void EndFrame();
    void SetClearColor(Vector4 color);
    IResourceFactory GetResourceFactory();
    ICommandExecutor GetCommandExecutor();
}
```

#### IResourceFactory
负责创建 GPU 资源:
- 顶点缓冲区 (Vertex Buffer)
- 索引缓冲区 (Index Buffer)
- 常量缓冲区 (Uniform Buffer)
- 渲染管线 (Pipeline)
- 着色器 (Shader)
- 资源集 (Resource Set)

#### ICommandExecutor
封装渲染命令的执行:
- 设置 Pipeline
- 绑定 Buffer
- 绘制命令 (Draw/DrawIndexed)
- 设置 Viewport/Scissor

## 🔧 平台特定实现

### Windows (Direct3D 11)

**特点:**
- 使用 `Silk.NET.Direct3D11` 和 `Silk.NET.DXGI`
- COM 互操作通过 `ComPtr<T>` 智能指针管理
- HLSL Shader 编译使用 `Silk.NET.Direct3D.Compilers`

**关键代码:**
```csharp
_d3d11.CreateDeviceAndSwapChain(
    (IDXGIAdapter*)null,
    D3DDriverType.Hardware,
    nint.Zero,
    (uint)CreateDeviceFlag.BgraSupport,
    null, 0, D3D11.SdkVersion,
    in swapchainDesc,
    swapchainPtr, devicePtr, &featureLevel, contextPtr
);
```

### macOS (Metal)

**特点:**
- 使用 Objective-C Runtime 互操作 (`libobjc.dylib`)
- 通过 `CAMetalLayer` 进行渲染
- 深度范围为 [0, 1]，需要自定义透视矩阵

**关键代码:**
```csharp
_device = MTLCreateSystemDefaultDevice();
_commandQueue = SendMessage(_device, "newCommandQueue");
_metalLayer = GetOrCreateMetalLayer(nsView);
SetLayerDevice(_metalLayer, _device);
```

### Linux (Vulkan)

**特点:**
- 完整的 Vulkan 渲染管线实现
- 支持 X11 和 Wayland Surface
- GLSL Shader 编译为 SPIR-V (使用 `Silk.NET.Shaderc`)

**关键组件:**
- Instance → Physical Device → Logical Device
- Swapchain → Image Views → Framebuffers
- Render Pass → Pipeline → Command Buffers
- Semaphores / Fences (同步)

## 🚀 使用方法

### 基本用法

```xml
<!-- MainWindow.axaml -->
<Window xmlns:videra="using:Videra.Avalonia.Controls">
    <videra:VideraView 
        BackgroundColor="#1E1E1E"
        Items="{Binding Models}"
        CameraInvertY="True"
        IsGridVisible="True" />
</Window>
```

```csharp
// ViewModel
public class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<Object3D> Models { get; } = new();
    
    public void LoadModel(string path)
    {
        var mesh = ModelImporter.LoadFromFile(path);
        var obj = new Object3D { Name = Path.GetFileName(path) };
        obj.Initialize(factory, device, mesh);
        
        Models.Add(obj);
    }
}
```

## 📦 依赖项

### 核心依赖
- .NET 8.0
- AvaloniaUI 11.3.9
- SharpGLTF.Toolkit 1.0.6

### 平台依赖 (Silk.NET 2.21.0)

**Windows:**
- Silk.NET.Direct3D11
- Silk.NET.DXGI
- Silk.NET.Direct3D.Compilers

**macOS:**
- Silk.NET.Metal
- Silk.NET.Core

**Linux:**
- Silk.NET.Vulkan
- Silk.NET.Vulkan.Extensions.KHR
- Silk.NET.Shaderc

## 🔨 构建说明

### macOS
```bash
dotnet build -r osx-arm64  # Apple Silicon
dotnet build -r osx-x64    # Intel Mac
```

### Windows
```bash
dotnet build -r win-x64
```

### Linux
```bash
dotnet build -r linux-x64
```

## ⚠️ 当前状态

### ✅ 已完成
- 抽象接口层设计
- Windows D3D11 后端基础框架
- macOS Metal 后端基础框架
- Linux Vulkan 后端基础框架
- VideraView 控件骨架
- 平台后端工厂
- 输入处理系统

### 🚧 进行中
- Shader 编译和管理系统
- Pipeline 创建和绑定
- 完整的资源管理
- 渲染循环集成

### 📋 待实现
- 完整的 D3D11 渲染管线
- Metal Shader Library 编译
- Vulkan Pipeline Cache
- 深度缓冲和模板测试
- 多对象渲染优化
- 性能分析工具

## 🎓 技术亮点

### 1. 分治策略
每个平台使用最优的原生 API，避免了抽象层的性能损失:
- Windows: D3D11 (DirectX 生态系统)
- macOS: Metal (Apple 优化)
- Linux: Vulkan (现代低开销 API)

### 2. 动态后端加载
```csharp
public static IGraphicsBackend CreateBackend()
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        var asm = Assembly.Load("Videra.Platform.Windows");
        var type = asm.GetType("Videra.Platform.Windows.D3D11Backend");
        return (IGraphicsBackend)Activator.CreateInstance(type);
    }
    // ... 其他平台
}
```

### 3. COM 智能指针 (D3D11)
```csharp
ComPtr<ID3D11Device> _device;
// 自动管理引用计数，防止内存泄漏
```

### 4. Objective-C Runtime 互操作 (Metal)
```csharp
[DllImport("/usr/lib/libobjc.dylib")]
private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);
```

### 5. Vulkan 显式同步
```csharp
_vk.WaitForFences(_device, 1, in _inFlightFence, true, ulong.MaxValue);
_khrSwapchain.AcquireNextImage(_device, _swapchain, ulong.MaxValue, 
    _imageAvailableSemaphore, default, imageIndex);
```

## 🐛 已知问题

1. **macOS 初始化延迟**: NSView 尺寸在 `CreateNativeControlCore` 时可能为 0，需要重试机制
2. **Vulkan Surface 创建**: X11/Wayland 双支持需要更多测试
3. **Shader 交叉编译**: 目前缺少统一的 Shader 管理策略

## 📄 许可证

请参考 LICENSE.txt

## 🙏 致谢

- Silk.NET 团队提供的优秀的 .NET 图形绑定
- Avalonia 社区的跨平台 UI 框架
- Veldrid 项目的设计灵感 (原实现方案)

## 📞 联系方式

如有问题或建议，请提交 Issue 或 Pull Request。
