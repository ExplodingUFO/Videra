# Contributing to Videra

[English](CONTRIBUTING.md) | [中文](docs/zh-CN/CONTRIBUTING.md)

Thanks for contributing to Videra. This guide explains how to build, validate, and submit changes in this repository.

## What We Welcome

- Bug fixes
- Documentation improvements
- Test coverage and stabilization work
- New platform integration and backend improvements
- Demo usability improvements

For larger capability changes, open an issue first to discuss scope and approach.

## Development Environment

### Required Tools

- .NET 8 SDK
- Git
- A C# IDE such as Visual Studio, Rider, or VS Code

### Platform Prerequisites

| Platform | Backend | Prerequisites |
| --- | --- | --- |
| Windows | Direct3D 11 | D3D11-capable GPU |
| Linux | Vulkan | Vulkan drivers and X11 runtime libraries |
| macOS | Metal | Metal-capable hardware |

## Local Setup

```bash
git clone https://github.com/ExplodingUFO/Videra.git
cd Videra
dotnet restore
dotnet build Videra.slnx
```

Run the demo:

```bash
dotnet run --project samples/Videra.Demo/Videra.Demo.csproj
```

## Working Principles

- Keep changes focused; do not mix unrelated refactors into the same change
- Update docs whenever a public API or workflow changes
- Validate platform-specific changes on the matching host whenever possible
- Do not commit local experiments, commented-out legacy code, or temporary debug scaffolding

## Validation

Run the standard verification entrypoint before submitting work:

```bash
# Unix shell
./verify.sh --configuration Release

# PowerShell
pwsh -File ./verify.ps1 -Configuration Release
```

If your change touches native-host paths, add explicit platform validation:

```bash
# Linux
./verify.sh --configuration Release --include-native-linux
pwsh -File ./verify.ps1 -Configuration Release -IncludeNativeLinux

# macOS
./verify.sh --configuration Release --include-native-macos
pwsh -File ./verify.ps1 -Configuration Release -IncludeNativeMacOS
```

You can also run tests directly when needed:

```bash
dotnet test Videra.slnx
```

## Documentation Expectations

Update docs when you:

- Change behavior or limitations documented in `README.md`
- Modify public `VideraView` APIs or usage examples
- Add platform dependencies, environment variables, or verification commands
- Add, deprecate, or archive a module or long-lived design decision

Primary docs to keep current:

- [README.md](README.md)
- [ARCHITECTURE.md](ARCHITECTURE.md)
- [docs/troubleshooting.md](docs/troubleshooting.md)
- Package-level README files

## Commit Style

Conventional Commits are recommended:

```text
<type>(<scope>): <summary>
```

Common types:

- `feat`
- `fix`
- `docs`
- `refactor`
- `test`
- `chore`

Examples:

```text
feat(core): add style preset serialization
fix(linux): handle missing X11 runtime fallback
docs(readme): clarify platform validation scope
test(macos): cover NSView host lifecycle
```

## Pull Requests

Before opening a PR, confirm that:

- The change goal is clear and the problem/solution are described
- Repository verification has been run
- Platform-specific changes were validated on the matching host, or gaps are stated explicitly
- Docs and examples were updated when public behavior changed
- No temporary debug output or local-only scaffolding remains

## Platform Notes

### Windows

- Re-check D3D11 initialization, swapchain, and resize behavior when touching host integration
- Re-run the standard verification path for any native host change

### Linux

- The current official native path is X11 + Vulkan
- Do not treat X11 validation as proof of Wayland support

### macOS

- The current backend depends on `NSView`, `CAMetalLayer`, and Objective-C runtime interop
- Validate on a real macOS host when touching native host or render-path code

## Questions and Feedback

- Use Issues for bugs, compatibility problems, and documentation gaps
- For larger design disagreements, sync in an issue before opening a major PR

## Chinese Docs

- [中文入口](docs/zh-CN/index.md)
