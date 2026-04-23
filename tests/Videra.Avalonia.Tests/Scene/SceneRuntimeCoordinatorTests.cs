using FluentAssertions;
using Videra.Avalonia.Runtime.Scene;
using Videra.Core.Graphics;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Avalonia.Tests.Scene;

public sealed class SceneRuntimeCoordinatorTests
{
    [Fact]
    public void AppendImportedAsset_creates_multi_object_runtime_entry_for_mixed_alpha_asset()
    {
        using var engine = new VideraEngine();
        var coordinator = new SceneRuntimeCoordinator(
            engine,
            refreshOverlay: static () => { },
            refreshSceneDiagnostics: static () => { },
            refreshBackendDiagnostics: static () => { },
            invalidateRender: static _ => { });
        var asset = SceneTestMeshes.CreateMixedAlphaImportedAsset();

        var entry = coordinator.AppendImportedAsset(asset);

        entry.RuntimeObjects.Should().HaveCount(2);
        entry.RuntimeObjects.Should().ContainSingle(static runtimeObject => runtimeObject.MaterialAlpha.Mode == MaterialAlphaMode.Blend);
        entry.RuntimeObjects.Should().ContainSingle(static runtimeObject => runtimeObject.MaterialAlpha.Mode != MaterialAlphaMode.Blend);
        coordinator.CurrentDocument.Entries.Should().ContainSingle();
        coordinator.CurrentDocument.SceneObjects.Should().HaveCount(2);
    }
}
