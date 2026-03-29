using FluentAssertions;
using Videra.Core.Graphics;
using Xunit;

namespace Videra.Core.Tests.Graphics;

public sealed class GraphicsBackendFactoryTests
{
    [Fact]
    public void GetPlatformName_Software_ReturnsSoftwareLabel()
    {
        // Clear env to ensure software fallback
        var orig = Environment.GetEnvironmentVariable("VIDERA_BACKEND");
        try
        {
            Environment.SetEnvironmentVariable("VIDERA_BACKEND", "software");
            var name = GraphicsBackendFactory.GetPlatformName();
            name.Should().Be("Software (CPU)");
        }
        finally
        {
            Environment.SetEnvironmentVariable("VIDERA_BACKEND", orig);
        }
    }

    [Fact]
    public void GetPlatformName_NullEnv_ReturnsPlatformName()
    {
        var orig = Environment.GetEnvironmentVariable("VIDERA_BACKEND");
        try
        {
            Environment.SetEnvironmentVariable("VIDERA_BACKEND", null);
            var name = GraphicsBackendFactory.GetPlatformName();
            name.Should().NotBeNullOrWhiteSpace();
        }
        finally
        {
            Environment.SetEnvironmentVariable("VIDERA_BACKEND", orig);
        }
    }

    [Fact]
    public void CreateBackend_Software_ReturnsSoftwareBackend()
    {
        using var backend = GraphicsBackendFactory.CreateBackend(GraphicsBackendPreference.Software);
        backend.Should().NotBeNull();
    }

    [Fact]
    public void CreateBackend_Auto_DoesNotThrow()
    {
        var act = () =>
        {
            using var backend = GraphicsBackendFactory.CreateBackend(GraphicsBackendPreference.Auto);
        };

        act.Should().NotThrow();
    }
}
