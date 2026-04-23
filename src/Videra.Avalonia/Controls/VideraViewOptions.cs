using System.ComponentModel;
using System.Runtime.CompilerServices;
using Videra.Core.Graphics;
using Videra.Core.Scene;

namespace Videra.Avalonia.Controls;

public sealed class VideraViewOptions : INotifyPropertyChanged
{
    private VideraBackendOptions _backend = new();
    private VideraDiagnosticsOptions _diagnostics = new();
    private Func<string, ImportedSceneAsset>? _modelImporter;

    public event PropertyChangedEventHandler? PropertyChanged;

    public VideraBackendOptions Backend
    {
        get => _backend;
        set => SetField(ref _backend, value);
    }

    public VideraDiagnosticsOptions Diagnostics
    {
        get => _diagnostics;
        set => SetField(ref _diagnostics, value);
    }

    public Func<string, ImportedSceneAsset>? ModelImporter
    {
        get => _modelImporter;
        set => SetField(ref _modelImporter, value);
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed class VideraBackendOptions : INotifyPropertyChanged
{
    private GraphicsBackendPreference _preferredBackend = GraphicsBackendPreference.Auto;
    private BackendEnvironmentOverrideMode _environmentOverrideMode = BackendEnvironmentOverrideMode.Disabled;
    private bool _allowSoftwareFallback = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    public GraphicsBackendPreference PreferredBackend
    {
        get => _preferredBackend;
        set => SetField(ref _preferredBackend, value);
    }

    public BackendEnvironmentOverrideMode EnvironmentOverrideMode
    {
        get => _environmentOverrideMode;
        set => SetField(ref _environmentOverrideMode, value);
    }

    public bool AllowSoftwareFallback
    {
        get => _allowSoftwareFallback;
        set => SetField(ref _allowSoftwareFallback, value);
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed class VideraDiagnosticsOptions : INotifyPropertyChanged
{
    private VideraRenderLoopMode _renderLoopMode = VideraRenderLoopMode.Dispatcher;

    public event PropertyChangedEventHandler? PropertyChanged;

    public VideraRenderLoopMode RenderLoopMode
    {
        get => _renderLoopMode;
        set => SetField(ref _renderLoopMode, value);
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public enum VideraRenderLoopMode
{
    Dispatcher
}
