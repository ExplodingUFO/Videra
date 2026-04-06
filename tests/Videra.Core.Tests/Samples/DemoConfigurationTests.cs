using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class DemoConfigurationTests
{
    [Fact]
    public void MainWindow_DefaultsToAutoBackend()
    {
        var mainWindowPath = Path.Combine(GetRepositoryRoot(), "samples", "Videra.Demo", "Views", "MainWindow.axaml");
        var xaml = File.ReadAllText(mainWindowPath);

        xaml.Should().Contain("PreferredBackend=\"Auto\"");
    }

    [Fact]
    public void Program_ShouldNotForceBackendThroughEnvironmentVariable()
    {
        var programPath = Path.Combine(GetRepositoryRoot(), "samples", "Videra.Demo", "Program.cs");
        var program = File.ReadAllText(programPath);

        program.Should().NotContain("SetEnvironmentVariable(\"VIDERA_BACKEND\"");
    }

    [Fact]
    public void MainWindow_ShouldNotCreateFallbackViewModelInstance()
    {
        var mainWindowCodeBehindPath = Path.Combine(GetRepositoryRoot(), "samples", "Videra.Demo", "Views", "MainWindow.axaml.cs");
        var codeBehind = File.ReadAllText(mainWindowCodeBehindPath);

        codeBehind.Should().NotContain("new MainWindowViewModel(null!)");
    }

    [Fact]
    public void MacOSNativeHost_ShouldUseSharedObjCRuntimeInterop()
    {
        var nativeHostPath = Path.Combine(GetRepositoryRoot(), "src", "Videra.Avalonia", "Controls", "VideraMacOSNativeHost.cs");
        var nativeHost = File.ReadAllText(nativeHostPath);

        nativeHost.Should().NotContain("[DllImport(\"/usr/lib/libobjc.dylib\"");
        nativeHost.Should().NotContain("private struct CGRect");
    }

    [Fact]
    public void Documentation_ShouldNotContainInternalAgentExecutionPlaybooks()
    {
        var docsDirectory = Path.Combine(GetRepositoryRoot(), "docs");
        var markdownFiles = Directory.GetFiles(docsDirectory, "*.md", SearchOption.AllDirectories);

        foreach (var markdownFile in markdownFiles)
        {
            var markdown = File.ReadAllText(markdownFile);
            markdown.Should().NotContain("For Claude", $"public docs should not embed agent instructions: {markdownFile}");
            markdown.Should().NotContain("For agentic workers", $"public docs should not embed agent instructions: {markdownFile}");
            markdown.Should().NotContain("superpowers:", $"public docs should not embed internal skill routing: {markdownFile}");
        }
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
