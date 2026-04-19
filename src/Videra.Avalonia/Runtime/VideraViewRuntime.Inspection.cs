using System.Diagnostics;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Controls.Interaction;
using Videra.Avalonia.Rendering;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Inspection;

namespace Videra.Avalonia.Runtime;

internal sealed partial class VideraViewRuntime
{
    private string? _lastSnapshotExportPath;
    private string? _lastSnapshotExportStatus;

    public VideraInspectionState CaptureInspectionState()
    {
        return new VideraInspectionState
        {
            CameraTarget = _engine.Camera.Target,
            CameraRadius = _engine.Camera.Radius,
            CameraYaw = _engine.Camera.Yaw,
            CameraPitch = _engine.Camera.Pitch,
            SelectedObjectIds = _selectionState.ObjectIds.ToArray(),
            PrimarySelectedObjectId = _selectionState.PrimaryObjectId,
            MeasurementSnapMode = _interactionOptions.MeasurementSnapMode,
            ClippingPlanes = _sceneCoordinator.ClippingPlanes.ToArray(),
            Measurements = CloneMeasurements(_measurements)
        };
    }

    public void ApplyInspectionState(VideraInspectionState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        _engine.Camera.SetOrbit(state.CameraTarget, state.CameraRadius, state.CameraYaw, state.CameraPitch);
        _selectionState = new VideraSelectionState
        {
            ObjectIds = state.SelectedObjectIds?.ToArray() ?? Array.Empty<Guid>(),
            PrimaryObjectId = state.PrimarySelectedObjectId
        };
        _interactionOptions.MeasurementSnapMode = state.MeasurementSnapMode;
        _measurements = CloneMeasurements(state.Measurements);
        _sceneCoordinator.UpdateClippingPlanes(state.ClippingPlanes);
        SynchronizeOverlayState();
        RefreshInspectionDiagnostics();
        RefreshBackendDiagnostics(_backendDiagnostics.LastInitializationError);
        _renderSession.Invalidate(RenderInvalidationKinds.Scene | RenderInvalidationKinds.Overlay);
    }

    public async Task<VideraSnapshotExportResult> ExportSnapshotAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var startedAt = Stopwatch.StartNew();
        var exportSize = ResolveSnapshotExportSize();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var overlayState = _sessionBridge.CreateOverlayState(
                _selectionState,
                _annotations,
                _measurements,
                new System.Numerics.Vector2(exportSize.Width, exportSize.Height));
            await VideraSnapshotExportService.ExportAsync(
                path,
                exportSize.Width,
                exportSize.Height,
                _engine,
                _sceneCoordinator.SceneObjects,
                _selectionState,
                _annotations,
                _measurements,
                overlayState,
                ResolveSnapshotReadbackBackend(exportSize),
                _logger,
                cancellationToken).ConfigureAwait(true);
            _lastSnapshotExportPath = path;
            _lastSnapshotExportStatus = "Succeeded";
            RefreshInspectionDiagnostics();
            RefreshBackendDiagnostics(_backendDiagnostics.LastInitializationError);
            return VideraSnapshotExportResult.Success(path, exportSize.Width, exportSize.Height, startedAt.Elapsed);
        }
        catch (OperationCanceledException)
        {
            _lastSnapshotExportPath = path;
            _lastSnapshotExportStatus = "Cancelled";
            RefreshInspectionDiagnostics();
            RefreshBackendDiagnostics(_backendDiagnostics.LastInitializationError);
            throw;
        }
        catch (Exception ex)
        {
            _lastSnapshotExportPath = path;
            _lastSnapshotExportStatus = $"Failed: {ex.Message}";
            RefreshInspectionDiagnostics();
            RefreshBackendDiagnostics(_backendDiagnostics.LastInitializationError);
            return VideraSnapshotExportResult.Failed(path, exportSize.Width, exportSize.Height, startedAt.Elapsed, ex);
        }
    }

    private static IReadOnlyList<VideraMeasurement> CloneMeasurements(IReadOnlyList<VideraMeasurement>? measurements)
    {
        return measurements?.Select(static measurement => new VideraMeasurement
        {
            Id = measurement.Id,
            Label = measurement.Label,
            IsVisible = measurement.IsVisible,
            Start = measurement.Start,
            End = measurement.End
        }).ToArray() ?? Array.Empty<VideraMeasurement>();
    }

    private (uint Width, uint Height) ResolveSnapshotExportSize()
    {
        var overlayViewport = CreateOverlayViewportSize();
        if (overlayViewport.X > 0f && overlayViewport.Y > 0f)
        {
            return ((uint)Math.Max(1, MathF.Round(overlayViewport.X)), (uint)Math.Max(1, MathF.Round(overlayViewport.Y)));
        }

        var snapshot = _renderSession.OrchestrationSnapshot;
        if (snapshot.Inputs.Width > 0 && snapshot.Inputs.Height > 0)
        {
            return (snapshot.Inputs.Width, snapshot.Inputs.Height);
        }

        return (512u, 512u);
    }

    private ISoftwareBackend? ResolveSnapshotReadbackBackend((uint Width, uint Height) exportSize)
    {
        if (!_renderSession.IsReady || !_renderSession.IsSoftwareBackend)
        {
            return null;
        }

        var snapshot = _renderSession.OrchestrationSnapshot;
        if (snapshot.Inputs.Width != exportSize.Width || snapshot.Inputs.Height != exportSize.Height)
        {
            return null;
        }

        return _renderSession.SoftwareBackend;
    }
}
