using FluentAssertions;
using Videra.Core.Exceptions;
using Xunit;

namespace Videra.Core.Tests.Exceptions;

public class VideraExceptionTests
{
    [Fact]
    public void VideraException_SetsOperationAndContext()
    {
        var ctx = new Dictionary<string, string?> { ["Key1"] = "Value1" };

        var ex = new VideraException("test message", "TestOp", ctx);

        ex.Message.Should().Be("test message");
        ex.Operation.Should().Be("TestOp");
        ex.Context.Should().ContainKey("Key1");
        ex.Context["Key1"].Should().Be("Value1");
    }

    [Fact]
    public void VideraException_NullContext_YieldsEmptyDictionary()
    {
        var ex = new VideraException("msg", "Op");

        ex.Context.Should().NotBeNull();
        ex.Context.Should().BeEmpty();
    }

    [Fact]
    public void VideraException_FiltersNullValuesFromContext()
    {
        var ctx = new Dictionary<string, string?>
        {
            ["Valid"] = "present",
            ["NullVal"] = null
        };

        var ex = new VideraException("msg", "Op", ctx);

        ex.Context.Should().ContainKey("Valid");
        ex.Context["Valid"].Should().Be("present");
        ex.Context.Should().NotContainKey("NullVal");
    }

    [Fact]
    public void VideraException_InnerException_IsPreserved()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new VideraException("msg", "Op", inner);

        ex.InnerException.Should().BeSameAs(inner);
    }
}

public class InvalidModelInputExceptionTests
{
    [Fact]
    public void InvalidModelInputException_InheritsFromVideraException()
    {
        var ex = new InvalidModelInputException("msg", "LoadModel");

        ex.Should().BeAssignableTo<VideraException>();
        ex.Operation.Should().Be("LoadModel");
    }

    [Fact]
    public void InvalidModelInputException_WithContext_PropagatesFields()
    {
        var ctx = new Dictionary<string, string?> { ["FilePath"] = "test.gltf" };

        var ex = new InvalidModelInputException("bad input", "LoadModel", ctx);

        ex.Context["FilePath"].Should().Be("test.gltf");
        ex.Message.Should().Contain("bad input");
    }
}

public class GraphicsInitializationExceptionTests
{
    [Fact]
    public void GraphicsInitializationException_SetsErrorCode()
    {
        var ex = new GraphicsInitializationException("init failed", "Initialize", errorCode: -2147024882);

        ex.ErrorCode.Should().Be(-2147024882);
        ex.Operation.Should().Be("Initialize");
    }

    [Fact]
    public void GraphicsInitializationException_NullErrorCode_ByDefault()
    {
        var ex = new GraphicsInitializationException("init failed", "Initialize");

        ex.ErrorCode.Should().BeNull();
    }
}

public class PlatformDependencyExceptionTests
{
    [Fact]
    public void PlatformDependencyException_SetsPlatform()
    {
        var ex = new PlatformDependencyException("missing lib", "Initialize", "Windows");

        ex.Platform.Should().Be("Windows");
        ex.Operation.Should().Be("Initialize");
    }

    [Fact]
    public void PlatformDependencyException_WithErrorCode()
    {
        var ex = new PlatformDependencyException("driver failure", "Initialize", "Linux", errorCode: -42);

        ex.ErrorCode.Should().Be(-42);
        ex.Platform.Should().Be("Linux");
    }
}

public class ResourceCreationExceptionTests
{
    [Fact]
    public void ResourceCreationException_InheritsFromVideraException()
    {
        var ex = new ResourceCreationException("buffer failed", "CreateVertexBuffer");

        ex.Should().BeAssignableTo<VideraException>();
        ex.Operation.Should().Be("CreateVertexBuffer");
    }
}

public class PipelineCreationExceptionTests
{
    [Fact]
    public void PipelineCreationException_InheritsFromVideraException()
    {
        var ex = new PipelineCreationException("pipeline failed", "CreatePipeline");

        ex.Should().BeAssignableTo<VideraException>();
        ex.Operation.Should().Be("CreatePipeline");
    }
}

public class UnsupportedOperationExceptionTests
{
    [Fact]
    public void UnsupportedOperationException_InheritsFromVideraException()
    {
        var ex = new UnsupportedOperationException("not supported", "ComputeShaders");

        ex.Should().BeAssignableTo<VideraException>();
        ex.Operation.Should().Be("ComputeShaders");
    }

    [Fact]
    public void UnsupportedOperationException_WithInnerException()
    {
        var inner = new NotImplementedException();
        var ex = new UnsupportedOperationException("not yet", "SomeOp", inner);

        ex.InnerException.Should().BeSameAs(inner);
    }
}
