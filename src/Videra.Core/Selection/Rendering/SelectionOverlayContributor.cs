using System.Runtime.CompilerServices;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.RenderPipeline.Extensibility;

namespace Videra.Core.Selection.Rendering;

public sealed class SelectionOverlayContributor : IRenderPassContributor, IDisposable
{
    private bool _disposed;

    public void Contribute(RenderPassContributionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (_disposed || !context.SelectionOverlay.HasOverlay)
        {
            return;
        }

        var selectedObjectIds = context.SelectionOverlay.SelectedObjectIds;
        foreach (var sceneObject in context.SceneObjects)
        {
            if (selectedObjectIds.Contains(sceneObject.Id))
            {
                RenderWireframeOverlay(
                    sceneObject,
                    context.CommandExecutor,
                    context.ResourceFactory,
                    context.MeshPipeline,
                    context.SelectionOverlay.SelectedLineColor);
            }
        }

        if (context.SelectionOverlay.HoverObjectId is not Guid hoverObjectId)
        {
            return;
        }

        var hoverObject = context.SceneObjects.FirstOrDefault(obj => obj.Id == hoverObjectId);
        if (hoverObject != null)
        {
            RenderWireframeOverlay(
                hoverObject,
                context.CommandExecutor,
                context.ResourceFactory,
                context.MeshPipeline,
                context.SelectionOverlay.HoverLineColor);
        }
    }

    public void Dispose()
    {
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private static void RenderWireframeOverlay(
        Object3D sceneObject,
        ICommandExecutor commandExecutor,
        IResourceFactory resourceFactory,
        IPipeline meshPipeline,
        RgbaFloat lineColor)
    {
        if (sceneObject.LineIndexBuffer == null ||
            sceneObject.WorldBuffer == null ||
            sceneObject.LineIndexCount == 0 ||
            !sceneObject.TryCreateColoredWireframeVertices(lineColor, out var coloredVertices))
        {
            return;
        }

        var vertexBufferSize = checked((uint)(coloredVertices.Length * Unsafe.SizeOf<VertexPositionNormalColor>()));
        using var vertexBuffer = resourceFactory.CreateVertexBuffer(vertexBufferSize);
        vertexBuffer.SetData(coloredVertices, 0);

        commandExecutor.SetPipeline(meshPipeline);
        commandExecutor.SetDepthState(testEnabled: false, writeEnabled: false);
        sceneObject.UpdateUniforms(commandExecutor);
        commandExecutor.SetVertexBuffer(vertexBuffer, RenderBindingSlots.Vertex);
        commandExecutor.SetVertexBuffer(sceneObject.WorldBuffer, RenderBindingSlots.World);
        commandExecutor.SetIndexBuffer(sceneObject.LineIndexBuffer);
        commandExecutor.DrawIndexed(PrimitiveCommandKind.LineList, sceneObject.LineIndexCount, 1, 0, 0, 0);
        commandExecutor.ResetDepthState();
    }
}
