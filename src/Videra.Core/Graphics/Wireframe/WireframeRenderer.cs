using Microsoft.Extensions.Logging;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics.Wireframe;

/// <summary>
/// 线框渲染器 - 负责渲染物体的边缘线
/// </summary>
public partial class WireframeRenderer : IDisposable
{
    private IResourceFactory? _factory;
    private bool _isInitialized;
    private RgbaFloat _lastLineColor;
    private readonly ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<WireframeRenderer>();

    /// <summary>线框显示模式</summary>
    public WireframeMode Mode { get; set; } = WireframeMode.None;

    /// <summary>线框颜色</summary>
    public RgbaFloat LineColor { get; set; } = new(0f, 0f, 0f, 1f); // 黑色

    /// <summary>隐藏线颜色（AllEdges模式下使用）</summary>
    public RgbaFloat HiddenLineColor { get; set; } = new(0.5f, 0.5f, 0.5f, 0.3f); // 半透明灰

    /// <summary>是否显示隐藏线（AllEdges模式专用）</summary>
    public bool ShowHiddenLines { get; set; } = true;

    public bool IsInitialized => _isInitialized;

    public void Initialize(IResourceFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
        _isInitialized = true;
        Log.Initialized(_logger);
    }

    /// <summary>
    /// 渲染场景中所有物体的线框
    /// </summary>
    public void RenderWireframes(
        IEnumerable<Object3D> objects,
        ICommandExecutor executor,
        IPipeline pipeline,
        OrbitCamera camera,
        uint width,
        uint height)
    {
        ArgumentNullException.ThrowIfNull(objects);
        ArgumentNullException.ThrowIfNull(executor);
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(camera);

        if (!_isInitialized || Mode == WireframeMode.None)
            return;

        // 检查颜色是否改变，需要更新顶点缓冲
        bool colorChanged = !_lastLineColor.Equals(LineColor);

        foreach (var obj in objects)
        {
            if (obj.LineIndexBuffer == null || obj.LineIndexCount == 0)
                continue;

            // 如果颜色改变，更新线框顶点缓冲
            if (colorChanged && obj.LineVertexBuffer != null)
            {
                obj.UpdateWireframeColor(LineColor);
            }

            RenderObjectWireframe(obj, executor, pipeline);
        }

        _lastLineColor = LineColor;
    }

    private void RenderObjectWireframe(
        Object3D obj,
        ICommandExecutor executor,
        IPipeline pipeline)
    {
        switch (Mode)
        {
            case WireframeMode.AllEdges:
                RenderAllEdges(obj, executor, pipeline);
                break;

            case WireframeMode.VisibleOnly:
                RenderWireframePass(obj, executor, pipeline, LineColor, testEnabled: true, writeEnabled: false);
                break;

            case WireframeMode.Overlay:
                RenderWireframePass(obj, executor, pipeline, LineColor, testEnabled: false, writeEnabled: false);
                break;

            case WireframeMode.WireframeOnly:
                RenderWireframePass(obj, executor, pipeline, LineColor, testEnabled: true, writeEnabled: true);
                break;
        }
    }

    private void RenderAllEdges(Object3D obj, ICommandExecutor executor, IPipeline pipeline)
    {
        if (ShowHiddenLines)
        {
            RenderWireframePass(obj, executor, pipeline, HiddenLineColor, testEnabled: false, writeEnabled: false);
        }

        RenderWireframePass(obj, executor, pipeline, LineColor, testEnabled: true, writeEnabled: false);
    }

    private static void RenderWireframePass(
        Object3D obj,
        ICommandExecutor executor,
        IPipeline pipeline,
        RgbaFloat lineColor,
        bool testEnabled,
        bool writeEnabled)
    {
        obj.UpdateWireframeColor(lineColor);
        RenderLinesWithDepth(obj, executor, pipeline, testEnabled, writeEnabled);
    }

    private static void RenderLinesWithDepth(Object3D obj, ICommandExecutor executor, IPipeline pipeline, bool testEnabled, bool writeEnabled)
    {
        // 确保设置pipeline
        executor.SetPipeline(pipeline);

        // 设置深度状态
        executor.SetDepthState(testEnabled, writeEnabled);

        // 更新物体的World矩阵
        obj.UpdateUniforms(executor);

        // 绑定线框专用顶点缓冲区（带自定义颜色）
        var vertexBuffer = obj.LineVertexBuffer ?? obj.VertexBuffer!;
        executor.SetVertexBuffer(vertexBuffer, RenderBindingSlots.Vertex);
        executor.SetVertexBuffer(obj.WorldBuffer!, RenderBindingSlots.World);
        executor.SetIndexBuffer(obj.LineIndexBuffer!);

        executor.DrawIndexed(PrimitiveCommandKind.LineList, obj.LineIndexCount, 1, 0, 0, 0);

        // 恢复默认深度状态
        executor.ResetDepthState();
    }

    /// <summary>
    /// 判断是否应该渲染实体
    /// </summary>
    public bool ShouldRenderSolid()
    {
        return Mode != WireframeMode.WireframeOnly;
    }

    public void Dispose()
    {
        _isInitialized = false;
        GC.SuppressFinalize(this);
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[WireframeRenderer] Initialized")]
        public static partial void Initialized(ILogger logger);
    }
}
