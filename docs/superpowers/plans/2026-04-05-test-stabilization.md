# Test Stabilization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Stabilize deterministic cross-platform test failures and align macOS Objective-C interop paths before rerunning full verification.

**Architecture:** Keep changes minimal and localized. Fix test assumptions that are invalid on the current host, replace missing file fixtures with generated data, and correct the macOS runtime interop path mismatch between production code and test helpers.

**Tech Stack:** .NET 8, xUnit, Objective-C interop, MSBuild

---

### Task 1: Stabilize deterministic test failures

**Files:**
- Modify: `tests/Videra.Core.Tests/NativeLibrary/NativeLibraryHelperTests.cs`
- Modify: `tests/Videra.Core.Tests/IO/ModelImporterTests.cs`
- Modify: `tests/Videra.Platform.Windows.Tests/Backend/D3D11BackendSmokeTests.cs`
- Modify: `tests/Videra.Platform.Windows.Tests/Backend/D3D11BackendBoundaryTests.cs`
- Modify: `tests/Videra.Platform.Linux.Tests/Backend/VulkanBackendSmokeTests.cs`

- [ ] Write platform-safe native library test inputs.
- [ ] Replace missing OBJ fixture file reads with generated temporary files.
- [ ] Add missing OS guards to platform-specific smoke and boundary tests.
- [ ] Run full verification to confirm deterministic failures are removed.

### Task 2: Align macOS Objective-C interop

**Files:**
- Modify: `src/Videra.Platform.macOS/ObjCRuntime.cs`

- [ ] Update production Objective-C runtime imports to use the same library path as test helpers.
- [ ] Re-run macOS verification and confirm remaining failures, if any, are not caused by the previous path mismatch.
