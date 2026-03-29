using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Videra.Core.Exceptions;
using Videra.Core.NativeLibrary;

namespace Videra.Platform.Linux;

/// <summary>
/// X11-specific Vulkan surface creation using VK_KHR_xlib_surface.
/// </summary>
internal unsafe class X11SurfaceCreator : ISurfaceCreator
{
    private IntPtr _x11Display;
    private KhrXlibSurface _khrXlibSurface;

    static X11SurfaceCreator()
    {
        NativeLibraryHelper.RegisterDllImportResolver("libX11.so.6", "libX11.so", "libX11");
    }

    public string RequiredExtensionName => "VK_KHR_xlib_surface";

    public SurfaceKHR CreateSurface(Vk vk, Instance instance, IntPtr windowHandle)
    {
        vk.TryGetInstanceExtension(instance, out _khrXlibSurface);

        if (windowHandle == IntPtr.Zero)
            throw new PlatformDependencyException(
                "Vulkan requires a valid X11 window handle.",
                "CreateSurface",
                "Linux");

        _x11Display = XOpenDisplay(IntPtr.Zero);
        if (_x11Display == IntPtr.Zero)
            throw new PlatformDependencyException(
                "Failed to open X11 display.",
                "CreateSurface",
                "Linux");

        var createInfo = new XlibSurfaceCreateInfoKHR
        {
            SType = StructureType.XlibSurfaceCreateInfoKhr,
            Dpy = (nint*)_x11Display,
            Window = (nint)windowHandle
        };

        var surface = new SurfaceKHR();
        if (_khrXlibSurface.CreateXlibSurface(instance, in createInfo, null, &surface) != Result.Success)
            throw new GraphicsInitializationException(
                "Failed to create X11 Vulkan surface.",
                "CreateSurface");

        return surface;
    }

    public void Cleanup()
    {
        if (_x11Display != IntPtr.Zero)
        {
            XCloseDisplay(_x11Display);
            _x11Display = IntPtr.Zero;
        }
    }

    [DllImport("libX11.so.6")]
    private static extern IntPtr XOpenDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern int XCloseDisplay(IntPtr display);
}
