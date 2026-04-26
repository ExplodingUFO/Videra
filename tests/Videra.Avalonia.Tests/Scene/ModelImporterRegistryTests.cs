using FluentAssertions;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Runtime.Scene;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Avalonia.Tests.Scene;

public sealed class ModelImporterRegistryTests
{
    [Fact]
    public void UseModelImporter_AddsImporterAndRaisesOptionsChange()
    {
        var options = new VideraViewOptions();
        var importer = new StubModelImporter(static _ => true);
        var changes = new List<string?>();
        options.PropertyChanged += (_, args) => changes.Add(args.PropertyName);

        var returned = options.UseModelImporter(importer);

        returned.Should().BeSameAs(options);
        options.ModelImporters.Should().ContainSingle().Which.Should().BeSameAs(importer);
        changes.Should().Contain(nameof(VideraViewOptions.ModelImporters));
    }

    [Fact]
    public async Task ImportSingleAsync_UsesFirstRegisteredImporterThatCanImportPath()
    {
        var unsupported = new StubModelImporter(static _ => false);
        var supported = new StubModelImporter(static path => path.EndsWith(".fake", StringComparison.OrdinalIgnoreCase));
        var service = new SceneImportService(modelImporters: [unsupported, supported]);

        var result = await service.ImportSingleAsync("scene.fake", CancellationToken.None);

        result.Asset.Should().NotBeNull();
        result.Failure.Should().BeNull();
        result.Diagnostics.Should().ContainSingle(diagnostic => diagnostic.Message == "stub import");
        unsupported.ImportCallCount.Should().Be(0);
        supported.ImportCallCount.Should().Be(1);
    }

    [Fact]
    public async Task ImportSingleAsync_WhenNoRegisteredImporterCanImportPath_ReportsFailure()
    {
        var service = new SceneImportService(modelImporters: [new StubModelImporter(static _ => false)]);

        var result = await service.ImportSingleAsync("scene.unknown", CancellationToken.None);

        result.Asset.Should().BeNull();
        result.Failure.Should().NotBeNull();
        result.Failure!.ErrorMessage.Should().Contain("No registered model importer can import");
        result.Diagnostics.Should().ContainSingle(diagnostic => diagnostic.Severity == ModelImportDiagnosticSeverity.Error);
    }

    private sealed class StubModelImporter : IVideraModelImporter
    {
        private readonly Predicate<string> _canImport;

        public StubModelImporter(Predicate<string> canImport)
        {
            _canImport = canImport;
        }

        public int ImportCallCount { get; private set; }

        public bool CanImport(string path) => _canImport(path);

        public ValueTask<ModelImportResult> ImportAsync(
            ModelImportRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ImportCallCount++;

            return ValueTask.FromResult(ModelImportResult.Success(
                CreateAsset(request.Path),
                [new ModelImportDiagnostic(ModelImportDiagnosticSeverity.Info, "stub import")],
                TimeSpan.FromMilliseconds(3)));
        }

        private static ImportedSceneAsset CreateAsset(string path)
        {
            var material = new MaterialInstance(MaterialInstanceId.New(), "material", RgbaFloat.LightGrey);
            var primitive = new MeshPrimitive(
                MeshPrimitiveId.New(),
                "primitive",
                new MeshData
                {
                    Vertices =
                    [
                        new VertexPositionNormalColor(new(0, 0, 0), new(0, 0, 1), RgbaFloat.White),
                        new VertexPositionNormalColor(new(1, 0, 0), new(0, 0, 1), RgbaFloat.White),
                        new VertexPositionNormalColor(new(0, 1, 0), new(0, 0, 1), RgbaFloat.White)
                    ],
                    Indices = [0, 1, 2],
                    Topology = MeshTopology.Triangles
                },
                material.Id);
            var node = new SceneNode(SceneNodeId.New(), "node", System.Numerics.Matrix4x4.Identity, null, [primitive.Id]);
            return new ImportedSceneAsset(path, Path.GetFileName(path), [node], [primitive], [material]);
        }
    }
}
