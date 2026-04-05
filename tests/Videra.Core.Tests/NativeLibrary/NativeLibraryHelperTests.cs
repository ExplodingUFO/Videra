using System.Runtime.InteropServices;
using Videra.Core.NativeLibrary;
using Xunit;

namespace Videra.Core.Tests.NativeLibrary;

public class NativeLibraryHelperTests
{
    private static readonly string[] LeadingInvalidCandidate = ["nonexistent_library_xyz.so"];
    private static readonly string[] InvalidLibraryCandidates = ["nonexistent_a.so", "nonexistent_b.so", "nonexistent_c.so"];

    private static string[] GetKnownLibraryCandidates()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new[] { "kernel32.dll", "ntdll.dll" };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return new[] { "libSystem.B.dylib", "libc.dylib" };
        }

        return new[] { "libc.so.6", "libc.so" };
    }

    private static string GetKnownExportName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "GetCurrentProcessId";
        }

        return "getpid";
    }

    [Fact]
    public void TryLoadWithFallback_SucceedsWithValidLibrary()
    {
        var candidates = LeadingInvalidCandidate
            .Concat(GetKnownLibraryCandidates())
            .ToArray();
        var result = NativeLibraryHelper.TryLoadWithFallback(candidates, out var handle);

        Assert.True(result);
        Assert.NotEqual(IntPtr.Zero, handle);
    }

    [Fact]
    public void TryLoadWithFallback_TriesNamesInOrder()
    {
        var candidates = GetKnownLibraryCandidates();
        var result = NativeLibraryHelper.TryLoadWithFallback(candidates, out var handle);

        Assert.True(result);
        Assert.NotEqual(IntPtr.Zero, handle);
    }

    [Fact]
    public void TryLoadWithFallback_ReturnsFalseForAllInvalid()
    {
        var result = NativeLibraryHelper.TryLoadWithFallback(InvalidLibraryCandidates, out var handle);

        Assert.False(result);
        Assert.Equal(IntPtr.Zero, handle);
    }

    [Fact]
    public void TryGetSymbol_FindsKnownExport()
    {
        var loadResult = NativeLibraryHelper.TryLoadWithFallback(GetKnownLibraryCandidates(), out var handle);
        Assert.True(loadResult);

        var result = NativeLibraryHelper.TryGetSymbol(handle, GetKnownExportName(), out var symbol);

        Assert.True(result);
        Assert.NotEqual(IntPtr.Zero, symbol);
    }

    [Fact]
    public void TryGetSymbol_ReturnsFalseForMissingExport()
    {
        var loadResult = NativeLibraryHelper.TryLoadWithFallback(GetKnownLibraryCandidates(), out var handle);
        Assert.True(loadResult);

        var result = NativeLibraryHelper.TryGetSymbol(handle, "CompletelyNonexistentFunction_XYZ", out var symbol);

        Assert.False(result);
        Assert.Equal(IntPtr.Zero, symbol);
    }
}
