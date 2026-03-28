using FluentAssertions;
using Moq;
using Videra.Core.Graphics.Abstractions;
using Xunit;

namespace Videra.Core.Tests.Graphics.Abstractions;

public class CommandExecutorMockTests
{
    [Fact]
    public void SetPipeline_CalledWithPipeline_InvokesMethod()
    {
        // Arrange
        var mockExecutor = new Mock<ICommandExecutor>();
        var mockPipeline = new Mock<IPipeline>();

        // Act
        mockExecutor.Object.SetPipeline(mockPipeline.Object);

        // Assert
        mockExecutor.Verify(e => e.SetPipeline(mockPipeline.Object), Times.Once);
    }

    [Fact]
    public void SetVertexBuffer_CalledWithBufferAndIndex_InvokesMethod()
    {
        // Arrange
        var mockExecutor = new Mock<ICommandExecutor>();
        var mockBuffer = new Mock<IBuffer>();

        // Act
        mockExecutor.Object.SetVertexBuffer(mockBuffer.Object, 0);

        // Assert
        mockExecutor.Verify(e => e.SetVertexBuffer(mockBuffer.Object, 0), Times.Once);
    }

    [Fact]
    public void SetVertexBuffer_CalledWithDefaultIndex_UsesZero()
    {
        // Arrange
        var mockExecutor = new Mock<ICommandExecutor>();
        var mockBuffer = new Mock<IBuffer>();

        // Act
        mockExecutor.Object.SetVertexBuffer(mockBuffer.Object);

        // Assert
        mockExecutor.Verify(e => e.SetVertexBuffer(mockBuffer.Object, 0), Times.Once);
    }

    [Fact]
    public void SetIndexBuffer_CalledWithBuffer_InvokesMethod()
    {
        // Arrange
        var mockExecutor = new Mock<ICommandExecutor>();
        var mockBuffer = new Mock<IBuffer>();

        // Act
        mockExecutor.Object.SetIndexBuffer(mockBuffer.Object);

        // Assert
        mockExecutor.Verify(e => e.SetIndexBuffer(mockBuffer.Object), Times.Once);
    }

    [Fact]
    public void SetResourceSet_CalledWithSlotAndSet_InvokesMethod()
    {
        // Arrange
        var mockExecutor = new Mock<ICommandExecutor>();
        var mockResourceSet = new Mock<IResourceSet>();

        // Act
        mockExecutor.Object.SetResourceSet(0, mockResourceSet.Object);

        // Assert
        mockExecutor.Verify(e => e.SetResourceSet(0, mockResourceSet.Object), Times.Once);
    }

    [Fact]
    public void DrawIndexed_CalledWithAllParameters_InvokesMethod()
    {
        // Arrange
        var mockExecutor = new Mock<ICommandExecutor>();

        // Act
        mockExecutor.Object.DrawIndexed(
            indexCount: 36,
            instanceCount: 1,
            firstIndex: 0,
            vertexOffset: 0,
            firstInstance: 0);

        // Assert
        mockExecutor.Verify(e => e.DrawIndexed(36, 1, 0, 0, 0), Times.Once);
    }

    [Fact]
    public void DrawIndexed_CalledWithPrimitiveTypeOverload_InvokesMethod()
    {
        // Arrange
        var mockExecutor = new Mock<ICommandExecutor>();

        // Act
        mockExecutor.Object.DrawIndexed(
            primitiveType: 0,
            indexCount: 24,
            instanceCount: 2,
            firstIndex: 6,
            vertexOffset: 0,
            firstInstance: 0);

        // Assert
        mockExecutor.Verify(e => e.DrawIndexed(0, 24, 2, 6, 0, 0), Times.Once);
    }

    [Fact]
    public void Draw_CalledWithAllParameters_InvokesMethod()
    {
        // Arrange
        var mockExecutor = new Mock<ICommandExecutor>();

        // Act
        mockExecutor.Object.Draw(
            vertexCount: 100,
            instanceCount: 1,
            firstVertex: 0,
            firstInstance: 0);

        // Assert
        mockExecutor.Verify(e => e.Draw(100, 1, 0, 0), Times.Once);
    }

    [Fact]
    public void SetViewport_CalledWithAllParameters_InvokesMethod()
    {
        // Arrange
        var mockExecutor = new Mock<ICommandExecutor>();

        // Act
        mockExecutor.Object.SetViewport(0f, 0f, 800f, 600f, 0f, 1f);

        // Assert
        mockExecutor.Verify(e => e.SetViewport(0f, 0f, 800f, 600f, 0f, 1f), Times.Once);
    }

    [Fact]
    public void SetViewport_CalledWithDefaults_UsesDefaultDepthRange()
    {
        // Arrange
        var mockExecutor = new Mock<ICommandExecutor>();

        // Act
        mockExecutor.Object.SetViewport(0f, 0f, 1024f, 768f);

        // Assert
        mockExecutor.Verify(e => e.SetViewport(0f, 0f, 1024f, 768f, 0f, 1f), Times.Once);
    }

    [Fact]
    public void SetScissorRect_CalledWithDimensions_InvokesMethod()
    {
        // Arrange
        var mockExecutor = new Mock<ICommandExecutor>();

        // Act
        mockExecutor.Object.SetScissorRect(0, 0, 800, 600);

        // Assert
        mockExecutor.Verify(e => e.SetScissorRect(0, 0, 800, 600), Times.Once);
    }

    [Fact]
    public void Clear_CalledWithColorValues_InvokesMethod()
    {
        // Arrange
        var mockExecutor = new Mock<ICommandExecutor>();

        // Act
        mockExecutor.Object.Clear(0.1f, 0.2f, 0.3f, 1.0f);

        // Assert
        mockExecutor.Verify(e => e.Clear(0.1f, 0.2f, 0.3f, 1.0f), Times.Once);
    }

    [Fact]
    public void SetDepthState_CalledWithFlags_InvokesMethod()
    {
        // Arrange
        var mockExecutor = new Mock<ICommandExecutor>();

        // Act
        mockExecutor.Object.SetDepthState(true, true);

        // Assert
        mockExecutor.Verify(e => e.SetDepthState(true, true), Times.Once);
    }

    [Fact]
    public void ResetDepthState_Called_InvokesMethod()
    {
        // Arrange
        var mockExecutor = new Mock<ICommandExecutor>();

        // Act
        mockExecutor.Object.ResetDepthState();

        // Assert
        mockExecutor.Verify(e => e.ResetDepthState(), Times.Once);
    }
}
