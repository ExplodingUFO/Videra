using System.Numerics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.RenderPipeline;
using Videra.Core.Graphics.RenderPipeline.Extensibility;
using Videra.Core.Graphics.Wireframe;
using Videra.Core.Scene;

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
                Log.DrawingFrame(_logger, _frameCount, _renderWorld.SceneObjects.Count, Grid.IsVisible);
            }

            RenderFrame(shouldLog);
        }
    }

    private bool CanDrawFrame()
    {
        return _state == EngineLifecycleState.Active
            && _resources.RenderSurface != null
            && _resources.CommandExecutor != null
            && _resources.MeshPipeline != null
            && _resources.TransparentMeshPipeline != null;
    }

    private void RenderFrame(bool shouldLog)
    {
        var effectiveWireframeMode = GetEffectiveWireframeMode();
        EnsureWireframeResources(effectiveWireframeMode);
        var plan = CreateFramePlan(effectiveWireframeMode);
        ExecuteFramePlan(plan, shouldLog);
    }

    private RenderFramePlan CreateFramePlan(WireframeMode effectiveWireframeMode)
    {
        var renderOverlayWireframe = HasOverlayWireframePassUnsafe();
        var renderGrid = Grid.IsVisible;
        var renderSolidGeometry = effectiveWireframeMode != WireframeMode.WireframeOnly;
        var renderWireframe = effectiveWireframeMode != WireframeMode.None || renderOverlayWireframe;
        var renderAxis = ShowAxis;
        var (renderOpaqueGeometry, renderTransparentGeometry) = DetermineSolidGeometryFeatures(renderSolidGeometry);
        var stages = new List<RenderPipelineStage>
        {
            RenderPipelineStage.PrepareFrame,
            RenderPipelineStage.BindSharedFrameState
        };

        if (renderGrid)
        {
            stages.Add(RenderPipelineStage.GridPass);
        }

        if (renderSolidGeometry)
        {
            stages.Add(RenderPipelineStage.SolidGeometryPass);
        }

        if (renderWireframe)
        {
            stages.Add(RenderPipelineStage.WireframePass);
        }

        if (renderAxis)
        {
            stages.Add(RenderPipelineStage.AxisPass);
        }

        stages.Add(RenderPipelineStage.PresentFrame);

        return new RenderFramePlan(
            DetermineProfile(effectiveWireframeMode, renderOverlayWireframe),
            DetermineActiveFeatures(renderGrid, renderOpaqueGeometry, renderTransparentGeometry, renderWireframe, renderAxis),
            effectiveWireframeMode,
            renderGrid,
            renderSolidGeometry,
            renderWireframe,
            renderAxis,
            stages);
    }

    private void ExecuteFramePlan(RenderFramePlan plan, bool shouldLog)
    {
        var executedStages = new List<RenderPipelineStage>(plan.PlannedStages.Count);
        var opaqueObjectCount = 0;
        var transparentObjectCount = 0;
        InvokeFrameHooks(RenderFrameHookPoint.FrameBegin, plan, lastPipelineSnapshot: null);

        using (BeginFrame())
        {
            executedStages.Add(RenderPipelineStage.PrepareFrame);

            BindSharedFrameState(shouldLog);
            executedStages.Add(RenderPipelineStage.BindSharedFrameState);
            InvokeFrameHooks(RenderFrameHookPoint.SceneSubmit, plan, lastPipelineSnapshot: null);

            if (plan.RenderGrid)
            {
                ExecutePassSlot(RenderPassSlot.Grid, plan, shouldLog);
                executedStages.Add(RenderPipelineStage.GridPass);
            }

            if (plan.RenderSolidGeometry)
            {
                var solidObjectCounts = ExecutePassSlot(RenderPassSlot.SolidGeometry, plan, shouldLog);
                opaqueObjectCount = solidObjectCounts.opaqueObjectCount;
                transparentObjectCount = solidObjectCounts.transparentObjectCount;
                executedStages.Add(RenderPipelineStage.SolidGeometryPass);
            }

            if (plan.RenderWireframe)
            {
                ExecutePassSlot(RenderPassSlot.Wireframe, plan, shouldLog);
                executedStages.Add(RenderPipelineStage.WireframePass);
            }

            if (plan.RenderAxis)
            {
                ExecutePassSlot(RenderPassSlot.Axis, plan, shouldLog);
                executedStages.Add(RenderPipelineStage.AxisPass);
            }
        }

        executedStages.Add(RenderPipelineStage.PresentFrame);

        var frameObjectCount = _renderWorld.SceneObjects.Count;
        LastPipelineSnapshot = new RenderPipelineSnapshot(
            plan.Profile,
            plan.ActiveFeatures,
            plan.EffectiveWireframeMode,
            plan.RenderGrid,
            plan.RenderSolidGeometry,
            plan.RenderWireframe,
            plan.RenderAxis,
            frameObjectCount,
            opaqueObjectCount,
            transparentObjectCount,
            executedStages);
        InvokeFrameHooks(RenderFrameHookPoint.FrameEnd, plan, LastPipelineSnapshot);
    }

    private IFrameContext BeginFrame()
    {
        var frameContext = _resources.RenderSurface!.BeginFrame(new Vector4(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A));
        _resources.CommandExecutor!.Clear(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A);
        _resources.CommandExecutor.SetViewport(0, 0, _width, _height);
        return frameContext;
    }

    private void BindSharedFrameState(bool shouldLog)
    {
        if (_resources.CameraBuffer != null)
        {
            _resources.CameraBuffer.Update(new CameraUniform(Camera.ViewMatrix, Camera.ProjectionMatrix));

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

        _resources.CommandExecutor!.SetPipeline(_resources.MeshPipeline!);

        if (_resources.CameraBuffer != null)
        {
            _resources.CommandExecutor.SetVertexBuffer(_resources.CameraBuffer, RenderBindingSlots.Camera);
            if (shouldLog)
            {
                Log.CameraUniformBound(_logger);
            }
        }

        if (_resources.StyleUniformBuffer != null)
        {
            _resources.CommandExecutor.SetVertexBuffer(_resources.StyleUniformBuffer, RenderBindingSlots.Style);
        }

        BindDefaultAlphaMaskState();
    }

    private void RenderGridPass(bool shouldLog)
    {
        if (shouldLog)
        {
            Log.DrawingGrid(_logger, Grid.IsVisible);
        }

        Grid.Draw(_resources.CommandExecutor, _resources.MeshPipeline, Camera, _width, _height);
        _resources.CommandExecutor!.SetPipeline(_resources.MeshPipeline!);
    }

    private (int opaqueObjectCount, int transparentObjectCount) RenderSolidObjects(bool shouldLog, WireframeMode effectiveWireframeMode)
    {
        bool shouldRenderSolid = effectiveWireframeMode != WireframeMode.WireframeOnly;

        if (shouldLog && _renderWorld.SceneObjects.Count > 0)
        {
            Log.StartingObjectRender(_logger, _renderWorld.SceneObjects.Count, shouldRenderSolid);
        }

        if (!shouldRenderSolid)
        {
            return (0, 0);
        }

        var opaqueObjectCount = RenderOpaqueObjects(shouldLog);
        var transparentObjectCount = RenderTransparentObjects(shouldLog);
        return (opaqueObjectCount, transparentObjectCount);
    }

    private int RenderOpaqueObjects(bool shouldLog)
    {
        var opaqueObjectCount = 0;

        foreach (var obj in _renderWorld.SceneObjects)
        {
            if (!obj.HasOpaqueGeometry)
            {
                continue;
            }

            RenderSolidObject(obj, shouldLog, transparentPass: false);
            opaqueObjectCount++;
        }

        return opaqueObjectCount;
    }

    private int RenderTransparentObjects(bool shouldLog)
    {
        var transparentObjects = GetTransparentObjectsInRenderOrder();
        if (transparentObjects.Count == 0)
        {
            return 0;
        }

        _resources.CommandExecutor!.SetPipeline(_resources.TransparentMeshPipeline!);
        _resources.CommandExecutor.SetDepthState(testEnabled: true, writeEnabled: false);

        try
        {
            foreach (var obj in transparentObjects)
            {
                RenderSolidObject(obj, shouldLog, transparentPass: true);
            }
        }
        finally
        {
            _resources.CommandExecutor.ResetDepthState();
            _resources.CommandExecutor.SetPipeline(_resources.MeshPipeline!);
        }

        return transparentObjects.Count;
    }

    private void RenderSolidObject(Object3D obj, bool shouldLog, bool transparentPass)
    {
        if (obj.VertexBuffer == null || obj.IndexBuffer == null || obj.WorldBuffer == null)
        {
            if (shouldLog)
            {
                Log.SkippingObjectMissingBuffers(_logger, obj.Name);
            }

            return;
        }

        obj.UpdateUniforms(_resources.CommandExecutor!);
        _resources.CommandExecutor!.SetVertexBuffer(obj.VertexBuffer, RenderBindingSlots.Vertex);
        _resources.CommandExecutor.SetVertexBuffer(obj.WorldBuffer, RenderBindingSlots.World);
        if (obj.AlphaMaskBuffer != null)
        {
            _resources.CommandExecutor.SetVertexBuffer(obj.AlphaMaskBuffer, RenderBindingSlots.AlphaMask);
        }
        _resources.CommandExecutor.SetIndexBuffer(obj.IndexBuffer);

        if (shouldLog)
        {
            Log.DrawingObject(_logger, obj.Name, obj.IndexCount);
        }

        var segments = obj.Segments;
        if (segments.Length == 0)
        {
            DrawSegment(obj, obj.MaterialAlpha, transparentPass, firstIndex: 0, indexCount: obj.IndexCount);
            return;
        }

        for (var i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            var isTransparent = segment.Alpha.Mode == Scene.MaterialAlphaMode.Blend;
            if (isTransparent != transparentPass)
            {
                continue;
            }

            DrawSegment(obj, segment.Alpha, transparentPass, segment.StartIndex, segment.IndexCount);
        }
    }

    private void DrawSegment(
        Object3D obj,
        MaterialAlphaSettings alpha,
        bool transparentPass,
        uint firstIndex,
        uint indexCount)
    {
        if (transparentPass && alpha.Mode != Scene.MaterialAlphaMode.Blend)
        {
            return;
        }

        if (!transparentPass && alpha.Mode == Scene.MaterialAlphaMode.Blend)
        {
            return;
        }

        obj.ApplyMaterialAlpha(alpha);

        switch (obj.Topology)
        {
            case MeshTopology.Lines:
                _resources.CommandExecutor!.DrawIndexed(PrimitiveCommandKind.LineList, indexCount, 1, firstIndex, 0, 0);
                break;
            case MeshTopology.Points:
                _resources.CommandExecutor!.DrawIndexed(PrimitiveCommandKind.PointList, indexCount, 1, firstIndex, 0, 0);
                break;
            default:
                _resources.CommandExecutor!.DrawIndexed(indexCount, 1, firstIndex, 0, 0);
                break;
        }
    }

    private static RenderPipelineProfile DetermineProfile(
        WireframeMode effectiveWireframeMode,
        bool renderOverlayWireframe)
    {
        if (effectiveWireframeMode == WireframeMode.None)
        {
            return renderOverlayWireframe
                ? RenderPipelineProfile.StandardWithWireframeOverlay
                : RenderPipelineProfile.Standard;
        }

        return effectiveWireframeMode == WireframeMode.WireframeOnly
            ? RenderPipelineProfile.WireframeOnly
            : RenderPipelineProfile.StandardWithWireframeOverlay;
    }

    private WireframeMode GetEffectiveWireframeMode()
    {
        if (Wireframe.Mode != WireframeMode.None)
        {
            return Wireframe.Mode;
        }

        return _styleService.CurrentParameters.Material.WireframeMode
            ? WireframeMode.WireframeOnly
            : WireframeMode.None;
    }

    private void EnsureWireframeResources(WireframeMode effectiveWireframeMode)
    {
        if ((effectiveWireframeMode == WireframeMode.None && !_renderWorld.SelectionOverlayState.HasOverlay) || _resources.ResourceFactory == null)
        {
            return;
        }

        _renderWorld.EnsureWireframeResources(_resources.ResourceFactory, _logger);
    }

    private void RenderWireframes(WireframeMode effectiveWireframeMode)
    {
        if (Wireframe.Mode == effectiveWireframeMode)
        {
            Wireframe.RenderWireframes(_renderWorld.SceneObjects, _resources.CommandExecutor!, _resources.MeshPipeline!, Camera, _width, _height);
            return;
        }

        var explicitMode = Wireframe.Mode;

        try
        {
            Wireframe.Mode = effectiveWireframeMode;
            Wireframe.RenderWireframes(_renderWorld.SceneObjects, _resources.CommandExecutor!, _resources.MeshPipeline!, Camera, _width, _height);
        }
        finally
        {
            Wireframe.Mode = explicitMode;
        }
    }

    private (int opaqueObjectCount, int transparentObjectCount) ExecutePassSlot(RenderPassSlot slot, RenderFramePlan plan, bool shouldLog)
    {
        BindDefaultAlphaMaskState();

        var sharedFrameState = CreateSharedFrameStateUnsafe();
        var context = new RenderPassContributionContext
        {
            Slot = slot,
            FramePlan = plan,
            ActiveFeatures = plan.ActiveFeatures,
            SlotFeatures = RenderPassSlotFeatureMap.Resolve(slot),
            CommandExecutor = sharedFrameState.CommandExecutor,
            ResourceFactory = sharedFrameState.ResourceFactory,
            MeshPipeline = sharedFrameState.MeshPipeline,
            SceneObjects = _renderWorld.SceneObjects,
            Width = sharedFrameState.Width,
            Height = sharedFrameState.Height,
            RenderScale = sharedFrameState.RenderScale,
            ShouldLog = shouldLog,
            IsInitialized = _state == EngineLifecycleState.Active,
            ActiveBackendPreference = sharedFrameState.ActiveBackendPreference,
            LastPipelineSnapshot = sharedFrameState.LastPipelineSnapshot,
            SelectionOverlay = _renderWorld.SelectionOverlayState,
            AnnotationOverlay = _renderWorld.AnnotationOverlayState
        };

        var hasReplacement = _passRegistry.TryGetReplacement(slot, out var replacement);
        var opaqueObjectCount = 0;
        var transparentObjectCount = 0;
        if (hasReplacement)
        {
            replacement!.Contribute(context);
        }
        else
        {
            (opaqueObjectCount, transparentObjectCount) = ExecuteBuiltInPassSlot(slot, context);

            if (slot == RenderPassSlot.Wireframe)
            {
                _selectionOverlayContributor.Contribute(context);
                _annotationOverlayContributor.Contribute(context);
            }
        }

        BindDefaultAlphaMaskState();

        foreach (var contributor in _passRegistry.GetRegistrations(slot))
        {
            contributor.Contribute(context);
        }

        return (opaqueObjectCount, transparentObjectCount);
    }

    private (int opaqueObjectCount, int transparentObjectCount) ExecuteBuiltInPassSlot(RenderPassSlot slot, RenderPassContributionContext context)
    {
        switch (slot)
        {
            case RenderPassSlot.Grid:
                RenderGridPass(context.ShouldLog);
                return (0, 0);
            case RenderPassSlot.SolidGeometry:
                return RenderSolidObjects(context.ShouldLog, context.FramePlan.EffectiveWireframeMode);
            case RenderPassSlot.Wireframe:
                RenderWireframes(context.FramePlan.EffectiveWireframeMode);
                return (0, 0);
            case RenderPassSlot.Axis:
                _axisRenderer.Draw(_resources.CommandExecutor!, _resources.MeshPipeline!, Camera, _width, _height, RenderScale);
                return (0, 0);
            default:
                throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown render pass slot.");
        }
    }

    private bool HasOverlayWireframePassUnsafe()
    {
        return _renderWorld.HasOverlayWireframe;
    }

    private void InvokeFrameHooks(
        RenderFrameHookPoint hookPoint,
        RenderFramePlan plan,
        RenderPipelineSnapshot? lastPipelineSnapshot)
    {
        var context = new RenderFrameHookContext
        {
            HookPoint = hookPoint,
            FramePlan = plan,
            ActiveFeatures = plan.ActiveFeatures,
            Width = _width,
            Height = _height,
            RenderScale = RenderScale,
            IsInitialized = _state == EngineLifecycleState.Active,
            ActiveBackendPreference = GetActiveBackendPreferenceUnsafe(),
            LastPipelineSnapshot = lastPipelineSnapshot
        };

        foreach (var hook in _passRegistry.GetHooks(hookPoint))
        {
            hook(context);
        }
    }

    private SharedFrameState CreateSharedFrameStateUnsafe()
    {
        return new SharedFrameState(
            _resources.CommandExecutor!,
            _resources.ResourceFactory!,
            _resources.MeshPipeline!,
            _resources.CameraBuffer,
            _resources.StyleUniformBuffer,
            _width,
            _height,
            RenderScale,
            GetActiveBackendPreferenceUnsafe(),
            LastPipelineSnapshot);
    }

    private static RenderFeatureSet DetermineActiveFeatures(
        bool renderGrid,
        bool renderOpaqueGeometry,
        bool renderTransparentGeometry,
        bool renderWireframe,
        bool renderAxis)
    {
        var features = RenderFeatureSet.None;

        if (renderOpaqueGeometry)
        {
            features |= RenderFeatureSet.Opaque;
        }

        if (renderTransparentGeometry)
        {
            features |= RenderFeatureSet.Transparent;
        }

        if (renderGrid || renderWireframe || renderAxis)
        {
            features |= RenderFeatureSet.Overlay;
        }

        return features;
    }

    private (bool hasOpaqueGeometry, bool hasTransparentGeometry) DetermineSolidGeometryFeatures(bool renderSolidGeometry)
    {
        if (!renderSolidGeometry)
        {
            return (false, false);
        }

        var hasOpaqueGeometry = false;
        var hasTransparentGeometry = false;

        foreach (var obj in _renderWorld.SceneObjects)
        {
            if (IsTransparentObject(obj))
            {
                hasTransparentGeometry = true;
            }
            else
            {
                hasOpaqueGeometry = true;
            }

            if (hasOpaqueGeometry && hasTransparentGeometry)
            {
                break;
            }
        }

        return (hasOpaqueGeometry, hasTransparentGeometry);
    }

    private List<Object3D> GetTransparentObjectsInRenderOrder()
    {
        var ordered = new List<(Object3D Object, float DistanceSquared, int Index)>();

        for (var index = 0; index < _renderWorld.SceneObjects.Count; index++)
        {
            var obj = _renderWorld.SceneObjects[index];
            if (!IsTransparentObject(obj))
            {
                continue;
            }

            var sortOrigin = obj.WorldBounds?.Center ?? obj.Position;
            ordered.Add((obj, Vector3.DistanceSquared(Camera.Position, sortOrigin), index));
        }

        ordered.Sort(static (left, right) =>
        {
            var distanceComparison = right.DistanceSquared.CompareTo(left.DistanceSquared);
            return distanceComparison != 0
                ? distanceComparison
                : left.Index.CompareTo(right.Index);
        });

        var transparentObjects = new List<Object3D>(ordered.Count);
        foreach (var entry in ordered)
        {
            transparentObjects.Add(entry.Object);
        }

        return transparentObjects;
    }

    private static bool IsTransparentObject(Object3D obj)
    {
        return obj.HasTransparentGeometry;
    }

    private void BindDefaultAlphaMaskState()
    {
        if (_resources.DefaultAlphaMaskBuffer != null)
        {
            _resources.CommandExecutor!.SetVertexBuffer(_resources.DefaultAlphaMaskBuffer, RenderBindingSlots.AlphaMask);
        }
    }
}
