using System.Numerics;
using FluentAssertions;
using Moq;
using Videra.Core.Graphics.Abstractions;
using Xunit;

namespace Videra.Core.Tests.Graphics.Abstractions;

public class GraphicsBackendMockTests
{
    [Fact]
    public void IsInitialized_ReturnsTrue_WhenSetup()
    {
        // Arrange
        var mockBackend = new Mock<IGraphicsBackend>();
        mockBackend.Setup(b => b.IsInitialized).Returns(true);

        // Act
        var result = mockBackend.Object.IsInitialized;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInitialized_ReturnsFalse_ByDefault()
    {
        // Arrange
        var mockBackend = new Mock<IGraphicsBackend>();
        mockBackend.Setup(b => b.IsInitialized).Returns(false);

        // Act
        var result = mockBackend.Object.IsInitialized;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Initialize_CalledWithWindowHandle_InvokesMethod()
    {
        // Arrange
        var mockBackend = new Mock<IGraphicsBackend>();
        var handle = new IntPtr(12345);

        // Act
        mockBackend.Object.Initialize(handle, 800, 600);

        // Assert
        mockBackend.Verify(b => b.Initialize(handle, 800, 600), Times.Once);
    }

    [Fact]
    public void Resize_CalledWithDimensions_InvokesMethod()
    {
        // Arrange
        var mockBackend = new Mock<IGraphicsBackend>();

        // Act
        mockBackend.Object.Resize(1024, 768);

        // Assert
        mockBackend.Verify(b => b.Resize(1024, 768), Times.Once);
    }

    [Fact]
    public void BeginFrame_Called_InvokesMethod()
    {
        // Arrange
        var mockBackend = new Mock<IGraphicsBackend>();

        // Act
        mockBackend.Object.BeginFrame();

        // Assert
        mockBackend.Verify(b => b.BeginFrame(), Times.Once);
    }

    [Fact]
    public void EndFrame_Called_InvokesMethod()
    {
        // Arrange
        var mockBackend = new Mock<IGraphicsBackend>();

        // Act
        mockBackend.Object.EndFrame();

        // Assert
        mockBackend.Verify(b => b.EndFrame(), Times.Once);
    }

    [Fact]
    public void BeginFrame_EndFrame_CalledInSequence_InvokesBothMethods()
    {
        // Arrange
        var mockBackend = new Mock<IGraphicsBackend>();

        // Act
        mockBackend.Object.BeginFrame();
        mockBackend.Object.EndFrame();

        // Assert
        mockBackend.Verify(b => b.BeginFrame(), Times.Once);
        mockBackend.Verify(b => b.EndFrame(), Times.Once);
    }

    [Fact]
    public void SetClearColor_CalledWithVector4_InvokesMethod()
    {
        // Arrange
        var mockBackend = new Mock<IGraphicsBackend>();
        var color = new Vector4(0.1f, 0.2f, 0.3f, 1.0f);

        // Act
        mockBackend.Object.SetClearColor(color);

        // Assert
        mockBackend.Verify(b => b.SetClearColor(color), Times.Once);
    }

    [Fact]
    public void GetResourceFactory_Called_ReturnsMockFactory()
    {
        // Arrange
        var mockBackend = new Mock<IGraphicsBackend>();
        var mockFactory = new Mock<IResourceFactory>();

        mockBackend.Setup(b => b.GetResourceFactory()).Returns(mockFactory.Object);

        // Act
        var result = mockBackend.Object.GetResourceFactory();

        // Assert
        result.Should().BeSameAs(mockFactory.Object);
        mockBackend.Verify(b => b.GetResourceFactory(), Times.Once);
    }

    [Fact]
    public void GetCommandExecutor_Called_ReturnsMockExecutor()
    {
        // Arrange
        var mockBackend = new Mock<IGraphicsBackend>();
        var mockExecutor = new Mock<ICommandExecutor>();

        mockBackend.Setup(b => b.GetCommandExecutor()).Returns(mockExecutor.Object);

        // Act
        var result = mockBackend.Object.GetCommandExecutor();

        // Assert
        result.Should().BeSameAs(mockExecutor.Object);
        mockBackend.Verify(b => b.GetCommandExecutor(), Times.Once);
    }

    [Fact]
    public void Dispose_Called_InvokesDispose()
    {
        // Arrange
        var mockBackend = new Mock<IGraphicsBackend>();

        // Act
        mockBackend.Object.Dispose();

        // Assert
        mockBackend.Verify(b => b.Dispose(), Times.Once);
    }
}
