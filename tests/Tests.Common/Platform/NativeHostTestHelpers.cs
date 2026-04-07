using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Tests.Common.Platform;

public static class NativeHostTestHelpers
{
    public static Win32TestWindow CreateHiddenWin32Window(int width = 64, int height = 64)
        => new(width, height);

    public static bool CanOpenX11Display()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISPLAY")))
        {
            return false;
        }

        var display = XOpenDisplayProbe(IntPtr.Zero);
        if (display == IntPtr.Zero)
        {
            return false;
        }

        XCloseDisplayProbe(display);
        return true;
    }

    public sealed class Win32TestWindow : IDisposable
    {
        private static readonly WndProcDelegate WndProcHandler = DefWindowProcW;
        private static readonly string ClassName = "Videra.Tests.HiddenWindow";
        private static ushort _classAtom;
        private IntPtr _hwnd;

        public IntPtr Handle => _hwnd;

        public Win32TestWindow(int width, int height)
        {
            EnsureWindowClass();

            _hwnd = CreateWindowExW(
                0,
                ClassName,
                string.Empty,
                WindowStyles.WS_OVERLAPPEDWINDOW,
                0,
                0,
                width,
                height,
                IntPtr.Zero,
                IntPtr.Zero,
                GetModuleHandleW(IntPtr.Zero),
                IntPtr.Zero);

            if (_hwnd == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to create hidden Win32 test window.");
            }

            ShowWindow(_hwnd, ShowWindowCommands.SW_HIDE);
            UpdateWindow(_hwnd);
        }

        public void Dispose()
        {
            if (_hwnd != IntPtr.Zero)
            {
                DestroyWindow(_hwnd);
                _hwnd = IntPtr.Zero;
            }
        }

        private static void EnsureWindowClass()
        {
            if (_classAtom != 0)
            {
                return;
            }

            var instance = GetModuleHandleW(IntPtr.Zero);
            var windowClass = new WindowClassW
            {
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(WndProcHandler),
                hInstance = instance,
                lpszClassName = ClassName
            };

            _classAtom = RegisterClassW(ref windowClass);
            if (_classAtom == 0)
            {
                var error = Marshal.GetLastWin32Error();
                const int AlreadyExists = 1410;
                if (error != AlreadyExists)
                {
                    throw new Win32Exception(error, "Failed to register Win32 test window class.");
                }
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetModuleHandleW(IntPtr lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern ushort RegisterClassW([In] ref WindowClassW lpWndClass);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateWindowExW(
            int dwExStyle,
            string lpClassName,
            string lpWindowName,
            int dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "DefWindowProcW")]
        private static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WindowClassW
    {
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string? lpszMenuName;
        public string lpszClassName;
    }

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private static class WindowStyles
    {
        public const int WS_OVERLAPPEDWINDOW = 0x00CF0000;
    }

    private static class ShowWindowCommands
    {
        public const int SW_HIDE = 0;
    }

    [DllImport("libX11.so.6", EntryPoint = "XOpenDisplay")]
    private static extern IntPtr XOpenDisplayProbe(IntPtr displayName);

    [DllImport("libX11.so.6", EntryPoint = "XCloseDisplay")]
    private static extern int XCloseDisplayProbe(IntPtr display);

    // --- Linux X11 native-host fixture ---

    /// <summary>
    /// Creates a hidden X11 window suitable for Vulkan backend initialization on Linux.
    /// Must be called on a Linux host with an X11 display server running.
    /// </summary>
    public static X11TestWindow CreateHiddenX11Window(int width = 64, int height = 64)
        => new(width, height);

    public sealed class X11TestWindow : IDisposable
    {
        private IntPtr _display;
        private IntPtr _window;

        /// <summary>
        /// Gets the X11 window handle suitable for VulkanBackend.Initialize.
        /// </summary>
        public IntPtr WindowHandle => _window;

        /// <summary>
        /// Gets the X11 display pointer.
        /// </summary>
        public IntPtr Display => _display;

        public X11TestWindow(int width, int height)
        {
            _display = XOpenDisplay(IntPtr.Zero);
            if (_display == IntPtr.Zero)
                throw new InvalidOperationException("Failed to open X11 display. Ensure an X11 display server is running.");

            var screen = XDefaultScreen(_display);
            _window = XCreateSimpleWindow(
                _display,
                XRootWindow(_display, screen),
                0, 0,
                (uint)width, (uint)height,
                1,
                IntPtr.Zero,
                IntPtr.Zero);

            if (_window == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create X11 test window.");

            XMapWindow(_display, _window);
            XFlush(_display);
        }

        public void Dispose()
        {
            if (_window != IntPtr.Zero && _display != IntPtr.Zero)
            {
                XDestroyWindow(_display, _window);
                _window = IntPtr.Zero;
            }

            if (_display != IntPtr.Zero)
            {
                XCloseDisplay(_display);
                _display = IntPtr.Zero;
            }
        }

        [DllImport("libX11.so.6", EntryPoint = "XOpenDisplay")]
        private static extern IntPtr XOpenDisplay(IntPtr displayName);

        [DllImport("libX11.so.6", EntryPoint = "XCloseDisplay")]
        private static extern int XCloseDisplay(IntPtr display);

        [DllImport("libX11.so.6", EntryPoint = "XDefaultScreen")]
        private static extern int XDefaultScreen(IntPtr display);

        [DllImport("libX11.so.6", EntryPoint = "XRootWindow")]
        private static extern IntPtr XRootWindow(IntPtr display, int screen);

        [DllImport("libX11.so.6", EntryPoint = "XCreateSimpleWindow")]
        private static extern IntPtr XCreateSimpleWindow(
            IntPtr display, IntPtr parent, int x, int y,
            uint width, uint height, uint borderWidth,
            IntPtr border, IntPtr background);

        [DllImport("libX11.so.6", EntryPoint = "XDestroyWindow")]
        private static extern int XDestroyWindow(IntPtr display, IntPtr window);

        [DllImport("libX11.so.6", EntryPoint = "XMapWindow")]
        private static extern int XMapWindow(IntPtr display, IntPtr window);

        [DllImport("libX11.so.6", EntryPoint = "XFlush")]
        private static extern int XFlush(IntPtr display);
    }

    // --- macOS NSView native-host fixture ---

    /// <summary>
    /// Creates an NSView-backed window suitable for Metal backend initialization on macOS.
    /// Must be called on a macOS host with the Metal framework available.
    /// Uses Objective-C runtime P/Invoke to create real Cocoa objects.
    /// </summary>
    public static NSViewTestWindow CreateHiddenNSViewWindow(int width = 64, int height = 64)
        => new(width, height);

    public sealed class NSViewTestWindow : IDisposable
    {
        private static IntPtr _appKitHandle;
        private IntPtr _nsView;

        [StructLayout(LayoutKind.Sequential)]
        private struct CGRect
        {
            public double X;
            public double Y;
            public double Width;
            public double Height;
        }

        /// <summary>
        /// Gets the NSView handle suitable for MetalBackend.Initialize.
        /// </summary>
        public IntPtr ViewHandle => _nsView;

        public NSViewTestWindow(int width, int height)
        {
            EnsureAppKitLoaded();

            var nsApplicationClass = objc_getClass("NSApplication");
            if (nsApplicationClass != IntPtr.Zero)
            {
                var sharedApplicationSel = sel_registerName("sharedApplication");
                objc_msgSend(nsApplicationClass, sharedApplicationSel);
            }

            var nsViewClass = objc_getClass("NSView");
            if (nsViewClass == IntPtr.Zero)
                throw new InvalidOperationException("Failed to get NSView class. Must run on macOS.");

            var allocSel = sel_registerName("alloc");
            var initSel = sel_registerName("initWithFrame:");
            _nsView = objc_msgSend_initWithFrame(
                objc_msgSend(nsViewClass, allocSel),
                initSel,
                new CGRect
                {
                    X = 0,
                    Y = 0,
                    Width = width,
                    Height = height
                });

            if (_nsView == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create NSView.");
        }

        private static void EnsureAppKitLoaded()
        {
            if (_appKitHandle != IntPtr.Zero)
            {
                return;
            }

            const string appKitPath = "/System/Library/Frameworks/AppKit.framework/AppKit";
            if (!NativeLibrary.TryLoad(appKitPath, out _appKitHandle))
            {
                throw new InvalidOperationException($"Failed to load AppKit framework from '{appKitPath}'.");
            }
        }

        public void Dispose()
        {
            if (_nsView != IntPtr.Zero)
            {
                var releaseSel = sel_registerName("release");
                objc_msgSend(_nsView, releaseSel);
                _nsView = IntPtr.Zero;
            }
        }

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_getClass")]
        private static extern IntPtr objc_getClass(string name);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName")]
        private static extern IntPtr sel_registerName(string name);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend_initWithFrame(
            IntPtr receiver,
            IntPtr selector,
            CGRect frame);
    }
}
