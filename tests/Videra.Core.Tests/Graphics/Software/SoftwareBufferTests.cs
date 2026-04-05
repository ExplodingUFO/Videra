using System.Numerics;
using FluentAssertions;
using Videra.Core.Graphics.Software;
using Xunit;

namespace Videra.Core.Tests.Graphics.Software;

public class SoftwareBufferTests
{
    private static readonly float[] SpanValues = [1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f];

    [Fact]
    public void Constructor_SetsSizeInBytes()
    {
        var buffer = new SoftwareBuffer(256);
        buffer.SizeInBytes.Should().Be(256u);
    }

    [Fact]
    public void Update_WritesStruct_ToBuffer()
    {
        var buffer = new SoftwareBuffer(64);
        var matrix = Matrix4x4.Identity;

        var act = () => buffer.Update(matrix);
        act.Should().NotThrow();
    }

    [Fact]
    public void UpdateArray_WritesArrayOfStructs_ToBuffer()
    {
        var buffer = new SoftwareBuffer(4096);
        var vertices = new[]
        {
            new System.Numerics.Vector3(1, 2, 3),
            new System.Numerics.Vector3(4, 5, 6),
        };

        var act = () => buffer.UpdateArray(vertices);
        act.Should().NotThrow();
    }

    [Fact]
    public void SetData_WithOffset_WritesAtCorrectPosition()
    {
        var buffer = new SoftwareBuffer(128);
        var mat1 = Matrix4x4.CreateTranslation(1, 0, 0);
        var mat2 = Matrix4x4.CreateTranslation(2, 0, 0);

        buffer.SetData(mat1, 0);
        buffer.SetData(mat2, 64);

        var read1 = buffer.Read<Matrix4x4>(0);
        var read2 = buffer.Read<Matrix4x4>(64);

        read1.Translation.X.Should().Be(1f);
        read2.Translation.X.Should().Be(2f);
    }

    [Fact]
    public void SetData_ExceedsBuffer_ThrowsArgumentOutOfRangeException()
    {
        var buffer = new SoftwareBuffer(32);
        var matrix = Matrix4x4.Identity; // 64 bytes

        var act = () => buffer.SetData(matrix, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void UpdateArray_EmptyArray_DoesNotThrow()
    {
        var buffer = new SoftwareBuffer(64);
        var act = () => buffer.UpdateArray(Array.Empty<System.Numerics.Vector3>());
        act.Should().NotThrow();
    }

    [Fact]
    public void Read_AtValidOffset_ReturnsWrittenData()
    {
        var buffer = new SoftwareBuffer(16);
        var value = 42.0f;
        buffer.Update(value);

        var read = buffer.Read<float>(0);
        read.Should().Be(42.0f);
    }

    [Fact]
    public void Read_ExceedsBuffer_ThrowsArgumentOutOfRangeException()
    {
        var buffer = new SoftwareBuffer(4);
        buffer.Update(1.0f);

        var act = () => buffer.Read<double>(0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AsSpan_ReturnsCorrectLength()
    {
        var buffer = new SoftwareBuffer(24);
        buffer.UpdateArray(SpanValues);

        var span = buffer.AsSpan<float>();
        span.Length.Should().Be(6);
        span[0].Should().Be(1.0f);
        span[5].Should().Be(6.0f);
    }

    [Fact]
    public void SetData_Array_ExceedsBuffer_ThrowsArgumentOutOfRangeException()
    {
        var buffer = new SoftwareBuffer(8);
        var data = new[] { 1.0f, 2.0f, 3.0f }; // 24 bytes

        var act = () => buffer.SetData(data, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
