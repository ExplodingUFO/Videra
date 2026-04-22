using System.Numerics;
using System.Runtime.InteropServices;
using Videra.Core.Scene;

namespace Videra.Core.Graphics;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct ObjectAlphaMaskUniformData
{
    public ObjectAlphaMaskUniformData(float maskEnabled, float cutoff)
    {
        MaskEnabled = maskEnabled;
        Cutoff = cutoff;
        Padding = Vector2.Zero;
    }

    public float MaskEnabled { get; }

    public float Cutoff { get; }

    public Vector2 Padding { get; }

    public static ObjectAlphaMaskUniformData From(MaterialAlphaSettings alpha)
    {
        return alpha.Mode == MaterialAlphaMode.Mask
            ? new ObjectAlphaMaskUniformData(maskEnabled: 1f, alpha.Cutoff)
            : new ObjectAlphaMaskUniformData(maskEnabled: 0f, cutoff: 0f);
    }
}
