using FluentAssertions;
using Moq;
using System.Numerics;
using System.Globalization;
using Videra.Core.Exceptions;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Software;
using Videra.Core.Scene;
using Videra.Import.Gltf;
using Videra.Import.Obj;
using Xunit;

namespace Videra.Core.Tests.IO;

public sealed class ModelImporterTests : IDisposable
{
    private readonly string _tempDir;
    private bool _disposed;

    public ModelImporterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"VideraCoreTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // Best-effort temp cleanup for test data.
        }

        GC.SuppressFinalize(this);
    }

    private string WriteObj(string fileName, string content)
    {
        return WriteTestFile(fileName, content);
    }

    private string WriteGltf(string fileName, string content)
    {
        return WriteTestFile(fileName, content);
    }

    private string WriteTestFile(string fileName, string content)
    {
        var path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void Import_CalledWithNullPath_ThrowsInvalidModelInputException()
    {
        var act = () => GltfModelImporter.Import(null!);

        act.Should().Throw<InvalidModelInputException>()
            .Which.Operation.Should().Be("LoadModel");
    }

    [Fact]
    public void Import_CalledWithEmptyPath_ThrowsInvalidModelInputException()
    {
        var act = () => GltfModelImporter.Import(string.Empty);

        var exception = act.Should().Throw<InvalidModelInputException>().Which;
        exception.Operation.Should().Be("LoadModel");
        exception.Context.Should().ContainKey("FilePath");
    }

    [Fact]
    public void Import_CalledWithWhitespacePath_ThrowsInvalidModelInputException()
    {
        var act = () => GltfModelImporter.Import("   ");

        act.Should().Throw<InvalidModelInputException>();
    }

    [Fact]
    public void Import_CalledWithDirectoryPath_ThrowsInvalidModelInputException()
    {
        var act = () => GltfModelImporter.Import(AppContext.BaseDirectory);

        var exception = act.Should().Throw<InvalidModelInputException>().Which;
        exception.Context.Should().ContainKey("NormalizedPath");
    }

    [Fact]
    public void Import_CalledWithNonExistentFile_ThrowsInvalidModelInputException()
    {
        var act = () => GltfModelImporter.Import("nonexistent_file.gltf");

        var exception = act.Should().Throw<InvalidModelInputException>().Which;
        exception.Context.Should().ContainKey("NormalizedPath");
    }

    [Fact]
    public void Import_CalledWithUnsupportedExtension_ThrowsInvalidModelInputException()
    {
        var act = () => ObjModelImporter.Import("model.xyz");

        var exception = act.Should().Throw<InvalidModelInputException>().Which;
        exception.Message.Should().Contain("supported");
        exception.Context.Should().ContainKey("Extension");
    }

    [Fact]
    public void SupportedFormats_Contains_ExpectedExtensions()
    {
        GltfModelImporter.SupportedFormats.Should().Contain("*.gltf");
        GltfModelImporter.SupportedFormats.Should().Contain("*.glb");
        ObjModelImporter.SupportedFormats.Should().Contain("*.obj");
    }

    [Fact]
    public void SupportedFormats_HasThreeFormats()
    {
        GltfModelImporter.SupportedFormats.Concat(ObjModelImporter.SupportedFormats).Should().HaveCount(3);
    }

    [Fact]
    public void Upload_NullFactory_ThrowsArgumentNullException()
    {
        var asset = ObjModelImporter.Import(WriteObj("upload_guard.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.5 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """));

        var act = () => SceneUploadCoordinator.Upload(asset, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateDeferredObject_ImportedObjAsset_ReturnsDeferredSceneObject()
    {
        var asset = ObjModelImporter.Import(WriteObj("deferred_triangle.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.5 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """));

        var obj = SceneUploadCoordinator.CreateDeferredObject(asset);

        obj.Should().NotBeNull();
        obj.Name.Should().Contain("deferred_triangle.obj");
    }

    [Fact]
    public void ImportAndUpload_ObjTriangle_ProducesCorrectObject()
    {
        var factory = new SoftwareResourceFactory();
        var path = WriteObj("triangle.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.5 1.0 0.0
            vn 0.0 0.0 1.0
            vn 0.0 0.0 1.0
            vn 0.0 0.0 1.0
            f 1/1 2/2 3/3
            """);

        var asset = ObjModelImporter.Import(path);
        var obj = SceneUploadCoordinator.Upload(asset, factory);

        obj.Should().NotBeNull();
        obj.Name.Should().Contain("triangle.obj");
    }

    [Fact]
    public void Import_ObjTriangle_ProducesSingleNodeAndPrimitiveContract()
    {
        var path = WriteObj("triangle_contract.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.5 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);

        var asset = ObjModelImporter.Import(path);

        asset.RootNodes.Should().ContainSingle();
        asset.Nodes.Should().ContainSingle();
        asset.Primitives.Should().ContainSingle();
        asset.Materials.Should().ContainSingle();
        asset.Nodes[0].PrimitiveIds.Should().ContainSingle().Which.Should().Be(asset.Primitives[0].Id);
        asset.Primitives[0].MaterialId.Should().Be(asset.Materials[0].Id);
        asset.Metrics.VertexCount.Should().Be(3);
        asset.Metrics.IndexCount.Should().Be(3);
    }

    [Fact]
    public void ImportAndUpload_ObjTwoTriangles_ProducesCorrectObject()
    {
        var factory = new SoftwareResourceFactory();
        var path = WriteObj("two_triangles.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.5 1.0 0.0
            v 1.5 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            f 2//1 4//1 3//1
            """);

        var asset = ObjModelImporter.Import(path);
        var obj = SceneUploadCoordinator.Upload(asset, factory);

        obj.Should().NotBeNull();
    }

    [Fact]
    public void ImportAndUpload_ObjWithInvariantDecimals_IgnoresCurrentCulture()
    {
        var mockFactory = new Mock<IResourceFactory>();
        var mockVertexBuffer = new Mock<IBuffer>();
        var mockIndexBuffer = new Mock<IBuffer>();
        var mockUniformBuffer = new Mock<IBuffer>();
        VertexPositionNormalColor[]? uploadedVertices = null;

        mockVertexBuffer.Setup(b => b.SizeInBytes).Returns(1024u);
        mockIndexBuffer.Setup(b => b.SizeInBytes).Returns(1024u);
        mockUniformBuffer.Setup(b => b.SizeInBytes).Returns(64u);
        mockVertexBuffer
            .Setup(b => b.SetData(It.IsAny<VertexPositionNormalColor[]>(), 0))
            .Callback<VertexPositionNormalColor[], uint>((data, _) => uploadedVertices = data.ToArray());
        mockIndexBuffer.Setup(b => b.SetData(It.IsAny<uint[]>(), 0));

        mockFactory.Setup(f => f.CreateVertexBuffer(It.IsAny<uint>())).Returns(mockVertexBuffer.Object);
        mockFactory.Setup(f => f.CreateIndexBuffer(It.IsAny<uint>())).Returns(mockIndexBuffer.Object);
        mockFactory.Setup(f => f.CreateUniformBuffer(It.IsAny<uint>())).Returns(mockUniformBuffer.Object);

        var path = WriteObj("culture_invariant.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.5 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);

        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE");

            var act = () => SceneUploadCoordinator.Upload(ObjModelImporter.Import(path), mockFactory.Object);

            act.Should().NotThrow();
            uploadedVertices.Should().NotBeNull();
            uploadedVertices![2].Position.X.Should().BeApproximately(0.5f, 0.0001f);
            uploadedVertices[2].Position.Y.Should().BeApproximately(1.0f, 0.0001f);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public void ImportAndUpload_ObjEmptyFile_ProducesEmptyObject()
    {
        var factory = new SoftwareResourceFactory();
        var path = WriteObj("empty.obj", string.Empty);

        // An OBJ with no faces should still load (produces empty mesh)
        var act = () => SceneUploadCoordinator.Upload(ObjModelImporter.Import(path), factory);
        // Empty mesh with no vertices — Object3D.Initialize may throw or handle gracefully
        // Either way it should not hang or crash unexpectedly
        act.Should().NotThrow<IndexOutOfRangeException>();
    }

    [Fact]
    public void ImportAndUpload_ObjBadIndices_DoesNotCrash()
    {
        var factory = new SoftwareResourceFactory();
        var path = WriteObj("bad_indices.obj", """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 5//1
            """);

        // Should not throw IndexOutOfRangeException from out-of-range vertex index
        var act = () => SceneUploadCoordinator.Upload(ObjModelImporter.Import(path), factory);
        act.Should().NotThrow<IndexOutOfRangeException>();
    }

    [Fact]
    public void ImportAndUpload_GltfWithMalformedNormals_FallsBackToRecoverableImport()
    {
        var factory = new SoftwareResourceFactory();
        var path = WriteGltf("malformed_normals.gltf", """
            {
              "asset": {
                "version": "2.0",
                "generator": "SOLIDWORKSGLTF"
              },
              "scene": 0,
              "scenes": [
                {
                  "nodes": [0]
                }
              ],
              "nodes": [
                {
                  "mesh": 0
                }
              ],
              "meshes": [
                {
                  "primitives": [
                    {
                      "attributes": {
                        "POSITION": 0,
                        "NORMAL": 1
                      },
                      "indices": 2
                    }
                  ]
                }
              ],
              "buffers": [
                {
                  "byteLength": 78,
                  "uri": "data:application/octet-stream;base64,AAAAAAAAAAAAAAAAAACAPwAAAAAAAAAAAAAAAAAAgD8AAAAAAAAAQAAAAAAAAAAAAAAAAAAAQEAAAAAAAAAAAAAAAAAAAIBAAAABAAIA"
                }
              ],
              "bufferViews": [
                {
                  "buffer": 0,
                  "byteOffset": 0,
                  "byteLength": 36,
                  "target": 34962
                },
                {
                  "buffer": 0,
                  "byteOffset": 36,
                  "byteLength": 36,
                  "target": 34962
                },
                {
                  "buffer": 0,
                  "byteOffset": 72,
                  "byteLength": 6,
                  "target": 34963
                }
              ],
              "accessors": [
                {
                  "bufferView": 0,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC3",
                  "min": [0.0, 0.0, 0.0],
                  "max": [1.0, 1.0, 0.0]
                },
                {
                  "bufferView": 1,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC3"
                },
                {
                  "bufferView": 2,
                  "componentType": 5123,
                  "count": 3,
                  "type": "SCALAR"
                }
              ]
            }
            """);

        var act = () => SceneUploadCoordinator.Upload(GltfModelImporter.Import(path), factory);

        act.Should().NotThrow();
    }

    [Fact]
    public void Import_GltfHierarchy_PreservesNodeHierarchyAndSharedPrimitiveIdentity()
    {
        var path = WriteGltf("hierarchy.gltf", """
            {
              "asset": { "version": "2.0" },
              "scene": 0,
              "scenes": [
                { "nodes": [0] }
              ],
              "nodes": [
                { "name": "Root", "children": [1, 2] },
                { "name": "Left", "mesh": 0 },
                { "name": "Right", "mesh": 0 }
              ],
              "meshes": [
                {
                  "name": "SharedMesh",
                  "primitives": [
                    {
                      "attributes": { "POSITION": 0, "NORMAL": 1 },
                      "indices": 2
                    }
                  ]
                }
              ],
              "buffers": [
                {
                  "byteLength": 78,
                  "uri": "data:application/octet-stream;base64,AAAAAAAAAAAAAAAAAACAPwAAAAAAAAAAAAAAAAAAgD8AAAAAAAAAQAAAAAAAAAAAAAAAAAAAQEAAAAAAAAAAAAAAAAAAAIBAAAABAAIA"
                }
              ],
              "bufferViews": [
                { "buffer": 0, "byteOffset": 0, "byteLength": 36, "target": 34962 },
                { "buffer": 0, "byteOffset": 36, "byteLength": 36, "target": 34962 },
                { "buffer": 0, "byteOffset": 72, "byteLength": 6, "target": 34963 }
              ],
              "accessors": [
                {
                  "bufferView": 0,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC3",
                  "min": [0.0, 0.0, 0.0],
                  "max": [1.0, 1.0, 0.0]
                },
                {
                  "bufferView": 1,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC3"
                },
                {
                  "bufferView": 2,
                  "componentType": 5123,
                  "count": 3,
                  "type": "SCALAR"
                }
              ]
            }
            """);

        var asset = GltfModelImporter.Import(path);
        var rootNode = asset.Nodes.Single(node => node.Name == "Root");
        var leftNode = asset.Nodes.Single(node => node.Name == "Left");
        var rightNode = asset.Nodes.Single(node => node.Name == "Right");

        asset.RootNodes.Should().ContainSingle().Which.Should().Be(rootNode);
        asset.Nodes.Should().HaveCount(3);
        asset.Primitives.Should().ContainSingle();
        asset.Materials.Should().ContainSingle();
        leftNode.ParentId.Should().Be(rootNode.Id);
        rightNode.ParentId.Should().Be(rootNode.Id);
        leftNode.PrimitiveIds.Should().ContainSingle().Which.Should().Be(asset.Primitives[0].Id);
        rightNode.PrimitiveIds.Should().ContainSingle().Which.Should().Be(asset.Primitives[0].Id);
        asset.Primitives[0].MaterialId.Should().Be(asset.Materials[0].Id);
    }

    [Fact]
    public void Import_GltfSkipsPrimitiveWithoutPositions_AndKeepsValidGeometry()
    {
        var path = WriteGltf("skip_invalid_primitive.gltf", """
            {
              "asset": { "version": "2.0" },
              "scene": 0,
              "scenes": [
                { "nodes": [0] }
              ],
              "nodes": [
                { "name": "Root", "mesh": 0 }
              ],
              "meshes": [
                {
                  "name": "MixedMesh",
                  "primitives": [
                    {
                      "attributes": { "POSITION": 0, "NORMAL": 1 },
                      "indices": 2
                    },
                    {
                      "attributes": { "NORMAL": 1 },
                      "indices": 2
                    }
                  ]
                }
              ],
              "buffers": [
                {
                  "byteLength": 78,
                  "uri": "data:application/octet-stream;base64,AAAAAAAAAAAAAAAAAACAPwAAAAAAAAAAAAAAAAAAgD8AAAAAAAAAQAAAAAAAAAAAAAAAAAAAQEAAAAAAAAAAAAAAAAAAAIBAAAABAAIA"
                }
              ],
              "bufferViews": [
                { "buffer": 0, "byteOffset": 0, "byteLength": 36, "target": 34962 },
                { "buffer": 0, "byteOffset": 36, "byteLength": 36, "target": 34962 },
                { "buffer": 0, "byteOffset": 72, "byteLength": 6, "target": 34963 }
              ],
              "accessors": [
                {
                  "bufferView": 0,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC3",
                  "min": [0.0, 0.0, 0.0],
                  "max": [1.0, 1.0, 0.0]
                },
                {
                  "bufferView": 1,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC3"
                },
                {
                  "bufferView": 2,
                  "componentType": 5123,
                  "count": 3,
                  "type": "SCALAR"
                }
              ]
            }
            """);

        var asset = GltfModelImporter.Import(path);

        asset.Nodes.Should().ContainSingle();
        asset.Primitives.Should().ContainSingle();
        asset.Materials.Should().ContainSingle();
        asset.Nodes[0].PrimitiveIds.Should().ContainSingle().Which.Should().Be(asset.Primitives[0].Id);
        asset.Primitives[0].MaterialId.Should().Be(asset.Materials[0].Id);
        asset.Metrics.VertexCount.Should().Be(3);
        asset.Metrics.IndexCount.Should().Be(3);
    }

    [Fact]
    public void Import_GltfBaseColorFactor_MapsToMaterialInstance()
    {
        var path = WriteGltf("material_factor.gltf", """
            {
              "asset": { "version": "2.0" },
              "scene": 0,
              "scenes": [
                { "nodes": [0] }
              ],
              "nodes": [
                { "name": "Root", "mesh": 0 }
              ],
              "meshes": [
                {
                  "name": "ColoredMesh",
                  "primitives": [
                    {
                      "attributes": { "POSITION": 0, "NORMAL": 1 },
                      "indices": 2,
                      "material": 0
                    }
                  ]
                }
              ],
              "materials": [
                {
                  "name": "Tinted",
                  "pbrMetallicRoughness": {
                    "baseColorFactor": [0.2, 0.4, 0.6, 0.8]
                  }
                }
              ],
              "buffers": [
                {
                  "byteLength": 78,
                  "uri": "data:application/octet-stream;base64,AAAAAAAAAAAAAAAAAACAPwAAAAAAAAAAAAAAAAAAgD8AAAAAAAAAQAAAAAAAAAAAAAAAAAAAQEAAAAAAAAAAAAAAAAAAAIBAAAABAAIA"
                }
              ],
              "bufferViews": [
                { "buffer": 0, "byteOffset": 0, "byteLength": 36, "target": 34962 },
                { "buffer": 0, "byteOffset": 36, "byteLength": 36, "target": 34962 },
                { "buffer": 0, "byteOffset": 72, "byteLength": 6, "target": 34963 }
              ],
              "accessors": [
                {
                  "bufferView": 0,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC3",
                  "min": [0.0, 0.0, 0.0],
                  "max": [1.0, 1.0, 0.0]
                },
                {
                  "bufferView": 1,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC3"
                },
                {
                  "bufferView": 2,
                  "componentType": 5123,
                  "count": 3,
                  "type": "SCALAR"
                }
              ]
            }
            """);

        var asset = GltfModelImporter.Import(path);

        asset.Materials.Should().ContainSingle();
        asset.Materials[0].Name.Should().Be("Tinted");
        asset.Materials[0].BaseColorFactor.Should().Be(new RgbaFloat(0.2f, 0.4f, 0.6f, 0.8f));
        asset.Primitives.Should().ContainSingle();
        asset.Primitives[0].MaterialId.Should().Be(asset.Materials[0].Id);
    }

    [Fact]
    public void Import_GltfVertexColors_AreMultipliedByBaseColorFactor()
    {
        var path = WriteGltf("vertex_color_factor.gltf", """
            {
              "asset": { "version": "2.0" },
              "scene": 0,
              "scenes": [
                { "nodes": [0] }
              ],
              "nodes": [
                { "name": "Root", "mesh": 0 }
              ],
              "meshes": [
                {
                  "name": "ColoredMesh",
                  "primitives": [
                    {
                      "attributes": { "POSITION": 0, "COLOR_0": 1 },
                      "indices": 2,
                      "material": 0
                    }
                  ]
                }
              ],
              "materials": [
                {
                  "name": "Tinted",
                  "pbrMetallicRoughness": {
                    "baseColorFactor": [0.5, 0.5, 0.5, 0.4]
                  }
                }
              ],
              "buffers": [
                {
                  "byteLength": 90,
                  "uri": "data:application/octet-stream;base64,AAAAAAAAAAAAAAAAAACAPwAAAAAAAAAAAAAAAAAAgD8AAAAAAACAPwAAgD8AAIA/AACAPgAAgD8AAIA/AACAPwAAgD4AAIA/AACAPwAAgD8AAIA+AAABAAIA"
                }
              ],
              "bufferViews": [
                { "buffer": 0, "byteOffset": 0, "byteLength": 36, "target": 34962 },
                { "buffer": 0, "byteOffset": 36, "byteLength": 48, "target": 34962 },
                { "buffer": 0, "byteOffset": 84, "byteLength": 6, "target": 34963 }
              ],
              "accessors": [
                {
                  "bufferView": 0,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC3",
                  "min": [0.0, 0.0, 0.0],
                  "max": [1.0, 1.0, 0.0]
                },
                {
                  "bufferView": 1,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC4"
                },
                {
                  "bufferView": 2,
                  "componentType": 5123,
                  "count": 3,
                  "type": "SCALAR"
                }
              ]
            }
            """);

        var asset = GltfModelImporter.Import(path);

        asset.Primitives.Should().ContainSingle();
        asset.Primitives[0].MeshData.Vertices.Should().OnlyContain(
            vertex => vertex.Color == new RgbaFloat(0.5f, 0.5f, 0.5f, 0.1f));
    }

    [Fact]
    public void Import_GltfBaseColorTexture_MapsTextureAndSamplerCatalogs()
    {
        var path = WriteGltf("material_texture.gltf", """
            {
              "asset": { "version": "2.0" },
              "scene": 0,
              "scenes": [
                { "nodes": [0] }
              ],
              "nodes": [
                { "name": "Root", "mesh": 0 }
              ],
              "meshes": [
                {
                  "name": "TexturedMesh",
                  "primitives": [
                    {
                      "attributes": { "POSITION": 0, "TEXCOORD_0": 1 },
                      "indices": 2,
                      "material": 0
                    }
                  ]
                }
              ],
              "materials": [
                {
                  "name": "Textured",
                  "pbrMetallicRoughness": {
                    "baseColorTexture": { "index": 0 }
                  }
                }
              ],
              "textures": [
                { "sampler": 0, "source": 0 }
              ],
              "samplers": [
                {
                  "minFilter": 9987,
                  "magFilter": 9728,
                  "wrapS": 33648,
                  "wrapT": 33071
                }
              ],
              "images": [
                {
                  "name": "BaseColorImage",
                  "uri": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO7ZcE0AAAAASUVORK5CYII="
                }
              ],
              "buffers": [
                {
                  "byteLength": 66,
                  "uri": "data:application/octet-stream;base64,AAAAAAAAAAAAAAAAAACAPwAAAAAAAAAAAAAAAAAAgD8AAAAAAAAAAAAAAAAAAIA/AAAAAAAAAAAAAIA/AAABAAIA"
                }
              ],
              "bufferViews": [
                { "buffer": 0, "byteOffset": 0, "byteLength": 36, "target": 34962 },
                { "buffer": 0, "byteOffset": 36, "byteLength": 24, "target": 34962 },
                { "buffer": 0, "byteOffset": 60, "byteLength": 6, "target": 34963 }
              ],
              "accessors": [
                {
                  "bufferView": 0,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC3",
                  "min": [0.0, 0.0, 0.0],
                  "max": [1.0, 1.0, 0.0]
                },
                {
                  "bufferView": 1,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC2"
                },
                {
                  "bufferView": 2,
                  "componentType": 5123,
                  "count": 3,
                  "type": "SCALAR"
                }
              ]
            }
            """);

        var asset = GltfModelImporter.Import(path);
        var material = asset.Materials.Should().ContainSingle().Subject;
        var texture = asset.Textures.Should().ContainSingle().Subject;
        var sampler = asset.Samplers.Should().ContainSingle().Subject;
        var mesh = asset.Primitives.Should().ContainSingle().Subject.MeshData;

        material.Name.Should().Be("Textured");
        material.BaseColorTexture.Should().NotBeNull();
        material.BaseColorTexture!.TextureId.Should().Be(texture.Id);
        material.BaseColorTexture.SamplerId.Should().Be(sampler.Id);
        material.BaseColorTexture.CoordinateSet.Should().Be(0);
        material.BaseColorTexture.ColorSpace.Should().Be(TextureColorSpace.Srgb);
        texture.Name.Should().Be("BaseColorImage");
        texture.Width.Should().Be(1);
        texture.Height.Should().Be(1);
        texture.ContentFormat.Should().Be(TextureImageFormat.Png);
        texture.ContentBytes.Length.Should().BeGreaterThan(0);
        sampler.MinFilter.Should().Be(TextureFilter.Linear);
        sampler.MagFilter.Should().Be(TextureFilter.Nearest);
        sampler.WrapU.Should().Be(TextureWrapMode.MirroredRepeat);
        sampler.WrapV.Should().Be(TextureWrapMode.ClampToEdge);
        asset.Primitives.Should().ContainSingle();
        asset.Primitives[0].MaterialId.Should().Be(material.Id);
        mesh.TextureCoordinateSets.Should().ContainSingle();
        mesh.TextureCoordinateSets[0].SetIndex.Should().Be(0);
        mesh.TextureCoordinateSets[0].Coordinates.ToArray().Should().Equal(
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f));
    }

    [Fact]
    public void Import_GltfBaseColorTexture_MapsRequestedTextureCoordinateSet()
    {
        var path = WriteGltf("material_texture_texcoord1.gltf", """
            {
              "asset": { "version": "2.0" },
              "scene": 0,
              "scenes": [
                { "nodes": [0] }
              ],
              "nodes": [
                { "name": "Root", "mesh": 0 }
              ],
              "meshes": [
                {
                  "name": "TexturedMesh",
                  "primitives": [
                    {
                      "attributes": { "POSITION": 0, "TEXCOORD_1": 1 },
                      "indices": 2,
                      "material": 0
                    }
                  ]
                }
              ],
              "materials": [
                {
                  "name": "Textured",
                  "pbrMetallicRoughness": {
                    "baseColorTexture": { "index": 0, "texCoord": 1 }
                  }
                }
              ],
              "textures": [
                { "sampler": 0, "source": 0 }
              ],
              "samplers": [
                {
                  "minFilter": 9729,
                  "magFilter": 9729,
                  "wrapS": 10497,
                  "wrapT": 10497
                }
              ],
              "images": [
                {
                  "name": "BaseColorImage",
                  "uri": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO7ZcE0AAAAASUVORK5CYII="
                }
              ],
              "buffers": [
                {
                  "byteLength": 66,
                  "uri": "data:application/octet-stream;base64,AAAAAAAAAAAAAAAAAACAPwAAAAAAAAAAAAAAAAAAgD8AAAAAAAAAAAAAAAAAAIA/AAAAAAAAAAAAAIA/AAABAAIA"
                }
              ],
              "bufferViews": [
                { "buffer": 0, "byteOffset": 0, "byteLength": 36, "target": 34962 },
                { "buffer": 0, "byteOffset": 36, "byteLength": 24, "target": 34962 },
                { "buffer": 0, "byteOffset": 60, "byteLength": 6, "target": 34963 }
              ],
              "accessors": [
                {
                  "bufferView": 0,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC3",
                  "min": [0.0, 0.0, 0.0],
                  "max": [1.0, 1.0, 0.0]
                },
                {
                  "bufferView": 1,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC2"
                },
                {
                  "bufferView": 2,
                  "componentType": 5123,
                  "count": 3,
                  "type": "SCALAR"
                }
              ]
            }
            """);

        var asset = GltfModelImporter.Import(path);
        var material = asset.Materials.Should().ContainSingle().Subject;
        var mesh = asset.Primitives.Should().ContainSingle().Subject.MeshData;

        material.BaseColorTexture.Should().NotBeNull();
        material.BaseColorTexture!.CoordinateSet.Should().Be(1);
        mesh.TextureCoordinateSets.Should().ContainSingle();
        mesh.TextureCoordinateSets[0].SetIndex.Should().Be(1);
    }

    [Fact]
    public void Import_GltfTextureTransform_MapsOffsetScaleRotationAndTexCoordOverride()
    {
        var path = WriteGltf("material_texture_transform.gltf", """
            {
              "asset": { "version": "2.0" },
              "extensionsUsed": ["KHR_texture_transform"],
              "scene": 0,
              "scenes": [
                { "nodes": [0] }
              ],
              "nodes": [
                { "name": "Root", "mesh": 0 }
              ],
              "meshes": [
                {
                  "name": "TexturedMesh",
                  "primitives": [
                    {
                      "attributes": { "POSITION": 0, "TEXCOORD_0": 1 },
                      "indices": 2,
                      "material": 0
                    }
                  ]
                }
              ],
              "materials": [
                {
                  "name": "Transformed",
                  "pbrMetallicRoughness": {
                    "baseColorTexture": {
                      "index": 0,
                      "texCoord": 0,
                      "extensions": {
                        "KHR_texture_transform": {
                          "offset": [0.25, 0.5],
                          "scale": [2.0, 3.0],
                          "rotation": 1.5707964,
                          "texCoord": 1
                        }
                      }
                    }
                  }
                }
              ],
              "textures": [
                { "sampler": 0, "source": 0 }
              ],
              "samplers": [
                {
                  "minFilter": 9729,
                  "magFilter": 9728,
                  "wrapS": 10497,
                  "wrapT": 10497
                }
              ],
              "images": [
                {
                  "name": "BaseColorImage",
                  "uri": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO7ZcE0AAAAASUVORK5CYII="
                }
              ],
              "buffers": [
                {
                  "byteLength": 66,
                  "uri": "data:application/octet-stream;base64,AAAAAAAAAAAAAAAAAACAPwAAAAAAAAAAAAAAAAAAgD8AAAAAAAAAAAAAAAAAAIA/AAAAAAAAAAAAAIA/AAABAAIA"
                }
              ],
              "bufferViews": [
                { "buffer": 0, "byteOffset": 0, "byteLength": 36, "target": 34962 },
                { "buffer": 0, "byteOffset": 36, "byteLength": 24, "target": 34962 },
                { "buffer": 0, "byteOffset": 60, "byteLength": 6, "target": 34963 }
              ],
              "accessors": [
                {
                  "bufferView": 0,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC3",
                  "min": [0.0, 0.0, 0.0],
                  "max": [1.0, 1.0, 0.0]
                },
                {
                  "bufferView": 1,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC2"
                },
                {
                  "bufferView": 2,
                  "componentType": 5123,
                  "count": 3,
                  "type": "SCALAR"
                }
              ]
            }
            """);

        var asset = GltfModelImporter.Import(path);
        var material = asset.Materials.Should().ContainSingle().Subject;

        material.BaseColorTexture.Should().NotBeNull();
        material.BaseColorTexture!.ColorSpace.Should().Be(TextureColorSpace.Srgb);
        material.BaseColorTexture.CoordinateSet.Should().Be(1);
        material.BaseColorTexture.Transform.Should().Be(new MaterialTextureTransform(
            new Vector2(0.25f, 0.5f),
            new Vector2(2.0f, 3.0f),
            1.5707964f));
    }

    [Fact]
    public void Import_GltfOcclusionTexture_MapsStrengthSamplerCoordinateAndColorSpace()
    {
        var path = WriteGltf("material_occlusion.gltf", """
            {
              "asset": { "version": "2.0" },
              "scene": 0,
              "scenes": [
                { "nodes": [0] }
              ],
              "nodes": [
                { "name": "Root", "mesh": 0 }
              ],
              "meshes": [
                {
                  "name": "OccludedMesh",
                  "primitives": [
                    {
                      "attributes": { "POSITION": 0, "TEXCOORD_1": 1 },
                      "indices": 2,
                      "material": 0
                    }
                  ]
                }
              ],
              "materials": [
                {
                  "name": "Occluded",
                  "occlusionTexture": {
                    "index": 0,
                    "strength": 0.35,
                    "texCoord": 1
                  }
                }
              ],
              "textures": [
                { "sampler": 0, "source": 0 }
              ],
              "samplers": [
                {
                  "minFilter": 9987,
                  "magFilter": 9728,
                  "wrapS": 33648,
                  "wrapT": 33071
                }
              ],
              "images": [
                {
                  "name": "OcclusionImage",
                  "uri": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO7ZcE0AAAAASUVORK5CYII="
                }
              ],
              "buffers": [
                {
                  "byteLength": 66,
                  "uri": "data:application/octet-stream;base64,AAAAAAAAAAAAAAAAAACAPwAAAAAAAAAAAAAAAAAAgD8AAAAAAAAAAAAAAAAAAIA/AAAAAAAAAAAAAIA/AAABAAIA"
                }
              ],
              "bufferViews": [
                { "buffer": 0, "byteOffset": 0, "byteLength": 36, "target": 34962 },
                { "buffer": 0, "byteOffset": 36, "byteLength": 24, "target": 34962 },
                { "buffer": 0, "byteOffset": 60, "byteLength": 6, "target": 34963 }
              ],
              "accessors": [
                {
                  "bufferView": 0,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC3",
                  "min": [0.0, 0.0, 0.0],
                  "max": [1.0, 1.0, 0.0]
                },
                {
                  "bufferView": 1,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC2"
                },
                {
                  "bufferView": 2,
                  "componentType": 5123,
                  "count": 3,
                  "type": "SCALAR"
                }
              ]
            }
            """);

        var asset = GltfModelImporter.Import(path);
        var material = asset.Materials.Should().ContainSingle().Subject;
        var texture = asset.Textures.Should().ContainSingle().Subject;
        var sampler = asset.Samplers.Should().ContainSingle().Subject;

        material.OcclusionTexture.Should().NotBeNull();
        material.OcclusionTexture!.Strength.Should().Be(0.35f);
        material.OcclusionTexture.Texture.TextureId.Should().Be(texture.Id);
        material.OcclusionTexture.Texture.SamplerId.Should().Be(sampler.Id);
        material.OcclusionTexture.Texture.CoordinateSet.Should().Be(1);
        material.OcclusionTexture.Texture.ColorSpace.Should().Be(TextureColorSpace.Linear);
        sampler.MinFilter.Should().Be(TextureFilter.Linear);
        sampler.MagFilter.Should().Be(TextureFilter.Nearest);
        sampler.WrapU.Should().Be(TextureWrapMode.MirroredRepeat);
        sampler.WrapV.Should().Be(TextureWrapMode.ClampToEdge);
    }

    [Fact]
    public void Import_GltfStaticPbrMaterial_MapsExtendedMaterialSemantics()
    {
        var path = WriteGltf("material_pbr_semantics.gltf", """
            {
              "asset": { "version": "2.0" },
              "scene": 0,
              "scenes": [
                { "nodes": [0] }
              ],
              "nodes": [
                { "name": "Root", "mesh": 0 }
              ],
              "meshes": [
                {
                  "name": "PbrMesh",
                  "primitives": [
                    {
                      "attributes": { "POSITION": 0, "TEXCOORD_0": 1 },
                      "indices": 2,
                      "material": 0
                    }
                  ]
                }
              ],
              "materials": [
                {
                  "name": "PbrMaterial",
                  "pbrMetallicRoughness": {
                    "baseColorFactor": [0.9, 0.8, 0.7, 0.6],
                    "baseColorTexture": { "index": 0 },
                    "metallicFactor": 0.25,
                    "roughnessFactor": 0.75,
                    "metallicRoughnessTexture": { "index": 1 }
                  },
                  "normalTexture": { "index": 2, "scale": 0.5 },
                  "emissiveFactor": [0.1, 0.2, 0.3],
                  "emissiveTexture": { "index": 3 },
                  "alphaMode": "MASK",
                  "alphaCutoff": 0.42,
                  "doubleSided": true
                }
              ],
              "textures": [
                { "sampler": 0, "source": 0 },
                { "sampler": 0, "source": 1 },
                { "sampler": 0, "source": 2 },
                { "sampler": 0, "source": 3 }
              ],
              "samplers": [
                {
                  "minFilter": 9729,
                  "magFilter": 9729,
                  "wrapS": 10497,
                  "wrapT": 10497
                }
              ],
              "images": [
                {
                  "name": "BaseColorImage",
                  "uri": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO7ZcE0AAAAASUVORK5CYII="
                },
                {
                  "name": "MetallicRoughnessImage",
                  "uri": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO7ZcE0AAAAASUVORK5CYII="
                },
                {
                  "name": "NormalImage",
                  "uri": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO7ZcE0AAAAASUVORK5CYII="
                },
                {
                  "name": "EmissiveImage",
                  "uri": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO7ZcE0AAAAASUVORK5CYII="
                }
              ],
              "buffers": [
                {
                  "byteLength": 66,
                  "uri": "data:application/octet-stream;base64,AAAAAAAAAAAAAAAAAACAPwAAAAAAAAAAAAAAAAAAgD8AAAAAAAAAAAAAAAAAAIA/AAAAAAAAAAAAAIA/AAABAAIA"
                }
              ],
              "bufferViews": [
                { "buffer": 0, "byteOffset": 0, "byteLength": 36, "target": 34962 },
                { "buffer": 0, "byteOffset": 36, "byteLength": 24, "target": 34962 },
                { "buffer": 0, "byteOffset": 60, "byteLength": 6, "target": 34963 }
              ],
              "accessors": [
                {
                  "bufferView": 0,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC3",
                  "min": [0.0, 0.0, 0.0],
                  "max": [1.0, 1.0, 0.0]
                },
                {
                  "bufferView": 1,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC2"
                },
                {
                  "bufferView": 2,
                  "componentType": 5123,
                  "count": 3,
                  "type": "SCALAR"
                }
              ]
            }
            """);

        var asset = GltfModelImporter.Import(path);
        var material = asset.Materials.Should().ContainSingle().Subject;
        var sampler = asset.Samplers.Should().ContainSingle().Subject;
        var baseColorTexture = asset.Textures.Single(texture => texture.Name == "BaseColorImage");
        var metallicRoughnessTexture = asset.Textures.Single(texture => texture.Name == "MetallicRoughnessImage");
        var normalTexture = asset.Textures.Single(texture => texture.Name == "NormalImage");
        var emissiveTexture = asset.Textures.Single(texture => texture.Name == "EmissiveImage");

        asset.Textures.Should().HaveCount(4);
        material.Name.Should().Be("PbrMaterial");
        material.BaseColorFactor.Should().Be(new RgbaFloat(0.9f, 0.8f, 0.7f, 0.6f));
        material.BaseColorTexture.Should().NotBeNull();
        material.BaseColorTexture!.TextureId.Should().Be(baseColorTexture.Id);
        material.BaseColorTexture.SamplerId.Should().Be(sampler.Id);
        material.BaseColorTexture.CoordinateSet.Should().Be(0);
        material.BaseColorTexture.ColorSpace.Should().Be(TextureColorSpace.Srgb);
        material.MetallicRoughness.MetallicFactor.Should().Be(0.25f);
        material.MetallicRoughness.RoughnessFactor.Should().Be(0.75f);
        material.MetallicRoughness.Texture.Should().NotBeNull();
        material.MetallicRoughness.Texture!.TextureId.Should().Be(metallicRoughnessTexture.Id);
        material.MetallicRoughness.Texture.SamplerId.Should().Be(sampler.Id);
        material.MetallicRoughness.Texture.CoordinateSet.Should().Be(0);
        material.MetallicRoughness.Texture.ColorSpace.Should().Be(TextureColorSpace.Linear);
        material.Alpha.Should().Be(new MaterialAlphaSettings(MaterialAlphaMode.Mask, 0.42f, true));
        material.Emissive.Factor.Should().Be(new Vector3(0.1f, 0.2f, 0.3f));
        material.Emissive.Texture.Should().NotBeNull();
        material.Emissive.Texture!.TextureId.Should().Be(emissiveTexture.Id);
        material.Emissive.Texture.SamplerId.Should().Be(sampler.Id);
        material.Emissive.Texture.CoordinateSet.Should().Be(0);
        material.Emissive.Texture.ColorSpace.Should().Be(TextureColorSpace.Srgb);
        material.NormalTexture.Should().NotBeNull();
        material.NormalTexture!.Texture.TextureId.Should().Be(normalTexture.Id);
        material.NormalTexture.Texture.SamplerId.Should().Be(sampler.Id);
        material.NormalTexture.Texture.CoordinateSet.Should().Be(0);
        material.NormalTexture.Texture.ColorSpace.Should().Be(TextureColorSpace.Linear);
        material.NormalTexture.Scale.Should().Be(0.5f);
    }

    [Fact]
    public void Import_GltfBlendMaterial_PreservesBlendAlphaMode()
    {
        var path = WriteGltf("blend_material.gltf", """
            {
              "asset": { "version": "2.0" },
              "scene": 0,
              "scenes": [
                { "nodes": [0] }
              ],
              "nodes": [
                { "mesh": 0 }
              ],
              "meshes": [
                {
                  "primitives": [
                    {
                      "attributes": { "POSITION": 0 },
                      "indices": 1,
                      "material": 0
                    }
                  ]
                }
              ],
              "materials": [
                {
                  "name": "BlendMaterial",
                  "alphaMode": "BLEND",
                  "doubleSided": true,
                  "pbrMetallicRoughness": {
                    "baseColorFactor": [0.8, 0.7, 0.6, 0.5]
                  }
                }
              ],
              "buffers": [
                {
                  "byteLength": 42,
                  "uri": "data:application/octet-stream;base64,AAAAAAAAAAAAAAAAAACAPwAAAAAAAAAAAAAAAAAAgD8AAAAAAAABAAIA"
                }
              ],
              "bufferViews": [
                { "buffer": 0, "byteOffset": 0, "byteLength": 36, "target": 34962 },
                { "buffer": 0, "byteOffset": 36, "byteLength": 6, "target": 34963 }
              ],
              "accessors": [
                {
                  "bufferView": 0,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC3",
                  "min": [0.0, 0.0, 0.0],
                  "max": [1.0, 1.0, 0.0]
                },
                {
                  "bufferView": 1,
                  "componentType": 5123,
                  "count": 3,
                  "type": "SCALAR"
                }
              ]
            }
            """);

        var asset = GltfModelImporter.Import(path);
        var material = asset.Materials.Should().ContainSingle().Subject;

        material.Name.Should().Be("BlendMaterial");
        material.BaseColorFactor.Should().Be(new RgbaFloat(0.8f, 0.7f, 0.6f, 0.5f));
        material.Alpha.Should().Be(new MaterialAlphaSettings(MaterialAlphaMode.Blend, 0.5f, true));
    }

    [Fact]
    public void Import_GltfTangents_RetainsTangentDataOnPrimitiveMesh()
    {
        var bufferBytes = BuildTriangleBufferWithTangents();
        var path = WriteGltf("tangent.gltf", $$"""
            {
              "asset": { "version": "2.0" },
              "scene": 0,
              "scenes": [
                { "nodes": [0] }
              ],
              "nodes": [
                { "name": "Root", "mesh": 0 }
              ],
              "meshes": [
                {
                  "name": "TangentMesh",
                  "primitives": [
                    {
                      "attributes": { "POSITION": 0, "NORMAL": 1, "TANGENT": 2 },
                      "indices": 3
                    }
                  ]
                }
              ],
              "buffers": [
                {
                  "byteLength": {{bufferBytes.Length}},
                  "uri": "data:application/octet-stream;base64,{{Convert.ToBase64String(bufferBytes)}}"
                }
              ],
              "bufferViews": [
                { "buffer": 0, "byteOffset": 0, "byteLength": 36, "target": 34962 },
                { "buffer": 0, "byteOffset": 36, "byteLength": 36, "target": 34962 },
                { "buffer": 0, "byteOffset": 72, "byteLength": 48, "target": 34962 },
                { "buffer": 0, "byteOffset": 120, "byteLength": 6, "target": 34963 }
              ],
              "accessors": [
                {
                  "bufferView": 0,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC3",
                  "min": [0.0, 0.0, 0.0],
                  "max": [1.0, 1.0, 0.0]
                },
                {
                  "bufferView": 1,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC3"
                },
                {
                  "bufferView": 2,
                  "componentType": 5126,
                  "count": 3,
                  "type": "VEC4"
                },
                {
                  "bufferView": 3,
                  "componentType": 5123,
                  "count": 3,
                  "type": "SCALAR"
                }
              ]
            }
            """);

        var asset = GltfModelImporter.Import(path);
        var primitive = asset.Primitives.Should().ContainSingle().Subject;

        primitive.MeshData.Tangents.Should().Equal(
            new Vector4(1f, 0f, 0f, 1f),
            new Vector4(1f, 0f, 0f, 1f),
            new Vector4(1f, 0f, 0f, 1f));
    }

    private static byte[] BuildTriangleBufferWithTangents()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        foreach (var value in new[]
                 {
                     0f, 0f, 0f,
                     1f, 0f, 0f,
                     0f, 1f, 0f
                 })
        {
            writer.Write(value);
        }

        foreach (var value in new[]
                 {
                     0f, 0f, 1f,
                     0f, 0f, 1f,
                     0f, 0f, 1f
                 })
        {
            writer.Write(value);
        }

        foreach (var value in new[]
                 {
                     1f, 0f, 0f, 1f,
                     1f, 0f, 0f, 1f,
                     1f, 0f, 0f, 1f
                 })
        {
            writer.Write(value);
        }

        writer.Write((ushort)0);
        writer.Write((ushort)1);
        writer.Write((ushort)2);

        return stream.ToArray();
    }
}
