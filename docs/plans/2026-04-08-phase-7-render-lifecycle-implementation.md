# Phase 7 Render Lifecycle Safety Implementation Plan

> **Execution Note:** Implement this plan task-by-task with verification between tasks.

**Goal:** Add explicit lifecycle contracts for `VideraEngine` and `RenderSession`, then make software depth-state, wireframe modes, and style-driven wireframe intent consistent with real rendering behavior.

**Architecture:** First stabilize lifecycle ownership with internal state machines and post-dispose `no-op` semantics. Then make software depth-state APIs real and bind wireframe pass selection to an explicit effective contract that combines explicit wireframe overrides with style-driven wireframe intent.

**Tech Stack:** .NET 8, xUnit, FluentAssertions, Avalonia, PowerShell, repository verification scripts

---

### Task 1: Codify lifecycle contract failures before refactoring

**Files:**
- Modify: `tests/Videra.Core.IntegrationTests/Rendering/VideraEngineIntegrationTests.cs`
- Modify: `tests/Videra.Core.IntegrationTests/Rendering/RenderSessionIntegrationTests.cs`
- Create if the existing files get too crowded: `tests/Videra.Core.IntegrationTests/Rendering/VideraEngineLifecycleContractTests.cs`
- Create if the existing files get too crowded: `tests/Videra.Core.IntegrationTests/Rendering/RenderSessionLifecycleContractTests.cs`

**Step 1: Write the failing test**

Add contract tests that assert:

- `VideraEngine.Dispose()` is idempotent
- after `Dispose`, `Draw`, `Resize`, `AddObject`, `RemoveObject`, and `ClearObjects` do not throw and do not resurrect state
- `RenderSession.Dispose()` is idempotent
- after `RenderSession.Dispose()`, `Attach`, `BindHandle`, `Resize`, and `RenderOnce` do not throw and do not recreate backends
- `BindHandle(IntPtr.Zero)` followed by rebind works before dispose but not after dispose

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraEngineLifecycleContractTests|FullyQualifiedName~RenderSessionLifecycleContractTests|FullyQualifiedName~VideraEngineIntegrationTests|FullyQualifiedName~RenderSessionIntegrationTests"
```

Expected: FAIL for the new lifecycle contract cases.

**Step 3: Write minimal implementation**

Do not change software-depth or style behavior yet. Only prepare the failing specification for lifecycle safety.

**Step 4: Run test to verify it fails**

Run the same command again and confirm the failures are stable and meaningful.

### Task 2: Introduce explicit lifecycle states for `VideraEngine`

**Files:**
- Modify: `src/Videra.Core/Graphics/VideraEngine.cs`
- Modify: `src/Videra.Core/Graphics/VideraEngine.Rendering.cs`
- Modify: `src/Videra.Core/Graphics/VideraEngine.Resources.cs`
- Modify: lifecycle tests from Task 1

**Step 1: Use the failing lifecycle tests**

Keep Task 1 tests as the active specification.

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraEngineLifecycleContractTests|FullyQualifiedName~VideraEngineIntegrationTests"
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Refactor `VideraEngine` so that:

- it has an explicit internal lifecycle state
- `Initialize`, `Suspend`, `Draw`, `Resize`, and `Dispose` transition through that state instead of relying only on `_disposed` and field presence
- disposed operations become harmless `no-op`
- suspended operations preserve scene intent without re-creating resources until reactivation

**Step 4: Run test to verify it passes**

Run the same command and confirm the lifecycle tests pass.

### Task 3: Introduce explicit lifecycle states for `RenderSession`

**Files:**
- Modify: `src/Videra.Avalonia/Rendering/RenderSession.cs`
- Modify: `tests/Videra.Core.IntegrationTests/Rendering/RenderSessionIntegrationTests.cs`
- Modify or create: `tests/Videra.Core.IntegrationTests/Rendering/RenderSessionLifecycleContractTests.cs`

**Step 1: Use the failing session tests**

Keep Task 1 session tests active.

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~RenderSessionLifecycleContractTests|FullyQualifiedName~RenderSessionIntegrationTests"
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Refactor `RenderSession` so that:

- it has an explicit internal session state (`Detached`, `WaitingForSize`, `WaitingForHandle`, `Ready`, `Faulted`, `Disposed`)
- render-loop ticks operate only on a stable ready snapshot
- suspend and dispose stop the render loop exactly once
- post-dispose entrypoints become `no-op`
- rebind and resize behavior are driven by explicit state transitions rather than field combinations

**Step 4: Run test to verify it passes**

Run the same command and confirm the session lifecycle tests pass.

### Task 4: Make software depth-state APIs real and fix wireframe-mode semantics

**Files:**
- Modify: `src/Videra.Core/Graphics/Software/SoftwareCommandExecutor.cs`
- Modify: `src/Videra.Core/Graphics/Software/SoftwareBackend.cs`
- Modify: `src/Videra.Core/Graphics/Wireframe/WireframeRenderer.cs`
- Modify: `tests/Videra.Core.Tests/Graphics/Software/SoftwareRasterizerTests.cs`
- Modify: `tests/Videra.Core.Tests/Graphics/Software/SoftwareBackendTests.cs`
- Modify: `tests/Videra.Core.IntegrationTests/Rendering/WireframeRendererIntegrationTests.cs`

**Step 1: Write the failing test**

Add tests that prove:

- `SetDepthState(false, false)` disables depth test and depth writes
- `SetDepthState(true, false)` preserves occlusion without mutating depth
- `ResetDepthState()` restores the solid-pass default
- `Overlay`, `VisibleOnly`, `AllEdges`, and `WireframeOnly` produce distinct behavior in software rendering

Prefer pixel-buffer or copied-frame assertions over “does not throw”.

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SoftwareRasterizerTests|FullyQualifiedName~SoftwareBackendTests"
dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~WireframeRendererIntegrationTests"
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Implement explicit software depth-state tracking in `SoftwareCommandExecutor` and route wireframe passes through the real depth semantics.

**Step 4: Run test to verify it passes**

Run the same commands and confirm the new depth and wireframe tests pass.

### Task 5: Connect style wireframe intent to actual pass selection

**Files:**
- Modify: `src/Videra.Core/Graphics/VideraEngine.Rendering.cs`
- Modify: `src/Videra.Core/Graphics/VideraEngine.Resources.cs`
- Modify: `src/Videra.Avalonia/Controls/VideraView.cs`
- Modify: `src/Videra.Core/Styles/Services/RenderStyleService.cs` only if needed for a clearer contract
- Modify: `tests/Videra.Core.IntegrationTests/Rendering/VideraEngineIntegrationTests.cs`
- Modify: `tests/Videra.Core.IntegrationTests/Styles/StyleEventIntegrationTests.cs`
- Modify or create: `tests/Videra.Core.IntegrationTests/Rendering/VideraViewRenderContractTests.cs`

**Step 1: Write the failing test**

Add tests that prove:

- `RenderStylePreset.Wireframe` changes actual render behavior even when no explicit wireframe override is set
- an explicit non-`None` wireframe mode overrides style-derived default wireframe behavior
- solid-pass suppression for effective `WireframeOnly` is real, not just a style uniform side effect

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraEngineIntegrationTests|FullyQualifiedName~StyleEventIntegrationTests|FullyQualifiedName~VideraViewRenderContractTests"
```

Expected: FAIL.

**Step 3: Write minimal implementation**

Implement an explicit effective-wireframe contract:

- explicit `Wireframe.Mode != None` wins
- otherwise, `StyleService.CurrentParameters.Material.WireframeMode == true` implies effective `WireframeOnly`
- render-loop solid-pass decisions and wireframe-pass execution use that effective mode consistently

**Step 4: Run test to verify it passes**

Run the same command and confirm the rendering contract tests pass.

### Task 6: Run full verification

**Files:**
- Modify only the files already touched if regressions appear

**Step 1: Run focused integration and unit verification**

Run:

```powershell
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SoftwareRasterizerTests|FullyQualifiedName~SoftwareBackendTests"
dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraEngine|FullyQualifiedName~RenderSession|FullyQualifiedName~WireframeRenderer|FullyQualifiedName~StyleEvent"
```

Expected: PASS.

**Step 2: Run full repository verification**

Run:

```powershell
pwsh -File ./verify.ps1 -Configuration Release
```

Expected: PASS.

**Step 3: Review contract consistency**

Confirm that:

- disposed operations are harmless
- software depth APIs are no longer no-op
- wireframe mode and style intent map to real behavior

**Step 4: Commit**

```bash
git add .
git commit -m "refactor: harden render lifecycle and wireframe contract"
```
