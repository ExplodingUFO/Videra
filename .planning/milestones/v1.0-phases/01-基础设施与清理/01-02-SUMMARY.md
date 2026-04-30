---
phase: 01-基础设施与清理
plan: 02
subsystem: logging
tags: [serilog, structured-logging, console-cleanup, microsoft.extensions.logging]
dependency_graph:
  requires: []
  provides: [LOG-01, LOG-02-partial]
  affects: [Videra.Core]
tech_stack:
  added:
    - Serilog 4.2.0
    - Serilog.Extensions.Logging 8.0.0
    - Serilog.Sinks.Console 6.0.0
    - Serilog.Sinks.File 6.0.0
    - Microsoft.Extensions.Logging 9.0.11
    - Microsoft.Extensions.Logging.Abstractions 9.0.11
  patterns:
    - ILogger injection via constructor (VideraEngine)
    - ILogger optional parameter fallback (Object3D, ModelImporter, GraphicsBackendFactory)
    - NullLoggerFactory fallback for non-DI contexts
    - Structured log templates with named properties
key_files:
  created:
    - src/Videra.Core/Logging/ILoggerExtensions.cs
  modified:
    - src/Videra.Core/Videra.Core.csproj
    - src/Videra.Core/Graphics/VideraEngine.cs
    - src/Videra.Core/IO/ModelImporter.cs
    - src/Videra.Core/Graphics/Object3D.cs
    - src/Videra.Core/Graphics/GraphicsBackendFactory.cs
    - src/Videra.Core/Graphics/AxisRenderer.cs
    - src/Videra.Core/Graphics/GridRenderer.cs
    - src/Videra.Core/Graphics/Wireframe/WireframeRenderer.cs
decisions:
  - "ILogger optional parameters with NullLogger fallback for backward compatibility"
  - "Static classes (ModelImporter, GraphicsBackendFactory) use string-named loggers, not generic type loggers"
  - "Frame-level debug logging uses _logger.LogDebug for VideraEngine's per-frame diagnostics"
  - "LogComponentInfo/Debug/Error/Warning helpers in ILoggerExtensions provide consistent structured templates"
metrics:
  duration: 12m
  completed: 2026-03-28
---

# Phase 1 Plan 02: Serilog Integration and Videra.Core Logging Summary

Integrated Serilog 4.2.x structured logging into Videra.Core, replacing all 47 Console.WriteLine calls with ILogger-based structured logging using named properties.

## Tasks Completed

| Task | Name | Commit | Key Files |
|------|------|--------|-----------|
| 1 | Add Serilog dependencies and create logging infrastructure | c7d43bf | Videra.Core.csproj, Logging/ILoggerExtensions.cs |
| 2 | Replace all Console.WriteLine with ILogger calls | 5623e23 | VideraEngine.cs, ModelImporter.cs, Object3D.cs, GraphicsBackendFactory.cs, AxisRenderer.cs, GridRenderer.cs, WireframeRenderer.cs |

## Implementation Details

### Task 1: Serilog Dependencies and Logging Infrastructure

Added 6 NuGet packages to Videra.Core.csproj:
- Serilog 4.2.0, Serilog.Extensions.Logging 8.0.0, Serilog.Sinks.Console 6.0.0, Serilog.Sinks.File 6.0.0
- Microsoft.Extensions.Logging 9.0.11, Microsoft.Extensions.Logging.Abstractions 9.0.11

Created `src/Videra.Core/Logging/ILoggerExtensions.cs` with 4 helper methods:
- `LogComponentInfo(logger, component, message)` - Information level with {Component} and {Message} properties
- `LogComponentDebug(logger, component, action, details)` - Debug level with {Component}, {Action}, {Details}
- `LogComponentError(logger, component, action, error, ex?)` - Error level with optional Exception
- `LogComponentWarning(logger, component, message)` - Warning level with {Component} and {Message}

### Task 2: Console.WriteLine Replacement (47 calls in 7 files)

**VideraEngine.cs** (19 calls): Added `ILogger<VideraEngine>` constructor injection with NullLogger fallback. Frame diagnostics use `_logger.LogDebug` guarded by `shouldLog` flag. Object lifecycle events use `LogInformation`.

**ModelImporter.cs** (10 calls): Static class with `ILogger?` optional parameter on `Load()`. Internal methods receive logger via parameter passing. Used string-named logger ("ModelImporter") since static types cannot be generic type parameters.

**Object3D.cs** (8 calls): Added `ILogger?` optional parameters to `Initialize()` and `InitializeWireframe()` methods with NullLogger fallback. GPU resource creation logs use Debug level, success/failure use Information/Error.

**GraphicsBackendFactory.cs** (7 calls): Static class with `ILoggerFactory?` optional parameter on `CreateBackend()`. Platform mismatch warnings use Warning level, load failures use Error level with exception.

**AxisRenderer.cs** (1 call), **GridRenderer.cs** (1 call), **WireframeRenderer.cs** (1 call): Each uses a NullLogger-backed ILogger field initialized inline.

### Backward Compatibility

All new ILogger/ILoggerFactory parameters are optional with NullLogger fallback, so existing callers (Videra.Avalonia, Videra.Demo) continue to work without changes. Callers can progressively add logger injection.

## Decisions Made

1. **Optional ILogger parameters over breaking changes**: Rather than requiring all callers to provide loggers immediately, all parameters default to NullLogger. This allows incremental migration -- platform projects will be wired in Plan 04.

2. **String-named loggers for static classes**: `CreateLogger("ModelImporter")` instead of `CreateLogger<ModelImporter>()` since C# disallows static types as generic type arguments.

3. **Preserved semantic log content**: Each original Console.WriteLine message's information (component prefix, object name, resource sizes, error details) is preserved as structured properties.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] CS0718 static type as generic parameter**
- **Found during:** Task 2, after writing ModelImporter.cs
- **Issue:** `NullLoggerFactory.Instance.CreateLogger<ModelImporter>()` fails because ModelImporter is a static class and C# disallows static types as generic type arguments
- **Fix:** Changed to `CreateLogger("ModelImporter")` using string-named logger
- **Files modified:** src/Videra.Core/IO/ModelImporter.cs
- **Commit:** 5623e23

## Verification Results

1. `grep -r "Console\.WriteLine" src/Videra.Core/ --include="*.cs"` -- 0 matches (was 47)
2. `dotnet build src/Videra.Core/Videra.Core.csproj` -- succeeded, 0 warnings, 0 errors
3. `dotnet build src/Videra.Avalonia/Videra.Avalonia.csproj` -- succeeded (downstream verification)
4. src/Videra.Core/Videra.Core.csproj contains Serilog 4.2.0 and Microsoft.Extensions.Logging 9.0.11
5. src/Videra.Core/Logging/ILoggerExtensions.cs exists with LogComponentInfo/Debug/Error/Warning methods

## Known Stubs

None. All log messages are fully wired with structured properties and appropriate log levels.

---

*Summary generated: 2026-03-28*
*Plan duration: ~12 minutes*

## Self-Check: PASSED

All 10 key files verified present. Both commits (c7d43bf, 5623e23) verified in git log.
