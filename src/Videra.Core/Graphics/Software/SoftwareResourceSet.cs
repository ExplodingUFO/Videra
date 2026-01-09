using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics.Software;

internal sealed class SoftwareResourceSet : IResourceSet
{
    public ResourceSetDescription Description { get; }

    public SoftwareResourceSet(ResourceSetDescription description)
    {
        Description = description;
    }

    public void Dispose()
    {
    }
}
