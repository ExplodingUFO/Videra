using FluentAssertions;
using Videra.Core.Scene;
using Videra.Import.Obj;
using Xunit;

namespace Videra.Core.Tests.IO;

public sealed class ModelImporterRegistryTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"VideraImporterRegistry_{Guid.NewGuid():N}");

    public ModelImporterRegistryTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public async Task ObjModelImporter_Create_ReturnsRegistryImporterWithDiagnosticsAndDuration()
    {
        var path = WriteObj("triangle.obj");
        var importer = ObjModelImporter.Create();

        importer.Should().BeAssignableTo<IVideraModelImporter>();
        importer.CanImport(path).Should().BeTrue();
        importer.CanImport(Path.Combine(_tempDir, "triangle.gltf")).Should().BeFalse();

        var result = await importer.ImportAsync(new ModelImportRequest(path), CancellationToken.None);

        result.Asset.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.ImportDuration.Should().BeGreaterThan(TimeSpan.Zero);
        result.Diagnostics.Should().ContainSingle(diagnostic => diagnostic.Severity == ModelImportDiagnosticSeverity.Info);
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
