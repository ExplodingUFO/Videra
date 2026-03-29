using FluentAssertions;
using Videra.Core.Exceptions;
using Xunit;

namespace Videra.Core.Tests.Exceptions;

public class ExceptionHierarchyTests
{
    [Fact]
    public void VideraException_StoresOperationAndContext()
    {
        var context = new Dictionary<string, string?> { ["Key1"] = "Value1", ["Key2"] = "Value2" };
        var ex = new VideraException("test message", "TestOp", context);

        ex.Message.Should().Be("test message");
        ex.Operation.Should().Be("TestOp");
        ex.Context.Should().ContainKey("Key1").WhoseValue.Should().Be("Value1");
        ex.Context.Should().ContainKey("Key2").WhoseValue.Should().Be("Value2");
    }

    [Fact]
    public void VideraException_FiltersNullValuesFromContext()
    {
        var context = new Dictionary<string, string?> { ["Present"] = "yes", ["Absent"] = null };
        var ex = new VideraException("msg", "Op", context);

        ex.Context.Should().ContainKey("Present");
        ex.Context.Should().NotContainKey("Absent");
    }

    [Fact]
    public void VideraException_DefaultContextIsEmpty()
    {
        var ex = new VideraException("msg", "Op");
        ex.Context.Should().BeEmpty();
    }

    [Fact]
    public void VideraException_PreservesInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new VideraException("msg", "Op", inner);
        ex.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void InvalidModelInputException_InheritsVideraException()
    {
        var ex = new InvalidModelInputException("bad input", "LoadModel");
        ex.Should().BeAssignableTo<VideraException>();
        ex.Operation.Should().Be("LoadModel");
    }

    [Fact]
    public void GraphicsInitializationException_StoresErrorCode()
    {
        var ex = new GraphicsInitializationException("init failed", "Initialize", errorCode: -2147024891);
        ex.ErrorCode.Should().Be(-2147024891);
        ex.Operation.Should().Be("Initialize");
    }

    [Fact]
    public void PlatformDependencyException_StoresPlatformAndErrorCode()
    {
        var ex = new PlatformDependencyException("missing lib", "LoadLibrary", "Linux", errorCode: 42);
        ex.Platform.Should().Be("Linux");
        ex.ErrorCode.Should().Be(42);
        ex.Operation.Should().Be("LoadLibrary");
        ex.Should().BeAssignableTo<VideraException>();
    }

    [Fact]
    public void ResourceCreationException_InheritsVideraException()
    {
        var ex = new ResourceCreationException("create failed", "CreateBuffer");
        ex.Should().BeAssignableTo<VideraException>();
        ex.Operation.Should().Be("CreateBuffer");
    }

    [Fact]
    public void PipelineCreationException_InheritsVideraException()
    {
        var ex = new PipelineCreationException("pipeline failed", "CreatePipeline");
        ex.Should().BeAssignableTo<VideraException>();
    }

    [Fact]
    public void UnsupportedOperationException_InheritsVideraException()
    {
        var ex = new UnsupportedOperationException("not supported", "ComputeShaders", "TestPlatform");
        ex.Should().BeAssignableTo<VideraException>();
        ex.Message.Should().Be("not supported");
    }

    [Fact]
    public void AllExceptions_InheritFromException()
    {
        typeof(InvalidModelInputException).Should().BeAssignableTo<Exception>();
        typeof(GraphicsInitializationException).Should().BeAssignableTo<Exception>();
        typeof(ResourceCreationException).Should().BeAssignableTo<Exception>();
        typeof(PipelineCreationException).Should().BeAssignableTo<Exception>();
        typeof(PlatformDependencyException).Should().BeAssignableTo<Exception>();
        typeof(UnsupportedOperationException).Should().BeAssignableTo<Exception>();
    }
}
