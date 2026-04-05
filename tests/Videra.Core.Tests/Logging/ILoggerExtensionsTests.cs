using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Videra.Core.Logging;
using Xunit;

namespace Videra.Core.Tests.Logging;

public class ILoggerExtensionsTests
{
    [Fact]
    public void LogComponentInfo_CallsLogger_WithInformationLevel()
    {
        // Arrange
        var mockLogger = CreateEnabledLogger();

        // Act
        mockLogger.Object.LogComponentInfo("TestComponent", "Test message");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("TestComponent") && v.ToString()!.Contains("Test message")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogComponentDebug_CallsLogger_WithDebugLevel()
    {
        // Arrange
        var mockLogger = CreateEnabledLogger();

        // Act
        mockLogger.Object.LogComponentDebug("TestComponent", "TestAction", "Test details");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) =>
                    v.ToString()!.Contains("TestComponent") &&
                    v.ToString()!.Contains("TestAction") &&
                    v.ToString()!.Contains("Test details")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogComponentError_CallsLogger_WithErrorLevel()
    {
        // Arrange
        var mockLogger = CreateEnabledLogger();

        // Act
        mockLogger.Object.LogComponentError("TestComponent", "TestAction", "Test error");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) =>
                    v.ToString()!.Contains("TestComponent") &&
                    v.ToString()!.Contains("TestAction") &&
                    v.ToString()!.Contains("Test error")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogComponentError_CallsLogger_WithException_WhenExceptionProvided()
    {
        // Arrange
        var mockLogger = CreateEnabledLogger();
        var exception = new InvalidOperationException("Test exception");

        // Act
        mockLogger.Object.LogComponentError("TestComponent", "TestAction", "Test error", exception);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("TestComponent")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogComponentWarning_CallsLogger_WithWarningLevel()
    {
        // Arrange
        var mockLogger = CreateEnabledLogger();

        // Act
        mockLogger.Object.LogComponentWarning("TestComponent", "Test warning");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) =>
                    v.ToString()!.Contains("TestComponent") &&
                    v.ToString()!.Contains("Test warning")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static Mock<ILogger> CreateEnabledLogger()
    {
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        return mockLogger;
    }
}
