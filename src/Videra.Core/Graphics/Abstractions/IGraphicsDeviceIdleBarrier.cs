namespace Videra.Core.Graphics.Abstractions;

/// <summary>
/// Optional internal contract for devices that can quiesce GPU work before dependent resources are released.
/// </summary>
internal interface IGraphicsDeviceIdleBarrier
{
    void WaitForIdle();
}
