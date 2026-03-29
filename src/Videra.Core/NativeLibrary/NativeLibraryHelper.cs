using System.Reflection;
using System.Runtime.InteropServices;

namespace Videra.Core.NativeLibrary;

/// <summary>
/// Centralized helper for loading native libraries with fallback paths.
/// Uses .NET Runtime's NativeLibrary API for cross-platform resolution.
/// </summary>
public static class NativeLibraryHelper
{
    /// <summary>
    /// Try loading a native library from multiple candidate names, in priority order.
    /// Uses <see cref="System.Runtime.InteropServices.NativeLibrary.TryLoad(string, out IntPtr)"/>
    /// which handles platform-specific suffix and extension conventions.
    /// </summary>
    /// <param name="candidateNames">Library names to try, in priority order (e.g., "libX11.so.6", "libX11.so", "libX11").</param>
    /// <param name="handle">The loaded library handle, or <see cref="IntPtr.Zero"/> if all candidates fail.</param>
    /// <returns><c>true</c> if a library was loaded successfully.</returns>
    public static bool TryLoadWithFallback(string[] candidateNames, out IntPtr handle)
    {
        foreach (var name in candidateNames)
        {
            if (System.Runtime.InteropServices.NativeLibrary.TryLoad(name, out handle))
                return true;
        }
        handle = IntPtr.Zero;
        return false;
    }

    /// <summary>
    /// Get an exported symbol (function pointer) from a loaded native library.
    /// </summary>
    /// <param name="library">Library handle from <see cref="TryLoadWithFallback"/>.</param>
    /// <param name="symbolName">The exported symbol name (e.g., "XOpenDisplay").</param>
    /// <param name="symbol">The symbol pointer, or <see cref="IntPtr.Zero"/> if not found.</param>
    /// <returns><c>true</c> if the symbol was found.</returns>
    public static bool TryGetSymbol(IntPtr library, string symbolName, out IntPtr symbol)
    {
        return System.Runtime.InteropServices.NativeLibrary.TryGetExport(library, symbolName, out symbol);
    }

    /// <summary>
    /// Register a <see cref="DllImportResolver"/> for the calling assembly that provides
    /// fallback library names when the primary name cannot be resolved.
    /// </summary>
    /// <param name="primaryName">
    /// The DllImport library name to intercept (e.g., "libX11.so.6").
    /// This must match exactly what appears in <c>[DllImport("...")]</c> attributes.
    /// </param>
    /// <param name="fallbackNames">
    /// Additional names to try, in order, if the primary name fails.
    /// The primary name is always tried first.
    /// </param>
    public static void RegisterDllImportResolver(string primaryName, params string[] fallbackNames)
    {
        var allNames = new[] { primaryName }.Concat(fallbackNames).ToArray();
        var callingAssembly = Assembly.GetCallingAssembly();

        System.Runtime.InteropServices.NativeLibrary.SetDllImportResolver(
            callingAssembly,
            Resolver);

        IntPtr Resolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName == primaryName)
            {
                foreach (var candidate in allNames)
                {
                    if (System.Runtime.InteropServices.NativeLibrary.TryLoad(candidate, assembly, searchPath, out var handle))
                        return handle;
                }
            }

            // Return IntPtr.Zero to let the default resolver handle everything else
            return IntPtr.Zero;
        }
    }
}
