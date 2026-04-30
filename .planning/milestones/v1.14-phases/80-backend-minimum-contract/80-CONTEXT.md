# Phase 80: Backend Minimum Contract - Context

**Gathered:** 2026-04-20  
**Status:** Executed and verified  
**Mode:** Autonomous

## Phase Boundary

Phase 80 normalizes the built-in backend story around the minimum contract Videra actually ships and tests today. The phase is intentionally not a new-backend expansion. It narrows contract drift across `D3D11`, `Vulkan`, and `Metal`, makes unsupported seams explicit, and updates docs/tests so the repo stops implying an `OpenGL` promise.

## Implementation Decisions

### Minimum contract truth
- **D-01:** Treat `CreatePipeline(PipelineDescription)` as part of the built-in minimum contract only when it can map onto the existing viewer pipeline shape.
- **D-02:** Treat shader/resource-set authoring seams as non-portable for the shipped backends unless a backend has a real maintained implementation.
- **D-03:** Prefer explicit `UnsupportedOperationException` over silent no-op or backend-specific partial behavior.

### Backend alignment
- **D-04:** Normalize `Vulkan` shader creation to fail explicitly instead of returning a backend-specific object that the broader contract does not support safely.
- **D-05:** Normalize `Metal` pipeline creation to the same minimum-contract pipeline path already used by the built-in viewer.
- **D-06:** Normalize `Metal` resource-set binding to fail explicitly instead of silently succeeding.

### Public truth
- **D-07:** Document the minimum contract in code comments, architecture docs, and support docs so contributors know which seams are stable.
- **D-08:** Keep backend support wording limited to `D3D11`, `Vulkan`, `Metal`, and software fallback; do not widen the story to `OpenGL`.

## Specific Ideas

- Push contract truth into `IResourceFactory` and `ICommandExecutor` XML docs so future backend work sees the same boundary at compile time.
- Lock the wording in repository tests so docs cannot drift into implying a fourth backend.
- Keep the contract small now, because adding a new backend on top of drifting abstractions would multiply maintenance cost.

## Canonical References

### Milestone and requirements
- `.planning/ROADMAP.md` — Phase 80 goal and success criteria.
- `.planning/REQUIREMENTS.md` — `BACK-01`, `BACK-02`, and `BACK-03`.

### Backend seams
- `src/Videra.Core/Graphics/Abstractions/IResourceFactory.cs`
- `src/Videra.Core/Graphics/Abstractions/ICommandExecutor.cs`
- `src/Videra.Platform.Windows/D3D11ResourceFactory.cs`
- `src/Videra.Platform.Linux/VulkanResourceFactory.cs`
- `src/Videra.Platform.macOS/MetalResourceFactory.cs`
- `src/Videra.Platform.macOS/MetalCommandExecutor.cs`

### Docs and contract tests
- `ARCHITECTURE.md`
- `docs/support-matrix.md`
- `src/Videra.Core/README.md`
- `tests/Videra.Core.Tests/Repository/RepositoryArchitectureTests.cs`

## Existing Code Insights

### Reusable assets
- `D3D11` already expressed the minimum built-in pipeline shape the other backends needed to match more closely.
- Repository contract tests already enforced docs truth, so support-matrix wording could be locked without inventing a new harness.

### Risks carried into the phase
- `Vulkan` and `Metal` still exposed seams that looked more capable than the shipped viewer/runtime actually depended on.
- Silent/no-op behavior in backend code made unsupported operations easy to misread as valid portable contracts.

## Deferred Ideas

- Any explicit `OpenGL` backend work.
- Any broader shader authoring or resource-set portability surface.
- Any compositor-native Wayland work; that belongs to host-path planning, not this backend contract phase.

---

*Phase: 80-backend-minimum-contract*  
*Context gathered: 2026-04-20*
