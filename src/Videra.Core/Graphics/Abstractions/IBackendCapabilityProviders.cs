namespace Videra.Core.Graphics.Abstractions;

/// <summary>
/// Optional capability provider for advanced resource factory operations.
/// </summary>
public interface IResourceFactoryCapabilities
{
    /// <summary>
    /// Gets a value indicating whether <see cref="IResourceFactory.CreateShader" /> is a supported public operation.
    /// </summary>
    bool SupportsShaderCreation { get; }

    /// <summary>
    /// Gets a value indicating whether <see cref="IResourceFactory.CreateResourceSet" /> is a supported public operation.
    /// </summary>
    bool SupportsResourceSetCreation { get; }
}

/// <summary>
/// Optional capability provider for advanced command executor operations.
/// </summary>
public interface ICommandExecutorCapabilities
{
    /// <summary>
    /// Gets a value indicating whether <see cref="ICommandExecutor.SetResourceSet" /> is a supported public operation.
    /// </summary>
    bool SupportsResourceSetBinding { get; }
}

