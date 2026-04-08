# Linux Wayland Support Implementation Plan

> **Execution Note:** Implement this plan task-by-task with verification between tasks.

**Goal:** Add real Linux Wayland support, keep X11 as a first-class path, allow XWayland fallback in Wayland sessions, and only then update the public Linux support claim.

**Architecture:** Introduce a compositional Linux display-server selection layer that detects candidate paths, chooses a matched native-host plus Vulkan-surface pair, and records the resolved display server in diagnostics. Keep detection, native-host creation, surface creation, and renderer initialization decoupled so Wayland support does not turn into Linux-specific branch sprawl inside `VideraView` or `VulkanBackend`.

**Tech Stack:** .NET 8, Avalonia, Silk.NET Vulkan, xUnit, FluentAssertions, GitHub Actions, Bash, PowerShell

---

### Task 1: Codify the Linux display-server contract with failing tests

**Files:**
- Create: `tests/Videra.Platform.Linux.Tests/Display/LinuxDisplayServerDetectorTests.cs`
- Create: `tests/Videra.Platform.Linux.Tests/Display/LinuxDisplayServerSelectionTests.cs`
- Modify: `tests/Videra.Core.IntegrationTests/Rendering/VideraViewSceneIntegrationTests.cs`
- Modify: `tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs`
- Modify: `tests/Videra.Core.Tests/Repository/RepositoryLocalizationTests.cs`

**Step 1: Write the failing test**

Add tests that assert:

- Linux display-server detection orders candidates correctly:
  - Wayland session with `WAYLAND_DISPLAY` and `DISPLAY` => `Wayland`, then `XWayland`
  - X11-only session => `X11`
  - Unknown session with no usable env vars => empty or explicit unsupported result
- `VideraBackendDiagnostics` can report the resolved Linux display server and whether a display-server fallback occurred
- `VideraView` diagnostics surface can distinguish `Wayland`, `XWayland`, and `X11`
- repository docs and CI are not allowed to claim Wayland support until Linux Wayland validation and docs are updated

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests/Videra.Platform.Linux.Tests/Videra.Platform.Linux.Tests.csproj -c Release --filter "FullyQualifiedName~LinuxDisplayServerDetectorTests|FullyQualifiedName~LinuxDisplayServerSelectionTests"
dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewSceneIntegrationTests"
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~RepositoryReleaseReadinessTests|FullyQualifiedName~RepositoryLocalizationTests"
```

Expected: FAIL because no Linux display-server model exists yet, diagnostics do not expose it, and docs/CI still describe Linux as X11-first.

**Step 3: Write minimal implementation**

Do not add production support yet. Only adjust test helpers if the test projects need new compile-time access to internal Linux types.

**Step 4: Run test to verify it fails**

Run the same commands again and confirm the failures are stable and point to missing display-server orchestration rather than broken test setup.

**Step 5: Commit**

```bash
git add tests/Videra.Platform.Linux.Tests/Display/LinuxDisplayServerDetectorTests.cs tests/Videra.Platform.Linux.Tests/Display/LinuxDisplayServerSelectionTests.cs tests/Videra.Core.IntegrationTests/Rendering/VideraViewSceneIntegrationTests.cs tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs tests/Videra.Core.Tests/Repository/RepositoryLocalizationTests.cs
git commit -m "test: codify linux display server contract"
```

### Task 2: Introduce Linux display-server model, detection, and diagnostics

**Files:**
- Create: `src/Videra.Platform.Linux/LinuxDisplayServerKind.cs`
- Create: `src/Videra.Platform.Linux/LinuxDisplayServerCandidate.cs`
- Create: `src/Videra.Platform.Linux/LinuxDisplayServerResolution.cs`
- Create: `src/Videra.Platform.Linux/LinuxDisplayServerDetector.cs`
- Modify: `src/Videra.Avalonia/Controls/VideraBackendDiagnostics.cs`
- Modify: `tests/Videra.Platform.Linux.Tests/Display/LinuxDisplayServerDetectorTests.cs`
- Modify: `tests/Videra.Platform.Linux.Tests/Display/LinuxDisplayServerSelectionTests.cs`
- Modify if needed for internal access: `src/Videra.Platform.Linux/Videra.Platform.Linux.csproj`

**Step 1: Use the failing tests from Task 1**

Keep the detector, selection, and diagnostics tests active.

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests/Videra.Platform.Linux.Tests/Videra.Platform.Linux.Tests.csproj -c Release --filter "FullyQualifiedName~LinuxDisplayServerDetectorTests|FullyQualifiedName~LinuxDisplayServerSelectionTests"
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Implement:

- `LinuxDisplayServerKind` with `Unknown`, `Wayland`, `X11`, `XWayland`
- `LinuxDisplayServerCandidate` as an immutable value carrying:
  - `DisplayServer`
  - `SessionKind`
  - `AllowsXWaylandFallback`
- `LinuxDisplayServerResolution` as an immutable value carrying:
  - requested candidates
  - resolved display server
  - fallback-used flag
  - failure reason
- `LinuxDisplayServerDetector` with pure detection methods driven by environment values and probe hooks, for example:

```csharp
public sealed class LinuxDisplayServerDetector
{
    public IReadOnlyList<LinuxDisplayServerCandidate> DetectCandidates(
        string? waylandDisplay,
        string? x11Display,
        string? sessionType);
}
```

Extend `VideraBackendDiagnostics` with fields such as:

- `string? ResolvedDisplayServer`
- `bool DisplayServerFallbackUsed`
- `string? DisplayServerFallbackReason`

Do not connect host creation yet. Make the detector and diagnostics contract pass first.

**Step 4: Run test to verify it passes**

Run the same detector and diagnostics tests and confirm they pass.

**Step 5: Commit**

```bash
git add src/Videra.Platform.Linux/LinuxDisplayServerKind.cs src/Videra.Platform.Linux/LinuxDisplayServerCandidate.cs src/Videra.Platform.Linux/LinuxDisplayServerResolution.cs src/Videra.Platform.Linux/LinuxDisplayServerDetector.cs src/Videra.Avalonia/Controls/VideraBackendDiagnostics.cs tests/Videra.Platform.Linux.Tests/Display/LinuxDisplayServerDetectorTests.cs tests/Videra.Platform.Linux.Tests/Display/LinuxDisplayServerSelectionTests.cs
git commit -m "feat(linux): add display server detection model"
```

### Task 3: Extract Linux native-host selection behind a factory seam

**Files:**
- Modify: `src/Videra.Avalonia/Controls/INativeHostFactory.cs`
- Modify: `src/Videra.Avalonia/Controls/DefaultNativeHostFactory.cs`
- Modify: `src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs`
- Create: `src/Videra.Avalonia/Controls/Linux/LinuxNativeHostFactory.cs`
- Create: `src/Videra.Avalonia/Controls/Linux/LinuxNativeHostSelection.cs`
- Create: `src/Videra.Avalonia/Controls/Linux/LinuxNativeHostSelectionResult.cs`
- Create: `src/Videra.Avalonia/Controls/Linux/X11NativeHost.cs`
- Modify: `src/Videra.Avalonia/Controls/VideraView.cs`
- Modify: `tests/Videra.Core.IntegrationTests/Rendering/VideraViewSceneIntegrationTests.cs`

**Step 1: Write the failing test**

Extend `VideraViewSceneIntegrationTests` with cases that assert:

- Linux host creation no longer lives entirely inside `VideraLinuxNativeHost`
- `VideraView` can consume a Linux native-host selection result without hardcoded X11 branching
- diagnostics can preserve the selected Linux display server through host creation

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewSceneIntegrationTests"
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Refactor the Linux host boundary so that:

- `INativeHostFactory.CreateHost()` becomes capable of returning a Linux selection result or equivalent host wrapper rather than blindly constructing `new VideraLinuxNativeHost()`
- `X11NativeHost` contains the current X11 window creation logic from `VideraLinuxNativeHost`
- `LinuxNativeHostFactory` coordinates:
  - candidate detection
  - pair selection
  - diagnostics-friendly selection result
- `VideraLinuxNativeHost` becomes either:
  - a lightweight coordinator wrapping the selected host, or
  - is replaced entirely if the coordinator layer makes it redundant

The important rule is that `VideraView` must not become the Linux decision engine.

**Step 4: Run test to verify it passes**

Run the same `VideraViewSceneIntegrationTests` filter and confirm the host-selection tests pass.

**Step 5: Commit**

```bash
git add src/Videra.Avalonia/Controls/INativeHostFactory.cs src/Videra.Avalonia/Controls/DefaultNativeHostFactory.cs src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs src/Videra.Avalonia/Controls/Linux/LinuxNativeHostFactory.cs src/Videra.Avalonia/Controls/Linux/LinuxNativeHostSelection.cs src/Videra.Avalonia/Controls/Linux/LinuxNativeHostSelectionResult.cs src/Videra.Avalonia/Controls/Linux/X11NativeHost.cs src/Videra.Avalonia/Controls/VideraView.cs tests/Videra.Core.IntegrationTests/Rendering/VideraViewSceneIntegrationTests.cs
git commit -m "refactor(linux): extract native host selection factory"
```

### Task 4: Implement Wayland native host and Wayland Vulkan surface creation with XWayland fallback

**Files:**
- Create: `src/Videra.Platform.Linux/WaylandSurfaceCreator.cs`
- Create: `src/Videra.Platform.Linux/WaylandNativeLibrary.cs`
- Create: `src/Videra.Avalonia/Controls/Linux/WaylandNativeHost.cs`
- Modify: `src/Videra.Platform.Linux/ISurfaceCreator.cs`
- Modify: `src/Videra.Platform.Linux/VulkanBackend.cs`
- Modify: `src/Videra.Avalonia/Controls/Linux/LinuxNativeHostFactory.cs`
- Modify: `src/Videra.Avalonia/Controls/VideraView.cs`
- Modify: `tests/Videra.Platform.Linux.Tests/Backend/VulkanBackendLifecycleTests.cs`
- Create: `tests/Videra.Platform.Linux.Tests/Backend/VulkanBackendWaylandLifecycleTests.cs`

**Step 1: Write the failing test**

Add tests that assert:

- a Wayland selection can create a Wayland host and initialize `VulkanBackend` with `WaylandSurfaceCreator`
- a simulated Wayland failure falls back to X11 and reports `XWayland`
- the X11 path still works unchanged

Where native Wayland is not available in the current environment, use seam-level tests with fake host and surface creator failures to prove fallback ordering.

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests/Videra.Platform.Linux.Tests/Videra.Platform.Linux.Tests.csproj -c Release --filter "FullyQualifiedName~VulkanBackendLifecycleTests|FullyQualifiedName~VulkanBackendWaylandLifecycleTests|FullyQualifiedName~LinuxDisplayServerSelectionTests"
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Implement:

- `WaylandNativeLibrary` to centralize Wayland P/Invoke loading and symbol resolution
- `WaylandSurfaceCreator` using `VK_KHR_wayland_surface`
- `WaylandNativeHost` for the Linux Wayland native-host path
- fallback logic in `LinuxNativeHostFactory`:
  - try `WaylandNativeHost + WaylandSurfaceCreator`
  - if that fails and X11 is usable in the same session, retry with `X11NativeHost + X11SurfaceCreator`
  - mark that second path as `XWayland`

Keep `VulkanBackend` consuming only the matched `ISurfaceCreator` and compatible native handle. Do not move session-detection logic into `VulkanBackend`.

**Step 4: Run test to verify it passes**

Run the same Linux platform test filter and confirm the selection, lifecycle, and fallback tests pass.

**Step 5: Commit**

```bash
git add src/Videra.Platform.Linux/WaylandSurfaceCreator.cs src/Videra.Platform.Linux/WaylandNativeLibrary.cs src/Videra.Avalonia/Controls/Linux/WaylandNativeHost.cs src/Videra.Platform.Linux/ISurfaceCreator.cs src/Videra.Platform.Linux/VulkanBackend.cs src/Videra.Avalonia/Controls/Linux/LinuxNativeHostFactory.cs src/Videra.Avalonia/Controls/VideraView.cs tests/Videra.Platform.Linux.Tests/Backend/VulkanBackendLifecycleTests.cs tests/Videra.Platform.Linux.Tests/Backend/VulkanBackendWaylandLifecycleTests.cs
git commit -m "feat(linux): add wayland host and xwayland fallback"
```

### Task 5: Expand Linux native fixtures and native-validation CI to prove both X11 and Wayland

**Files:**
- Modify: `tests/Tests.Common/Platform/NativeHostTestHelpers.cs`
- Modify: `tests/Tests.Common/Platform/SupportedOSFactAttribute.cs`
- Modify: `tests/Videra.Platform.Linux.Tests/Backend/VulkanBackendLifecycleTests.cs`
- Modify: `tests/Videra.Platform.Linux.Tests/Videra.Platform.Linux.Tests.csproj`
- Modify: `scripts/run-native-validation.sh`
- Modify: `verify.sh`
- Modify: `verify.ps1`
- Modify: `.github/workflows/native-validation.yml`
- Modify: `.github/workflows/publish-nuget.yml`
- Modify: `tests/Videra.Core.Tests/Repository/RepositoryNativeValidationTests.cs`
- Modify: `tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs`

**Step 1: Write the failing test**

Add repository and test-helper assertions that require:

- a Wayland-capable native fixture alongside the X11 fixture
- Linux native validation scripts that can target X11 and Wayland independently
- GitHub Actions Linux native validation split into separate X11 and Wayland jobs
- release gating to depend on both Linux X11 and Linux Wayland evidence

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~RepositoryNativeValidationTests|FullyQualifiedName~RepositoryReleaseReadinessTests"
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Update:

- `NativeHostTestHelpers` with a Wayland fixture helper and explicit XWayland/fallback helpers if needed
- `run-native-validation.sh` to support `linux-x11` and `linux-wayland`
- `verify.sh` / `verify.ps1` with matching optional flags
- `native-validation.yml` so Linux native evidence becomes:
  - `linux-x11-native`
  - `linux-wayland-native`
- `publish-nuget.yml` so publishing depends on both Linux jobs, plus macOS and Windows

**Step 4: Run test to verify it passes**

Run the same repository test filter and confirm the CI contract now requires both Linux paths.

**Step 5: Commit**

```bash
git add tests/Tests.Common/Platform/NativeHostTestHelpers.cs tests/Tests.Common/Platform/SupportedOSFactAttribute.cs tests/Videra.Platform.Linux.Tests/Backend/VulkanBackendLifecycleTests.cs tests/Videra.Platform.Linux.Tests/Videra.Platform.Linux.Tests.csproj scripts/run-native-validation.sh verify.sh verify.ps1 .github/workflows/native-validation.yml .github/workflows/publish-nuget.yml tests/Videra.Core.Tests/Repository/RepositoryNativeValidationTests.cs tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs
git commit -m "ci(linux): require x11 and wayland native validation"
```

### Task 6: Update docs and support claims only after dual Linux native evidence is green

**Files:**
- Modify: `README.md`
- Modify: `docs/troubleshooting.md`
- Modify: `docs/native-validation.md`
- Modify: `src/Videra.Platform.Linux/README.md`
- Modify: `docs/zh-CN/README.md`
- Modify: `docs/zh-CN/troubleshooting.md`
- Modify: `docs/zh-CN/native-validation.md`
- Modify: `docs/zh-CN/modules/platform-linux.md`
- Modify: `tests/Videra.Core.Tests/Repository/RepositoryLocalizationTests.cs`
- Modify: `tests/Videra.Core.Tests/Samples/DemoConfigurationTests.cs`

**Step 1: Write the failing test**

Add doc-level assertions that require:

- Linux is described as supporting both X11 and Wayland
- `Auto` prefers Wayland in Wayland sessions
- XWayland is documented as a compatibility fallback
- the old “Wayland is not yet supported” wording is removed from English and Chinese entrypoints

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~RepositoryLocalizationTests|FullyQualifiedName~DemoConfigurationTests|FullyQualifiedName~RepositoryReleaseReadinessTests"
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Update docs to reflect the final runtime truth:

- Linux native support: X11 and Wayland
- `Auto` selection order
- XWayland fallback behavior
- Linux native validation now includes both X11 and Wayland evidence

Do not update these claims before the workflow and native tests from Task 5 are in place.

**Step 4: Run test to verify it passes**

Run the same doc-focused repository tests and confirm they pass.

**Step 5: Commit**

```bash
git add README.md docs/troubleshooting.md docs/native-validation.md src/Videra.Platform.Linux/README.md docs/zh-CN/README.md docs/zh-CN/troubleshooting.md docs/zh-CN/native-validation.md docs/zh-CN/modules/platform-linux.md tests/Videra.Core.Tests/Repository/RepositoryLocalizationTests.cs tests/Videra.Core.Tests/Samples/DemoConfigurationTests.cs
git commit -m "docs(linux): publish x11 and wayland support contract"
```

### Task 7: Run full verification and capture final support evidence

**Files:**
- Modify only the files already touched if regressions appear

**Step 1: Run focused Linux verification**

Run:

```powershell
dotnet test tests/Videra.Platform.Linux.Tests/Videra.Platform.Linux.Tests.csproj -c Release
dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewSceneIntegrationTests"
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~RepositoryNativeValidationTests|FullyQualifiedName~RepositoryReleaseReadinessTests|FullyQualifiedName~RepositoryLocalizationTests"
```

Expected: PASS.

**Step 2: Run full repository verification**

Run:

```powershell
pwsh -File ./verify.ps1 -Configuration Release
```

Expected: PASS.

**Step 3: Run hosted native validation evidence**

Run or verify:

- Linux X11 native validation job green
- Linux Wayland native validation job green
- macOS native validation job green
- Windows native validation job green

Expected: all required hosted checks PASS.

**Step 4: Review support claim truth**

Confirm all of the following are true before declaring success:

- `Auto` selects Wayland first in Wayland sessions
- XWayland fallback is visible in diagnostics
- X11 still works on native X11 sessions
- docs and CI both require dual Linux evidence

**Step 5: Commit**

```bash
git add .
git commit -m "feat(linux): ship wayland support with xwayland fallback"
```

