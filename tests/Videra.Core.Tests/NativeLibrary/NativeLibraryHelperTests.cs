using System.Runtime.InteropServices;
using Videra.Core.NativeLibrary;
using Xunit;

namespace Videra.Core.Tests.NativeLibrary;

public class NativeLibraryHelperTests
{
    [Fact]
    public void TryLoadWithFallback_SucceedsWithValidLibrary()
    {
        // On Windows, kernel32.dll is always available
        var candidates = new[] { "nonexistent_library_xyz.so", "kernel32.dll" };
        var result = NativeLibraryHelper.TryLoadWithFallback(candidates, out var handle);

        Assert.True(result);
        Assert.NotEqual(IntPtr.Zero, handle);
    }

    [Fact]
    public void TryLoadWithFallback_TriesNamesInOrder()
    {
        // First candidate is valid, should succeed immediately
        var candidates = new[] { "kernel32.dll", "ntdll.dll" };
        NativeLibraryHelper.TryLoadWithFallback(candidates, out var handle);

        Assert.NotEqual(IntPtr.Zero, handle);
    }

    [Fact]
    public void TryLoadWithFallback_ReturnsFalseForAllInvalid()
    {
        var candidates = new[] { "nonexistent_a.so", "nonexistent_b.so", "nonexistent_c.so" };
        var result = NativeLibraryHelper.TryLoadWithFallback(candidates, out var handle);

        Assert.False(result);
        Assert.Equal(IntPtr.Zero, handle);
    }

    [Fact]
    public void TryGetSymbol_FindsKnownExport()
    {
        // Load a known library and find a known export
        NativeLibraryHelper.TryLoadWithFallback(new[] { "kernel32.dll" }, out var handle);
        var result = NativeLibraryHelper.TryGetSymbol(handle, "GetVersion", out var symbol);

        Assert.True(result);
        Assert.NotEqual(IntPtr.Zero, symbol);
    }

    [Fact]
    public void TryGetSymbol_ReturnsFalseForMissingExport()
    {
        NativeLibraryHelper.TryLoadWithFallback(new[] { "kernel32.dll" }, out var handle);
        var result = NativeLibraryHelper.TryGetSymbol(handle, "CompletelyNonexistentFunction_XYZ", out var symbol);

        Assert.False(result);
        Assert.Equal(IntPtr.Zero, symbol);
    }
}
