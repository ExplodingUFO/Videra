using Microsoft.Extensions.Logging;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics.Wireframe;

/// <summary>
/// 线框渲染器 - 负责渲染物体的边缘线
/// </summary>
public class WireframeRenderer : IDisposable
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
        _logger.LogInformation("[WireframeRenderer] Initialized");
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
                // 显示所有边缘（包括被遮挡的）
                // 第一遍：用隐藏线颜色绘制所有线（包括被遮挡的）
                if (ShowHiddenLines)
                {
                    obj.UpdateWireframeColor(HiddenLineColor);
                    RenderLinesWithDepth(obj, executor, pipeline, testEnabled: false, writeEnabled: false);
                }
                // 第二遍：用主颜色绘制可见线（覆盖在隐藏线上）
                obj.UpdateWireframeColor(LineColor);
                RenderLinesWithDepth(obj, executor, pipeline, testEnabled: true, writeEnabled: false);
                break;

            case WireframeMode.VisibleOnly:
                // 只显示可见边缘（使用深度测试）
                RenderLinesWithDepth(obj, executor, pipeline, testEnabled: true, writeEnabled: false);
                break;

            case WireframeMode.Overlay:
                // 覆盖模式：线框显示在模型上方（禁用深度测试）
                RenderLinesWithDepth(obj, executor, pipeline, testEnabled: false, writeEnabled: false);
                break;

            case WireframeMode.WireframeOnly:
                // 纯线框模式：只显示线框（需要在VideraEngine中禁用实体渲染）
                RenderLinesWithDepth(obj, executor, pipeline, testEnabled: true, writeEnabled: true);
                break;
        }
    }

    private void RenderLinesWithDepth(Object3D obj, ICommandExecutor executor, IPipeline pipeline, bool testEnabled, bool writeEnabled)
    {
        // 确保设置pipeline
        executor.SetPipeline(pipeline);

        // 设置深度状态
        executor.SetDepthState(testEnabled, writeEnabled);

        // 更新物体的World矩阵
        obj.UpdateUniforms(executor);

        // 绑定线框专用顶点缓冲区（带自定义颜色）
        var vertexBuffer = obj.LineVertexBuffer ?? obj.VertexBuffer!;
        executor.SetVertexBuffer(vertexBuffer, 0);
        executor.SetVertexBuffer(obj.WorldBuffer!, 2);
        executor.SetIndexBuffer(obj.LineIndexBuffer!);

        // 使用Line拓扑绘制 (primitiveType = 1 表示 LineList)
        executor.DrawIndexed(1, obj.LineIndexCount, 1, 0, 0, 0);

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
    }
}
