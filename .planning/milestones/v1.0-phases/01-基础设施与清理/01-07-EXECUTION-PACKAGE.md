# 01-07 Native Execution Package

## Purpose

Close the remaining `TEST-03` environment gap by running the already-written native-host suites on matching operating systems.

This package is for execution only. No new code work is expected before these runs.

## Linux Host Package

### Preconditions

1. Linux host with X11 session available
2. Vulkan-capable driver stack available
3. Repository checked out at the Phase 5 completion state

### Commands

```bash
dotnet test tests/Videra.Platform.Linux.Tests/Videra.Platform.Linux.Tests.csproj -c Release -v q
```

Optional broader verification:

```bash
./verify.sh --configuration Release --include-native-linux
```

### Proof Required

1. `VulkanBackendLifecycleTests` executes on Linux without being skipped by OS guards
2. At least one real X11-backed init/lifecycle/render-path test passes
3. No fallback to `IntPtr.Zero`-only or placeholder-only evidence

## macOS Host Package

### Preconditions

1. macOS host with Cocoa / NSWindow / NSView support
2. Metal framework available
3. Repository checked out at the Phase 5 completion state

### Commands

```bash
dotnet test tests/Videra.Platform.macOS.Tests/Videra.Platform.macOS.Tests.csproj -c Release -v q
```

Optional broader verification:

```bash
./verify.sh --configuration Release --include-native-macos
```

### Proof Required

1. `MetalBackendLifecycleTests` executes on macOS without being skipped by OS guards
2. At least one real NSView-backed init/lifecycle/render-path test passes
3. No fallback to constructor-only or placeholder-only evidence

## Close-Out Rule

After both host packages pass:

1. Update `.planning/phases/01-基础设施与清理/01-VERIFICATION.md`
2. Update `.planning/ROADMAP.md` and `.planning/STATE.md`
3. Re-run `$gsd-progress`
