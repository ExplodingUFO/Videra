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

View3D.Engine.RegisterFrameHook(RenderFrameHookPoint.FrameEnd, context =>
{
    Console.WriteLine(context.HookPoint);
});

View3D.FrameAll();
var diagnostics = View3D.BackendDiagnostics;
var capabilities = View3D.RenderCapabilities;
```

`VideraView.Engine` is the public extensibility root for custom contributors and frame hooks. `VideraView.BackendDiagnostics` remains the backend/runtime diagnostics shell, while `VideraView.RenderCapabilities` exposes the Core-side capability snapshot.

For the complete public flow, see [docs/extensibility.md](../../docs/extensibility.md) and [samples/Videra.ExtensibilitySample](../../samples/Videra.ExtensibilitySample/README.md). The narrow sample uses `VideraView.Engine`, `RegisterPassContributor(...)`, `RegisterFrameHook(...)`, `LoadModelAsync(...)`, `FrameAll()`, `RenderCapabilities`, and `BackendDiagnostics` together.

Contract notes:

- After the engine is `disposed`, additional contributor and hook registrations are ignored as a `no-op`.
- `RenderCapabilities` remains queryable before initialization and after disposal.
- With `AllowSoftwareFallback = true`, `BackendDiagnostics.IsUsingSoftwareFallback` and `BackendDiagnostics.FallbackReason` explain native backend fallback.
- With `AllowSoftwareFallback = false`, the view stays not ready until the native backend issue is fixed; it does not silently recover through fallback.
- `package discovery` and `plugin loading` remain out of scope.

## Native Host Coverage

- Windows: child `HWND` for Direct3D 11
- Linux: X11 window for Vulkan, or XWayland compatibility inside Wayland sessions
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
- [Extensibility Contract](../../docs/extensibility.md)
- [Architecture](../../ARCHITECTURE.md)
- [Chinese Module Doc](../../docs/zh-CN/modules/videra-avalonia.md)
