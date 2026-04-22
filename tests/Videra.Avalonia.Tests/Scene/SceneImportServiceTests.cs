using System.Numerics;
using System.Reflection;
using FluentAssertions;
using Videra.Avalonia.Runtime.Scene;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Avalonia.Tests.Scene;

public sealed class SceneImportServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly SceneImportService _service = new(new SceneDocumentMutator());

    public SceneImportServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"VideraSceneImport_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public async Task Import_batch_preserves_input_order_and_reports_failures_without_throwing()
    {
        var first = WriteObj("a.obj");
        var missing = Path.Combine(_tempDir, "missing.obj");
        var second = WriteObj("b.obj");

        var result = await _service.ImportBatchAsync([first, missing, second], CancellationToken.None);

        result.Entries.Should().HaveCount(2);
        result.Entries[0].ImportedAsset!.FilePath.Should().Be(first);
        result.Entries[1].ImportedAsset!.FilePath.Should().Be(second);
        result.Failures.Should().ContainSingle().Which.Path.Should().Be(missing);
    }

    [Fact]
    public async Task Import_single_preserves_material_catalog_on_imported_asset()
    {
        var path = WriteObj("material.obj");

        var result = await _service.ImportSingleAsync(path, CancellationToken.None);

        result.Entry.Should().NotBeNull();
        result.Entry!.ImportedAsset.Should().NotBeNull();
        result.Entry.ImportedAsset!.Materials.Should().ContainSingle();
        result.Entry.ImportedAsset.Primitives.Should().ContainSingle();
        result.Entry.ImportedAsset.Primitives[0].MaterialId.Should().Be(result.Entry.ImportedAsset.Materials[0].Id);
    }

    [Fact]
    public async Task Import_single_preserves_gltf_texture_binding_and_uv_truth_on_imported_asset()
    {
        var path = WriteGltf("textured.gltf", """
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

        var result = await _service.ImportSingleAsync(path, CancellationToken.None);

        result.Entry.Should().NotBeNull();
        result.Entry!.ImportedAsset.Should().NotBeNull();
        result.Entry.ImportedAsset!.Textures.Should().ContainSingle();
        result.Entry.ImportedAsset.Samplers.Should().ContainSingle();
        result.Entry.ImportedAsset.Materials.Should().ContainSingle();
        result.Entry.ImportedAsset.Primitives.Should().ContainSingle();

        var material = result.Entry.ImportedAsset.Materials[0];
        var texture = result.Entry.ImportedAsset.Textures[0];
        var sampler = result.Entry.ImportedAsset.Samplers[0];
        var mesh = result.Entry.ImportedAsset.Primitives[0].MeshData;

        material.BaseColorTexture.Should().NotBeNull();
        material.BaseColorTexture!.TextureId.Should().Be(texture.Id);
        material.BaseColorTexture.SamplerId.Should().Be(sampler.Id);
        material.BaseColorTexture.CoordinateSet.Should().Be(0);
        material.BaseColorTexture.ColorSpace.Should().Be(TextureColorSpace.Srgb);
        mesh.TextureCoordinateSets.Should().ContainSingle();
        mesh.TextureCoordinateSets[0].SetIndex.Should().Be(0);
    }

    [Fact]
    public async Task Import_single_preserves_gltf_pbr_material_semantics_on_imported_asset()
    {
        var path = WriteGltf("pbr.gltf", """
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

        var result = await _service.ImportSingleAsync(path, CancellationToken.None);

        result.Entry.Should().NotBeNull();
        result.Entry!.ImportedAsset.Should().NotBeNull();
        result.Entry.ImportedAsset!.Textures.Should().HaveCount(4);
        result.Entry.ImportedAsset.Samplers.Should().ContainSingle();
        result.Entry.ImportedAsset.Materials.Should().ContainSingle();

        var material = result.Entry.ImportedAsset.Materials[0];
        var sampler = result.Entry.ImportedAsset.Samplers[0];
        var baseColorTexture = result.Entry.ImportedAsset.Textures.Single(texture => texture.Name == "BaseColorImage");
        var metallicRoughnessTexture = result.Entry.ImportedAsset.Textures.Single(texture => texture.Name == "MetallicRoughnessImage");
        var normalTexture = result.Entry.ImportedAsset.Textures.Single(texture => texture.Name == "NormalImage");
        var emissiveTexture = result.Entry.ImportedAsset.Textures.Single(texture => texture.Name == "EmissiveImage");

        material.BaseColorFactor.Should().Be(new RgbaFloat(0.9f, 0.8f, 0.7f, 0.6f));
        material.BaseColorTexture.Should().NotBeNull();
        material.BaseColorTexture!.TextureId.Should().Be(baseColorTexture.Id);
        material.BaseColorTexture.SamplerId.Should().Be(sampler.Id);
        material.BaseColorTexture.ColorSpace.Should().Be(TextureColorSpace.Srgb);
        material.MetallicRoughness.MetallicFactor.Should().Be(0.25f);
        material.MetallicRoughness.RoughnessFactor.Should().Be(0.75f);
        material.MetallicRoughness.Texture.Should().NotBeNull();
        material.MetallicRoughness.Texture!.TextureId.Should().Be(metallicRoughnessTexture.Id);
        material.MetallicRoughness.Texture.ColorSpace.Should().Be(TextureColorSpace.Linear);
        material.Alpha.Should().Be(new MaterialAlphaSettings(MaterialAlphaMode.Mask, 0.42f, true));
        material.Emissive.Factor.Should().Be(new Vector3(0.1f, 0.2f, 0.3f));
        material.Emissive.Texture.Should().NotBeNull();
        material.Emissive.Texture!.TextureId.Should().Be(emissiveTexture.Id);
        material.Emissive.Texture.ColorSpace.Should().Be(TextureColorSpace.Srgb);
        material.NormalTexture.Should().NotBeNull();
        material.NormalTexture!.Texture.TextureId.Should().Be(normalTexture.Id);
        material.NormalTexture.Texture.ColorSpace.Should().Be(TextureColorSpace.Linear);
        material.NormalTexture.Scale.Should().Be(0.5f);
    }

    [Fact]
    public async Task Import_single_reuses_same_imported_asset_for_same_unchanged_path()
    {
        var path = WriteTriangleGltf("reused-single.gltf");

        var first = await _service.ImportSingleAsync(path, CancellationToken.None);
        var second = await _service.ImportSingleAsync(path, CancellationToken.None);

        first.Entry.Should().NotBeNull();
        second.Entry.Should().NotBeNull();

        first.Entry!.ImportedAsset.Should().BeSameAs(second.Entry!.ImportedAsset);
        first.Entry.SceneObject.Should().NotBeSameAs(second.Entry.SceneObject);

        var payloadProperty = typeof(Object3D).GetProperty("MeshPayload", BindingFlags.Instance | BindingFlags.NonPublic);
        payloadProperty.Should().NotBeNull();
        payloadProperty!.GetValue(first.Entry.SceneObject).Should().BeSameAs(payloadProperty.GetValue(second.Entry.SceneObject));
    }

    [Fact]
    public async Task Import_batch_reuses_same_imported_asset_for_duplicate_paths()
    {
        var path = WriteTriangleGltf("reused-batch.gltf");

        var result = await _service.ImportBatchAsync([path, path], CancellationToken.None);

        result.Failures.Should().BeEmpty();
        result.Entries.Should().HaveCount(2);
        result.Entries[0].ImportedAsset.Should().BeSameAs(result.Entries[1].ImportedAsset);
        result.Entries[0].SceneObject.Should().NotBeSameAs(result.Entries[1].SceneObject);

        var payloadProperty = typeof(Object3D).GetProperty("MeshPayload", BindingFlags.Instance | BindingFlags.NonPublic);
        payloadProperty.Should().NotBeNull();
        payloadProperty!.GetValue(result.Entries[0].SceneObject).Should().BeSameAs(payloadProperty.GetValue(result.Entries[1].SceneObject));
    }

    [Fact]
    public async Task Import_batch_preserves_request_path_truth_for_equivalent_spellings()
    {
        var path = WriteTriangleGltf("truth-batch.gltf");
        var dotSegmentPath = Path.Combine(_tempDir, ".", "truth-batch.gltf");

        var result = await _service.ImportBatchAsync([path, dotSegmentPath], CancellationToken.None);

        result.Failures.Should().BeEmpty();
        result.Entries.Should().HaveCount(2);
        result.Entries[0].ImportedAsset!.FilePath.Should().Be(path);
        result.Entries[1].ImportedAsset!.FilePath.Should().Be(dotSegmentPath);
        result.Entries[0].ImportedAsset.Should().NotBeSameAs(result.Entries[1].ImportedAsset);
    }

    [Fact]
    public async Task Import_batch_reuses_same_imported_asset_across_unchanged_calls()
    {
        var path = WriteTriangleGltf("repeat-batch.gltf");

        var first = await _service.ImportBatchAsync([path], CancellationToken.None);
        var second = await _service.ImportBatchAsync([path], CancellationToken.None);

        first.Failures.Should().BeEmpty();
        second.Failures.Should().BeEmpty();
        first.Entries.Should().ContainSingle();
        second.Entries.Should().ContainSingle();
        first.Entries[0].ImportedAsset.Should().BeSameAs(second.Entries[0].ImportedAsset);
    }

    [Fact]
    public async Task Import_single_does_not_reuse_imported_asset_after_file_timestamp_changes()
    {
        var path = WriteTriangleGltf("reused-single-refresh.gltf");

        var first = await _service.ImportSingleAsync(path, CancellationToken.None);
        File.SetLastWriteTimeUtc(path, File.GetLastWriteTimeUtc(path).AddMinutes(1));
        var second = await _service.ImportSingleAsync(path, CancellationToken.None);

        first.Entry.Should().NotBeNull();
        second.Entry.Should().NotBeNull();
        first.Entry!.ImportedAsset.Should().NotBeSameAs(second.Entry!.ImportedAsset);
    }

    private string WriteObj(string name)
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, """
            v 0.0 0.0 0.0
            v 1.0 0.0 0.0
            v 0.0 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);
        return path;
    }

    private string WriteGltf(string name, string content)
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }

    private string WriteTriangleGltf(string name)
    {
        return WriteGltf(name, """
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
                  "name": "Triangle",
                  "primitives": [
                    {
                      "attributes": { "POSITION": 0 },
                      "indices": 1
                    }
                  ]
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
                  "componentType": 5123,
                  "count": 3,
                  "type": "SCALAR"
                }
              ]
            }
            """);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // Best-effort cleanup for per-test temp assets.
        }
    }
}
