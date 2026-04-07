using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class RepositoryLocalizationTests
{
    [Fact]
    public void Readme_ShouldBeEnglishPrimaryWithChineseSwitch()
    {
        var readme = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "README.md"));

        readme.Should().Contain("English");
        readme.Should().Contain("中文");
        readme.Should().Contain("docs/zh-CN/README.md");
        readme.Should().Contain("Cross-platform");
    }

    [Fact]
    public void EnglishEntryDocs_ShouldExposeChineseNavigation()
    {
        var docsIndex = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "docs", "index.md"));
        var architecture = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "ARCHITECTURE.md"));
        var contributing = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "CONTRIBUTING.md"));
        var troubleshooting = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "docs", "troubleshooting.md"));

        docsIndex.Should().Contain("zh-CN/index.md");
        architecture.Should().Contain("docs/zh-CN/ARCHITECTURE.md");
        contributing.Should().Contain("docs/zh-CN/CONTRIBUTING.md");
        troubleshooting.Should().Contain("zh-CN/troubleshooting.md");
    }

    [Fact]
    public void ChineseEntryDocs_ShouldExist()
    {
        var repositoryRoot = GetRepositoryRoot();
        var expectedFiles = new[]
        {
            Path.Combine(repositoryRoot, "docs", "zh-CN", "README.md"),
            Path.Combine(repositoryRoot, "docs", "zh-CN", "ARCHITECTURE.md"),
            Path.Combine(repositoryRoot, "docs", "zh-CN", "CONTRIBUTING.md"),
            Path.Combine(repositoryRoot, "docs", "zh-CN", "index.md"),
            Path.Combine(repositoryRoot, "docs", "zh-CN", "troubleshooting.md")
        };

        foreach (var file in expectedFiles)
        {
            File.Exists(file).Should().BeTrue($"expected Chinese documentation file {file} to exist");
        }
    }

    [Fact]
    public void ChinesePackageReadmeMirrors_ShouldExist()
    {
        var repositoryRoot = GetRepositoryRoot();
        var expectedFiles = new[]
        {
            Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-core.md"),
            Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-avalonia.md"),
            Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "platform-windows.md"),
            Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "platform-linux.md"),
            Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "platform-macos.md")
        };

        foreach (var file in expectedFiles)
        {
            File.Exists(file).Should().BeTrue($"expected Chinese package mirror {file} to exist");
        }
    }

    [Fact]
    public void ChineseDemoDoc_ShouldMentionHighLevelViewerApi()
    {
        var demoDoc = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "docs", "zh-CN", "modules", "demo.md"));

        demoDoc.Should().Contain("LoadModelAsync");
        demoDoc.Should().Contain("FrameAll");
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
