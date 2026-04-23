using System.Text;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Videra.Avalonia.Controls;
using Videra.Core.Graphics.RenderPipeline;
using Videra.Core.Graphics.RenderPipeline.Extensibility;
using Videra.ExtensibilitySample.Extensibility;
using Videra.Import.Obj;

namespace Videra.ExtensibilitySample.Views;

public partial class MainWindow : Window
{
    private readonly RecordingContributor _recordingContributor = new();
    private readonly TextBlock _loadStatusText;
    private readonly TextBlock _contributorStatusText;
    private readonly TextBlock _frameHookStatusText;
    private readonly TextBlock _capabilitiesStatusText;
    private readonly TextBlock _diagnosticsStatusText;

    private bool _sampleStarted;
    private string _loadSummary = "Waiting for backend readiness.";
    private string _frameHookSummary = "Waiting for the first FrameEnd hook.";
    private string _contributorSummary = "Waiting for the first SolidGeometry contributor call.";

    public MainWindow()
    {
        InitializeComponent();

        _loadStatusText = this.FindControl<TextBlock>("LoadStatusText")
            ?? throw new InvalidOperationException("Load status control is missing.");
        _contributorStatusText = this.FindControl<TextBlock>("ContributorStatusText")
            ?? throw new InvalidOperationException("Contributor status control is missing.");
        _frameHookStatusText = this.FindControl<TextBlock>("FrameHookStatusText")
            ?? throw new InvalidOperationException("Frame hook status control is missing.");
        _capabilitiesStatusText = this.FindControl<TextBlock>("CapabilitiesStatusText")
            ?? throw new InvalidOperationException("Capabilities status control is missing.");
        _diagnosticsStatusText = this.FindControl<TextBlock>("DiagnosticsStatusText")
            ?? throw new InvalidOperationException("Diagnostics status control is missing.");

        View3D.Options = new VideraViewOptions
        {
            ModelImporter = static path => ObjModelImporter.Import(path)
        };

        _recordingContributor.ObservationRecorded += OnContributorObservationRecorded;

        View3D.Engine.RegisterPassContributor(RenderPassSlot.SolidGeometry, _recordingContributor);
        View3D.Engine.RegisterFrameHook(RenderFrameHookPoint.FrameEnd, OnFrameEnd);

        View3D.BackendReady += OnBackendReady;
        View3D.BackendStatusChanged += OnBackendStatusChanged;
        View3D.InitializationFailed += OnInitializationFailed;

        Opened += OnOpened;
        Closed += OnClosed;

        UpdateStatusPanel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        _ = TryRunSampleAsync();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        Opened -= OnOpened;
        Closed -= OnClosed;
        View3D.BackendReady -= OnBackendReady;
        View3D.BackendStatusChanged -= OnBackendStatusChanged;
        View3D.InitializationFailed -= OnInitializationFailed;
        _recordingContributor.ObservationRecorded -= OnContributorObservationRecorded;
    }

    private void OnBackendReady(object? sender, EventArgs e)
    {
        _ = TryRunSampleAsync();
    }

    private void OnBackendStatusChanged(object? sender, VideraBackendStatusChangedEventArgs e)
    {
        _ = sender;
        _ = e;
        UpdateStatusPanel();
    }

    private void OnInitializationFailed(object? sender, VideraBackendFailureEventArgs e)
    {
        _ = sender;
        _loadSummary = $"Backend initialization failed: {e.Exception.Message}";
        UpdateStatusPanel();
    }

    private void OnContributorObservationRecorded(object? sender, RenderPassObservationEventArgs observation)
    {
        _ = sender;
        _contributorSummary = observation.ToString();
        _ = Dispatcher.UIThread.InvokeAsync(UpdateStatusPanel);
    }

    private void OnFrameEnd(RenderFrameHookContext context)
    {
        var stageNames = context.LastPipelineSnapshot?.StageNames;
        var stages = stageNames is { Count: > 0 }
            ? string.Join(", ", stageNames)
            : "Unavailable";
        var features = context.ActiveFeatures.ToFeatureNames();
        var featureSummary = features.Count > 0
            ? string.Join(", ", features)
            : "None";

        _frameHookSummary =
            $"HookPoint={context.HookPoint}; Backend={context.ActiveBackendPreference?.ToString() ?? "Unknown"}; " +
            $"Profile={context.LastPipelineSnapshot?.Profile.ToString() ?? "Unavailable"}; Features={featureSummary}; Stages={stages}";

        _ = Dispatcher.UIThread.InvokeAsync(UpdateStatusPanel);
    }

    private async Task TryRunSampleAsync()
    {
        if (_sampleStarted || !View3D.BackendDiagnostics.IsReady)
        {
            UpdateStatusPanel();
            return;
        }

        _sampleStarted = true;

        try
        {
            var loadResult = await View3D.LoadModelAsync("Assets/reference-cube.obj").ConfigureAwait(true);

            if (loadResult.Succeeded)
            {
                var framed = View3D.FrameAll();
                _loadSummary =
                    $"LoadModelAsync(\"Assets/reference-cube.obj\") succeeded in {loadResult.Duration.TotalMilliseconds:N0} ms. " +
                    $"FrameAll() returned {framed}.";
            }
            else
            {
                _loadSummary =
                    $"LoadModelAsync(\"Assets/reference-cube.obj\") failed after {loadResult.Duration.TotalMilliseconds:N0} ms: " +
                    $"{loadResult.Failure?.ErrorMessage ?? "Unknown failure."}";
            }

            View3D.InvalidateVisual();
        }
        catch (Exception ex)
        {
            _loadSummary = $"Sample flow failed: {ex.Message}";
        }
        finally
        {
            UpdateStatusPanel();
        }
    }

    private void UpdateStatusPanel()
    {
        _loadStatusText.Text = _loadSummary;
        _contributorStatusText.Text = _contributorSummary;
        _frameHookStatusText.Text = _frameHookSummary;
        _capabilitiesStatusText.Text = FormatCapabilities();
        _diagnosticsStatusText.Text = FormatDiagnostics();
    }

    private string FormatCapabilities()
    {
        var capabilities = View3D.RenderCapabilities;
        var snapshot = capabilities.LastPipelineSnapshot;
        var stages = snapshot?.StageNames is { Count: > 0 }
            ? string.Join(", ", snapshot.StageNames)
            : "Unavailable";
        var supportedFeatures = capabilities.SupportedFeatureNames.Count > 0
            ? string.Join(", ", capabilities.SupportedFeatureNames)
            : "None";
        var lastFrameFeatures = snapshot?.FeatureNames is { Count: > 0 }
            ? string.Join(", ", snapshot.FeatureNames)
            : "Unavailable";

        var builder = new StringBuilder();
        builder.AppendLine($"IsInitialized: {capabilities.IsInitialized}");
        builder.AppendLine($"ActiveBackendPreference: {capabilities.ActiveBackendPreference?.ToString() ?? "Unknown"}");
        builder.AppendLine($"SupportsPassContributors: {capabilities.SupportsPassContributors}");
        builder.AppendLine($"SupportsPassReplacement: {capabilities.SupportsPassReplacement}");
        builder.AppendLine($"SupportsFrameHooks: {capabilities.SupportsFrameHooks}");
        builder.AppendLine($"SupportsPipelineSnapshots: {capabilities.SupportsPipelineSnapshots}");
        builder.AppendLine($"SupportedFeatureNames: {supportedFeatures}");
        builder.AppendLine($"LastPipelineProfile: {snapshot?.Profile.ToString() ?? "Unavailable"}");
        builder.AppendLine($"LastFrameFeatureNames: {lastFrameFeatures}");
        builder.Append($"LastPipelineStages: {stages}");
        return builder.ToString();
    }

    private string FormatDiagnostics()
    {
        var diagnostics = View3D.BackendDiagnostics;
        var stages = diagnostics.LastFrameStageNames is { Count: > 0 }
            ? string.Join(", ", diagnostics.LastFrameStageNames)
            : "Unavailable";
        var lastFrameFeatures = diagnostics.LastFrameFeatureNames is { Count: > 0 }
            ? string.Join(", ", diagnostics.LastFrameFeatureNames)
            : "Unavailable";
        var supportedFeatures = diagnostics.SupportedRenderFeatureNames is { Count: > 0 }
            ? string.Join(", ", diagnostics.SupportedRenderFeatureNames)
            : "Unavailable";

        var builder = new StringBuilder();
        builder.AppendLine($"IsReady: {diagnostics.IsReady}");
        builder.AppendLine($"RequestedBackend: {diagnostics.RequestedBackend}");
        builder.AppendLine($"ResolvedBackend: {diagnostics.ResolvedBackend}");
        builder.AppendLine($"IsUsingSoftwareFallback: {diagnostics.IsUsingSoftwareFallback}");
        builder.AppendLine($"FallbackReason: {diagnostics.FallbackReason ?? "None"}");
        builder.AppendLine($"NativeHostBound: {diagnostics.NativeHostBound}");
        builder.AppendLine($"RenderPipelineProfile: {diagnostics.RenderPipelineProfile ?? "Unavailable"}");
        builder.AppendLine($"LastFrameStageNames: {stages}");
        builder.AppendLine($"LastFrameFeatureNames: {lastFrameFeatures}");
        builder.AppendLine($"LastFrameObjectCount: {diagnostics.LastFrameObjectCount} | LastFrameOpaqueObjectCount: {diagnostics.LastFrameOpaqueObjectCount} | LastFrameTransparentObjectCount: {diagnostics.LastFrameTransparentObjectCount}");
        builder.AppendLine($"SupportedRenderFeatureNames: {supportedFeatures}");
        builder.AppendLine($"UsesSoftwarePresentationCopy: {diagnostics.UsesSoftwarePresentationCopy}");
        builder.Append($"LastInitializationError: {diagnostics.LastInitializationError ?? "None"}");
        return builder.ToString();
    }
}
