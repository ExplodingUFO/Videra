using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics.Software;

internal sealed class SoftwarePipeline : IPipeline
{
    public PrimitiveTopology Topology { get; }

    public SoftwarePipeline(PrimitiveTopology topology)
    {
        Topology = topology;
    }

    public void Dispose()
    {
    }
}
