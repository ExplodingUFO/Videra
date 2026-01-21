namespace Videra.Core.Graphics.Wireframe;

/// <summary>
/// 线框显示模式
/// </summary>
public enum WireframeMode
{
    /// <summary>不显示线框</summary>
    None = 0,

    /// <summary>显示所有边缘线（包括被遮挡的线，使用半透明或虚线）</summary>
    AllEdges,

    /// <summary>只显示可见的边缘线（隐藏线移除）</summary>
    VisibleOnly,

    /// <summary>线框叠加在实体渲染之上</summary>
    Overlay,

    /// <summary>只显示线框，不显示实体</summary>
    WireframeOnly
}
