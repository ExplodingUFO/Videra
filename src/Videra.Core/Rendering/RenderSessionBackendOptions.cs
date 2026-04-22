using Videra.Core.Graphics;

namespace Videra.Core.Rendering;

internal readonly record struct RenderSessionBackendOptions(
    BackendEnvironmentOverrideMode EnvironmentOverrideMode,
    bool AllowSoftwareFallback);
