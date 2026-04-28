using System.Runtime.CompilerServices;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Styles.Services;

namespace Videra.Core.Graphics;

public partial class VideraEngine
{
    private void OnStyleChanged(object? sender, StyleChangedEventArgs e)
    {
        if (_resources.StyleUniformBuffer != null)
        {
            _resources.StyleUniformBuffer.Update(e.UniformData);
        }
    }

	/// <summary>
	/// Releases all GPU resources held by the engine, including pipelines, buffers,
	/// renderers, scene objects, and the graphics back-end.
	/// Sets <see cref="IsInitialized"/> to <see langword="false"/>.
	/// </summary>
	public void Dispose()
	{
        lock (_lock)
        {
            if (_state == EngineLifecycleState.Disposed)
            {
                return;
            }

            _styleService.StyleChanged -= OnStyleChanged;
            _selectionOverlayContributor.Dispose();
            _annotationOverlayContributor.Dispose();
            ReleaseGraphicsResourcesUnsafe(preserveSceneObjects: false, disposeBackend: true);
            TransitionToStateUnsafe(EngineLifecycleState.Disposed);
        }

        GC.SuppressFinalize(this);
    }

    internal void Initialize(IGraphicsDevice device, IRenderSurface renderSurface)
	{
        lock (_lock)
        {
            if (_state == EngineLifecycleState.Disposed)
            {
                return;
            }

            ArgumentNullException.ThrowIfNull(device);
            ArgumentNullException.ThrowIfNull(renderSurface);

            if (_state == EngineLifecycleState.Active)
            {
                return;
            }

            _resources.Attach(device, renderSurface);

            CreateResourcesUnsafe();
            RestoreGraphicsResourcesUnsafe();
            TransitionToStateUnsafe(EngineLifecycleState.Active);
        }

        Log.Initialized(_logger);
    }

    internal void Suspend()
    {
        lock (_lock)
        {
            if (_state != EngineLifecycleState.Active)
            {
                return;
            }

            ReleaseGraphicsResourcesUnsafe(preserveSceneObjects: true, disposeBackend: true);
            TransitionToStateUnsafe(EngineLifecycleState.Suspended);
        }

        Log.Suspended(_logger);
    }

    private void CreateResourcesUnsafe()
    {
        _resources.CreateResources(_styleService);
        Log.ResourcesCreated(_logger);
    }

    private void RestoreGraphicsResourcesUnsafe()
    {
        if (_resources.ResourceFactory == null)
        {
            return;
        }

        _resources.RestoreGraphicsResources(Grid, _axisRenderer, Wireframe, _renderWorld, _logger);
    }

    private void ReleaseGraphicsResourcesUnsafe(bool preserveSceneObjects, bool disposeBackend)
    {
        _resources.ReleaseGraphicsResources(Grid, _axisRenderer, Wireframe, _renderWorld, preserveSceneObjects, disposeBackend);
        _width = 0;
        _height = 0;
    }

	/// <summary>
	/// Resizes the internal render target and updates the camera projection matrix.
	/// Ignored when the engine is not initialized, when dimensions are zero, or when
	/// the size has not changed.
	/// </summary>
	/// <param name="width">The new render target width in pixels.</param>
	/// <param name="height">The new render target height in pixels.</param>
	public void Resize(uint width, uint height)
	{
        lock (_lock)
        {
            if (_state != EngineLifecycleState.Active || width == 0 || height == 0)
            {
                Log.ResizeIgnored(_logger, IsInitialized, width, height);
                return;
            }

            if (_width == width && _height == height) return;

            try
            {
                Log.Resizing(_logger, width, height);
                _resources.RenderSurface?.Resize((int)width, (int)height);
                Camera.UpdateProjection(width, height);
                _width = width;
                _height = height;
            }
            catch (Exception ex)
            {
                Log.ResizeFailed(_logger, ex.Message, ex);
            }
        }
    }
}
