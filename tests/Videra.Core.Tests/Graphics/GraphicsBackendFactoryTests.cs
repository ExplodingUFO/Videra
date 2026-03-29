using FluentAssertions;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Xunit;

namespace Videra.Core.Tests.Graphics;

public sealed class GraphicsBackendFactoryTests
{
    [Fact]
    public void GetPlatformName_Software_ReturnsSoftwareLabel()
    {
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

    [Fact]
    public void CreateBackend_D3D11Preference_DoesNotThrow()
    {
        var act = () =>
        {
            using var backend = GraphicsBackendFactory.CreateBackend(GraphicsBackendPreference.D3D11);
        };
        act.Should().NotThrow();
    }

    [Fact]
    public void CreateBackend_VulkanPreference_DoesNotThrow()
    {
        var act = () =>
        {
            using var backend = GraphicsBackendFactory.CreateBackend(GraphicsBackendPreference.Vulkan);
        };
        act.Should().NotThrow();
    }

    [Fact]
    public void CreateBackend_MetalPreference_DoesNotThrow()
    {
        var act = () =>
        {
            using var backend = GraphicsBackendFactory.CreateBackend(GraphicsBackendPreference.Metal);
        };
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("d3d11")]
    [InlineData("D3D")]
    [InlineData("vulkan")]
    [InlineData("vk")]
    [InlineData("metal")]
    [InlineData("native")]
    [InlineData("auto")]
    [InlineData("unknown_value")]
    public void CreateBackend_EnvVar_ParsesCorrectly(string envValue)
    {
        var orig = Environment.GetEnvironmentVariable("VIDERA_BACKEND");
        try
        {
            Environment.SetEnvironmentVariable("VIDERA_BACKEND", envValue);
            var act = () =>
            {
                using var backend = GraphicsBackendFactory.CreateBackend(GraphicsBackendPreference.Auto);
            };
            act.Should().NotThrow();
        }
        finally
        {
            Environment.SetEnvironmentVariable("VIDERA_BACKEND", orig);
        }
    }

    [Fact]
    public void GetPlatformName_NonSoftwareEnv_ReturnsPlatformLabel()
    {
        var orig = Environment.GetEnvironmentVariable("VIDERA_BACKEND");
        try
        {
            Environment.SetEnvironmentVariable("VIDERA_BACKEND", "d3d11");
            var name = GraphicsBackendFactory.GetPlatformName();
            name.Should().NotBe("Software (CPU)");
        }
        finally
        {
            Environment.SetEnvironmentVariable("VIDERA_BACKEND", orig);
        }
    }

    [Fact]
    public void CreateBackend_Software_ReturnsNonNullBackend()
    {
        using var backend = GraphicsBackendFactory.CreateBackend(GraphicsBackendPreference.Software);
        backend.Should().BeAssignableTo<IGraphicsBackend>();
    }

    [Fact]
    public void CreateBackend_Software_CanInitializeAndDispose()
    {
        var backend = GraphicsBackendFactory.CreateBackend(GraphicsBackendPreference.Software);
        var act = () =>
        {
            backend.Initialize(IntPtr.Zero, 100, 100);
            backend.Dispose();
        };
        act.Should().NotThrow();
    }
}
