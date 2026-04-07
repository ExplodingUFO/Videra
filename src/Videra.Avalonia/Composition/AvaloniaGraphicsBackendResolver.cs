using Microsoft.Extensions.Logging;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Avalonia.Composition;

/// <summary>
/// Avalonia-side backend resolver. Composition belongs here rather than in Videra.Core.
/// </summary>
public sealed partial class AvaloniaGraphicsBackendResolver : IGraphicsBackendResolver
{
    private static readonly AvaloniaGraphicsBackendResolver Instance = new();
    private static bool _registered;

    public GraphicsBackendResolverResult ResolveBackend(GraphicsBackendPreference preference, ILoggerFactory? loggerFactory = null)
    {
        return AvaloniaRuntimeBackendDiscovery.ResolveBackend(preference, loggerFactory);
    }

    internal static void EnsureRegistered()
    {
        if (_registered)
            return;

        GraphicsBackendFactory.ConfigureResolver(Instance);
        _registered = true;
    }
}
