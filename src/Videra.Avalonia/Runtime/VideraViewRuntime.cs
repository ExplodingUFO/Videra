using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Controls.Interaction;
using Videra.Avalonia.Rendering;
using Videra.Avalonia.Runtime.Scene;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Inspection;
using Videra.Core.Scene;

namespace Videra.Avalonia.Runtime;

internal sealed partial class VideraViewRuntime : IDisposable
{
    private readonly VideraView _owner;
    private readonly VideraEngine _engine;
    private readonly ILogger _logger;
    private readonly Func<uint, uint, WriteableBitmap?>? _bitmapFactory;

    private RenderSession _renderSession;
    private VideraViewSessionBridge _sessionBridge;
    private VideraViewOptions _options = new();
    private VideraBackendDiagnostics _backendDiagnostics;
    private SceneDocument _sceneDocument = SceneDocument.Empty;
    private readonly SceneRuntimeCoordinator _sceneCoordinator;
    private SceneResidencyDiagnostics _sceneDiagnostics;
    private InspectionWorkflowDiagnostics _inspectionDiagnostics = InspectionWorkflowDiagnostics.Empty;
    private SceneUploadFlushResult _lastSceneUploadFlushResult = SceneUploadFlushResult.Empty;
    private SceneUploadBudget _lastResolvedSceneUploadBudget = SceneUploadBudget.None;
    private RuntimeFramePrelude _framePrelude;
    private VideraSelectionState _selectionState = new();
    private IReadOnlyList<VideraAnnotation> _annotations = Array.Empty<VideraAnnotation>();
    private IReadOnlyList<VideraMeasurement> _measurements = Array.Empty<VideraMeasurement>();
    private VideraViewOverlayState _overlayState = VideraViewOverlayState.Empty;
    private VideraInteractionOptions _interactionOptions = new();
    private VideraInteractionController _interactionController;
    private VideraInteractionRouter _interactionRouter;

    public VideraViewRuntime(
        VideraView owner,
        VideraEngine engine,
        ILogger logger,
        Func<uint, uint, WriteableBitmap?>? bitmapFactory)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _bitmapFactory = bitmapFactory;
        _sceneCoordinator = new SceneRuntimeCoordinator(
            _engine,
            refreshOverlay: SynchronizeOverlayPresentation,
            refreshSceneDiagnostics: RefreshSceneDiagnostics,
            refreshBackendDiagnostics: RefreshSceneBackendDiagnosticsFromCoordinator,
            invalidateRender: InvalidateSceneRenderFromCoordinator);
        _interactionController = new VideraInteractionController(_owner, _logger);
        _interactionRouter = new VideraInteractionRouter(_owner, _interactionController);
        _renderSession = CreateRenderSession();
        _sessionBridge = CreateSessionBridge();
        _framePrelude = CreateFramePrelude();
        SubscribeToOptions(_options);
        RefreshSceneDiagnostics();
        RefreshInspectionDiagnostics();
        _backendDiagnostics = _sessionBridge.CreateDiagnosticsSnapshot(lastInitializationError: null, _sceneDiagnostics, _inspectionDiagnostics);
    }

    public RenderSession RenderSession => _renderSession;

    public VideraBackendDiagnostics BackendDiagnostics => _backendDiagnostics;

    public VideraViewOptions Options
    {
        get => _options;
        set
        {
            UnsubscribeFromOptions(_options);
            _options = value ?? new VideraViewOptions();
            SubscribeToOptions(_options);
            RefreshBackendDiagnostics(_backendDiagnostics.LastInitializationError);
            SynchronizeSessionFromCurrentBounds(retryCount: 0, useBackendChangePath: true);
        }
    }

    public VideraSelectionState SelectionState
    {
        get => _selectionState;
        set
        {
            _selectionState = value ?? new VideraSelectionState();
            SynchronizeOverlayState();
            _renderSession.Invalidate(RenderInvalidationKinds.Overlay);
        }
    }

    public IReadOnlyList<VideraAnnotation> Annotations
    {
        get => _annotations;
        set
        {
            _annotations = value ?? Array.Empty<VideraAnnotation>();
            SynchronizeOverlayState();
            _renderSession.Invalidate(RenderInvalidationKinds.Overlay);
        }
    }

    public IReadOnlyList<VideraMeasurement> Measurements
    {
        get => _measurements;
        set
        {
            _measurements = value ?? Array.Empty<VideraMeasurement>();
            SynchronizeOverlayState();
            RefreshInspectionDiagnostics();
            RefreshBackendDiagnostics(_backendDiagnostics.LastInitializationError);
            _renderSession.Invalidate(RenderInvalidationKinds.Overlay);
        }
    }

    public IReadOnlyList<VideraClipPlane> ClippingPlanes
    {
        get => _sceneCoordinator.ClippingPlanes;
        set
        {
            _sceneCoordinator.UpdateClippingPlanes(value);
            RefreshInspectionDiagnostics();
            RefreshBackendDiagnostics(_backendDiagnostics.LastInitializationError);
        }
    }

    public VideraInteractionOptions InteractionOptions
    {
        get => _interactionOptions;
        set => _interactionOptions = value ?? new VideraInteractionOptions();
    }

    public VideraViewOverlayState OverlayState => _overlayState;

    internal IReadOnlyList<Object3D> SceneObjects => _sceneCoordinator.SceneObjects;

    public void Dispose()
    {
        UnsubscribeFromOptions(_options);
        _interactionRouter.Dispose();
        _interactionController.Dispose();
        _renderSession.Dispose();
    }

    internal RenderSession CreateRenderSession()
    {
        var session = new RenderSession(
            _engine,
            beforeRender: OnBeforeRender,
            requestRender: OnRenderSessionFrameRequested,
            logger: _logger,
            renderLoopFactory: static () => new RenderSession.DispatcherRenderLoopDriver(),
            bitmapFactory: _bitmapFactory);
        session.BackendReady += OnRenderSessionBackendReady;
        return session;
    }

    internal RuntimeFramePrelude CreateFramePrelude()
    {
        return new RuntimeFramePrelude(
            _sceneCoordinator.UploadQueue,
            _sceneCoordinator.ResidencyRegistry,
            new SceneEngineApplicator(),
            _engine,
            () => _renderSession.ResourceFactory,
            () => _renderSession.HasInteractiveLease,
            () => _sceneCoordinator.ResourceEpoch,
            PushOverlayRenderState,
            _logger);
    }

    internal VideraViewSessionBridge CreateSessionBridge()
    {
        return new VideraViewSessionBridge(
            _renderSession,
            isPreferredBackendOverrideSet: _owner.IsPreferredBackendOverrideSetForRuntime,
            preferredBackendValue: () => _owner.PreferredBackend,
            backendOptionsAccessor: () => _options.Backend,
            diagnosticsOptionsAccessor: () => _options.Diagnostics);
    }

    internal void OnPreferredBackendChanged()
    {
        RefreshBackendDiagnostics(_backendDiagnostics.LastInitializationError);
        SynchronizeSessionFromCurrentBounds(retryCount: 0, useBackendChangePath: true);
    }

    internal bool WantsNativeBackend() => _sessionBridge.WantsNativeBackend();

    internal void OnViewAttached()
    {
        VideraView.Log.ViewAttached(_logger);
        var becameReady = _sessionBridge.OnViewAttached();
        RefreshBackendDiagnostics(lastInitializationError: null);

        if (becameReady)
        {
            ApplyViewState();
        }

        if (_sessionBridge.WantsNativeBackend())
        {
            _owner.EnsureNativeHostForRuntime();
        }
        else
        {
            _owner.ReleaseNativeHostForRuntime();
        }

        if (_owner.Bounds.Width <= 0 || _owner.Bounds.Height <= 0)
        {
            return;
        }

        var scaling = _owner.ResolveRenderScalingForRuntime();
        var widthPx = (uint)Math.Max(64, Math.Round(_owner.Bounds.Width * scaling));
        var heightPx = (uint)Math.Max(64, Math.Round(_owner.Bounds.Height * scaling));
        SynchronizeSession(widthPx, heightPx, retryCount: 0, useBackendChangePath: false);
    }

    internal void OnViewDetached()
    {
        VideraView.Log.ViewDetached(_logger);
        if (_owner.Items is INotifyCollectionChanged incc)
        {
            incc.CollectionChanged -= OnCollectionChanged;
        }

        _owner.ReleaseNativeHostForRuntime();
        _sessionBridge.OnViewDetached();
        _renderSession = CreateRenderSession();
        _sessionBridge = CreateSessionBridge();
        _framePrelude = CreateFramePrelude();
        RefreshBackendDiagnostics(lastInitializationError: null);
    }

    internal void SynchronizeSessionFromCurrentBounds(int retryCount, bool useBackendChangePath)
    {
        if (_owner.Bounds.Width <= 0 || _owner.Bounds.Height <= 0)
        {
            return;
        }

        var scaling = _owner.ResolveRenderScalingForRuntime();
        var widthPx = (uint)Math.Max(64, Math.Round(_owner.Bounds.Width * scaling));
        var heightPx = (uint)Math.Max(64, Math.Round(_owner.Bounds.Height * scaling));
        SynchronizeSession(widthPx, heightPx, retryCount, useBackendChangePath);
    }

    internal void SynchronizeSession(uint widthPx, uint heightPx, int retryCount, bool useBackendChangePath)
    {
        if (widthPx == 0 || heightPx == 0)
        {
            return;
        }

        try
        {
            var scaling = (float)_owner.ResolveRenderScalingForRuntime();
            var becameReady = useBackendChangePath
                ? _sessionBridge.OnBackendOptionsChanged(widthPx, heightPx, scaling)
                : _sessionBridge.OnSizeChanged(widthPx, heightPx, scaling);
            RefreshBackendDiagnostics(lastInitializationError: null);

            if (becameReady)
            {
                ApplyViewState();
            }

            SynchronizeOverlayPresentation();
        }
        catch (Exception ex)
        {
            VideraView.Log.RenderSessionInitFailed(_logger, retryCount + 1, ex.Message, ex);
            RefreshBackendDiagnostics(ex.Message);
            _owner.RaiseInitializationFailedFromRuntime(_backendDiagnostics, ex);

            if (retryCount < 5)
            {
                Task.Delay(100).ContinueWith(_ =>
                {
                    Dispatcher.UIThread.Post(() => SynchronizeSession(widthPx, heightPx, retryCount + 1, useBackendChangePath));
                });
            }
        }
    }

    internal void ApplyViewState()
    {
        var color = _owner.BackgroundColor;
        _engine.BackgroundColor = new RgbaFloat(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        _engine.Camera.InvertX = _owner.CameraInvertX;
        _engine.Camera.InvertY = _owner.CameraInvertY;
        ApplyGridSettings();
        if (_owner.Items is not null)
        {
            UpdateItemsSubscription(_owner.Items, _owner.Items);
        }
        else
        {
            RefreshSceneDiagnostics();
        }

        SynchronizeOverlayState();
    }

    internal void ApplyGridSettings()
    {
        _engine.Grid.IsVisible = _owner.IsGridVisible;
        _engine.Grid.Height = _owner.GridHeight;
        var c = _owner.GridColor;
        _engine.Grid.GridColor = new RgbaFloat(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        _engine.Grid.Rebuild(GetResourceFactory());
        _renderSession.Invalidate(RenderInvalidationKinds.Style);
    }

    internal void OnNativeHandleCreated(IntPtr handle)
    {
        var diagnostics = _owner.GetNativeHostDisplayServerDiagnosticsForRuntime();
        VideraView.Log.NativeHandleCreated(_logger, handle.ToInt64());

        var scaling = _owner.ResolveRenderScalingForRuntime();
        var widthPx = (uint)Math.Max(64, Math.Round(_owner.Bounds.Width * scaling));
        var heightPx = (uint)Math.Max(64, Math.Round(_owner.Bounds.Height * scaling));
        var becameReady = _sessionBridge.OnNativeHandleCreated(
            handle,
            diagnostics.ResolvedDisplayServer,
            diagnostics.FallbackUsed,
            diagnostics.FallbackReason,
            widthPx,
            heightPx,
            (float)scaling);
        RefreshBackendDiagnostics(lastInitializationError: null);
        if (becameReady)
        {
            ApplyViewState();
        }
    }

    internal void OnNativeHandleDestroyed()
    {
        _sessionBridge.OnNativeHandleDestroyed();
        RefreshBackendDiagnostics(_backendDiagnostics.LastInitializationError);
    }

    internal void RefreshBackendDiagnostics(string? lastInitializationError)
    {
        RefreshSceneDiagnostics();
        RefreshInspectionDiagnostics();
        var next = _sessionBridge.CreateDiagnosticsSnapshot(lastInitializationError, _sceneDiagnostics, _inspectionDiagnostics);
        _backendDiagnostics = next;
        _owner.PublishBackendDiagnosticsFromRuntime(next);
    }

    internal void RefreshSceneDiagnostics()
    {
        _sceneDocument = _sceneCoordinator.CurrentDocument;
        _sceneDiagnostics = _sceneCoordinator.CreateDiagnostics(
            _lastSceneUploadFlushResult,
            _lastResolvedSceneUploadBudget);
    }

    internal void RefreshInspectionDiagnostics()
    {
        _inspectionDiagnostics = new InspectionWorkflowDiagnostics(
            _sceneCoordinator.ClippingPlanes.Count > 0,
            _sceneCoordinator.ClippingPlanes.Count,
            MeasurementCount: _measurements.Count,
            LastSnapshotExportPath: _lastSnapshotExportPath,
            LastSnapshotExportStatus: _lastSnapshotExportStatus);
    }

    private void OnBeforeRender()
    {
        var flushResult = _framePrelude.Execute();
        if (flushResult.ResolvedBudget.MaxObjectsPerFrame > 0 &&
            flushResult.ResolvedBudget.MaxBytesPerFrame > 0)
        {
            _lastResolvedSceneUploadBudget = flushResult.ResolvedBudget;
        }

        if (flushResult.HasChanges)
        {
            _lastSceneUploadFlushResult = flushResult;
        }

        RefreshSceneDiagnostics();
        RefreshBackendDiagnostics(_backendDiagnostics.LastInitializationError);
    }

    private void RefreshSceneBackendDiagnosticsFromCoordinator()
    {
        RefreshBackendDiagnostics(_backendDiagnostics.LastInitializationError);
    }

    private void InvalidateSceneRenderFromCoordinator(RenderInvalidationKinds flags)
    {
        _renderSession.Invalidate(flags);
    }

    private void SubscribeToOptions(VideraViewOptions options)
    {
        options.PropertyChanged += OnOptionsPropertyChanged;
        options.Backend.PropertyChanged += OnBackendOptionsPropertyChanged;
        options.Diagnostics.PropertyChanged += OnDiagnosticsOptionsPropertyChanged;
    }

    private void UnsubscribeFromOptions(VideraViewOptions options)
    {
        options.PropertyChanged -= OnOptionsPropertyChanged;
        options.Backend.PropertyChanged -= OnBackendOptionsPropertyChanged;
        options.Diagnostics.PropertyChanged -= OnDiagnosticsOptionsPropertyChanged;
    }

    private void OnOptionsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not VideraViewOptions options)
        {
            return;
        }

        if (e.PropertyName == nameof(VideraViewOptions.Backend))
        {
            RefreshBackendDiagnostics(_backendDiagnostics.LastInitializationError);
            SynchronizeSessionFromCurrentBounds(retryCount: 0, useBackendChangePath: true);
        }
        else if (e.PropertyName == nameof(VideraViewOptions.Diagnostics))
        {
            RefreshBackendDiagnostics(_backendDiagnostics.LastInitializationError);
        }

        options.Backend.PropertyChanged -= OnBackendOptionsPropertyChanged;
        options.Backend.PropertyChanged += OnBackendOptionsPropertyChanged;
        options.Diagnostics.PropertyChanged -= OnDiagnosticsOptionsPropertyChanged;
        options.Diagnostics.PropertyChanged += OnDiagnosticsOptionsPropertyChanged;
    }

    private void OnBackendOptionsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _ = sender;
        _ = e;
        RefreshBackendDiagnostics(_backendDiagnostics.LastInitializationError);
        SynchronizeSessionFromCurrentBounds(retryCount: 0, useBackendChangePath: true);
    }

    private void OnDiagnosticsOptionsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _ = sender;
        _ = e;
        RefreshBackendDiagnostics(_backendDiagnostics.LastInitializationError);
    }
}
