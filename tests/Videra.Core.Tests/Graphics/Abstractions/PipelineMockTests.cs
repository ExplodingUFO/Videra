using FluentAssertions;
using Moq;
using Videra.Core.Graphics.Abstractions;
using Xunit;

namespace Videra.Core.Tests.Graphics.Abstractions;

public class PipelineMockTests
{
    [Fact]
    public void Dispose_CalledOnMockPipeline_InvokesDispose()
    {
        // Arrange
        var mockPipeline = new Mock<IPipeline>();

        // Act
        mockPipeline.Object.Dispose();

        // Assert
        mockPipeline.Verify(p => p.Dispose(), Times.Once);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_InvokesCorrectCount()
    {
        // Arrange
        var mockPipeline = new Mock<IPipeline>();

        // Act
        mockPipeline.Object.Dispose();
        mockPipeline.Object.Dispose();

        // Assert
        mockPipeline.Verify(p => p.Dispose(), Times.Exactly(2));
    }

    [Fact]
    public void MockPipeline_CanBeUsedAsPipelineParameter()
    {
        // Arrange
        var mockPipeline = new Mock<IPipeline>();
        var mockExecutor = new Mock<ICommandExecutor>();

        // Act
        mockExecutor.Object.SetPipeline(mockPipeline.Object);

        // Assert
        mockExecutor.Verify(e => e.SetPipeline(mockPipeline.Object), Times.Once);
    }
}
