# Coding Conventions

**Analysis Date:** 2026-04-02

## Naming Patterns

**Namespaces and projects:**
- Namespaces follow project and folder boundaries, such as `Videra.Core.Graphics`, `Videra.Avalonia.Controls`, and `Videra.Platform.Windows`.
- Projects mirror their runtime role: shared engine code in `src/Videra.Core/`, UI bridge code in `src/Videra.Avalonia/`, and native GPU backends in `src/Videra.Platform.*`.

**Types and members:**
- Public types, methods, properties, enums, and records use PascalCase throughout `src/` and `tests/`.
- Interfaces use the `I` prefix, for example `src/Videra.Core/Graphics/Abstractions/IGraphicsBackend.cs` and `src/Videra.Core/Graphics/Abstractions/IResourceFactory.cs`.
- Private fields use `_camelCase`. This is enforced in `.editorconfig`.
- Test names prefer behavior-oriented method names such as `CreateBackend_Software_ReturnsSoftwareBackend` in `tests/Videra.Core.Tests/Graphics/GraphicsBackendFactoryTests.cs`.

## Shared Tooling

**Compiler and analyzer baseline:**
- All projects target `net8.0` in their `*.csproj` files under `src/`, `tests/`, and `samples/`.
- Shared analyzer policy lives in `Directory.Build.props`.
- `Directory.Build.props` enables .NET analyzers, `latest-recommended` analysis level, and `TreatWarningsAsErrors` for Debug builds.
- Most projects also carry explicit `NoWarn` lists in their own `*.csproj` files to keep existing warnings from blocking the current cleanup effort.

**Editor rules:**
- `.editorconfig` is present and sets naming expectations for private fields plus a few analyzer severities.
- Nullable reference types and implicit usings are enabled across the repo via project files.
- XML documentation generation is enabled in shipping projects and the demo app.

## Logging and Diagnostics

**Current pattern:**
- Production code now largely uses `Microsoft.Extensions.Logging.ILogger` rather than `Console.WriteLine`.
- Shared logging helpers live in `src/Videra.Core/Logging/ILoggerExtensions.cs`.
- Core types commonly fall back to `NullLoggerFactory.Instance` when no logger is injected, for example `src/Videra.Core/Graphics/VideraEngine.cs`, `src/Videra.Core/Graphics/Object3D.cs`, and `src/Videra.Avalonia/Controls/VideraView.cs`.

**What is still inconsistent:**
- The demo bootstrap still uses `Debug.WriteLine` in `samples/Videra.Demo/App.axaml.cs` for Assimp preload and exception tracing.
- `src/Videra.Core/Videra.Core.csproj` references Serilog packages, but no active repo-level logger bootstrap such as `UseSerilog` or `LoggerConfiguration` is wired in source today.

## Error Handling Conventions

**Preferred style:**
- Public-facing unsupported paths now throw domain exceptions instead of `NotImplementedException`.
- `src/Videra.Core/Exceptions/UnsupportedOperationException.cs` is the explicit replacement for unsupported API surface.
- Platform backends use guard clauses for null handles, invalid dimensions, and lifecycle misuse before touching native APIs.

**Observed mix:**
- The repo has moved toward domain-specific exceptions in `src/Videra.Core/Exceptions/`.
- Native backend initialization and resource creation still use some generic `Exception` throws for low-level failures, especially in Linux and macOS paths. That is a current pattern, not a desired end state.

## Resource and Lifecycle Patterns

**Disposal:**
- GPU resources implement `IDisposable` and are explicitly cleaned up in platform backends and buffer wrappers.
- `Object3D`, backend classes, and render helpers use clear initialize -> use -> dispose lifecycles.
- Tests actively exercise double-dispose, reinitialize, and partial initialization behavior in platform lifecycle suites.

**Initialization discipline:**
- Backends are typically single-init components. Reinitialization after success is treated as misuse and tested accordingly.
- `src/Videra.Avalonia/Controls/VideraView.cs` retries initialization with bounded `Task.Delay` retries when native handles are not ready yet.
- Scene mutation in `src/Videra.Core/Graphics/VideraEngine.cs` is guarded with `lock (_lock)` around shared object-list access.

## Native and Unsafe Code

**Where unsafe is accepted:**
- Unsafe code is intentionally confined to native backend implementations in `src/Videra.Platform.Windows/`, `src/Videra.Platform.Linux/`, and `src/Videra.Platform.macOS/`.
- Direct `DllImport` calls are also concentrated in platform host helpers and test fixtures.

**Mitigations in use:**
- Linux native library resolution is being centralized through `src/Videra.Core/NativeLibrary/NativeLibraryHelper.cs`.
- macOS interop is partially consolidated through `src/Videra.Platform.macOS/ObjCRuntime.cs`, although some direct Objective-C interop still exists elsewhere.

## Test Coding Style

**Framework choices:**
- Tests use xUnit plus FluentAssertions across `tests/Videra.Core.Tests/`, `tests/Videra.Core.IntegrationTests/`, and the platform test projects.
- Moq is used for abstraction-level unit tests in `tests/Videra.Core.Tests/Graphics/Abstractions/`.

**Test naming and layout:**
- Test classes are grouped by subsystem, for example `tests/Videra.Core.Tests/Graphics/`, `tests/Videra.Core.Tests/Styles/`, and `tests/Videra.Platform.Windows.Tests/Backend/`.
- Platform tests use `RuntimeInformation.IsOSPlatform(...)` guards to keep the full solution runnable on non-matching hosts.

## Practical Rules For New Code

- Put backend-neutral logic in `src/Videra.Core/` first, then add platform-specific glue only where unavoidable.
- Prefer `ILogger`-based diagnostics over `Console.WriteLine` or `Debug.WriteLine`.
- Keep unsupported functionality explicit by throwing `UnsupportedOperationException` with platform context.
- Follow the existing project split instead of adding cross-project shortcuts that bypass `Videra.Core` abstractions.
- Add tests in the mirrored test project rather than treating the demo app as the only verification path.

## Drift Since The Previous Map

- The old map said there was no meaningful test infrastructure. That is no longer true: the repo now has six test projects wired into `Videra.slnx`.
- The old map treated logging as console-based. Current code has substantially migrated to `ILogger`, with only a few debug-oriented leftovers.
- The old map highlighted widespread `NotImplementedException` usage. Current public-facing unsupported paths now use `UnsupportedOperationException` instead.

---

*Convention analysis refreshed against the live repo on 2026-04-02*
