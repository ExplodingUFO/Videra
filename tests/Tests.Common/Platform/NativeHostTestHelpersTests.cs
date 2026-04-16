using FluentAssertions;
using Xunit;

namespace Tests.Common.Platform;

public sealed class NativeHostTestHelpersTests
{
    [Fact]
    public void ProbeX11DisplayWithRetry_EventualSuccess_ReturnsTrueAndClosesDisplay()
    {
        var attempts = 0;
        var closedHandle = IntPtr.Zero;

        var result = NativeHostTestHelpers.ProbeX11DisplayWithRetry(
            openDisplay: () =>
            {
                attempts++;
                return attempts < 3 ? IntPtr.Zero : new IntPtr(1234);
            },
            closeDisplay: handle => closedHandle = handle,
            maxAttempts: 3,
            retryDelay: TimeSpan.Zero);

        result.Should().BeTrue();
        attempts.Should().Be(3);
        closedHandle.Should().Be(new IntPtr(1234));
    }
}
