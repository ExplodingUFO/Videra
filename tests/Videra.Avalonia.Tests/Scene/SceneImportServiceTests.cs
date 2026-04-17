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
