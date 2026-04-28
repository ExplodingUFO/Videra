using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class WorkbenchSampleConfigurationTests
{
    [Fact]
    public void WorkbenchSample_ShouldUsePublicEvidenceFormatters_ForSupportCapture()
    {
        var repositoryRoot = GetRepositoryRoot();
        var sampleRoot = Path.Combine(repositoryRoot, "samples", "Videra.AvaloniaWorkbenchSample");
        var supportCapture = File.ReadAllText(Path.Combine(sampleRoot, "WorkbenchSupportCapture.cs"));
        var codeBehind = File.ReadAllText(Path.Combine(sampleRoot, "Views", "MainWindow.axaml.cs"));
        var markup = File.ReadAllText(Path.Combine(sampleRoot, "Views", "MainWindow.axaml"));
        var readme = File.ReadAllText(Path.Combine(sampleRoot, "README.md"));

        supportCapture.Should().Contain("ViewerInteractionEvidence:");
        supportCapture.Should().Contain("VideraInteractionEvidenceFormatter.Format");
        supportCapture.Should().Contain("SurfaceChartProbeEvidenceFormatter.Format");

        codeBehind.Should().Contain("VideraInteractionEvidenceFormatter.Create");
        codeBehind.Should().Contain("View3D.CaptureInspectionState()");
        codeBehind.Should().Contain("VideraInteractionDiagnostics.CreateDefault()");
        codeBehind.Should().Contain("SurfaceChartProbeEvidenceFormatter.Create");

        markup.Should().Contain("Interaction evidence");

        readme.Should().Contain("viewer interaction evidence");
        readme.Should().Contain("chart probe evidence");
        readme.Should().Contain("without adding chart semantics to `VideraView`");

        supportCapture.Should().NotContain("VideraViewRuntime");
        codeBehind.Should().NotContain("VideraViewRuntime");
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
