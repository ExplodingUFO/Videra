# Videra.Core - 核心渲染模块

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
    public AxisRenderer Axis { get; }

    public void Initialize(IGraphicsBackend backend);
    public void AddObject(Object3D obj);
    public void RemoveObject(Object3D obj);
    public void Draw();
}
```

### Object3D

表示场景中的3D对象。

```csharp
public class Object3D
{
    public string Name { get; set; }
    public Matrix4x4 Transform { get; set; }
    public IBuffer? VertexBuffer { get; set; }
    public IBuffer? IndexBuffer { get; set; }
    public PrimitiveTopology Topology { get; set; }
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
    IBuffer CreateIndexBuffer(uint[] indices);
    IBuffer CreateUniformBuffer<T>(T data) where T : unmanaged;
    IPipeline CreatePipeline(IShader vertexShader, IShader fragmentShader);
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
��   ├── Object3D.cs
│   └── VideraEngine.cs
└── IO/
    └── ModelImporter.cs        # 模型导入
```

## 依赖

- .NET 8.0
- System.Numerics.Vectors
- SharpGLTF.Core (模型导入)
