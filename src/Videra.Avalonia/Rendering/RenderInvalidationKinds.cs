using System;

namespace Videra.Avalonia.Rendering;

[Flags]
internal enum RenderInvalidationKinds
{
    None = 0,
    Lifecycle = 1 << 0,
    Scene = 1 << 1,
    Style = 1 << 2,
    Overlay = 1 << 3,
    Interaction = 1 << 4
}
