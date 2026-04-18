using FluentAssertions;
using Videra.Avalonia.Controls;
using Videra.Core.Graphics;
using Xunit;

namespace Videra.Avalonia.Tests.Controls;

public sealed class VideraDiagnosticsSnapshotFormatterTests
{
    [Fact]
    public void Format_ShouldIncludeCanonicalAlphaSupportFields()
    {
        var diagnostics = new VideraBackendDiagnostics
        {
            RequestedBackend = GraphicsBackendPreference.Auto,
            ResolvedBackend = GraphicsBackendPreference.Software,
            IsReady = true,
            IsUsingSoftwareFallback = true,
            FallbackReason = "Native backend unavailable.",
            NativeHostBound = false,
            ResolvedDisplayServer = "XWayland",
            DisplayServerFallbackUsed = true,
            DisplayServerFallbackReason = "Wayland host unavailable.",
            LastInitializationError = "No hardware backend succeeded.",
            SceneDocumentVersion = 3,
            PendingSceneUploads = 1,
            PendingSceneUploadBytes = 4096,
            ResidentSceneObjects = 2,
            DirtySceneObjects = 0,
            FailedSceneUploads = 0,
            LastFrameUploadedObjects = 1,
            LastFrameUploadedBytes = 4096,
            LastFrameUploadFailures = 0,
            LastFrameUploadDuration = TimeSpan.FromMilliseconds(12),
            ResolvedUploadBudgetObjects = 2,
            ResolvedUploadBudgetBytes = 16384
        };

        var snapshot = VideraDiagnosticsSnapshotFormatter.Format(diagnostics);

        snapshot.Should().Contain("Videra diagnostics snapshot");
        snapshot.Should().Contain("RequestedBackend: Auto");
        snapshot.Should().Contain("ResolvedBackend: Software");
        snapshot.Should().Contain("IsReady: True");
        snapshot.Should().Contain("IsUsingSoftwareFallback: True");
        snapshot.Should().Contain("FallbackReason: Native backend unavailable.");
        snapshot.Should().Contain("ResolvedDisplayServer: XWayland");
        snapshot.Should().Contain("DisplayServerFallbackUsed: True");
        snapshot.Should().Contain("PendingSceneUploadBytes: 4096");
        snapshot.Should().Contain("ResolvedUploadBudgetBytes: 16384");
    }
}
