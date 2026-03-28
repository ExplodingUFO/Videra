using System.Numerics;
using FluentAssertions;
using Videra.Core.Geometry;
using Xunit;

namespace Videra.Core.Tests.Geometry;

public class VertexPositionNormalColorTests
{
    [Fact]
    public void Constructor_SetsProperties_Correctly()
    {
        // Arrange
        var pos = new Vector3(1, 2, 3);
        var normal = Vector3.UnitY;
        var color = RgbaFloat.Red;

        // Act
        var vertex = new VertexPositionNormalColor(pos, normal, color);

        // Assert
        vertex.Position.Should().Be(pos);
        vertex.Normal.Should().Be(normal);
        vertex.Color.Should().Be(color);
    }

    [Fact]
    public void SizeInBytes_IsFortyBytes()
    {
        // Assert
        VertexPositionNormalColor.SizeInBytes.Should().Be(40u);
    }

    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var vertex = new VertexPositionNormalColor();

        // Assert
        vertex.Position.Should().Be(Vector3.Zero);
        vertex.Normal.Should().Be(Vector3.Zero);
        // RgbaFloat default struct values
        vertex.Color.R.Should().Be(0f);
        vertex.Color.G.Should().Be(0f);
        vertex.Color.B.Should().Be(0f);
        vertex.Color.A.Should().Be(0f);
    }
}

public class RgbaFloatTests
{
    [Fact]
    public void Constructor_SetsComponents_Correctly()
    {
        // Arrange & Act
        var color = new RgbaFloat(0.1f, 0.2f, 0.3f, 0.4f);

        // Assert
        color.R.Should().Be(0.1f);
        color.G.Should().Be(0.2f);
        color.B.Should().Be(0.3f);
        color.A.Should().Be(0.4f);
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        var a = new RgbaFloat(1.0f, 0.5f, 0.0f, 1.0f);
        var b = new RgbaFloat(1.0f, 0.5f, 0.0f, 1.0f);

        // Assert
        a.Equals(b).Should().BeTrue();
        a.Should().Be(b);
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        // Arrange
        var a = new RgbaFloat(1.0f, 0.5f, 0.0f, 1.0f);
        var b = new RgbaFloat(0.0f, 0.5f, 0.0f, 1.0f);

        // Assert
        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_NullObject_ReturnsFalse()
    {
        // Arrange
        var color = RgbaFloat.Red;

        // Assert
        color.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_SameValues_ReturnsTrue()
    {
        // Arrange
        var a = RgbaFloat.White;
        var b = RgbaFloat.White;

        // Assert
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_DifferentValues_ReturnsTrue()
    {
        // Arrange
        var a = RgbaFloat.Red;
        var b = RgbaFloat.Blue;

        // Assert
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHash()
    {
        // Arrange
        var a = new RgbaFloat(0.5f, 0.5f, 0.5f, 1.0f);
        var b = new RgbaFloat(0.5f, 0.5f, 0.5f, 1.0f);

        // Assert
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void ToVector4_ReturnsCorrectVector()
    {
        // Arrange
        var color = new RgbaFloat(1.0f, 0.5f, 0.25f, 0.75f);

        // Act
        var vec = color.ToVector4();

        // Assert
        vec.Should().Be(new Vector4(1.0f, 0.5f, 0.25f, 0.75f));
    }

    [Fact]
    public void PredefinedColors_Red_HasCorrectValues()
    {
        RgbaFloat.Red.R.Should().Be(1f);
        RgbaFloat.Red.G.Should().Be(0f);
        RgbaFloat.Red.B.Should().Be(0f);
        RgbaFloat.Red.A.Should().Be(1f);
    }

    [Fact]
    public void PredefinedColors_Green_HasCorrectValues()
    {
        RgbaFloat.Green.R.Should().Be(0f);
        RgbaFloat.Green.G.Should().Be(1f);
        RgbaFloat.Green.B.Should().Be(0f);
        RgbaFloat.Green.A.Should().Be(1f);
    }

    [Fact]
    public void PredefinedColors_Blue_HasCorrectValues()
    {
        RgbaFloat.Blue.R.Should().Be(0f);
        RgbaFloat.Blue.G.Should().Be(0f);
        RgbaFloat.Blue.B.Should().Be(1f);
        RgbaFloat.Blue.A.Should().Be(1f);
    }

    [Fact]
    public void PredefinedColors_White_HasCorrectValues()
    {
        RgbaFloat.White.R.Should().Be(1f);
        RgbaFloat.White.G.Should().Be(1f);
        RgbaFloat.White.B.Should().Be(1f);
        RgbaFloat.White.A.Should().Be(1f);
    }

    [Fact]
    public void PredefinedColors_Black_HasCorrectValues()
    {
        RgbaFloat.Black.R.Should().Be(0f);
        RgbaFloat.Black.G.Should().Be(0f);
        RgbaFloat.Black.B.Should().Be(0f);
        RgbaFloat.Black.A.Should().Be(1f);
    }
}
