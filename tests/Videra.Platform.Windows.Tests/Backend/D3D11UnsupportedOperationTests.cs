using FluentAssertions;
using Videra.Core.Exceptions;
using Xunit;

namespace Videra.Platform.Windows.Tests.Backend;

public class D3D11UnsupportedOperationTests
{
    [Fact]
    public void UnsupportedOperationException_ContainsPlatform()
    {
        var ex = new UnsupportedOperationException(
            "Test operation not supported.",
            "TestOperation",
            "Windows");

        ex.Platform.Should().Be("Windows");
        ex.Operation.Should().Be("TestOperation");
        ex.Message.Should().Contain("Test operation not supported.");
    }

    [Fact]
    public void UnsupportedOperationException_IsVideraException()
    {
        var ex = new UnsupportedOperationException(
            "Not supported.",
            "Op",
            "Windows");

        ex.Should().BeAssignableTo<VideraException>();
    }
}
