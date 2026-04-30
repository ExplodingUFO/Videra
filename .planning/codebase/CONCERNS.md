# Codebase Concerns

**Analysis Date:** 2026-04-02

## Snapshot

**Current project status from planning state:**
- `.planning/STATE.md` reports `2/4` phases complete, `23/24` mapped requirements code-complete, and one remaining strict blocker.
- The blocker is still `TEST-03`: Linux and macOS real native-host lifecycle/render-path execution have not been completed on matching hosts.

## Confirmed Issues

### 1. Phase 1 is still blocked by native-host execution, not by missing code

**Evidence:**
- `.planning/STATE.md` says Windows is validated while Linux and macOS remain environment-blocked.
- `tests/Videra.Platform.Linux.Tests/Backend/VulkanBackendLifecycleTests.cs` and `tests/Videra.Platform.macOS.Tests/Backend/MetalBackendLifecycleTests.cs` exist and are code-ready.
- The default verification scripts only run native Linux/macOS suites when explicitly requested with `--include-native-linux` or `--include-native-macos`.

**Impact:**
- The repo is further along than the old map suggested, but it cannot honestly claim full three-platform native validation yet.
- Phase 1 cannot be marked complete until those suites run successfully on Linux and macOS hosts.

### 2. Legacy smoke placeholders still exist beside the stronger lifecycle suites

**Evidence:**
- `tests/Videra.Platform.Linux.Tests/Backend/VulkanBackendSmokeTests.cs` still contains `VulkanBackend_RealWindowInitialization_CurrentlyRequiresReusableX11Fixture`.
- `tests/Videra.Platform.macOS.Tests/Backend/MetalBackendSmokeTests.cs` still contains `MetalBackend_RealViewInitialization_CurrentlyRequiresReusableNsViewFixture`.

**Impact:**
- These tests can confuse readers into thinking native-host validation is still structurally missing.
- They also weaken the signal of the newer lifecycle suites, which are already the authoritative path.

### 3. Wayland support remains open

**Evidence:**
- `.planning/STATE.md` explicitly calls out Wayland as a remaining gap.
- `src/Videra.Platform.Linux/ISurfaceCreator.cs` and `src/Videra.Platform.Linux/VulkanBackend.cs` mention a future Wayland surface creator, but the repo currently ships only X11-oriented behavior.
- `docs/troubleshooting.md` also states that X11 support should not be treated as Wayland support.

**Impact:**
- Linux support is still effectively "Linux with X11" rather than broad desktop Linux coverage.
- Modern Wayland-first environments remain a known compatibility gap.

### 4. macOS interop is improved but still low-level and fragile

**Evidence:**
- `src/Videra.Platform.macOS/ObjCRuntime.cs` centralizes many Objective-C calls.
- `src/Videra.Platform.macOS/MetalBackend.cs`, `src/Videra.Platform.macOS/MetalResourceFactory.cs`, and `src/Videra.Avalonia/Controls/VideraMacOSNativeHost.cs` still rely on raw `DllImport` plus selector-based messaging.
- `.planning/STATE.md` keeps the higher-level Metal binding replacement as an open item.

**Impact:**
- The repo has reduced duplication, but macOS still depends on stringly typed runtime messaging with limited compile-time guarantees.
- This remains one of the highest-risk maintenance areas in the codebase.

### 5. Logging migration is incomplete at the application boundary

**Evidence:**
- Most production code now uses `ILogger`.
- `samples/Videra.Demo/App.axaml.cs` still uses `Debug.WriteLine` for Assimp resolver diagnostics and unhandled exception traces.
- `src/Videra.Core/Videra.Core.csproj` includes Serilog packages, but no active logger bootstrap was found in source.

**Impact:**
- Library internals are more structured than before, but actual application logging is still only partially wired.
- The repo carries logging dependencies that are not yet fully exercised end to end.

### 6. CI packaging does not enforce quality gates

**Evidence:**
- `.github/workflows/publish-nuget.yml` packs and publishes NuGet packages.
- The workflow does not run `dotnet test`, `verify.ps1`, or `verify.sh`.

**Impact:**
- A tagged release can be packaged without the current automated suite passing in CI.
- This is acceptable for local-only validation strategy, but it is still a release-risk gap for an open-source-ready milestone.

### 7. Unsupported API surface is now explicit, but still present

**Evidence:**
- `src/Videra.Platform.Windows/D3D11ResourceFactory.cs`, `src/Videra.Platform.Windows/D3D11CommandExecutor.cs`, `src/Videra.Platform.Linux/VulkanResourceFactory.cs`, `src/Videra.Platform.Linux/VulkanCommandExecutor.cs`, and `src/Videra.Platform.macOS/MetalResourceFactory.cs` throw `UnsupportedOperationException` for some resource-set or shader paths.

**Impact:**
- This is better than the old `NotImplementedException` behavior because it is explicit and tested.
- It still means parts of the abstraction surface remain intentionally unsupported and must stay documented clearly.

## Areas That Improved Since The Previous Map

- The repo is no longer "untested"; it now has six test projects and substantial automated coverage.
- `Console.WriteLine`-style production logging has mostly been replaced with `ILogger`.
- Public-facing unsupported paths have moved from vague not-implemented behavior to `UnsupportedOperationException`.
- Linux native library resolution is more robust because `src/Videra.Core/NativeLibrary/NativeLibraryHelper.cs` now exists.

## Inferred Risks

### Native-host behavior may still drift across platforms

**Why this is an inference:**
- The three native host controls in `src/Videra.Avalonia/Controls/` are structurally similar but not unified under one lifecycle implementation.
- The actual unresolved work is platform-specific timing, handle creation, and host execution, which often regresses unevenly.

### Runtime fallback behavior is honest but still somewhat implicit

**Why this is an inference:**
- `src/Videra.Core/Graphics/GraphicsBackendFactory.cs` falls back to software when native backend creation fails.
- That is pragmatic, but silent fallback can mask partial native regressions unless logs are wired and observed.

### Analyzer suppression may hide cleanup debt

**Why this is an inference:**
- Several shipping projects carry long `NoWarn` lists in their `*.csproj` files.
- This is a reasonable transitional choice, but it makes it harder to distinguish old debt from new violations over time.

## Priority Order

1. Execute Linux native-host lifecycle tests on a real Linux X11/Vulkan machine.
2. Execute macOS native-host lifecycle tests on a real macOS Metal machine.
3. Decide whether to retire or demote the remaining placeholder smoke assertions.
4. Close the Wayland gap or document Linux support more narrowly and permanently.
5. Finish the logging story by wiring a real logger factory in the demo/app entry path.
6. Add at least one CI job that runs build plus tests before packaging.

## Bottom Line

The codebase is materially ahead of the old `codebase` map. The main story is no longer "missing architecture and missing tests." The real story is "architecture is in place, most cleanup/test work landed, but release confidence is still limited by unexecuted Linux/macOS native validation plus a few remaining platform-readiness gaps."

---

*Concerns analysis refreshed against the live repo on 2026-04-02*
