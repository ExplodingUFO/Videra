using System.Reflection;
using System.Xml.Linq;
using FluentAssertions;
using Videra.Avalonia.Controls;
using Videra.Import.Gltf;
using Videra.Import.Obj;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class HostingBoundaryTests
{
    [Fact]
    public void HostingBoundaryDocs_ShouldDescribeCanonicalCompositionAndInternalOwners()
    {
        var repositoryRoot = GetRepositoryRoot();
        var hostingBoundary = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "hosting-boundary.md"));
        var docsIndex = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "index.md"));
        var readme = File.ReadAllText(Path.Combine(repositoryRoot, "README.md"));
        var architecture = File.ReadAllText(Path.Combine(repositoryRoot, "ARCHITECTURE.md"));
        var avaloniaReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "README.md"));
        var coreReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Core", "README.md"));

        File.Exists(Path.Combine(repositoryRoot, "docs", "hosting-boundary.md")).Should().BeTrue();

        hostingBoundary.Should().Contain("Videra.Core");
        hostingBoundary.Should().Contain("Videra.Import.Gltf");
        hostingBoundary.Should().Contain("Videra.Import.Obj");
        hostingBoundary.Should().Contain("Videra.Avalonia");
        hostingBoundary.Should().Contain("Videra.Platform.Windows");
        hostingBoundary.Should().Contain("Videra.SurfaceCharts.*");
        hostingBoundary.Should().Contain("## Internal Seam Owners");
        hostingBoundary.Should().Contain("internal composition seams");
        hostingBoundary.Should().Contain("native handle creation/binding seam");
        hostingBoundary.Should().Contain("scene/runtime coordination and deferred upload path");
        hostingBoundary.Should().Contain("internal");
        hostingBoundary.Should().Contain("LoadModelAsync");
        hostingBoundary.Should().Contain("LoadModelsAsync");
        hostingBoundary.Should().Contain("`Videra.Import.*`");
        hostingBoundary.Should().Contain("public surface");

        docsIndex.Should().Contain("hosting-boundary.md");
        readme.Should().Contain("docs/hosting-boundary.md");
        architecture.Should().Contain("docs/hosting-boundary.md");
        avaloniaReadme.Should().Contain("docs/hosting-boundary.md");
        coreReadme.Should().Contain("docs/hosting-boundary.md");
    }

    [Fact]
    public void AvaloniaPublicApi_ShouldNotLeakImportOrInternalHostingTypes()
    {
        var publicSurfaceTypeNames = GetPublicSurfaceTypeNames(typeof(VideraView).Assembly);

        publicSurfaceTypeNames.Should().NotContain(name => name.StartsWith("Videra.Import.", StringComparison.Ordinal));
        publicSurfaceTypeNames.Should().NotContain(name => name.Contains("VideraViewRuntime", StringComparison.Ordinal));
        publicSurfaceTypeNames.Should().NotContain(name => name.Contains("VideraViewSessionBridge", StringComparison.Ordinal));
        publicSurfaceTypeNames.Should().NotContain(name => name.Contains("RenderSession", StringComparison.Ordinal));
        publicSurfaceTypeNames.Should().NotContain(name => name.Contains("NativeHost", StringComparison.Ordinal));
        publicSurfaceTypeNames.Should().Contain("Videra.Avalonia.Controls.VideraView");
        publicSurfaceTypeNames.Should().Contain("Videra.Core.Graphics.Abstractions.IResourceFactory");
    }

    [Fact]
    public void ImportPackages_ShouldStayCoreAnchoredInPublicApi()
    {
        foreach (var assembly in new[] { typeof(GltfModelImporter).Assembly, typeof(ObjModelImporter).Assembly })
        {
            var publicSurfaceTypeNames = GetPublicSurfaceTypeNames(assembly);

            publicSurfaceTypeNames.Should().Contain(name => name.StartsWith("Videra.Core.", StringComparison.Ordinal));
            publicSurfaceTypeNames.Should().OnlyContain(
                name => name.StartsWith("System.", StringComparison.Ordinal)
                    || name.StartsWith("Microsoft.Extensions.Logging.", StringComparison.Ordinal)
                    || name.StartsWith("Videra.Core.", StringComparison.Ordinal)
                    || name.StartsWith("Videra.Import.Gltf.", StringComparison.Ordinal)
                    || name.StartsWith("Videra.Import.Obj.", StringComparison.Ordinal),
                "import-package public APIs should stay core-based and avoid UI, platform, chart, or third-party scene/runtime leaks");
        }
    }

    [Fact]
    public void ProjectReferenceGraph_ShouldPreserveViewerProductBoundary()
    {
        var repositoryRoot = GetRepositoryRoot();
        var coreReferences = GetProjectReferenceFileNames(Path.Combine(repositoryRoot, "src", "Videra.Core", "Videra.Core.csproj"));
        var gltfReferences = GetProjectReferenceFileNames(Path.Combine(repositoryRoot, "src", "Videra.Import.Gltf", "Videra.Import.Gltf.csproj"));
        var objReferences = GetProjectReferenceFileNames(Path.Combine(repositoryRoot, "src", "Videra.Import.Obj", "Videra.Import.Obj.csproj"));
        var avaloniaReferences = GetProjectReferenceFileNames(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Videra.Avalonia.csproj"));
        var windowsReferences = GetProjectReferenceFileNames(Path.Combine(repositoryRoot, "src", "Videra.Platform.Windows", "Videra.Platform.Windows.csproj"));
        var linuxReferences = GetProjectReferenceFileNames(Path.Combine(repositoryRoot, "src", "Videra.Platform.Linux", "Videra.Platform.Linux.csproj"));
        var macosReferences = GetProjectReferenceFileNames(Path.Combine(repositoryRoot, "src", "Videra.Platform.macOS", "Videra.Platform.macOS.csproj"));

        coreReferences.Should().BeEmpty("Videra.Core must stay free of UI, import, and platform project references");

        gltfReferences.Should().BeEquivalentTo("Videra.Core.csproj");
        objReferences.Should().BeEquivalentTo("Videra.Core.csproj");
        avaloniaReferences.Should().BeEquivalentTo(
            "Videra.Core.csproj",
            "Videra.Import.Gltf.csproj",
            "Videra.Import.Obj.csproj");
        windowsReferences.Should().BeEquivalentTo("Videra.Core.csproj");
        linuxReferences.Should().BeEquivalentTo("Videra.Core.csproj");
        macosReferences.Should().BeEquivalentTo("Videra.Core.csproj");
    }

    [Theory]
    [InlineData(@"..\Videra.Core\Videra.Core.csproj", "Videra.Core.csproj")]
    [InlineData("../Videra.Core/Videra.Core.csproj", "Videra.Core.csproj")]
    public void ProjectReferenceFileNameExtraction_ShouldNormalizeCrossPlatformSeparators(
        string includePath,
        string expectedFileName)
    {
        ArgumentNullException.ThrowIfNull(includePath);
        ArgumentNullException.ThrowIfNull(expectedFileName);
        GetProjectReferenceFileName(includePath).Should().Be(expectedFileName);
    }

    private static HashSet<string> GetPublicSurfaceTypeNames(Assembly assembly)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<Type>();

        foreach (var type in assembly.GetExportedTypes())
        {
            CollectType(type, names, visited);

            foreach (var interfaceType in type.GetInterfaces())
            {
                CollectType(interfaceType, names, visited);
            }

            CollectType(type.BaseType, names, visited);

            foreach (var constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                foreach (var parameter in constructor.GetParameters())
                {
                    CollectType(parameter.ParameterType, names, visited);
                }
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                CollectType(method.ReturnType, names, visited);

                foreach (var parameter in method.GetParameters())
                {
                    CollectType(parameter.ParameterType, names, visited);
                }
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                CollectType(property.PropertyType, names, visited);
            }

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                CollectType(field.FieldType, names, visited);
            }

            foreach (var eventInfo in type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                CollectType(eventInfo.EventHandlerType, names, visited);
            }
        }

        return names;
    }

    private static void CollectType(Type? type, HashSet<string> names, HashSet<Type> visited)
    {
        if (type is null || !visited.Add(type))
        {
            return;
        }

        if (type.HasElementType)
        {
            CollectType(type.GetElementType(), names, visited);
        }

        if (type.IsGenericType)
        {
            CollectType(type.GetGenericTypeDefinition(), names, visited);
            foreach (var argument in type.GetGenericArguments())
            {
                CollectType(argument, names, visited);
            }
        }

        if (!string.IsNullOrWhiteSpace(type.FullName))
        {
            names.Add(type.FullName!);
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

    private static string[] GetProjectReferenceFileNames(string projectPath)
    {
        var document = XDocument.Load(projectPath);

        return document
            .Descendants()
            .Where(element => element.Name.LocalName == "ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include => GetProjectReferenceFileName(include!))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray()!;
    }

    private static string GetProjectReferenceFileName(string includePath)
    {
        ArgumentNullException.ThrowIfNull(includePath);
        return Path.GetFileName(includePath.Replace('\\', '/'));
    }
}
