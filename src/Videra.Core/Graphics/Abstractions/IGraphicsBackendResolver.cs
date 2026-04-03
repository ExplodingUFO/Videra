using Microsoft.Extensions.Logging;

namespace Videra.Core.Graphics.Abstractions;

/// <summary>
/// Resolves a concrete graphics backend implementation for the current composition environment.
/// </summary>
public interface IGraphicsBackendResolver
{
    /// <summary>
    /// Creates a backend for the requested preference or returns <c>null</c> when the resolver
    /// cannot satisfy the request in the current environment.
    /// </summary>
    IGraphicsBackend? CreateBackend(GraphicsBackendPreference preference, ILoggerFactory? loggerFactory = null);
}

