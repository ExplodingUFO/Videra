using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class PublicApiContractRepositoryTests
{
    private static readonly string[] CanonicalPublicPackageIds =
    [
        "Videra.Core",
        "Videra.Import.Gltf",
        "Videra.Import.Obj",
        "Videra.Avalonia",
        "Videra.Platform.Windows",
        "Videra.Platform.Linux",
        "Videra.Platform.macOS",
        "Videra.SurfaceCharts.Core",
        "Videra.SurfaceCharts.Rendering",
        "Videra.SurfaceCharts.Processing",
        "Videra.SurfaceCharts.Avalonia"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Regex NamespaceDeclaration = new(
        @"^namespace\s+(?<name>[A-Za-z0-9_.]+);",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex PublicTypeDeclaration = new(
        @"^public\s+(?:(?:sealed|abstract|static|partial|readonly)\s+)*(?:class|interface|enum|struct|record(?:\s+(?:struct|class))?)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    [Fact]
    public void PublicApiContract_ShouldListCanonicalPublicPackageProjects()
    {
        var repositoryRoot = GetRepositoryRoot();
        var contract = ReadContract(repositoryRoot);

        contract.Version.Should().Be(1);
        contract.Packages.Select(package => package.Id).Should().Equal(CanonicalPublicPackageIds);

        foreach (var package in contract.Packages)
        {
            Path.GetFileNameWithoutExtension(package.Project).Should().Be(package.Id);
            File.Exists(Path.Combine(repositoryRoot, package.Project)).Should().BeTrue();
            Directory.Exists(Path.Combine(repositoryRoot, package.SourceRoot)).Should().BeTrue();
            package.PublicTypes.Should().OnlyHaveUniqueItems();
            package.PublicTypes.Should().BeInAscendingOrder(StringComparer.Ordinal);
        }
    }

    [Fact]
    public void PublicApiContract_ShouldMatchCurrentTopLevelPublicTypes()
    {
        var repositoryRoot = GetRepositoryRoot();
        var contract = ReadContract(repositoryRoot);

        foreach (var package in contract.Packages)
        {
            var actualTypes = GetTopLevelPublicTypes(Path.Combine(repositoryRoot, package.SourceRoot));

            actualTypes.Should().Equal(
                package.PublicTypes,
                $"{package.Id} public type drift must be reviewed through eng/public-api-contract.json");
        }
    }

    private static PublicApiContract ReadContract(string repositoryRoot)
    {
        var contractPath = Path.Combine(repositoryRoot, "eng", "public-api-contract.json");
        File.Exists(contractPath).Should().BeTrue();

        var contract = JsonSerializer.Deserialize<PublicApiContract>(
            File.ReadAllText(contractPath),
            JsonOptions);

        contract.Should().NotBeNull();
        return contract!;
    }

    private static string[] GetTopLevelPublicTypes(string sourceRoot)
    {
        return Directory
            .EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.Ordinal)
            .SelectMany(ReadTopLevelPublicTypes)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
    }

    private static IEnumerable<string> ReadTopLevelPublicTypes(string sourcePath)
    {
        string? currentNamespace = null;

        foreach (var line in File.ReadLines(sourcePath))
        {
            var namespaceMatch = NamespaceDeclaration.Match(line);
            if (namespaceMatch.Success)
            {
                currentNamespace = namespaceMatch.Groups["name"].Value;
                continue;
            }

            var typeMatch = PublicTypeDeclaration.Match(line);
            if (!typeMatch.Success)
            {
                continue;
            }

            var typeName = typeMatch.Groups["name"].Value;
            yield return currentNamespace is null ? typeName : $"{currentNamespace}.{typeName}";
        }
    }

    private sealed record PublicApiContract(int Version, PublicPackageContract[] Packages);

    private sealed record PublicPackageContract(
        string Id,
        string Project,
        string SourceRoot,
        string[] PublicTypes);

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
