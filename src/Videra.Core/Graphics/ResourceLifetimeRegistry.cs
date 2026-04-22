using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Styles.Services;

namespace Videra.Core.Graphics;

internal sealed class ResourceLifetimeRegistry
{
    public IGraphicsDevice? Device { get; private set; }

    public IRenderSurface? RenderSurface { get; private set; }

    public IResourceFactory? ResourceFactory { get; private set; }

    public ICommandExecutor? CommandExecutor { get; private set; }

    public IPipeline? MeshPipeline { get; private set; }

    public IBuffer? CameraBuffer { get; private set; }

    public IBuffer? StyleUniformBuffer { get; private set; }

    public IBuffer? DefaultAlphaMaskBuffer { get; private set; }

    public void Attach(IGraphicsDevice device, IRenderSurface renderSurface)
    {
        Device = device;
        RenderSurface = renderSurface;
        ResourceFactory = device.ResourceFactory;
        CommandExecutor = device.CommandExecutor;
    }

    public void CreateResources(IRenderStyleService styleService)
    {
        if (ResourceFactory == null)
        {
            return;
        }

        CameraBuffer = ResourceFactory.CreateUniformBuffer(128);
        StyleUniformBuffer = ResourceFactory.CreateUniformBuffer(128);
        StyleUniformBuffer.Update(styleService.CurrentParameters.ToUniformData());
        DefaultAlphaMaskBuffer = ResourceFactory.CreateUniformBuffer(16);
        DefaultAlphaMaskBuffer.Update(ObjectAlphaMaskUniformData.From(Scene.MaterialAlphaSettings.Opaque));
        MeshPipeline = ResourceFactory.CreatePipeline(
            vertexSize: (uint)Unsafe.SizeOf<VertexPositionNormalColor>(),
            hasNormals: true,
            hasColors: true);
    }

    public void RestoreGraphicsResources(
        GridRenderer grid,
        AxisRenderer axisRenderer,
        Wireframe.WireframeRenderer wireframeRenderer,
        RenderWorld renderWorld,
        ILogger logger)
    {
        if (ResourceFactory == null)
        {
            return;
        }

        grid.Initialize(ResourceFactory);
        axisRenderer.Initialize(ResourceFactory);
        wireframeRenderer.Initialize(ResourceFactory);
        renderWorld.RestoreGraphicsResources(ResourceFactory, logger);
    }

    public void ReleaseGraphicsResources(
        GridRenderer grid,
        AxisRenderer axisRenderer,
        Wireframe.WireframeRenderer wireframeRenderer,
        RenderWorld renderWorld,
        bool preserveSceneObjects,
        bool disposeDevice)
    {
        if (disposeDevice && Device is IGraphicsDeviceIdleBarrier idleBarrier)
        {
            idleBarrier.WaitForIdle();
        }

        grid.Dispose();
        axisRenderer.Dispose();
        wireframeRenderer.Dispose();

        MeshPipeline?.Dispose();
        MeshPipeline = null;

        CameraBuffer?.Dispose();
        CameraBuffer = null;

        StyleUniformBuffer?.Dispose();
        StyleUniformBuffer = null;

        DefaultAlphaMaskBuffer?.Dispose();
        DefaultAlphaMaskBuffer = null;

        renderWorld.ReleaseGraphicsResources(preserveSceneObjects);

        if (disposeDevice)
        {
            RenderSurface?.Dispose();
            Device?.Dispose();
        }

        RenderSurface = null;
        Device = null;
        ResourceFactory = null;
        CommandExecutor = null;
    }
}
