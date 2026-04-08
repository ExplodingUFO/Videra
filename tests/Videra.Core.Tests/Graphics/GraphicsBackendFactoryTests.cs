using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Xunit;

namespace Videra.Core.Tests.Graphics;

public sealed class GraphicsBackendFactoryTests
{
    [Fact]
    public void RenderBindingSlots_HaveStableIndices()
    {
        RenderBindingSlots.Vertex.Should().Be(0);
        RenderBindingSlots.Camera.Should().Be(1);
        RenderBindingSlots.World.Should().Be(2);
        RenderBindingSlots.Style.Should().Be(3);
    }

    [Fact]
    public void PrimitiveCommandKind_HaveStableValues()
    {
        PrimitiveCommandKind.LineList.Should().Be(1u);
        PrimitiveCommandKind.PointList.Should().Be(2u);
        PrimitiveCommandKind.TriangleList.Should().Be(3u);
    }

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

    [Fact]
    public void CreateBackend_WithConfiguredResolver_UsesResolverForNativePreference()
    {
        var resolver = new TestGraphicsBackendResolver();
        GraphicsBackendFactory.ConfigureResolver(resolver);

        try
        {
            using var backend = GraphicsBackendFactory.CreateBackend(GraphicsBackendPreference.D3D11);

            backend.Should().BeSameAs(resolver.LastBackend);
            resolver.LastPreference.Should().Be(GraphicsBackendPreference.D3D11);
        }
        finally
        {
            GraphicsBackendFactory.ConfigureResolver(null);
        }
    }

    [Fact]
    public void CreateBackend_WithoutResolver_FallsBackToSoftwareForNativePreference()
    {
        GraphicsBackendFactory.ConfigureResolver(null);

        using var backend = GraphicsBackendFactory.CreateBackend(GraphicsBackendPreference.D3D11);

        backend.Should().BeAssignableTo<IGraphicsBackend>();
        backend.Should().NotBeNull();
        backend.GetType().Name.Should().Be("SoftwareBackend");
    }

    [Fact]
    public void CreateBackend_AutoPreference_PassesParsedEnvPreferenceToResolver()
    {
        var resolver = new TestGraphicsBackendResolver();
        var original = Environment.GetEnvironmentVariable("VIDERA_BACKEND");
        GraphicsBackendFactory.ConfigureResolver(resolver);

        try
        {
            Environment.SetEnvironmentVariable("VIDERA_BACKEND", "vk");

            using var backend = GraphicsBackendFactory.CreateBackend(GraphicsBackendPreference.Auto, NullLoggerFactory.Instance);

            backend.Should().BeSameAs(resolver.LastBackend);
            resolver.LastPreference.Should().Be(GraphicsBackendPreference.Vulkan);
        }
        finally
        {
            GraphicsBackendFactory.ConfigureResolver(null);
            Environment.SetEnvironmentVariable("VIDERA_BACKEND", original);
        }
    }

    [Fact]
    public void ResolveBackend_DisabledEnvironmentOverrides_UsesRequestedPreference()
    {
        var resolver = new TestGraphicsBackendResolver();
        var original = Environment.GetEnvironmentVariable("VIDERA_BACKEND");
        GraphicsBackendFactory.ConfigureResolver(resolver);

        try
        {
            Environment.SetEnvironmentVariable("VIDERA_BACKEND", "software");

            var resolution = GraphicsBackendFactory.ResolveBackend(
                new GraphicsBackendRequest(
                    GraphicsBackendPreference.Vulkan,
                    BackendEnvironmentOverrideMode.Disabled,
                    AllowSoftwareFallback: true,
                    LoggerFactory: NullLoggerFactory.Instance));

            resolution.RequestedPreference.Should().Be(GraphicsBackendPreference.Vulkan);
            resolution.ResolvedPreference.Should().Be(GraphicsBackendPreference.Vulkan);
            resolution.EnvironmentOverrideApplied.Should().BeFalse();
            resolution.IsUsingSoftwareFallback.Should().BeFalse();
            resolver.LastPreference.Should().Be(GraphicsBackendPreference.Vulkan);
            resolution.Backend.Dispose();
        }
        finally
        {
            GraphicsBackendFactory.ConfigureResolver(null);
            Environment.SetEnvironmentVariable("VIDERA_BACKEND", original);
        }
    }

    [Fact]
    public void ResolveBackend_WithoutResolver_FallsBackToSoftwareWithReason()
    {
        GraphicsBackendFactory.ConfigureResolver(null);

        var resolution = GraphicsBackendFactory.ResolveBackend(
            new GraphicsBackendRequest(
                GraphicsBackendPreference.D3D11,
                BackendEnvironmentOverrideMode.Disabled,
                AllowSoftwareFallback: true,
                LoggerFactory: NullLoggerFactory.Instance));

        resolution.RequestedPreference.Should().Be(GraphicsBackendPreference.D3D11);
        resolution.ResolvedPreference.Should().Be(GraphicsBackendPreference.Software);
        resolution.IsUsingSoftwareFallback.Should().BeTrue();
        resolution.FallbackReason.Should().NotBeNullOrWhiteSpace();
        resolution.Backend.GetType().Name.Should().Be("SoftwareBackend");
        resolution.Backend.Dispose();
    }

    [Fact]
    public void ResolveBackend_ResolverUnavailableReason_FlowsIntoFallbackReason()
    {
        var resolver = new TestGraphicsBackendResolver
        {
            Result = new GraphicsBackendResolverResult(
                Backend: null,
                UnavailableReason: "Backend package 'Videra.Platform.Linux' is not installed.")
        };
        GraphicsBackendFactory.ConfigureResolver(resolver);

        try
        {
            var resolution = GraphicsBackendFactory.ResolveBackend(
                new GraphicsBackendRequest(
                    GraphicsBackendPreference.Vulkan,
                    BackendEnvironmentOverrideMode.Disabled,
                    AllowSoftwareFallback: true,
                    LoggerFactory: NullLoggerFactory.Instance));

            resolution.ResolvedPreference.Should().Be(GraphicsBackendPreference.Software);
            resolution.IsUsingSoftwareFallback.Should().BeTrue();
            resolution.FallbackReason.Should().Be("Backend package 'Videra.Platform.Linux' is not installed.");
            resolution.Backend.Dispose();
        }
        finally
        {
            GraphicsBackendFactory.ConfigureResolver(null);
        }
    }

    [Fact]
    public void ResolveBackend_ResolverUnavailableReason_ThrowsSameReason_WhenFallbackDisabled()
    {
        var resolver = new TestGraphicsBackendResolver
        {
            Result = new GraphicsBackendResolverResult(
                Backend: null,
                UnavailableReason: "Backend package 'Videra.Platform.Linux' is not installed.")
        };
        GraphicsBackendFactory.ConfigureResolver(resolver);

        try
        {
            // Contract: AllowSoftwareFallback = false throws the same unavailable reason instead of populating FallbackReason.
            var act = () => GraphicsBackendFactory.ResolveBackend(
                new GraphicsBackendRequest(
                    GraphicsBackendPreference.Vulkan,
                    BackendEnvironmentOverrideMode.Disabled,
                    AllowSoftwareFallback: false,
                    LoggerFactory: NullLoggerFactory.Instance));

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Backend package 'Videra.Platform.Linux' is not installed.");
        }
        finally
        {
            GraphicsBackendFactory.ConfigureResolver(null);
        }
    }

    private sealed class TestGraphicsBackendResolver : IGraphicsBackendResolver
    {
        public GraphicsBackendPreference? LastPreference { get; private set; }
        public TestGraphicsBackend? LastBackend { get; private set; }
        public GraphicsBackendResolverResult? Result { get; init; }

        public GraphicsBackendResolverResult ResolveBackend(GraphicsBackendPreference preference, ILoggerFactory? loggerFactory = null)
        {
            LastPreference = preference;
            if (Result is { } configuredResult)
            {
                LastBackend = configuredResult.Backend as TestGraphicsBackend;
                return configuredResult;
            }

            LastBackend = new TestGraphicsBackend();
            return new GraphicsBackendResolverResult(LastBackend);
        }
    }

    private sealed class TestGraphicsBackend : IGraphicsBackend
    {
        public bool IsInitialized { get; private set; }

        public void Initialize(IntPtr windowHandle, int width, int height)
        {
            IsInitialized = true;
        }

        public void Resize(int width, int height)
        {
        }

        public void BeginFrame()
        {
        }

        public void EndFrame()
        {
        }

        public void SetClearColor(System.Numerics.Vector4 color)
        {
        }

        public IResourceFactory GetResourceFactory()
        {
            throw new NotSupportedException();
        }

        public ICommandExecutor GetCommandExecutor()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
        }
    }
}
