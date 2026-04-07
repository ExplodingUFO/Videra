using Microsoft.Extensions.Logging;

namespace Videra.Core.Graphics.Abstractions;

/// <summary>
/// Resolves a concrete graphics backend implementation for the current composition environment.
/// </summary>
public readonly record struct GraphicsBackendResolverResult(
    IGraphicsBackend? Backend,
    string? UnavailableReason = null);

public interface IGraphicsBackendResolver
{
    /// <summary>
    /// Resolves a backend for the requested preference or returns an unavailable reason when the
    /// current composition environment cannot satisfy the request.
    /// </summary>
    GraphicsBackendResolverResult ResolveBackend(GraphicsBackendPreference preference, ILoggerFactory? loggerFactory = null);
}
