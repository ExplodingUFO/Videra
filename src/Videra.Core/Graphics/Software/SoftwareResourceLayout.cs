using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics.Software;

internal sealed class SoftwareResourceLayout : IResourceLayout
{
    public ResourceLayoutDescription Description { get; }

    public SoftwareResourceLayout(ResourceLayoutDescription description)
    {
        Description = description;
    }

    public void Dispose()
    {
    }
}
