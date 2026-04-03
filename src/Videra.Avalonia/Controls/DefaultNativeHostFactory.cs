namespace Videra.Avalonia.Controls;

internal sealed class DefaultNativeHostFactory : INativeHostFactory
{
    public IVideraNativeHost? CreateHost()
    {
        if (OperatingSystem.IsWindows())
        {
            return new VideraNativeHost();
        }

        if (OperatingSystem.IsLinux())
        {
            return new VideraLinuxNativeHost();
        }

        if (OperatingSystem.IsMacOS())
        {
            return new VideraMacOSNativeHost();
        }

        return null;
    }
}
