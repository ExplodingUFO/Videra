using System.Numerics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Wireframe;

namespace Videra.Core.Graphics;

public partial class VideraEngine
{
    private int _frameCount;

	/// <summary>
	/// Renders a single frame: clears the back-end, updates camera and style uniforms,
	/// draws the grid, solid scene objects, wireframe overlays, and axis helper,
	/// then presents the result.
	/// This method is thread-safe; calls are serialized via an internal lock.
	/// </summary>
	public void Draw()
	{
        lock (_lock)
        {
            if (!CanDrawFrame())
            {
                return;
            }

            _frameCount++;
            bool shouldLog = EnableFrameLogging && _frameCount % 60 == 0;

            if (shouldLog)
            {
                Log.DrawingFrame(_logger, _frameCount, _sceneObjects.Count, Grid.IsVisible);
            }

            RenderFrame(shouldLog);
        }
    }

    private bool CanDrawFrame()
    {
        return _state == EngineLifecycleState.Active
            && _backend != null
            && _executor != null
            && _meshPipeline != null;
    }

    private void RenderFrame(bool shouldLog)
    {
        BeginFrame();
        BindSharedFrameState(shouldLog);
        RenderGridPass(shouldLog);
        RenderSolidObjects(shouldLog);
        RenderOverlayPasses();
        _backend!.EndFrame();
    }

    private void BeginFrame()
    {
        _backend!.SetClearColor(new Vector4(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A));
        _backend.BeginFrame();
        _executor!.Clear(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A);
        _executor.SetViewport(0, 0, _width, _height);
    }

    private void BindSharedFrameState(bool shouldLog)
    {
        if (_cameraBuffer != null)
        {
            _cameraBuffer.Update(new CameraUniform(Camera.ViewMatrix, Camera.ProjectionMatrix));

            if (shouldLog)
            {
                Log.CameraPosition(_logger, Camera.Position, Camera.Target);
                Log.CameraRotation(_logger, Camera.Yaw, Camera.Pitch);

                var viewMatrix = Camera.ViewMatrix;
                Log.ViewMatrix(_logger, viewMatrix.M11, viewMatrix.M12, viewMatrix.M13, viewMatrix.M14);

                var projectionMatrix = Camera.ProjectionMatrix;
                Log.ProjectionMatrix(_logger, projectionMatrix.M11, projectionMatrix.M22, projectionMatrix.M33, projectionMatrix.M43);
            }
        }

        _executor!.SetPipeline(_meshPipeline!);

        if (_cameraBuffer != null)
        {
            _executor.SetVertexBuffer(_cameraBuffer, RenderBindingSlots.Camera);
            if (shouldLog)
            {
                Log.CameraUniformBound(_logger);
            }
        }

        if (_styleUniformBuffer != null)
        {
            _executor.SetVertexBuffer(_styleUniformBuffer, RenderBindingSlots.Style);
        }
    }

    private void RenderGridPass(bool shouldLog)
    {
        if (shouldLog)
        {
            Log.DrawingGrid(_logger, Grid.IsVisible);
        }

        Grid.Draw(_executor, _meshPipeline, Camera, _width, _height);
        _executor!.SetPipeline(_meshPipeline!);
    }

    private void RenderSolidObjects(bool shouldLog)
    {
        bool shouldRenderSolid = Wireframe.ShouldRenderSolid();

        if (shouldLog && _sceneObjects.Count > 0)
        {
            Log.StartingObjectRender(_logger, _sceneObjects.Count, shouldRenderSolid);
        }

        if (!shouldRenderSolid)
        {
            return;
        }

        foreach (var obj in _sceneObjects)
        {
            RenderSolidObject(obj, shouldLog);
        }
    }

    private void RenderSolidObject(Object3D obj, bool shouldLog)
    {
        if (obj.VertexBuffer == null || obj.IndexBuffer == null || obj.WorldBuffer == null)
        {
            if (shouldLog)
            {
                Log.SkippingObjectMissingBuffers(_logger, obj.Name);
            }

            return;
        }

        obj.UpdateUniforms(_executor!);
        _executor!.SetVertexBuffer(obj.VertexBuffer, RenderBindingSlots.Vertex);
        _executor.SetVertexBuffer(obj.WorldBuffer, RenderBindingSlots.World);
        _executor.SetIndexBuffer(obj.IndexBuffer);

        if (shouldLog)
        {
            Log.DrawingObject(_logger, obj.Name, obj.IndexCount);
        }

        switch (obj.Topology)
        {
            case MeshTopology.Lines:
                _executor.DrawIndexed(PrimitiveCommandKind.LineList, obj.IndexCount, 1, 0, 0, 0);
                break;
            case MeshTopology.Points:
                _executor.DrawIndexed(PrimitiveCommandKind.PointList, obj.IndexCount, 1, 0, 0, 0);
                break;
            default:
                _executor.DrawIndexed(obj.IndexCount, 1, 0, 0, 0);
                break;
        }
    }

    private void RenderOverlayPasses()
    {
        if (Wireframe.Mode != WireframeMode.None)
        {
            Wireframe.RenderWireframes(_sceneObjects, _executor!, _meshPipeline!, Camera, _width, _height);
        }

        _axisRenderer.Draw(_executor!, _meshPipeline!, Camera, _width, _height, RenderScale);
    }
}
