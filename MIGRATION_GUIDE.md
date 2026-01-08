# Videra 迁移指南: 从 Veldrid 到 Silk.NET

## 📝 概述

本文档说明如何将现有的 Videra 项目从 Veldrid 抽象层迁移到基于 Silk.NET 的分平台实现。

## 🔄 迁移策略

### 阶段 1: 抽象接口层 ✅
- 创建 `IGraphicsBackend`、`IResourceFactory`、`ICommandExecutor` 等接口
- 定义平台无关的资源描述符
- 实现 `GraphicsBackendFactory` 工厂模式

### 阶段 2: 平台后端实现 ✅
- Windows: `D3D11Backend`
- macOS: `MetalBackend`
- Linux: `VulkanBackend`

### 阶段 3: VideraView 重构 ✅
- 创建 `VideraViewNew` 控件
- 使用新的后端接口替代 Veldrid API
- 保持与原有 API 的兼容性

### 阶段 4: VideraEngine 适配 🚧
- 重写 `CreateResources()` 方法
- 使用 `IResourceFactory` 创建资源
- 使用 `ICommandExecutor` 执行渲染命令

## 📋 详细步骤

### 步骤 1: 更新依赖项

#### Videra.Core.csproj
```xml
<!-- 移除 -->
<PackageReference Include="Veldrid" Version="4.9.0" />
<PackageReference Include="Veldrid.SPIRV" Version="1.0.15" />

<!-- 保留 -->
<PackageReference Include="SharpGLTF.Toolkit" Version="1.0.6" />
```

#### Videra.Avalonia.csproj
```xml
<!-- 添加平台特定引用 -->
<ItemGroup Condition="$([MSBuild]::IsOSPlatform('Windows'))">
  <ProjectReference Include="..\Videra.Platform.Windows\Videra.Platform.Windows.csproj" />
</ItemGroup>

<ItemGroup Condition="$([MSBuild]::IsOSPlatform('OSX'))">
  <ProjectReference Include="..\Videra.Platform.macOS\Videra.Platform.macOS.csproj" />
</ItemGroup>

<ItemGroup Condition="$([MSBuild]::IsOSPlatform('Linux'))">
  <ProjectReference Include="..\Videra.Platform.Linux\Videra.Platform.Linux.csproj" />
</ItemGroup>
```

### 步骤 2: 修改 VideraEngine

#### 原代码 (Veldrid)
```csharp
public void Initialize(GraphicsDeviceOptions options, SwapchainDescription swapchainDesc)
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        GraphicsDevice = GraphicsDevice.CreateMetal(options, swapchainDesc);
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        GraphicsDevice = GraphicsDevice.CreateD3D11(options, swapchainDesc);
    else
        GraphicsDevice = GraphicsDevice.CreateVulkan(options, swapchainDesc);

    _factory = GraphicsDevice.ResourceFactory;
    _cl = _factory.CreateCommandList();
    CreateResources();
}
```

#### 新代码 (Silk.NET)
```csharp
private IGraphicsBackend _backend;
private IResourceFactory _factory;
private ICommandExecutor _executor;

public void Initialize(IntPtr windowHandle, int width, int height)
{
    // 创建平台特定后端
    _backend = GraphicsBackendFactory.CreateBackend();
    _backend.Initialize(windowHandle, width, height);
    
    _factory = _backend.GetResourceFactory();
    _executor = _backend.GetCommandExecutor();
    
    CreateResources();
}

public void Render()
{
    _backend.BeginFrame();
    
    // 使用 _executor 进行渲染
    _executor.SetPipeline(_meshPipeline);
    _executor.SetVertexBuffer(_vertexBuffer);
    _executor.SetIndexBuffer(_indexBuffer);
    _executor.SetResourceSet(0, _projViewSet);
    
    foreach (var obj in _sceneObjects)
    {
        _executor.SetResourceSet(1, obj.WorldResourceSet);
        _executor.DrawIndexed(obj.IndexCount);
    }
    
    _backend.EndFrame();
}
```

### 步骤 3: 资源创建适配

#### 原代码 (Veldrid)
```csharp
_vertexBuffer = _factory.CreateBuffer(new BufferDescription(
    (uint)(vertices.Length * Unsafe.SizeOf<VertexPositionNormalColor>()),
    BufferUsage.VertexBuffer));
    
_device.UpdateBuffer(_vertexBuffer, 0, vertices);
```

#### 新代码 (Silk.NET)
```csharp
// 通过抽象接口创建
_vertexBuffer = _factory.CreateVertexBuffer(vertices);

// 或者更新数据
_vertexBuffer.UpdateArray(vertices);
```

### 步骤 4: Pipeline 创建

#### 需要实现的 Shader 编译策略

**方案 A: 统一 GLSL + 运行时编译**
```csharp
// 使用 Silk.NET.Shaderc 或平台工具链
// D3D11: GLSL → HLSL (通过 SPIRV-Cross) → DXBC
// Metal: GLSL → MSL (通过 SPIRV-Cross) → Metal Library
// Vulkan: GLSL → SPIR-V
```

**方案 B: 预编译平台特定 Shader**
```
shaders/
├── d3d11/
│   ├── vertex.hlsl.dxbc
│   └── fragment.hlsl.dxbc
├── metal/
│   ├── vertex.metal
│   └── fragment.metal
└── vulkan/
    ├── vertex.spv
    └── fragment.spv
```

### 步骤 5: 更新 Object3D

#### 原代码
```csharp
public void Initialize(ResourceFactory factory, GraphicsDevice gd, MeshData mesh)
{
    VertexBuffer = factory.CreateBuffer(new BufferDescription(...));
    IndexBuffer = factory.CreateBuffer(new BufferDescription(...));
    WorldBuffer = factory.CreateBuffer(new BufferDescription(...));
    
    gd.UpdateBuffer(VertexBuffer, 0, mesh.Vertices);
    gd.UpdateBuffer(IndexBuffer, 0, mesh.Indices);
}
```

#### 新代码
```csharp
public void Initialize(IResourceFactory factory, MeshData mesh)
{
    VertexBuffer = factory.CreateVertexBuffer(mesh.Vertices);
    IndexBuffer = factory.CreateIndexBuffer(mesh.Indices);
    WorldBuffer = factory.CreateUniformBuffer(64); // sizeof(Matrix4x4)
    
    // 初始化 World 矩阵
    WorldBuffer.Update(WorldMatrix);
}

public void UpdateWorldMatrix()
{
    WorldBuffer.Update(WorldMatrix);
}
```

## 🎯 API 映射表

| Veldrid API | Silk.NET 抽象 | D3D11 实现 | Metal 实现 | Vulkan 实现 |
|-------------|---------------|-----------|-----------|------------|
| `GraphicsDevice` | `IGraphicsBackend` | `D3D11Backend` | `MetalBackend` | `VulkanBackend` |
| `ResourceFactory` | `IResourceFactory` | `D3D11ResourceFactory` | `MetalResourceFactory` | `VulkanResourceFactory` |
| `CommandList` | `ICommandExecutor` | `D3D11CommandExecutor` | `MetalCommandExecutor` | `VulkanCommandExecutor` |
| `DeviceBuffer` | `IBuffer` | `D3D11Buffer` | `MetalBuffer` | `VulkanBuffer` |
| `Pipeline` | `IPipeline` | `D3D11Pipeline` | `MetalPipeline` | `VulkanPipeline` |
| `Shader` | `IShader` | `D3D11Shader` | `MetalShader` | `VulkanShader` |
| `ResourceSet` | `IResourceSet` | `D3D11ResourceSet` | `MetalResourceSet` | `VulkanDescriptorSet` |

## 🔍 关键差异

### 1. 初始化流程

**Veldrid:**
```csharp
var options = new GraphicsDeviceOptions { ... };
var swapchainDesc = new SwapchainDescription { ... };
Engine.Initialize(options, swapchainDesc);
```

**Silk.NET:**
```csharp
var backend = GraphicsBackendFactory.CreateBackend();
backend.Initialize(windowHandle, width, height);
```

### 2. 资源生命周期

**Veldrid:** 自动管理 + 引用计数  
**Silk.NET:** 需要显式 Dispose (特别是 Vulkan)

```csharp
// 确保正确释放
public void Dispose()
{
    _vertexBuffer?.Dispose();
    _indexBuffer?.Dispose();
    _pipeline?.Dispose();
    _backend?.Dispose();
}
```

### 3. 坐标系统

**Veldrid:** 统一的坐标系 (通过 `PreferStandardClipSpaceYDirection`)  
**Silk.NET:** 需要手动处理平台差异

- **D3D11/Vulkan**: 左手系，Y 向下
- **Metal**: 左手系，Y 向上
- **解决方案**: 在 Vertex Shader 中翻转 Y 坐标

### 4. 深度范围

**Veldrid:** 统一处理 (通过 `PreferDepthRangeZeroToOne`)  
**Silk.NET:** 平台特定

- **D3D11/Metal**: [0, 1]
- **Vulkan**: 默认 [0, 1] (可配置)
- **OpenGL**: [-1, 1] (不支持)

## ⚡ 性能优化建议

### 1. 减少状态切换
```csharp
// ❌ 糟糕
foreach (var obj in objects)
{
    _executor.SetPipeline(obj.Pipeline);
    _executor.DrawIndexed(obj.IndexCount);
}

// ✅ 优化
var groupedByPipeline = objects.GroupBy(o => o.Pipeline);
foreach (var group in groupedByPipeline)
{
    _executor.SetPipeline(group.Key);
    foreach (var obj in group)
        _executor.DrawIndexed(obj.IndexCount);
}
```

### 2. 批量资源更新 (Vulkan)
```csharp
// 使用 Staging Buffer
var staging = factory.CreateStagingBuffer(data);
commandBuffer.CopyBuffer(staging, destination);
```

### 3. Pipeline Cache (Vulkan)
```csharp
// 保存编译好的 Pipeline
var cache = File.ReadAllBytes("pipeline.cache");
var pipelineCache = CreatePipelineCacheFromData(cache);
```

## 🐛 常见问题

### Q1: macOS 初始化失败 (尺寸为 0)
**A:** 使用重试机制，等待 NSView 完成布局
```csharp
private void TryInitializeOrResize(uint width, uint height, int retryCount = 0)
{
    if (width < 64 || height < 64)
    {
        if (retryCount < 5)
            Task.Delay(100).ContinueWith(_ => TryInitialize(width, height, retryCount + 1));
        return;
    }
    // ... 初始化
}
```

### Q2: Vulkan Validation Layers 错误
**A:** 确保正确的同步:
```csharp
_vk.WaitForFences(_device, 1, in _fence, true, ulong.MaxValue);
_vk.ResetFences(_device, 1, in _fence);
```

### Q3: D3D11 COM 对象泄漏
**A:** 使用 `ComPtr<T>` 智能指针:
```csharp
ComPtr<ID3D11Device> _device;
// 自动调用 Release()
```

## 📚 参考资料

- [Silk.NET Documentation](https://dotnet.github.io/Silk.NET/)
- [Direct3D 11 Programming Guide](https://docs.microsoft.com/en-us/windows/win32/direct3d11/)
- [Metal Programming Guide](https://developer.apple.com/metal/)
- [Vulkan Tutorial](https://vulkan-tutorial.com/)

## ✅ 完成检查清单

- [ ] 移除 Veldrid 依赖
- [ ] 创建抽象接口层
- [ ] 实现 Windows D3D11 后端
- [ ] 实现 macOS Metal 后端
- [ ] 实现 Linux Vulkan 后端
- [ ] 重构 VideraView 控件
- [ ] 适配 VideraEngine 渲染循环
- [ ] 实现 Shader 编译和管理
- [ ] 测试三个平台的渲染效果
- [ ] 性能基准测试
- [ ] 更新文档和示例

## 🎉 预期收益

1. **性能提升**: 使用原生 API 减少抽象开销
2. **更好的调试**: 直接访问平台特定工具 (RenderDoc, Metal Debugger, Vulkan Validation)
3. **更细粒度的控制**: 可以针对每个平台进行优化
4. **学习价值**: 深入理解现代图形 API

## ⚠️ 注意事项

1. 代码量会增加 (每个平台约 1000-2000 行)
2. 需要分别测试和维护三个后端
3. Shader 管理变得更复杂
4. 不再有统一的抽象层保护

建议在确保团队有足够的图形编程经验后再进行完整迁移。
