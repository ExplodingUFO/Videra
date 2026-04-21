using FluentAssertions;
using Videra.Avalonia.Runtime.Scene;
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

        result.SceneObjects.Should().HaveCount(2);
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
