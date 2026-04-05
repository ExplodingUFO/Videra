using System.Reflection;
using FluentAssertions;
using Videra.Core.Graphics.Abstractions;
using Videra.Platform.macOS;
using Xunit;

namespace Videra.Platform.macOS.Tests.Backend;

public sealed class MetalBackendDepthMappingTests
{
    [Fact]
    public void MapDepthComparison_LessEqual_UsesMetalLessEqualEnumValue()
    {
        var method = typeof(MetalBackend).GetMethod(
            "MapDepthComparison",
            BindingFlags.Static | BindingFlags.NonPublic);

        method.Should().NotBeNull();

        var value = (int)method!.Invoke(null, [DepthComparisonFunction.LessEqual])!;

        value.Should().Be(3);
    }
}
