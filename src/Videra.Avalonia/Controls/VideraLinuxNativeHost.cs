using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Videra.Avalonia.Controls.Linux;

namespace Videra.Avalonia.Controls;

internal sealed partial class VideraLinuxNativeHost : NativeControlHost, IVideraNativeHost
{
    private readonly LinuxNativeHostFactory _nativeHostFactory;
    private ILinuxPlatformNativeHost? _selectedHost;
    private LinuxNativeHostSelectionResult? _selection;
    private bool _isDisposed;

    public event Action<IntPtr>? HandleCreated;
    public event Action? HandleDestroyed;
    public event Action<NativePointerEvent>? NativePointer;

    public string? ResolvedDisplayServer { get; private set; }

    public bool DisplayServerFallbackUsed { get; private set; }

    public string? DisplayServerFallbackReason { get; private set; }

    public VideraLinuxNativeHost()
        : this(new LinuxNativeHostFactory())
    {
    }

    internal VideraLinuxNativeHost(LinuxNativeHostFactory nativeHostFactory)
    {
        _nativeHostFactory = nativeHostFactory ?? throw new ArgumentNullException(nameof(nativeHostFactory));
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (!OperatingSystem.IsLinux())
            throw new PlatformNotSupportedException("Linux native host is only supported on Linux.");

        _isDisposed = false;
        _selection = _nativeHostFactory.CreateHost();
        _selectedHost = _selection.Host;
        ResolvedDisplayServer = _selection.ResolvedDisplayServer;
        DisplayServerFallbackUsed = _selection.FallbackUsed;
        DisplayServerFallbackReason = _selection.FallbackReason;

        var handle = _selectedHost.Create(parent, Bounds.Size, VisualRoot?.RenderScaling ?? 1.0);
        HandleCreated?.Invoke(handle.Handle);

        return handle;
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        if (_selectedHost != null)
        {
            var selectedHost = _selectedHost;
            _selectedHost = null;
            HandleDestroyed?.Invoke();
            selectedHost.Destroy();
        }

        _selection = null;
        ResolvedDisplayServer = null;
        DisplayServerFallbackUsed = false;
        DisplayServerFallbackReason = null;

        base.DestroyNativeControlCore(control);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateNativeSize();
    }

    private void UpdateNativeSize()
    {
        if (_selectedHost == null)
            return;

        _selectedHost.Resize(Bounds.Size, VisualRoot?.RenderScaling ?? 1.0);
    }
}
