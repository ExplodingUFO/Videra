namespace Videra.Core.Rendering;

internal readonly record struct RenderSessionHandle(IntPtr Handle, int Generation)
{
    public static RenderSessionHandle Unbound => new(IntPtr.Zero, 0);

    public bool IsBound => Handle != IntPtr.Zero;

    public static RenderSessionHandle Create(IntPtr handle)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(handle, IntPtr.Zero);
        return new RenderSessionHandle(handle, 1);
    }

    public RenderSessionHandle Clear()
    {
        return new RenderSessionHandle(IntPtr.Zero, Generation + 1);
    }

    public RenderSessionHandle Rebind(IntPtr handle)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(handle, IntPtr.Zero);
        return new RenderSessionHandle(handle, Generation + 1);
    }
}
