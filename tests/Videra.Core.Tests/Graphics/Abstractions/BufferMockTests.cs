using FluentAssertions;
using Moq;
using Videra.Core.Graphics.Abstractions;
using Xunit;

namespace Videra.Core.Tests.Graphics.Abstractions;

public class BufferMockTests
{
    [Fact]
    public void SizeInBytes_ReturnsConfiguredValue_WhenMockSetup()
    {
        // Arrange
        var mockBuffer = new Mock<IBuffer>();
        mockBuffer.Setup(b => b.SizeInBytes).Returns(1024u);

        // Act
        var size = mockBuffer.Object.SizeInBytes;

        // Assert
        size.Should().Be(1024u);
    }

    [Fact]
    public void Update_CalledWithUnmanagedData_InvokesMethod()
    {
        // Arrange
        var mockBuffer = new Mock<IBuffer>();
        var data = 42;

        // Act
        mockBuffer.Object.Update(data);

        // Assert
        mockBuffer.Verify(b => b.Update(data), Times.Once);
    }

    [Fact]
    public void UpdateArray_CalledWithArray_InvokesMethod()
    {
        // Arrange
        var mockBuffer = new Mock<IBuffer>();
        var data = new uint[] { 1, 2, 3 };

        // Act
        mockBuffer.Object.UpdateArray(data);

        // Assert
        mockBuffer.Verify(b => b.UpdateArray(data), Times.Once);
    }

    [Fact]
    public void SetData_CalledWithValueAndOffset_InvokesMethod()
    {
        // Arrange
        var mockBuffer = new Mock<IBuffer>();
        var data = 99;
        var offset = 16u;

        // Act
        mockBuffer.Object.SetData(data, offset);

        // Assert
        mockBuffer.Verify(b => b.SetData(data, offset), Times.Once);
    }

    [Fact]
    public void SetData_CalledWithArrayAndOffset_InvokesMethod()
    {
        // Arrange
        var mockBuffer = new Mock<IBuffer>();
        var data = new float[] { 1.0f, 2.0f, 3.0f };
        var offset = 64u;

        // Act
        mockBuffer.Object.SetData(data, offset);

        // Assert
        mockBuffer.Verify(b => b.SetData(data, offset), Times.Once);
    }

    [Fact]
    public void Dispose_CalledOnMockBuffer_InvokesDispose()
    {
        // Arrange
        var mockBuffer = new Mock<IBuffer>();

        // Act
        mockBuffer.Object.Dispose();

        // Assert
        mockBuffer.Verify(b => b.Dispose(), Times.Once);
    }

    [Fact]
    public void Update_CalledMultipleTimes_InvokesCorrectCount()
    {
        // Arrange
        var mockBuffer = new Mock<IBuffer>();

        // Act
        mockBuffer.Object.Update(1);
        mockBuffer.Object.Update(2);
        mockBuffer.Object.Update(3);

        // Assert
        mockBuffer.Verify(b => b.Update(It.IsAny<int>()), Times.Exactly(3));
    }
}
