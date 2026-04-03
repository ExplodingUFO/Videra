namespace Videra.Platform.Linux;

internal readonly record struct X11NativeHandle(IntPtr Display, IntPtr Window, bool OwnsDisplay)
{
    public bool IsValid => Display != IntPtr.Zero && Window != IntPtr.Zero;
}

internal static class X11NativeHandleRegistry
{
    private static readonly Dictionary<nint, IntPtr> BorrowedDisplays = new();
    private static readonly object Sync = new();

    public static void Register(IntPtr window, IntPtr display)
    {
        if (window == IntPtr.Zero || display == IntPtr.Zero)
        {
            return;
        }

        lock (Sync)
        {
            BorrowedDisplays[window] = display;
        }
    }

    public static void Unregister(IntPtr window)
    {
        if (window == IntPtr.Zero)
        {
            return;
        }

        lock (Sync)
        {
            BorrowedDisplays.Remove(window);
        }
    }

    public static X11NativeHandle Resolve(IntPtr window, Func<IntPtr> openDisplay)
    {
        if (window == IntPtr.Zero)
        {
            return default;
        }

        lock (Sync)
        {
            if (BorrowedDisplays.TryGetValue(window, out var display) && display != IntPtr.Zero)
            {
                return new X11NativeHandle(display, window, OwnsDisplay: false);
            }
        }

        var ownedDisplay = openDisplay();
        return new X11NativeHandle(ownedDisplay, window, OwnsDisplay: true);
    }
}
