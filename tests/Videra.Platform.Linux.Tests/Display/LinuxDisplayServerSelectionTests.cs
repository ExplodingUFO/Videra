using FluentAssertions;
using Xunit;

namespace Videra.Platform.Linux.Tests.Display;

public sealed class LinuxDisplayServerSelectionTests
{
    [Fact]
    public void LinuxDisplayServerKind_ShouldDefineWaylandXWaylandX11AndUnknown()
    {
        var kindType = GetLinuxType("LinuxDisplayServerKind");
        kindType.Should().NotBeNull("Linux diagnostics must expose a stable display-server enum.");
        kindType!.IsEnum.Should().BeTrue();

        Enum.GetNames(kindType).Should().Contain(new[] { "Unknown", "Wayland", "X11", "XWayland" });
    }

    [Fact]
    public void LinuxDisplayServerResolution_ShouldExposeResolvedDisplayServerAndFallbackMetadata()
    {
        var resolutionType = GetLinuxType("LinuxDisplayServerResolution");
        resolutionType.Should().NotBeNull("runtime selection must return a structured Linux display-server resolution.");

        resolutionType!.GetProperty("ResolvedDisplayServer").Should().NotBeNull();
        resolutionType.GetProperty("FallbackUsed").Should().NotBeNull();
        resolutionType.GetProperty("FailureReason").Should().NotBeNull();
    }

    [Fact]
    public void LinuxDisplayServerCandidate_ShouldExposeDisplayServerAndSessionMetadata()
    {
        var candidateType = GetLinuxType("LinuxDisplayServerCandidate");
        candidateType.Should().NotBeNull("selection ordering needs an immutable Linux display-server candidate model.");

        candidateType!.GetProperty("DisplayServer").Should().NotBeNull();
        candidateType.GetProperty("SessionKind").Should().NotBeNull();
    }

    private static Type? GetLinuxType(string typeName)
    {
        return Type.GetType($"Videra.Platform.Linux.{typeName}, Videra.Platform.Linux");
    }
}
