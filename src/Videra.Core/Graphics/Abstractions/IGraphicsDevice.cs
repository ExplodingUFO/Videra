namespace Videra.Core.Graphics.Abstractions;

/// <summary>
/// Internal graphics device abstraction that owns reusable rendering resources
/// and can create one or more render surfaces.
/// </summary>
internal interface IGraphicsDevice : IDisposable
{
    GraphicsBackendPreference? ActiveBackendPreference { get; }

    bool IsSoftwareBackend { get; }

    IResourceFactory ResourceFactory { get; }

    ICommandExecutor CommandExecutor { get; }

    IRenderSurface CreateRenderSurface();
}
