# Videra.Avalonia

[English](README.md) | [中文](../../docs/zh-CN/modules/videra-avalonia.md)

`Videra.Avalonia` is the Avalonia integration layer for Videra. It exposes the `VideraView` control, coordinates backend selection, and bridges Avalonia with native host handles on each platform.

Current status: `alpha`. `Videra.Avalonia` is the entry package for Avalonia apps, but it no longer implicitly brings every native backend with it.

## Responsibilities

- Expose the `VideraView` control
- Connect Avalonia visual-tree lifecycle to backend initialization
- Coordinate backend preference and render-session creation
- Map pointer input to camera interaction
- Manage native-host integration for Windows, Linux, and macOS

## Install

Configure GitHub Packages before adding the package:

```bash
dotnet nuget add source "https://nuget.pkg.github.com/ExplodingUFO/index.json" \
  --name github-ExplodingUFO \
  --username YOUR_GITHUB_USER \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text
```

Install the Avalonia entry package and exactly one matching platform package:

```bash
dotnet add package Videra.Avalonia --version 0.1.0-alpha.1 --source github-ExplodingUFO
dotnet add package Videra.Platform.Windows --version 0.1.0-alpha.1 --source github-ExplodingUFO
# or
dotnet add package Videra.Platform.Linux --version 0.1.0-alpha.1 --source github-ExplodingUFO
# or
dotnet add package Videra.Platform.macOS --version 0.1.0-alpha.1 --source github-ExplodingUFO
```

If no matching platform package is installed, the software fallback path can still help with diagnostics, but it does not install missing platform packages.

`PreferredBackend` and `VIDERA_BACKEND` only change backend preference. They do not install missing platform packages and do not replace matching-host native validation.

## Example

```xml
<Window xmlns:videra="using:Videra.Avalonia.Controls">
    <videra:VideraView
        x:Name="View3D"
        BackgroundColor="{Binding BackgroundColor}"
        RenderStyle="{Binding RenderStyle}"
        WireframeMode="{Binding WireframeMode}"
        WireframeColor="{Binding WireframeColor}"
        IsGridVisible="{Binding IsGridVisible}"
        PreferredBackend="Auto" />
</Window>
```

```csharp
using Videra.Avalonia.Controls;
using Videra.Core.Graphics;

View3D.Options = new VideraViewOptions
{
    Backend =
    {
        PreferredBackend = GraphicsBackendPreference.Auto,
        AllowSoftwareFallback = true
    }
};

var result = await View3D.LoadModelsAsync(new[] { "Models/ship.glb", "Models/cube.obj" });
if (!result.Succeeded)
{
    foreach (var failure in result.Failures)
    {
        Console.WriteLine($"{failure.Path}: {failure.ErrorMessage}");
    }
}

var singleResult = await View3D.LoadModelAsync("Models/highlight.glb");
if (!singleResult.Succeeded && singleResult.Failure is not null)
{
    Console.WriteLine(singleResult.Failure.ErrorMessage);
}

View3D.FrameAll();
var diagnostics = View3D.BackendDiagnostics;
```

## Native Host Coverage

- Windows: child `HWND` for Direct3D 11
- Linux: X11 window for Vulkan
- macOS: `NSView` for Metal

## Validation

Use the repository verification scripts for standard and native-host validation:

```bash
./verify.sh --configuration Release
pwsh -File ./verify.ps1 -Configuration Release
```

Linux and macOS native-host validation still require explicit opt-in switches and matching hosts.

## Related Docs

- [Repository README](../../README.md)
- [Architecture](../../ARCHITECTURE.md)
- [Chinese Module Doc](../../docs/zh-CN/modules/videra-avalonia.md)
