using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.macOS;

/// <summary>
/// Metal 渲染管线
/// </summary>
internal sealed class MetalPipeline : IPipeline
{
    private IntPtr _pipelineState;
    private bool _disposed;

    public IntPtr NativePipelineState => _pipelineState;

    public MetalPipeline(IntPtr pipelineState)
    {
        _pipelineState = pipelineState;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_pipelineState != IntPtr.Zero)
        {
            ObjCRuntime.SendMessageVoid(_pipelineState, ObjCRuntime.SEL("release"));
            _pipelineState = IntPtr.Zero;
        }
    }
}
