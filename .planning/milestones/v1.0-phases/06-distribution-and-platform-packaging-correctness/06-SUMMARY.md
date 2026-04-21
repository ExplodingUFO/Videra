---
phase: 06-distribution-and-platform-packaging-correctness
completed: 2026-04-08
requirements_completed:
  - PLAT-03
  - DOC-02
  - DOC-03
---

# Phase 6 Summary

## Outcome

Phase 6 closed the distribution truth gap. `Videra.Avalonia` is no longer treated as a build-host-dependent "all native backends included" package, package publishing is now gated by matching-host evidence, and the public install story is aligned across English and Chinese entrypoints.

## Delivered Changes

### 06-01: Runtime backend discovery and explicit package composition
- `Videra.Avalonia` no longer uses build-host `#if VIDERA_*` defines or host-OS-conditional platform `ProjectReference` items.
- `AvaloniaRuntimeBackendDiscovery` and the updated `AvaloniaGraphicsBackendResolver` moved backend availability decisions to runtime.
- Demo/source-build composition was made explicit: the sample app now references the matching platform backend instead of relying on `Videra.Avalonia` to carry all native implementations implicitly.
- Repository tests now lock the runtime discovery and sample-composition contract.

### 06-02: Matching-host release evidence and package semantic validation
- CI and publish workflows were restructured around matching-host evidence instead of Windows-only packing.
- Package validation was centralized in `scripts/Validate-Packages.ps1` and applied to all five published packages.
- Publish flow now depends on fresh Linux, macOS, and Windows native validation plus package evidence.
- Repository release-readiness tests were extended so workflow drift is caught in-repo before release-time.

### 06-03: Install/docs truth closure
- Root README, troubleshooting, docs index, module READMEs, and Chinese mirrors now describe the same package model:
  - `Videra.Core` for core-only consumption
  - `Videra.Avalonia` as the UI/control entrypoint
  - matching `Videra.Platform.*` packages for native backend support
- `VIDERA_BACKEND` is documented as a preference selector only, not an installation mechanism.
- Package READMEs now stand on their own with feed setup, alpha boundary, and matching-host validation guidance.
- Repository localization and release-readiness tests guard the distribution truth in both English and Chinese docs.

## Requirements Closed In This Phase

- `PLAT-03`: cross-platform build, packaging, and hosted validation evidence are now part of the release contract.
- `DOC-02`: install and consumption guidance is explicit for core-only, Avalonia, and platform-specific package consumers.
- `DOC-03`: troubleshooting, package READMEs, publish workflow, and validation guidance are aligned and test-guarded.

## Key Files

### New or Introduced
- `src/Videra.Avalonia/Composition/AvaloniaRuntimeBackendDiscovery.cs`
- `scripts/Validate-Packages.ps1`
- `docs/plans/2026-04-08-native-ci-gating-design.md`
- `docs/plans/2026-04-08-native-ci-gating-implementation.md`

### Updated
- `src/Videra.Avalonia/Videra.Avalonia.csproj`
- `src/Videra.Avalonia/Composition/AvaloniaGraphicsBackendResolver.cs`
- `samples/Videra.Demo/Videra.Demo.csproj`
- `.github/workflows/ci.yml`
- `.github/workflows/native-validation.yml`
- `.github/workflows/publish-nuget.yml`
- `verify.ps1`
- `verify.sh`
- `README.md`
- `docs/index.md`
- `docs/troubleshooting.md`
- `src/Videra.Avalonia/README.md`
- `src/Videra.Core/README.md`
- `src/Videra.Platform.Windows/README.md`
- `src/Videra.Platform.Linux/README.md`
- `src/Videra.Platform.macOS/README.md`
- `docs/zh-CN/README.md`
- `docs/zh-CN/modules/videra-avalonia.md`
- `docs/zh-CN/modules/videra-core.md`
- `docs/zh-CN/modules/platform-windows.md`
- `docs/zh-CN/modules/platform-linux.md`
- `docs/zh-CN/modules/platform-macos.md`

## Result

Phase 6 removed the largest remaining "distribution is whatever the current machine built" ambiguity. The package graph, release workflows, install docs, and repository guards now describe the same cross-platform distribution model.
