using System.Numerics;
using System.Runtime.InteropServices;

namespace Videra.Core.Graphics;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct CameraUniform
{
    public readonly Matrix4x4 View;
    public readonly Matrix4x4 Projection;

    public CameraUniform(Matrix4x4 view, Matrix4x4 projection)
    {
        View = view;
        Projection = projection;
    }
}
