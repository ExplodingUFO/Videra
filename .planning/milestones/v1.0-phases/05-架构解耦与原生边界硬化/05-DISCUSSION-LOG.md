# Phase 5: 架构解耦与原生边界硬化 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-04-02
**Phase:** 05-架构解耦与原生边界硬化
**Areas discussed:** backend composition, render session ownership, native risk hardening, Rust adoption boundary

---

## Backend Composition

| Option | Description | Selected |
|--------|-------------|----------|
| Keep `GraphicsBackendFactory` in `Videra.Core` | Preserve current reflection-driven composition inside Core | |
| Move composition to resolver/registry seam | Keep Core on abstractions, let composition layer decide backend registration | ✓ |
| Full backend packaging rewrite now | Reorganize packages and discovery together in one pass | |

**User's choice:** Auto-selected the resolver/registry seam path.
**Notes:** Phase 5 should not start with a package reshuffle. Additive seam first, compatibility adapter second.

---

## Render Session Ownership

| Option | Description | Selected |
|--------|-------------|----------|
| Keep `VideraView` as the all-in-one owner | UI control continues owning backend, engine, timer, resize, native lifecycle | |
| Extract `RenderSession` / controller | Move backend/session/timer/native lifecycle orchestration out of the control | ✓ |
| Push all lifecycle into `VideraEngine` | Make engine absorb UI and native host concerns | |

**User's choice:** Auto-selected extraction of a render-session/controller seam.
**Notes:** `VideraView` should remain a UI shell plus input surface, not the render orchestration root.

---

## Native Hardening Priority

| Option | Description | Selected |
|--------|-------------|----------|
| Focus evenly on all three native backends | Spread effort across Windows/Linux/macOS immediately | |
| Prioritize macOS then Linux | Fix the highest-risk ObjC/Metal and Vulkan/X11 edges first; keep Windows mostly to low-risk fixes | ✓ |
| Prioritize Windows first | Use the currently available host to continue deeper Windows cleanup before touching other platforms | |

**User's choice:** Auto-selected macOS first, Linux second, Windows last.
**Notes:** Windows already has the strongest host-side evidence. macOS ObjC/Metal and Linux Vulkan have higher unchecked native risk.

---

## Rust Adoption Boundary

| Option | Description | Selected |
|--------|-------------|----------|
| Broad Rust adoption | Move major runtime, native, or engine surfaces into Rust | |
| Narrow, evidence-driven Rust only | Default to no Rust; allow only small coarse-grained spikes after profiling/safety proof | ✓ |
| Immediate Rust for native glue | Replace ObjC/X11/host glue early to improve safety | |

**User's choice:** Auto-selected narrow, evidence-driven Rust only.
**Notes:** Candidate order is: `ModelImporter`/mesh preprocessing first, software rasterizer only if profiled as real bottleneck, macOS native boundary only if C# hardening still leaves unacceptable risk.

---

## the agent's Discretion

- Exact names for seams such as `IGraphicsBackendResolver`, `INativeHostFactory`, and `RenderSession`.
- Whether to split Phase 5 plans by wave into decoupling-first and native-hardening-second, as long as the dependency chain stays explicit.

## Deferred Ideas

- Full `Videra.Core` package split
- Universal graphics abstraction rewrite
- Broad Rust migration
- Rust host/interoperability rewrite before C# hardening

