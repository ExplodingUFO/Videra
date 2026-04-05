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
