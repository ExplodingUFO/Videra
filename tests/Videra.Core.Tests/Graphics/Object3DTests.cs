using System.Numerics;
using FluentAssertions;
using Moq;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Xunit;

namespace Videra.Core.Tests.Graphics;

public class Object3DTests
{
    private static MeshData CreateTestMesh()
    {
        return new MeshData
        {
            Vertices = new VertexPositionNormalColor[]
            {
                new(new Vector3(0, 0, 0), Vector3.UnitY, RgbaFloat.White),
                new(new Vector3(1, 0, 0), Vector3.UnitY, RgbaFloat.White),
                new(new Vector3(0, 1, 0), Vector3.UnitY, RgbaFloat.White),
            },
            Indices = new uint[] { 0, 1, 2 },
            Topology = MeshTopology.Triangles
        };
    }

    private static Mock<IResourceFactory> CreateMockFactory(out Mock<IBuffer> sharedMockBuffer)
    {
        var mockFactory = new Mock<IResourceFactory>();
        var mockBuffer = new Mock<IBuffer>();
        mockBuffer.Setup(b => b.SizeInBytes).Returns(1024u);

        mockFactory.Setup(f => f.CreateVertexBuffer(It.IsAny<uint>())).Returns(mockBuffer.Object);
        mockFactory.Setup(f => f.CreateIndexBuffer(It.IsAny<uint>())).Returns(mockBuffer.Object);
        mockFactory.Setup(f => f.CreateUniformBuffer(It.IsAny<uint>())).Returns(mockBuffer.Object);

        sharedMockBuffer = mockBuffer;
        return mockFactory;
    }

    private static Mock<IResourceFactory> CreateMockFactory()
    {
        return CreateMockFactory(out _);
    }

    [Fact]
    public void Name_DefaultValue_IsObject()
    {
        // Arrange & Act
        var obj = new Object3D();

        // Assert
        obj.Name.Should().Be("Object");
    }

    [Fact]
    public void Name_CanBeSet_ToCustomValue()
    {
        // Arrange & Act
        var obj = new Object3D { Name = "TestMesh" };

        // Assert
        obj.Name.Should().Be("TestMesh");
    }

    [Fact]
    public void Position_DefaultValue_IsZero()
    {
        // Arrange & Act
        var obj = new Object3D();

        // Assert
        obj.Position.Should().Be(Vector3.Zero);
    }

    [Fact]
    public void Scale_DefaultValue_IsOne()
    {
        // Arrange & Act
        var obj = new Object3D();

        // Assert
        obj.Scale.Should().Be(Vector3.One);
    }

    [Fact]
    public void Rotation_DefaultValue_IsZero()
    {
        // Arrange & Act
        var obj = new Object3D();

        // Assert
        obj.Rotation.Should().Be(Vector3.Zero);
    }

    [Fact]
    public void WorldMatrix_DefaultIdentityPosition_ReturnsIdentityScale()
    {
        // Arrange
        var obj = new Object3D();

        // Act
        var world = obj.WorldMatrix;

        // Assert
        world.Should().Be(Matrix4x4.Identity);
    }

    [Fact]
    public void WorldMatrix_WithTranslation_ContainsPosition()
    {
        // Arrange
        var obj = new Object3D { Position = new Vector3(10, 20, 30) };

        // Act
        var world = obj.WorldMatrix;

        // Assert
        world.Translation.Should().Be(new Vector3(10, 20, 30));
    }

    [Fact]
    public void WorldMatrix_WithScale_ContainsScale()
    {
        // Arrange
        var obj = new Object3D { Scale = new Vector3(2, 3, 4) };

        // Act
        var world = obj.WorldMatrix;

        // Assert
        // Scale matrix diagonal should be (2, 3, 4, 1) for pure scale with identity rotation/translation
        var expectedScale = new Vector3(world.M11, world.M22, world.M33);
        expectedScale.Should().Be(new Vector3(2, 3, 4));
    }

    [Fact]
    public void Initialize_CalledWithValidMesh_CreatesBuffers()
    {
        // Arrange
        var obj = new Object3D { Name = "TestObj" };
        var mockFactory = CreateMockFactory();
        var mesh = CreateTestMesh();

        // Act
        obj.Initialize(mockFactory.Object, mesh);

        // Assert
        mockFactory.Verify(f => f.CreateVertexBuffer(It.IsAny<uint>()), Times.Once);
        mockFactory.Verify(f => f.CreateIndexBuffer(It.IsAny<uint>()), Times.Once);
        mockFactory.Verify(f => f.CreateUniformBuffer(64u), Times.Once);
    }

    [Fact]
    public void Initialize_CalledWithNullMesh_ThrowsArgumentException()
    {
        // Arrange
        var obj = new Object3D();
        var mockFactory = CreateMockFactory();

        // Act
        var act = () => obj.Initialize(mockFactory.Object, null!);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Invalid mesh data*");
    }

    [Fact]
    public void Initialize_CalledWithEmptyVertices_ThrowsArgumentException()
    {
        // Arrange
        var obj = new Object3D();
        var mockFactory = CreateMockFactory();
        var mesh = new MeshData
        {
            Vertices = Array.Empty<VertexPositionNormalColor>(),
            Indices = new uint[] { 0, 1, 2 }
        };

        // Act
        var act = () => obj.Initialize(mockFactory.Object, mesh);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Invalid mesh data*");
    }

    [Fact]
    public void Initialize_CalledWithEmptyIndices_ThrowsArgumentException()
    {
        // Arrange
        var obj = new Object3D();
        var mockFactory = CreateMockFactory();
        var mesh = new MeshData
        {
            Vertices = new VertexPositionNormalColor[]
            {
                new(Vector3.Zero, Vector3.UnitY, RgbaFloat.White)
            },
            Indices = Array.Empty<uint>()
        };

        // Act
        var act = () => obj.Initialize(mockFactory.Object, mesh);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Invalid index data*");
    }

    [Fact]
    public void UpdateUniforms_CalledWithWorldBuffer_SetsData()
    {
        // Arrange
        var obj = new Object3D { Name = "TestObj" };
        var mockFactory = CreateMockFactory();
        var mockBuffer = new Mock<IBuffer>();
        mockFactory.Setup(f => f.CreateVertexBuffer(It.IsAny<uint>())).Returns(mockBuffer.Object);
        mockFactory.Setup(f => f.CreateIndexBuffer(It.IsAny<uint>())).Returns(mockBuffer.Object);
        mockFactory.Setup(f => f.CreateUniformBuffer(It.IsAny<uint>())).Returns(mockBuffer.Object);

        var mesh = CreateTestMesh();
        obj.Initialize(mockFactory.Object, mesh);

        var mockExecutor = new Mock<ICommandExecutor>();

        // Act
        obj.UpdateUniforms(mockExecutor.Object);

        // Assert
        mockBuffer.Verify(b => b.SetData(It.IsAny<Matrix4x4>(), 0), Times.Once);
    }

    [Fact]
    public void InitializeWireframe_CalledAfterInitialize_CreatesLineBuffers()
    {
        // Arrange
        var obj = new Object3D { Name = "TestObj" };
        var mockFactory = CreateMockFactory();
        var mesh = CreateTestMesh();
        obj.Initialize(mockFactory.Object, mesh);

        // Act
        obj.InitializeWireframe(mockFactory.Object);

        // Assert
        // Should have created an index buffer for the line indices
        mockFactory.Verify(f => f.CreateIndexBuffer(It.IsAny<uint>()), Times.AtLeast(2));
    }

    [Fact]
    public void InitializeWireframe_CalledWithoutInitialize_ReturnsEarly()
    {
        // Arrange
        var obj = new Object3D { Name = "TestObj" };
        var mockFactory = CreateMockFactory();

        // Act
        obj.InitializeWireframe(mockFactory.Object);

        // Assert - should not create any additional buffers
        mockFactory.Verify(f => f.CreateIndexBuffer(It.IsAny<uint>()), Times.Never);
    }

    [Fact]
    public void UpdateWireframeColor_CalledAfterWireframeInit_SetsDataOnLineBuffer()
    {
        // Arrange
        var obj = new Object3D { Name = "TestObj" };
        var mockFactory = CreateMockFactory(out var mockBuffer);
        var mesh = CreateTestMesh();
        obj.Initialize(mockFactory.Object, mesh);
        obj.InitializeWireframe(mockFactory.Object);

        // Act
        obj.UpdateWireframeColor(new RgbaFloat(1.0f, 0.0f, 0.0f, 1.0f));

        // Assert - verify SetData was called for the line vertex buffer with colored vertices
        mockBuffer.Verify(b => b.SetData(It.IsAny<VertexPositionNormalColor[]>(), 0), Times.AtLeastOnce);
    }

    [Fact]
    public void Dispose_Called_DisposesBuffers()
    {
        // Arrange
        var obj = new Object3D { Name = "TestObj" };
        var mockFactory = CreateMockFactory(out var mockBuffer);
        var mesh = CreateTestMesh();
        obj.Initialize(mockFactory.Object, mesh);

        // Act
        obj.Dispose();

        // Assert - buffers should be disposed (shared mock, multiple calls)
        mockBuffer.Verify(b => b.Dispose(), Times.AtLeast(3));
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotDoubleDispose()
    {
        var obj = new Object3D { Name = "TestObj" };
        var mockFactory = CreateMockFactory(out var mockBuffer);
        var mesh = CreateTestMesh();
        obj.Initialize(mockFactory.Object, mesh);

        obj.Dispose();
        var firstDisposeCount = mockBuffer.Invocations.Count(i => i.Method.Name == "Dispose");
        obj.Dispose();
        var secondDisposeCount = mockBuffer.Invocations.Count(i => i.Method.Name == "Dispose");

        secondDisposeCount.Should().Be(firstDisposeCount, "second dispose should be a no-op");
    }
}
