using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class RepositoryReleaseReadinessTests
{
    [Fact]
    public void Readme_ShouldDocumentGitHubPackagesInstallation()
    {
        var readme = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "README.md"));

        readme.Should().Contain("https://nuget.pkg.github.com/ExplodingUFO/index.json");
        readme.Should().Contain("dotnet nuget add source");
        readme.Should().Contain("dotnet add package Videra.Avalonia");
        readme.Should().Contain("dotnet add package Videra.Platform.Windows");
        readme.Should().Contain("dotnet add package Videra.Platform.Linux");
        readme.Should().Contain("dotnet add package Videra.Platform.macOS");
        readme.Should().Contain("VIDERA_BACKEND");
        readme.Should().Contain("does not install missing platform packages");
        readme.Should().Contain("matching-host native validation");
        readme.Should().Contain("LoadModelAsync");
    }

    [Fact]
    public void AvaloniaReadme_ShouldPromoteHighLevelViewerApi()
    {
        var readme = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "src", "Videra.Avalonia", "README.md"));

        readme.Should().Contain("LoadModelAsync");
        readme.Should().Contain("FrameAll");
        readme.Should().Contain("BackendDiagnostics");
    }

    [Fact]
    public void RootAndPackageDocs_ShouldRouteToExtensibilityContractAndLifecycleVocabulary()
    {
        var repositoryRoot = GetRepositoryRoot();
        var rootReadme = File.ReadAllText(Path.Combine(repositoryRoot, "README.md"));
        var coreReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Core", "README.md"));
        var avaloniaReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "README.md"));

        foreach (var readme in new[] { rootReadme, coreReadme, avaloniaReadme })
        {
            readme.Should().Contain("docs/extensibility.md");
            readme.Should().Contain("Videra.ExtensibilitySample");
            readme.Should().Contain("disposed");
            readme.Should().Contain("no-op");
            readme.Should().Contain("AllowSoftwareFallback");
            readme.Should().Contain("package discovery");
            readme.Should().Contain("plugin loading");
        }

        rootReadme.Should().Contain("FallbackReason");
        coreReadme.Should().Contain("FallbackReason");
        avaloniaReadme.Should().Contain("FallbackReason");
    }

    [Fact]
    public void ExtensibilityContract_ShouldDescribeReadyDisposedAndFallbackStates()
    {
        var contract = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "docs", "extensibility.md"));

        contract.Should().Contain("Ready");
        contract.Should().Contain("Pre-initialization");
        contract.Should().Contain("disposed");
        contract.Should().Contain("AllowSoftwareFallback = true");
        contract.Should().Contain("AllowSoftwareFallback = false");
        contract.Should().Contain("FallbackReason");
        contract.Should().Contain("LoadModelAsync");
        contract.Should().Contain("FrameAll()");
    }

    [Fact]
    public void Troubleshooting_ShouldClarifyBackendPreferenceAndValidationBoundaries()
    {
        var troubleshooting = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "docs", "troubleshooting.md"));

        troubleshooting.Should().Contain("VIDERA_BACKEND");
        troubleshooting.Should().Contain("does not install missing platform packages");
        troubleshooting.Should().Contain("does not replace matching-host native validation");
        troubleshooting.Should().Contain("matching-host");
    }

    [Fact]
    public void PackageReadmes_ShouldDescribeStandaloneInstallPrerequisites()
    {
        var repositoryRoot = GetRepositoryRoot();
        var packageReadmes = new[]
        {
            Path.Combine(repositoryRoot, "src", "Videra.Core", "README.md"),
            Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "README.md"),
            Path.Combine(repositoryRoot, "src", "Videra.Platform.Windows", "README.md"),
            Path.Combine(repositoryRoot, "src", "Videra.Platform.Linux", "README.md"),
            Path.Combine(repositoryRoot, "src", "Videra.Platform.macOS", "README.md")
        };

        foreach (var readmePath in packageReadmes)
        {
            var readme = File.ReadAllText(readmePath);

            readme.Should().Contain("dotnet nuget add source");
            readme.Should().Contain("https://nuget.pkg.github.com/ExplodingUFO/index.json");
            readme.Should().Contain("alpha");
        }

        var avaloniaReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "README.md"));
        avaloniaReadme.Should().Contain("Videra.Avalonia");
        avaloniaReadme.Should().Contain("Videra.Platform.Windows");
        avaloniaReadme.Should().Contain("Videra.Platform.Linux");
        avaloniaReadme.Should().Contain("Videra.Platform.macOS");
        avaloniaReadme.Should().Contain("PreferredBackend");
        avaloniaReadme.Should().Contain("VIDERA_BACKEND");
        avaloniaReadme.Should().Contain("does not install missing platform packages");

        var linuxReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.Linux", "README.md"));
        linuxReadme.Should().Contain("matching-host validation");
        linuxReadme.Should().Contain("X11");
        linuxReadme.Should().Contain("XWayland");
        linuxReadme.Should().Contain("Wayland session");

        var macosReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.macOS", "README.md"));
        macosReadme.Should().Contain("matching-host validation");
        macosReadme.Should().Contain("NSView");
        macosReadme.Should().Contain("CAMetalLayer");
    }

    [Fact]
    public void PublishWorkflow_ShouldUseTaggedReleasesAndValidatePackages()
    {
        var repositoryRoot = GetRepositoryRoot();
        var workflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "publish-nuget.yml"));
        var ciWorkflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "ci.yml"));
        var packageValidationScript = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "Validate-Packages.ps1"));

        workflow.Should().NotContain("workflow_dispatch:");
        workflow.Should().Contain("linux-x11-native-validation:");
        workflow.Should().Contain("linux-wayland-xwayland-native-validation:");
        workflow.Should().Contain("macos-native-validation:");
        workflow.Should().Contain("windows-native-validation:");
        workflow.Should().Contain("linux-package-evidence:");
        workflow.Should().Contain("macos-package-evidence:");
        workflow.Should().Contain("windows-package-evidence:");
        workflow.Should().Contain("neutral-package-evidence:");
        workflow.Should().Contain("needs:");
        workflow.Should().Contain("- linux-x11-native-validation");
        workflow.Should().Contain("- linux-wayland-xwayland-native-validation");
        workflow.Should().Contain("- macos-native-validation");
        workflow.Should().Contain("- windows-native-validation");
        workflow.Should().Contain("- linux-package-evidence");
        workflow.Should().Contain("- macos-package-evidence");
        workflow.Should().Contain("- windows-package-evidence");
        workflow.Should().Contain("- neutral-package-evidence");
        workflow.Should().Contain("actions/upload-artifact@");
        workflow.Should().Contain("actions/download-artifact@");
        workflow.Should().Contain("pwsh -File ./scripts/Validate-Packages.ps1");
        workflow.Should().Contain("src/Videra.Platform.Windows/Videra.Platform.Windows.csproj");
        workflow.Should().Contain("src/Videra.Platform.Linux/Videra.Platform.Linux.csproj");
        workflow.Should().Contain("src/Videra.Platform.macOS/Videra.Platform.macOS.csproj");
        workflow.Should().Contain("src/Videra.Core/Videra.Core.csproj");
        workflow.Should().Contain("src/Videra.Avalonia/Videra.Avalonia.csproj");
        workflow.Should().Contain("pwsh -File ./scripts/verify.ps1 -Configuration Release");
        workflow.Should().NotContain("pwsh -File ./verify.ps1 -Configuration Release");
        ciWorkflow.Should().Contain("ubuntu-latest");
        ciWorkflow.Should().Contain("macos-latest");
        ciWorkflow.Should().Contain("dotnet pack");
        packageValidationScript.Should().Contain("param(");
        packageValidationScript.Should().Contain("Videra.Core");
        packageValidationScript.Should().Contain("Videra.Avalonia");
        packageValidationScript.Should().Contain("Videra.Platform.Windows");
        packageValidationScript.Should().Contain("Videra.Platform.Linux");
        packageValidationScript.Should().Contain("Videra.Platform.macOS");
        packageValidationScript.Should().Contain("RepositoryUrl");
        packageValidationScript.Should().Contain("PackageLicenseExpression");
    }

    [Fact]
    public void Workflows_ShouldUseNode24CompatibleOfficialGitHubActions()
    {
        var workflowRoot = Path.Combine(GetRepositoryRoot(), ".github", "workflows");
        var workflows = Directory.GetFiles(workflowRoot, "*.yml");

        workflows.Should().NotBeEmpty();

        foreach (var workflowPath in workflows)
        {
            var workflow = File.ReadAllText(workflowPath);

            workflow.Should().NotContain("actions/checkout@v4");
            workflow.Should().NotContain("actions/setup-dotnet@v4");
        }

        File.ReadAllText(Path.Combine(workflowRoot, "ci.yml")).Should().Contain("actions/checkout@v6");
        File.ReadAllText(Path.Combine(workflowRoot, "ci.yml")).Should().Contain("actions/setup-dotnet@v5");
        File.ReadAllText(Path.Combine(workflowRoot, "native-validation.yml")).Should().Contain("actions/checkout@v6");
        File.ReadAllText(Path.Combine(workflowRoot, "native-validation.yml")).Should().Contain("actions/setup-dotnet@v5");
        File.ReadAllText(Path.Combine(workflowRoot, "publish-nuget.yml")).Should().Contain("actions/checkout@v6");
        File.ReadAllText(Path.Combine(workflowRoot, "publish-nuget.yml")).Should().Contain("actions/setup-dotnet@v5");
    }

    [Fact]
    public void Changelog_ShouldContainInitialAlphaReleaseEntry()
    {
        var changelog = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "CHANGELOG.md"));

        changelog.Should().Contain("## [0.1.0-alpha.1] - 2026-04-06");
    }

    [Fact]
    public void PullRequestTemplate_ShouldRequestIssueAndBreakingChangeContext()
    {
        var template = File.ReadAllText(Path.Combine(GetRepositoryRoot(), ".github", "pull_request_template.md"));

        template.Should().Contain("Related issue");
        template.Should().Contain("Breaking change?");
        template.Should().Contain("Platform validation");
    }

    [Fact]
    public void SecurityPolicy_ShouldDescribePrivateReportingAndResponseWindow()
    {
        var policy = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "SECURITY.md"));

        policy.Should().Contain("GitHub private vulnerability reporting");
        policy.Should().Contain("within 5 business days");
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
