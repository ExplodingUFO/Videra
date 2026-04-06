# Videra.Core - 核心渲染模块

[English](../../../src/Videra.Core/README.md) | [中文](videra-core.md)

平台无关的3D渲染核心，提供抽象接口和通用渲染逻辑。

## 模块架构

```mermaid
graph TB
    subgraph "Graphics"
        Engine[VideraEngine<br/>渲染引擎]
        Factory[GraphicsBackendFactory<br/>后端工厂]
        Object3D[Object3D<br/>3D对象]
        Grid[GridRenderer<br/>网格渲染器]
        Axis[AxisRenderer<br/>坐标轴渲染器]
    end

    subgraph "Abstractions"
        IBackend[IGraphicsBackend]
        IBuffer[IBuffer]
        IPipeline[IPipeline]
        IFactory[IResourceFactory]
        IExecutor[ICommandExecutor]
    end

    subgraph "Cameras"
        OrbitCamera[OrbitCamera<br/>轨道相机]
    end

    subgraph "Software"
        SoftwareBackend[SoftwareBackend<br/>软件渲染]
    end

    Engine --> IBackend
    Engine --> Object3D
    Engine --> Grid
    Engine --> Axis
    Engine --> OrbitCamera
    Factory --> IBackend
    Factory --> SoftwareBackend
    IBackend --> IBuffer
    IBackend --> IPipeline
    IBackend --> IFactory
    IBackend --> IExecutor
```

## 核心类说明

### VideraEngine

渲染引擎核心，管理场景对象、相机和渲染循环。

```csharp
public class VideraEngine : IDisposable
{
    public OrbitCamera Camera { get; }
    public GridRenderer Grid { get; }
    public WireframeRenderer Wireframe { get; }
    public bool ShowAxis { get; set; }
    public bool IsInitialized { get; }

    public void Initialize(IGraphicsBackend backend);
    public void Resize(uint width, uint height);
    public void AddObject(Object3D obj);
    public void RemoveObject(Object3D obj);
    public void Draw();
}
```

### Object3D

表示场景中的 3D 对象，包含独立的位置 / 旋转 / 缩放变换，以及运行时创建的 GPU 缓冲区资源。

```csharp
public class Object3D : IDisposable
{
    public string Name { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Scale { get; set; }
    public Matrix4x4 WorldMatrix { get; }

    public void Initialize(IResourceFactory factory, MeshData mesh, ILogger? logger = null);
    public void UpdateUniforms(ICommandExecutor executor);
    public void Dispose();
}
```

## 渲染流程

```mermaid
sequenceDiagram
    participant Engine as VideraEngine
    participant Backend as IGraphicsBackend
    participant Grid as GridRenderer
    participant Axis as AxisRenderer
    participant Objects as Object3D[]

    Engine->>Backend: BeginFrame()
    Backend->>Backend: 清除颜色/深度缓冲

    Engine->>Grid: Draw()
    Grid->>Backend: 绘制网格线

    Engine->>Axis: Draw()
    Axis->>Backend: 绘制XYZ轴

    loop 每个对象
        Engine->>Objects: GetTransform()
        Engine->>Backend: 更新Uniform
        Engine->>Backend: DrawIndexed()
    end

    Engine->>Backend: EndFrame()
    Backend->>Backend: 呈现到屏幕
```

## 抽象接口

### IGraphicsBackend

图形后端抽象接口，各平台需实现此接口。

```csharp
public interface IGraphicsBackend : IDisposable
{
    bool IsInitialized { get; }
    void Initialize(IntPtr windowHandle, int width, int height);
    void Resize(int width, int height);
    void BeginFrame();
    void EndFrame();
    void SetClearColor(Vector4 color);
    IResourceFactory GetResourceFactory();
    ICommandExecutor GetCommandExecutor();
}
```

### IResourceFactory

资源创建工厂接口。

```csharp
public interface IResourceFactory
{
    IBuffer CreateVertexBuffer(VertexPositionNormalColor[] vertices);
    IBuffer CreateVertexBuffer(uint sizeInBytes);
    IBuffer CreateIndexBuffer(uint[] indices);
    IBuffer CreateIndexBuffer(uint sizeInBytes);
    IBuffer CreateUniformBuffer(uint sizeInBytes);
    IPipeline CreatePipeline(PipelineDescription description);
    IPipeline CreatePipeline(uint vertexSize, bool hasNormals, bool hasColors);
    IShader CreateShader(ShaderStage stage, byte[] bytecode, string entryPoint);
    IResourceSet CreateResourceSet(ResourceSetDescription description);
}
```

## 软件渲染

当硬件加速不可用时，自动回退到CPU软件渲染。

```mermaid
flowchart LR
    Input[顶点数据] --> Transform[顶点变换]
    Transform --> Rasterize[光栅化]
    Rasterize --> Fragment[片段着色]
    Fragment --> DepthTest[深度测试]
    DepthTest --> Output[帧缓冲]
```

## 文件结构

```
Videra.Core/
├── Cameras/
│   └── OrbitCamera.cs          # 轨道相机
├── Geometry/
│   └── VertexPositionNormalColor.cs  # 顶点结构
├── Graphics/
│   ├── Abstractions/           # 抽象接口
│   │   ├── IBuffer.cs
│   │   ├── ICommandExecutor.cs
│   │   ├── IGraphicsBackend.cs
│   │   ├── IPipeline.cs
│   │   ├── IResourceFactory.cs
│   │   └── ISoftwareBackend.cs
│   ├── Software/               # 软件渲染实现
│   │   ├── SoftwareBackend.cs
│   │   ├── SoftwareBuffer.cs
│   │   └── ...
│   ├── AxisRenderer.cs
│   ├── CameraUniform.cs
│   ├── GraphicsBackendFactory.cs
│   ├── GridRenderer.cs
│   ├── Object3D.cs
│   └── VideraEngine.cs
└── IO/
    └── ModelImporter.cs        # 模型导入
```

## 验证流程

在仓库根目录使用统一验证入口：

```bash
# Unix shell
./verify.sh --configuration Release

# PowerShell
pwsh -File ./verify.ps1 -Configuration Release
```

如需仅执行 Core 相关测试，可直接运行：

```bash
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release
dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release
```

## 依赖

- .NET 8.0
- System.Numerics.Vectors
- SharpGLTF.Core (模型导入)
