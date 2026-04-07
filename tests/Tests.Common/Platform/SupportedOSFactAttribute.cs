using System.Runtime.InteropServices;
using Xunit;

namespace Tests.Common.Platform;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class WindowsFactAttribute : FactAttribute
{
    public WindowsFactAttribute()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Skip = "Requires Windows.";
        }
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class LinuxFactAttribute : FactAttribute
{
    public LinuxFactAttribute()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Skip = "Requires Linux.";
        }
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class LinuxNativeFactAttribute : FactAttribute
{
    public LinuxNativeFactAttribute()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Skip = "Requires Linux.";
            return;
        }

        if (!string.Equals(
                Environment.GetEnvironmentVariable("VIDERA_RUN_LINUX_NATIVE_TESTS"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            Skip = "Requires explicit Linux native validation.";
            return;
        }

        if (!NativeHostTestHelpers.CanOpenX11Display())
        {
            Skip = "Requires Linux with an accessible X11 display.";
        }
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class MacOSFactAttribute : FactAttribute
{
    public MacOSFactAttribute()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Skip = "Requires macOS.";
        }
    }
}
