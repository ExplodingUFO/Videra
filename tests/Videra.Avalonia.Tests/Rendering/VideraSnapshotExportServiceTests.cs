using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Controls.Interaction;
using Videra.Avalonia.Runtime;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Inspection;
using Xunit;

namespace Videra.Avalonia.Tests.Rendering;

public sealed class VideraSnapshotExportServiceTests : IDisposable
{
    private readonly string _tempDirectory = Path.Combine(
        Path.GetTempPath(),
        "Videra.Avalonia.Tests",
        Guid.NewGuid().ToString("N"));

    public VideraSnapshotExportServiceTests()
    {
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public async Task ExportAsync_UsesPreferredReadbackBackend_WithoutCloneFallback()
    {
        var outputPath = Path.Combine(_tempDirectory, "fast-path.png");
        var backend = new TrackingSoftwareBackend(width: 4, height: 3);
        using var engine = new VideraEngine();

        await VideraSnapshotExportService.ExportAsync(
            outputPath,
            width: 4,
            height: 3,
            engine,
            sceneObjects: Array.Empty<Object3D>(),
            selectionState: new VideraSelectionState(),
            annotations: Array.Empty<VideraAnnotation>(),
            measurements: Array.Empty<VideraMeasurement>(),
            overlayState: VideraViewOverlayState.Empty,
            preferredReadbackBackend: backend,
            logger: NullLogger.Instance,
            cancellationToken: CancellationToken.None);

        File.Exists(outputPath).Should().BeTrue();
        new FileInfo(outputPath).Length.Should().BeGreaterThan(0L);
        backend.CopyFrameToCalls.Should().Be(1);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
        }
        catch
        {
            // Best-effort cleanup for generated export artifacts.
        }
    }

    private sealed class TrackingSoftwareBackend : ISoftwareBackend
    {
        public TrackingSoftwareBackend(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; }

        public int Height { get; }

        public int CopyFrameToCalls { get; private set; }

        public void CopyFrameTo(IntPtr destination, int destinationStride)
        {
            CopyFrameToCalls++;

            for (var y = 0; y < Height; y++)
            {
                var row = new byte[destinationStride];
                for (var x = 0; x < Width; x++)
                {
                    var offset = x * 4;
                    row[offset + 0] = 0x33;
                    row[offset + 1] = 0x66;
                    row[offset + 2] = 0x99;
                    row[offset + 3] = 0xFF;
                }

                Marshal.Copy(row, 0, destination + (y * destinationStride), destinationStride);
            }
        }
    }
}
