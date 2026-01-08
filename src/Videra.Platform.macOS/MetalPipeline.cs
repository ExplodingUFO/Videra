using System.Runtime.InteropServices;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.macOS;

/// <summary>
/// Metal 渲染管线
/// </summary>
internal class MetalPipeline : IPipeline
{
    private IntPtr _pipelineState;
    
    public IntPtr NativePipelineState => _pipelineState;
    
    public MetalPipeline(IntPtr pipelineState)
    {
        _pipelineState = pipelineState;
    }

    public void Dispose()
    {
        if (_pipelineState != IntPtr.Zero)
        {
            // Release pipeline state
            objc_msgSend(_pipelineState, sel_registerName("release"));
            _pipelineState = IntPtr.Zero;
        }
    }
    
    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr sel_registerName(string name);
    
    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend(IntPtr receiver, IntPtr selector);
}
