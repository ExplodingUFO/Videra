using System.Runtime.CompilerServices;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Styles.Services;

namespace Videra.Core.Graphics;

public partial class VideraEngine
{
    private void OnStyleChanged(object? sender, StyleChangedEventArgs e)
    {
        if (_styleUniformBuffer != null)
        {
            _styleUniformBuffer.Update(e.UniformData);
        }
    }

	/// <summary>
	/// Releases all GPU resources held by the engine, including pipelines, buffers,
	/// renderers, scene objects, and the graphics back-end.
	/// Sets <see cref="IsInitialized"/> to <see langword="false"/>.
	/// </summary>
	public void Dispose()
	{
	    if (_disposed) return;
	    _disposed = true;

        _styleService.StyleChanged -= OnStyleChanged;
        ReleaseGraphicsResources(preserveSceneObjects: false, disposeBackend: true);
        GC.SuppressFinalize(this);
    }

	/// <summary>
	/// Initializes the engine with the specified graphics back-end.
	/// Creates GPU resources (camera buffer, style buffer, mesh pipeline) and
	/// initializes all sub-renderers (grid, axis, wireframe).
	/// If the engine is already initialized this method returns immediately.
	/// </summary>
	/// <param name="backend">The platform-specific graphics back-end to use for rendering.</param>
	public void Initialize(IGraphicsBackend backend)
	{
	    ArgumentNullException.ThrowIfNull(backend);
        if (IsInitialized) return;

        _backend = backend;
        _factory = backend.GetResourceFactory();
        _executor = backend.GetCommandExecutor();

        CreateResources();
        RestoreGraphicsResources();

        IsInitialized = true;
        Log.Initialized(_logger);
    }

    internal void Suspend()
    {
        if (!IsInitialized)
        {
            return;
        }

        ReleaseGraphicsResources(preserveSceneObjects: true, disposeBackend: true);
        Log.Suspended(_logger);
    }

    private void CreateResources()
    {
        if (_factory == null) return;

        _cameraBuffer = _factory.CreateUniformBuffer(128);
        _styleUniformBuffer = _factory.CreateUniformBuffer(128);
        _styleUniformBuffer.Update(_styleService.CurrentParameters.ToUniformData());
        _meshPipeline = _factory.CreatePipeline(
            vertexSize: (uint)Unsafe.SizeOf<VertexPositionNormalColor>(),
            hasNormals: true,
            hasColors: true);

        Log.ResourcesCreated(_logger);
    }

    private void RestoreGraphicsResources()
    {
        if (_factory == null)
        {
            return;
        }

        Grid.Initialize(_factory);
        _axisRenderer.Initialize(_factory);
        Wireframe.Initialize(_factory);

        foreach (var obj in _sceneObjects)
        {
            obj.RecreateGraphicsResources(_factory, _logger);
        }
    }

    private void ReleaseGraphicsResources(bool preserveSceneObjects, bool disposeBackend)
    {
        Grid.Dispose();
        _axisRenderer.Dispose();
        Wireframe.Dispose();

        _meshPipeline?.Dispose();
        _meshPipeline = null;

        _cameraBuffer?.Dispose();
        _cameraBuffer = null;

        _styleUniformBuffer?.Dispose();
        _styleUniformBuffer = null;

        if (preserveSceneObjects)
        {
            foreach (var obj in _sceneObjects)
            {
                obj.ReleaseGpuResources();
            }
        }
        else
        {
            ClearObjects();
        }

        if (disposeBackend)
        {
            _backend?.Dispose();
        }

        _backend = null;
        _factory = null;
        _executor = null;
        _width = 0;
        _height = 0;
        IsInitialized = false;
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
        if (!IsInitialized || width == 0 || height == 0)
        {
            Log.ResizeIgnored(_logger, IsInitialized, width, height);
            return;
        }

        if (_width == width && _height == height) return;

        try
        {
            Log.Resizing(_logger, width, height);
            _backend?.Resize((int)width, (int)height);
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
