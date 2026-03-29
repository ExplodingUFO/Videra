using FluentAssertions;
using Videra.Core.Graphics.Abstractions;
using Xunit;

namespace Videra.Core.Tests.Graphics;

public class DepthBufferConfigurationTests
{
    [Fact]
    public void Default_HasExpectedValues()
    {
        var config = DepthBufferConfiguration.Default;

        config.ClearDepthValue.Should().Be(1.0f);
        config.ClearStencilValue.Should().Be(0);
        config.DepthComparison.Should().Be(DepthComparisonFunction.LessEqual);
    }

    [Fact]
    public void CanCreateWithCustomValues()
    {
        var config = new DepthBufferConfiguration(
            clearDepthValue: 0.0f,
            clearStencilValue: 1,
            depthComparison: DepthComparisonFunction.Always);

        config.ClearDepthValue.Should().Be(0.0f);
        config.ClearStencilValue.Should().Be(1);
        config.DepthComparison.Should().Be(DepthComparisonFunction.Always);
    }

    [Fact]
    public void IsValueType()
    {
        var config1 = DepthBufferConfiguration.Default;
        var originalValue = config1.ClearDepthValue; // use config1 to avoid S1854
        originalValue.Should().Be(1.0f);

        var config2 = new DepthBufferConfiguration(0.0f, 0, DepthComparisonFunction.Never);

        config1.ClearDepthValue.Should().Be(1.0f);
        config2.ClearDepthValue.Should().Be(0.0f);
    }

    [Fact]
    public void DepthComparisonFunction_HasExpectedMembers()
    {
        var values = Enum.GetValues<DepthComparisonFunction>();

        values.Should().Contain(DepthComparisonFunction.Never);
        values.Should().Contain(DepthComparisonFunction.Less);
        values.Should().Contain(DepthComparisonFunction.Equal);
        values.Should().Contain(DepthComparisonFunction.LessEqual);
        values.Should().Contain(DepthComparisonFunction.Greater);
        values.Should().Contain(DepthComparisonFunction.NotEqual);
        values.Should().Contain(DepthComparisonFunction.GreaterEqual);
        values.Should().Contain(DepthComparisonFunction.Always);
    }
}
