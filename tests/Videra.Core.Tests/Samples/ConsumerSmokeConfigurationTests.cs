using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class ConsumerSmokeConfigurationTests
{
    [Fact]
    public void ConsumerSmoke_ShouldKeepProofHoldOptInOnly()
    {
        var repositoryRoot = GetRepositoryRoot();
        var smokeRoot = Path.Combine(repositoryRoot, "smoke", "Videra.ConsumerSmoke");
        var mainWindowCodeBehind = File.ReadAllText(Path.Combine(smokeRoot, "Views", "MainWindow.axaml.cs"));
        var invokeScript = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "Invoke-ConsumerSmoke.ps1"));

        File.Exists(Path.Combine(smokeRoot, "Views", "MainWindow.axaml.cs")).Should().BeTrue();
        File.Exists(Path.Combine(repositoryRoot, "scripts", "Invoke-ConsumerSmoke.ps1")).Should().BeTrue();

        invokeScript.Should().Contain("LightingProofHoldSeconds");
        invokeScript.Should().Contain("VIDERA_LIGHTING_PROOF_HOLD_SECONDS");
        invokeScript.Should().Contain("Remove-Item Env:VIDERA_LIGHTING_PROOF_HOLD_SECONDS");

        mainWindowCodeBehind.Should().Contain("ResolveLightingProofHoldSeconds()");
        mainWindowCodeBehind.Should().Contain("VIDERA_LIGHTING_PROOF_HOLD_SECONDS");
        mainWindowCodeBehind.Should().Contain("Lighting proof hold active for");
        mainWindowCodeBehind.Should().Contain("await Task.Delay(TimeSpan.FromSeconds(_lightingProofHoldSeconds)).ConfigureAwait(true);");
        mainWindowCodeBehind.Should().Contain("_lightingProofHoldSeconds");
        mainWindowCodeBehind.Should().Contain("int LightingProofHoldSeconds");
        mainWindowCodeBehind.Should().Contain("_lightingProofHoldSeconds);");
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
