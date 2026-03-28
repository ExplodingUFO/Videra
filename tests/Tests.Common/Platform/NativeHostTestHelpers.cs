using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Tests.Common.Platform;

public static class NativeHostTestHelpers
{
    public static Win32TestWindow CreateHiddenWin32Window(int width = 64, int height = 64)
        => new(width, height);

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
            var windowClass = new WNDCLASSW
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
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASSW
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

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr GetModuleHandleW(IntPtr lpModuleName);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern ushort RegisterClassW([In] ref WNDCLASSW lpWndClass);

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

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
}
