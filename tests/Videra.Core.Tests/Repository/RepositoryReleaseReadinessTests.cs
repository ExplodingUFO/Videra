using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class RepositoryReleaseReadinessTests
{
    [Fact]
    public void Readme_ShouldDocumentPublicEntryPointsAndPackageCategories()
    {
        var readme = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "README.md"));

        readme.Should().Contain("## What It Is");
        readme.Should().Contain("## Who It Is For");
        readme.Should().Contain("## Current Status");
        readme.Should().Contain("## Getting Started");
        readme.Should().Contain("## Published packages");
        readme.Should().Contain("## Source-only modules");
        readme.Should().Contain("## Samples and demos");
        readme.Should().Contain("docs/package-matrix.md");
        readme.Should().Contain("docs/support-matrix.md");
        readme.Should().Contain("docs/release-policy.md");
        readme.Should().Contain("nuget.org");
        readme.Should().Contain("GitHub Packages");
        readme.Should().Contain("preview");
        readme.Should().Contain("Videra.Avalonia");
        readme.Should().Contain("Videra.Platform.Windows");
        readme.Should().Contain("Videra.SurfaceCharts.Core");
        readme.Should().Contain("Videra.SurfaceCharts.Avalonia");
        readme.Should().Contain("Videra.SurfaceCharts.Processing");
        readme.Should().Contain("Videra.MinimalSample");
        readme.Should().Contain("Videra.Demo");
        readme.Should().Contain("Videra.SurfaceCharts.Demo");
        readme.Should().Contain("Videra.ExtensibilitySample");
        readme.Should().Contain("Videra.InteractionSample");
        readme.Should().Contain("does not install missing platform packages");
    }

    [Fact]
    public void PublicDocs_ShouldExposePackageMatrixSupportMatrixAndReleasePolicy()
    {
        var repositoryRoot = GetRepositoryRoot();
        var docsIndex = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "index.md"));
        var chineseReadme = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "README.md"));
        var chineseIndex = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "index.md"));

        File.Exists(Path.Combine(repositoryRoot, "docs", "package-matrix.md")).Should().BeTrue();
        File.Exists(Path.Combine(repositoryRoot, "docs", "support-matrix.md")).Should().BeTrue();
        File.Exists(Path.Combine(repositoryRoot, "docs", "release-policy.md")).Should().BeTrue();
        File.Exists(Path.Combine(repositoryRoot, "docs", "releasing.md")).Should().BeTrue();

        docsIndex.Should().Contain("package-matrix.md");
        docsIndex.Should().Contain("support-matrix.md");
        docsIndex.Should().Contain("release-policy.md");
        docsIndex.Should().Contain("releasing.md");

        chineseReadme.Should().Contain("package-matrix.md");
        chineseReadme.Should().Contain("support-matrix.md");
        chineseReadme.Should().Contain("release-policy.md");
        chineseIndex.Should().Contain("package-matrix.md");
        chineseIndex.Should().Contain("support-matrix.md");
        chineseIndex.Should().Contain("release-policy.md");
    }

    [Fact]
    public void AvaloniaReadme_ShouldPromoteHighLevelViewerApi()
    {
        var readme = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "src", "Videra.Avalonia", "README.md"));

        readme.Should().Contain("LoadModelAsync");
        readme.Should().Contain("FrameAll");
        readme.Should().Contain("ResetCamera");
        readme.Should().Contain("BackendDiagnostics");
        readme.Should().Contain("Videra.MinimalSample");
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
    public void PublicPackageReadmes_ShouldDescribeNuGetOrgAsDefaultAndGitHubPackagesAsPreview()
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

            readme.Should().Contain("nuget.org");
            readme.Should().Contain("GitHub Packages");
            readme.Should().Contain("preview");
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
    public void PublicPackageProjects_ShouldDefineIconSourceLinkAndSymbolMetadata()
    {
        var repositoryRoot = GetRepositoryRoot();
        var directoryBuildProps = File.ReadAllText(Path.Combine(repositoryRoot, "Directory.Build.props"));
        var coreProject = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Core", "Videra.Core.csproj"));
        var avaloniaProject = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Videra.Avalonia.csproj"));
        var windowsProject = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.Windows", "Videra.Platform.Windows.csproj"));
        var linuxProject = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.Linux", "Videra.Platform.Linux.csproj"));
        var macosProject = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.macOS", "Videra.Platform.macOS.csproj"));

        File.Exists(Path.Combine(repositoryRoot, "eng", "icon.png")).Should().BeTrue();

        directoryBuildProps.Should().Contain("PackageIcon");
        directoryBuildProps.Should().Contain("icon.png");
        directoryBuildProps.Should().Contain("IncludeSymbols");
        directoryBuildProps.Should().Contain("SymbolPackageFormat");
        directoryBuildProps.Should().Contain("snupkg");
        directoryBuildProps.Should().Contain("ContinuousIntegrationBuild");
        directoryBuildProps.Should().Contain("Deterministic");
        directoryBuildProps.Should().Contain("EmbedUntrackedSources");
        directoryBuildProps.Should().Contain("Microsoft.SourceLink.GitHub");

        foreach (var project in new[] { coreProject, avaloniaProject, windowsProject, linuxProject, macosProject })
        {
            project.Should().Contain("PackageReadmeFile");
            project.Should().Contain("PackagePath=\"\\\"");
            project.Should().Contain("icon.png");
        }
    }

    [Fact]
    public void PublishWorkflows_ShouldSeparatePublicAndPreviewPackageFlows()
    {
        var repositoryRoot = GetRepositoryRoot();
        var workflowRoot = Path.Combine(repositoryRoot, ".github", "workflows");
        var publicWorkflowPath = Path.Combine(workflowRoot, "publish-public.yml");
        var previewWorkflowPath = Path.Combine(workflowRoot, "publish-github-packages.yml");
        var ciWorkflow = File.ReadAllText(Path.Combine(workflowRoot, "ci.yml"));
        var packageValidationScript = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "Validate-Packages.ps1"));

        File.Exists(publicWorkflowPath).Should().BeTrue();
        File.Exists(previewWorkflowPath).Should().BeTrue();
        File.Exists(Path.Combine(workflowRoot, "publish-nuget.yml")).Should().BeFalse();

        var publicWorkflow = File.ReadAllText(publicWorkflowPath);
        var previewWorkflow = File.ReadAllText(previewWorkflowPath);

        publicWorkflow.Should().Contain("tags:");
        publicWorkflow.Should().Contain("v*");
        publicWorkflow.Should().Contain("linux-x11-native-validation:");
        publicWorkflow.Should().Contain("linux-wayland-xwayland-native-validation:");
        publicWorkflow.Should().Contain("macos-native-validation:");
        publicWorkflow.Should().Contain("windows-native-validation:");
        publicWorkflow.Should().Contain("dotnet nuget push");
        publicWorkflow.Should().Contain("https://api.nuget.org/v3/index.json");
        publicWorkflow.Should().Contain("NUGET_API_KEY");
        publicWorkflow.Should().Contain(".snupkg");
        publicWorkflow.Should().Contain("actions/upload-artifact@");
        publicWorkflow.Should().Contain("actions/download-artifact@");
        publicWorkflow.Should().Contain(".github/release.yml");
        publicWorkflow.Should().Contain("scripts/Validate-Packages.ps1");

        previewWorkflow.Should().Contain("GitHub Packages");
        previewWorkflow.Should().Contain("workflow_dispatch:");
        previewWorkflow.Should().Contain("https://nuget.pkg.github.com/ExplodingUFO/index.json");
        previewWorkflow.Should().Contain("dotnet nuget push");

        ciWorkflow.Should().Contain("pwsh -File ./scripts/verify.ps1 -Configuration Release");
        ciWorkflow.Should().Contain("ubuntu-latest");
        ciWorkflow.Should().Contain("macos-latest");
        ciWorkflow.Should().Contain("dotnet pack");

        packageValidationScript.Should().Contain("Videra.Core");
        packageValidationScript.Should().Contain("Videra.Avalonia");
        packageValidationScript.Should().Contain("Videra.Platform.Windows");
        packageValidationScript.Should().Contain("Videra.Platform.Linux");
        packageValidationScript.Should().Contain("Videra.Platform.macOS");
        packageValidationScript.Should().Contain("PackageIcon");
        packageValidationScript.Should().Contain(".snupkg");
    }

    [Fact]
    public void CollaborationAndMaintenanceMetadata_ShouldExist()
    {
        var repositoryRoot = GetRepositoryRoot();
        var issueConfigPath = Path.Combine(repositoryRoot, ".github", "ISSUE_TEMPLATE", "config.yml");
        var bugFormPath = Path.Combine(repositoryRoot, ".github", "ISSUE_TEMPLATE", "bug_report.yml");
        var featureFormPath = Path.Combine(repositoryRoot, ".github", "ISSUE_TEMPLATE", "feature_request.yml");
        var releaseConfigPath = Path.Combine(repositoryRoot, ".github", "release.yml");
        var dependabotPath = Path.Combine(repositoryRoot, ".github", "dependabot.yml");
        var contributing = File.ReadAllText(Path.Combine(repositoryRoot, "CONTRIBUTING.md"));
        var security = File.ReadAllText(Path.Combine(repositoryRoot, "SECURITY.md"));

        File.Exists(issueConfigPath).Should().BeTrue();
        File.Exists(bugFormPath).Should().BeTrue();
        File.Exists(featureFormPath).Should().BeTrue();
        File.Exists(releaseConfigPath).Should().BeTrue();
        File.Exists(dependabotPath).Should().BeTrue();
        File.Exists(Path.Combine(repositoryRoot, ".github", "ISSUE_TEMPLATE", "bug_report.md")).Should().BeFalse();
        File.Exists(Path.Combine(repositoryRoot, ".github", "ISSUE_TEMPLATE", "feature_request.md")).Should().BeFalse();

        var issueConfig = File.ReadAllText(issueConfigPath);
        issueConfig.Should().Contain("blank_issues_enabled: false");
        issueConfig.Should().Contain("discussions");
        issueConfig.Should().Contain("security/policy");

        var bugForm = File.ReadAllText(bugFormPath);
        bugForm.Should().Contain("kind: bug");
        bugForm.Should().Contain("area: core");
        bugForm.Should().Contain("area: avalonia");
        bugForm.Should().Contain("area: windows");
        bugForm.Should().Contain("area: linux");
        bugForm.Should().Contain("area: macos");
        bugForm.Should().Contain("area: surfacecharts");

        var featureForm = File.ReadAllText(featureFormPath);
        featureForm.Should().Contain("kind: feature");
        featureForm.Should().Contain("area: core");
        featureForm.Should().Contain("area: avalonia");
        featureForm.Should().Contain("area: windows");
        featureForm.Should().Contain("area: linux");
        featureForm.Should().Contain("area: macos");
        featureForm.Should().Contain("area: surfacecharts");

        var releaseConfig = File.ReadAllText(releaseConfigPath);
        releaseConfig.Should().Contain("Breaking");
        releaseConfig.Should().Contain("Features");
        releaseConfig.Should().Contain("Fixes");
        releaseConfig.Should().Contain("Docs");
        releaseConfig.Should().Contain("CI / Build");

        var dependabot = File.ReadAllText(dependabotPath);
        dependabot.Should().Contain("package-ecosystem: \"nuget\"");
        dependabot.Should().Contain("package-ecosystem: \"github-actions\"");
        dependabot.Should().Contain("schedule:");
        dependabot.Should().Contain("interval: weekly");

        contributing.Should().Contain("Discussions");
        contributing.Should().Contain("Security");
        contributing.Should().Contain("kind: bug");
        contributing.Should().Contain("kind: feature");
        contributing.Should().Contain("kind: docs");
        contributing.Should().Contain("dependencies");
        security.Should().Contain("Do not open a public issue");
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
        File.ReadAllText(Path.Combine(workflowRoot, "publish-public.yml")).Should().Contain("actions/checkout@v6");
        File.ReadAllText(Path.Combine(workflowRoot, "publish-public.yml")).Should().Contain("actions/setup-dotnet@v5");
        File.ReadAllText(Path.Combine(workflowRoot, "publish-github-packages.yml")).Should().Contain("actions/checkout@v6");
        File.ReadAllText(Path.Combine(workflowRoot, "publish-github-packages.yml")).Should().Contain("actions/setup-dotnet@v5");
    }

    [Fact]
    public void Changelog_ShouldContainInitialAlphaReleaseEntry()
    {
        var changelog = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "CHANGELOG.md"));

        changelog.Should().Contain("## [0.1.0-alpha.3] - 2026-04-17");
        changelog.Should().Contain("## [0.1.0-alpha.2] - 2026-04-17");
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
