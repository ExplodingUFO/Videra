using FluentAssertions;
using Xunit;

namespace Videra.Platform.Linux.Tests.Display;

public sealed class LinuxDisplayServerDetectorTests
{
    [Fact]
    public void WaylandSession_WithWaylandAndX11Display_PrefersWaylandThenXWayland()
    {
        var detector = CreateDetector();

        var candidates = DetectCandidates(detector, waylandDisplay: "wayland-0", x11Display: ":1", sessionType: "wayland");

        candidates.Should().ContainInOrder("Wayland", "XWayland");
    }

    [Fact]
    public void X11Session_WithOnlyDisplay_ReturnsX11Candidate()
    {
        var detector = CreateDetector();

        var candidates = DetectCandidates(detector, waylandDisplay: null, x11Display: ":0", sessionType: "x11");

        candidates.Should().Equal("X11");
    }

    [Fact]
    public void UnknownSession_WithoutDisplayVariables_ReturnsNoCandidates()
    {
        var detector = CreateDetector();

        var candidates = DetectCandidates(detector, waylandDisplay: null, x11Display: null, sessionType: null);

        candidates.Should().BeEmpty();
    }

    private static object CreateDetector()
    {
        var detectorType = GetLinuxType("LinuxDisplayServerDetector");
        detectorType.Should().NotBeNull("Linux display-server auto-selection requires a dedicated detector.");

        var detector = Activator.CreateInstance(detectorType!);
        detector.Should().NotBeNull();
        return detector!;
    }

    private static IReadOnlyList<string> DetectCandidates(object detector, string? waylandDisplay, string? x11Display, string? sessionType)
    {
        var method = detector.GetType().GetMethod("DetectCandidates", new[] { typeof(string), typeof(string), typeof(string) });
        method.Should().NotBeNull("the detector must expose a pure DetectCandidates(string?, string?, string?) API.");

        var result = method!.Invoke(detector, new object?[] { waylandDisplay, x11Display, sessionType });
        result.Should().BeAssignableTo<System.Collections.IEnumerable>();

        var values = new List<string>();
        foreach (var candidate in (System.Collections.IEnumerable)result!)
        {
            candidate.Should().NotBeNull();
            var property = candidate!.GetType().GetProperty("DisplayServer");
            property.Should().NotBeNull("candidate results must expose the resolved display-server kind.");
            values.Add(property!.GetValue(candidate)!.ToString()!);
        }

        return values;
    }

    private static Type? GetLinuxType(string typeName)
    {
        return Type.GetType($"Videra.Core.Platform.Linux.{typeName}, Videra.Core");
    }
}
