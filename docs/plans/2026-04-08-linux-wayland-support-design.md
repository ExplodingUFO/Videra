# Linux Wayland Support Design

## Scope Adjustment

Execution uncovered a framework-level constraint in the current Avalonia `11.3.9` stack: Linux native control hosting still embeds through X11 handles, and the available hosted implementation is `X11NativeControlHost`. Because of that, this design is no longer being executed toward compositor-native Wayland embedding in the current milestone.

The delivered scope is now:

- native Linux X11 rendering
- automatic Wayland-session detection
- explicit `XWayland` compatibility fallback in Wayland sessions
- diagnostics, CI, and docs that report that compatibility path truthfully

True compositor-native Wayland embedding is deferred until the UI stack provides a maintainable native-host path for it.

## Goal

Enable Videra to truthfully claim Linux support for both X11 and Wayland, with `PreferredBackend="Auto"` selecting the correct native path at runtime. In Wayland sessions, the runtime must prefer a native Wayland path and may fall back to XWayland when native Wayland host or Vulkan surface creation is unavailable.

## Approved Constraints

- `Auto` must detect the Linux display server automatically. The host application must not need to configure X11 versus Wayland manually.
- XWayland is an acceptable fallback path in a Wayland session.
- Top priorities are low coupling, maintainability, and code quality.
- The change must include strong automated coverage plus real native validation evidence before public support wording is updated.

## Problem Statement

The current Linux runtime is X11-only in practice:

- [VideraLinuxNativeHost.cs](F:/CodeProjects/DotnetCore/Videra/src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs) hardcodes X11 window creation.
- [VulkanBackend.cs](F:/CodeProjects/DotnetCore/Videra/src/Videra.Platform.Linux/VulkanBackend.cs) defaults to [X11SurfaceCreator.cs](F:/CodeProjects/DotnetCore/Videra/src/Videra.Platform.Linux/X11SurfaceCreator.cs).
- Native validation and test helpers only cover X11-backed lifecycle execution.
- Public docs explicitly describe Linux as X11-first and Wayland as unsupported.

That is a truthful alpha status today, but it blocks the stronger external claim the user wants: Linux should be presented as supporting both X11 and Wayland.

## Recommended Approach

Implement a compositional Linux display-server selection layer:

1. Detect session capabilities separately from window or Vulkan creation.
2. Choose a host/surface pair from an ordered candidate list.
3. Keep host creation, surface creation, and graphics backend orchestration decoupled.
4. Record the resolved Linux display server in diagnostics so UI, docs, and tests can express the actual path used.

This is preferable to:

- only adding Wayland Vulkan surface creation while keeping an X11-only native host
- broad Linux platform rewrites that unify everything in one giant manager

The first would not satisfy the public support claim. The second would raise risk and coupling unnecessarily.

## Runtime Architecture

### 1. Display-Server Model

Introduce a Linux display-server model that is independent from graphics backend preference.

Recommended values:

- `Unknown`
- `Wayland`
- `X11`
- `XWayland`

This model should be used for diagnostics and orchestration. It should not leak Vulkan details into the host layer or X11/Wayland windowing details into the renderer.

### 2. Detection Layer

Add a dedicated detector, for example `LinuxDisplayServerDetector`, that inspects runtime environment and host availability:

- `WAYLAND_DISPLAY`
- `DISPLAY`
- `XDG_SESSION_TYPE`
- native library availability for Wayland/X11
- actual host creation viability when needed

The detector should return an ordered list of runtime candidates rather than a single final answer. This keeps detection simple and makes fallback behavior explicit and testable.

Example candidate ordering:

- Wayland session with both `WAYLAND_DISPLAY` and `DISPLAY` present:
  - `Wayland`
  - `XWayland`
- Wayland session with only `WAYLAND_DISPLAY`:
  - `Wayland`
- X11 session:
  - `X11`
- unknown session with both env vars present:
  - `Wayland`
  - `XWayland`
- unknown session with only `DISPLAY`:
  - `X11`

### 3. Native Host Factory

Replace the current monolithic Linux host implementation with a factory-driven model.

Recommended structure:

- `LinuxNativeHostFactory`
- `IX11NativeHost` or concrete `X11NativeHost`
- `WaylandNativeHost`
- a lightweight coordinating host exposed to Avalonia if needed

Responsibilities:

- native host classes create and manage the platform window/surface handle
- the factory decides which host to instantiate
- `VideraView` consumes a host abstraction, not X11- or Wayland-specific logic

The current [VideraLinuxNativeHost.cs](F:/CodeProjects/DotnetCore/Videra/src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs) should be decomposed rather than extended with large `if/else` branches.

### 4. Surface Creation Boundary

Preserve the existing Linux Vulkan seam:

- [ISurfaceCreator.cs](F:/CodeProjects/DotnetCore/Videra/src/Videra.Platform.Linux/ISurfaceCreator.cs)
- [X11SurfaceCreator.cs](F:/CodeProjects/DotnetCore/Videra/src/Videra.Platform.Linux/X11SurfaceCreator.cs)

Add:

- `WaylandSurfaceCreator`

`VulkanBackend` should continue to consume only the chosen `ISurfaceCreator`. It should not know whether the session was Wayland, XWayland, or X11; it only receives a compatible platform surface creator.

This keeps the rendering backend decoupled from display-server detection policy.

### 5. Startup Flow

The runtime flow under `Auto` should be:

1. detect Linux display-server candidates
2. try creating `WaylandNativeHost + WaylandSurfaceCreator`
3. if that fails and X11 is viable in the same session, try `X11NativeHost + X11SurfaceCreator`
4. label that fallback path as `XWayland` when running inside a Wayland session
5. if neither native path works, continue into the existing backend failure or software fallback logic

The key point is pairwise selection. Host and surface creator must be selected together as a matched tuple. That prevents mismatches such as a Wayland host with an X11 Vulkan surface strategy.

## Diagnostics Contract

Extend [VideraBackendDiagnostics.cs](F:/CodeProjects/DotnetCore/Videra/src/Videra.Avalonia/Controls/VideraBackendDiagnostics.cs) so the runtime can truthfully report:

- resolved display server: `Wayland`, `XWayland`, or `X11`
- whether a display-server fallback occurred
- optional last native-host selection error

This information is required for:

- public docs
- demo status surfaces
- troubleshooting
- native validation assertions

Without this, the project cannot honestly claim Wayland support because users and tests cannot tell which path was actually used.

## Testing Strategy

### Unit Tests

Add decision-layer tests for:

- candidate ordering from environment combinations
- fallback ordering rules
- `Wayland -> XWayland -> failure` transitions
- diagnostics mapping from resolved path to user-visible truth

These tests should not require a real Linux display server.

### Integration Tests

Expand Linux integration coverage to include:

- X11 lifecycle and draw path
- Wayland lifecycle and draw path
- Wayland session falling back to XWayland
- failure behavior when neither path is usable

Current X11-only fixtures in [NativeHostTestHelpers.cs](F:/CodeProjects/DotnetCore/Videra/tests/Tests.Common/Platform/NativeHostTestHelpers.cs) need a Wayland counterpart rather than more X11 specialization.

### Repository / Contract Tests

Add repository-level tests to fix the public contract:

- Linux docs no longer say “Wayland is not supported”
- Linux runtime path is described as X11 + Wayland capable
- diagnostics surface includes display-server resolution

### Native Validation

The current [native-validation.yml](F:/CodeProjects/DotnetCore/Videra/.github/workflows/native-validation.yml) only proves Linux X11 via `xvfb`.

To support the new external claim, Linux validation must split into at least:

- `linux-x11-native`
- `linux-wayland-native`

Both must become required checks before public support wording is updated.

## Documentation Impact

Update all Linux-facing public entrypoints only after native evidence is green:

- [README.md](F:/CodeProjects/DotnetCore/Videra/README.md)
- [docs/troubleshooting.md](F:/CodeProjects/DotnetCore/Videra/docs/troubleshooting.md)
- [src/Videra.Platform.Linux/README.md](F:/CodeProjects/DotnetCore/Videra/src/Videra.Platform.Linux/README.md)
- Chinese mirrors under [docs/zh-CN](F:/CodeProjects/DotnetCore/Videra/docs/zh-CN)

New wording should state:

- Linux supports both X11 and Wayland
- `Auto` prefers native Wayland in Wayland sessions
- XWayland may be used as a compatibility fallback

## Non-Goals

- rewriting the Vulkan backend architecture outside the existing surface seam
- changing public backend preference semantics beyond Linux display-server auto-selection
- broad Linux platform abstraction beyond what is required for low-coupling Wayland support
- making software fallback the primary Linux path

## Risks

### Wayland Native Host Complexity

Wayland host integration is likely the highest-risk area because it affects both window creation and lifetime handling. Mitigation: keep it behind a factory boundary and add native validation early.

### CI Environment Fragility

Wayland CI is more fragile than X11 + `xvfb`. Mitigation: isolate Linux X11 and Linux Wayland jobs, and keep diagnostics rich enough to debug hosted failures.

### Diagnostic Drift

If the runtime falls back silently, docs and tests can diverge from reality. Mitigation: make resolved display server part of the diagnostics contract and assert it in tests.

## Recommended Delivery Slices

1. Introduce Linux display-server model, detector, and factory seams with unit tests.
2. Implement Wayland host + `WaylandSurfaceCreator`, plus targeted integration tests.
3. Expand diagnostics and demo/doc surfaces to expose resolved display-server truth.
4. Add Linux X11 and Linux Wayland hosted native validation jobs.
5. Only then update public support wording from “X11-first” to “X11 and Wayland supported”.

## Decision

Proceed with the compositional Linux display-server architecture:

- native Wayland first under `Auto`
- XWayland accepted as fallback
- X11 retained as a first-class Linux path
- diagnostics and native validation promoted to support-contract gates
