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
            RenderPipelineProfile = "Standard",
            LastFrameStageNames = ["PrepareFrame", "PresentFrame"],
            LastFrameFeatureNames = ["Opaque", "Overlay"],
            SupportedRenderFeatureNames = ["Opaque", "Transparent", "Overlay", "Picking", "Screenshot"],
            TransparentFeatureStatus = VideraBackendDiagnostics.CurrentTransparentFeatureStatus,
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
            ResolvedUploadBudgetBytes = 16384,
            IsClippingActive = true,
            ActiveClippingPlaneCount = 1,
            MeasurementCount = 2,
            LastSnapshotExportPath = "artifacts/inspection/snapshot.png",
            LastSnapshotExportStatus = "Succeeded"
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
        snapshot.Should().Contain("DisplayServerCompatibility: Wayland session using XWayland compatibility fallback; compositor-native Wayland embedding is not active.");
        snapshot.Should().Contain("RenderPipelineProfile: Standard");
        snapshot.Should().Contain("LastFrameStageNames: PrepareFrame, PresentFrame");
        snapshot.Should().Contain("LastFrameFeatureNames: Opaque, Overlay");
        snapshot.Should().Contain("SupportedRenderFeatureNames: Opaque, Transparent, Overlay, Picking, Screenshot");
        snapshot.Should().Contain($"TransparentFeatureStatus: {VideraBackendDiagnostics.CurrentTransparentFeatureStatus}");
        snapshot.Should().Contain("PendingSceneUploadBytes: 4096");
        snapshot.Should().Contain("ResolvedUploadBudgetBytes: 16384");
        snapshot.Should().Contain("IsClippingActive: True");
        snapshot.Should().Contain("ActiveClippingPlaneCount: 1");
        snapshot.Should().Contain("MeasurementCount: 2");
        snapshot.Should().Contain("LastSnapshotExportPath: artifacts/inspection/snapshot.png");
        snapshot.Should().Contain("LastSnapshotExportStatus: Succeeded");
    }
}
