---
phase: 01-基础设施与清理
plan: 04
subsystem: platform-logging
tags: [ilogger, console-cleanup, macos, windows, linux, avalonia]

# Dependency graph
requires:
  - phase: 01-02
    provides: "Videra.Core structured logging pattern and ILoggerExtensions"
provides:
  - "Zero Console.WriteLine calls remain across platform and Avalonia source files"
  - "macOS debug counters removed from MetalCommandExecutor"
  - "Platform and Avalonia projects reference Microsoft.Extensions.Logging.Abstractions"
affects: [Videra.Platform.macOS, Videra.Platform.Windows, Videra.Platform.Linux, Videra.Avalonia]

# Tech tracking
tech-stack:
  added:
    - Microsoft.Extensions.Logging.Abstractions 9.0.11 (platform and Avalonia projects)
  patterns:
    - ILogger injection for platform/resource/backend classes
    - LogDebug for hot-path diagnostics
    - structured logging alignment with Videra.Core plan 01-02

key-files:
  modified:
    - src/Videra.Platform.macOS/MetalCommandExecutor.cs
    - src/Videra.Platform.macOS/MetalResourceFactory.cs
    - src/Videra.Platform.macOS/MetalBackend.cs
    - src/Videra.Platform.macOS/Videra.Platform.macOS.csproj
    - src/Videra.Platform.Windows/D3D11ResourceFactory.cs
    - src/Videra.Platform.Windows/D3D11CommandExecutor.cs
    - src/Videra.Platform.Windows/Videra.Platform.Windows.csproj
    - src/Videra.Platform.Linux/VulkanBackend.cs
    - src/Videra.Platform.Linux/VulkanResourceFactory.cs
    - src/Videra.Platform.Linux/VulkanCommandExecutor.cs
    - src/Videra.Platform.Linux/Videra.Platform.Linux.csproj
    - src/Videra.Avalonia/Controls/VideraView.cs
    - src/Videra.Avalonia/Controls/VideraView.Input.cs
    - src/Videra.Avalonia/Controls/VideraNativeHost.cs
    - src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs
    - src/Videra.Avalonia/Controls/VideraMacOSNativeHost.cs
    - src/Videra.Avalonia/Videra.Avalonia.csproj

decisions:
  - "Platform and Avalonia projects use Microsoft.Extensions.Logging.Abstractions only, keeping logging implementation centralized elsewhere"
  - "Hot-path logging remains at Debug level to avoid runtime overhead"
  - "macOS debug counters were fully removed rather than partially retained behind conditions"

requirements-completed: [LOG-02, LOG-03]

# Metrics
duration: ~22m
completed: 2026-03-28
---

# Phase 1 Plan 04: Platform Logging Cleanup Summary

Replaced all remaining `Console.WriteLine` usage in platform and Avalonia source files with `ILogger`-based structured logging, and removed macOS Metal debug counters from `MetalCommandExecutor`.

## Accomplishments

- Removed all `Console.WriteLine` calls from:
  - `src/Videra.Platform.macOS/`
  - `src/Videra.Platform.Windows/`
  - `src/Videra.Platform.Linux/`
  - `src/Videra.Avalonia/`
- Removed macOS debug counters:
  - `_frameDebugCount`
  - `_setBufferCallCount`
  - `_drawCallCount`
  - `_viewportCallCount`
- Added `Microsoft.Extensions.Logging.Abstractions` package references to platform and Avalonia projects
- Aligned platform logging approach with Phase 01-02 Videra.Core logging pattern
- Verified solution builds successfully with 0 warnings and 0 errors

## Task Commits

1. **Task 1: Remove debug counters and replace Console.WriteLine in platform projects** - `ce35409` (feat)

## Verification Results

1. `grep -r "Console\.WriteLine" src/Videra.Platform.macOS src/Videra.Platform.Windows src/Videra.Platform.Linux src/Videra.Avalonia --include="*.cs" | wc -l` → `0`
2. `grep -r "_frameDebugCount\|_setBufferCallCount\|_drawCallCount\|_viewportCallCount" src/Videra.Platform.macOS src/Videra.Platform.Windows src/Videra.Platform.Linux src/Videra.Avalonia --include="*.cs" | wc -l` → `0`
3. `dotnet build Videra.slnx` → succeeded with 0 warnings, 0 errors

## Self-Check: PASSED

- Commit `ce35409` verified in git log
- All required source areas cleaned
- Build passes cleanly
- Requirements `LOG-02` and `LOG-03` satisfied

---
*Phase: 01-基础设施与清理*
*Completed: 2026-03-28*
