using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class ConsumerSmokeConfigurationTests
{
    [Fact]
    public void ConsumerSmoke_ShouldExposeProofHoldAndValidationWiring()
    {
        var repositoryRoot = GetRepositoryRoot();
        var smokeRoot = Path.Combine(repositoryRoot, "smoke", "Videra.ConsumerSmoke");
        var mainWindowCodeBehind = File.ReadAllText(Path.Combine(smokeRoot, "Views", "MainWindow.axaml.cs"));
        var sceneFactoryCode = File.ReadAllText(Path.Combine(smokeRoot, "Views", "SmokeSceneFactory.cs"));
        var invokeScript = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "Invoke-ConsumerSmoke.ps1"));
        var consumerSmokeWorkflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "consumer-smoke.yml"));
        var publicWorkflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "publish-public.yml"));
        var existingReleaseWorkflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "publish-existing-public-release.yml"));

        File.Exists(Path.Combine(smokeRoot, "Views", "MainWindow.axaml.cs")).Should().BeTrue();
        File.Exists(Path.Combine(smokeRoot, "Views", "SmokeSceneFactory.cs")).Should().BeTrue();
        File.Exists(Path.Combine(repositoryRoot, "scripts", "Invoke-ConsumerSmoke.ps1")).Should().BeTrue();

        invokeScript.Should().Contain("LightingProofHoldSeconds");
        invokeScript.Should().Contain("VIDERA_LIGHTING_PROOF_HOLD_SECONDS");
        invokeScript.Should().Contain("Remove-Item Env:VIDERA_LIGHTING_PROOF_HOLD_SECONDS");
        consumerSmokeWorkflow.Should().Contain("-LightingProofHoldSeconds 10");
        publicWorkflow.Should().Contain("-LightingProofHoldSeconds 10");
        existingReleaseWorkflow.Should().Contain("-LightingProofHoldSeconds 10");

        mainWindowCodeBehind.Should().Contain("ResolveLightingProofHoldSeconds()");
        mainWindowCodeBehind.Should().Contain("VIDERA_LIGHTING_PROOF_HOLD_SECONDS");
        mainWindowCodeBehind.Should().Contain("Lighting proof hold active for");
        mainWindowCodeBehind.Should().Contain("await Task.Delay(TimeSpan.FromSeconds(_lightingProofHoldSeconds)).ConfigureAwait(true);");
        mainWindowCodeBehind.Should().Contain("_lightingProofHoldSeconds");
        mainWindowCodeBehind.Should().Contain("int LightingProofHoldSeconds");
        mainWindowCodeBehind.Should().Contain("SmokeSceneFactory.CreateEmissiveNormalProofObject()");
        mainWindowCodeBehind.Should().Contain("SmokeSceneFactory.EmissiveNormalProofObjectName");
        mainWindowCodeBehind.Should().Contain("_lightingProofHoldSeconds);");

        sceneFactoryCode.Should().Contain("CreateEmissiveNormalProofObject");
        sceneFactoryCode.Should().Contain("ConsumerSmokeEmissiveNormalProofQuad");
        sceneFactoryCode.Should().Contain("MaterialEmissive");
        sceneFactoryCode.Should().Contain("MaterialNormalTextureBinding");
        sceneFactoryCode.Should().Contain("SceneObjectFactory.CreateDeferred");
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Videra.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root containing Videra.slnx.");
    }
}
