# ADR-005: Rust Boundary Policy

- Status: Accepted
- Date: 2026-04-03

## Decision

No Rust by default.

Videra should continue the current C#-first path for architecture decoupling, render-path optimization, and native-boundary hardening. Rust is not approved for broad adoption, engine-core rewrites, Avalonia host glue, or blanket replacement of existing platform backends.

## Context

Phase 5 reviewed two questions:

1. Can the repository be further decoupled without a language change?
2. Is Rust currently justified for performance or safety?

The strongest findings did not point to language as the primary bottleneck:

- `VideraView` owned too many runtime concerns and needed explicit render-session seams.
- Backend composition lived too high in the core path.
- The highest-risk safety issues were native boundary contracts on macOS and Linux.
- The most expensive rendering costs were API usage and architecture decisions, not managed-language throughput.

## Allowed Future Rust Candidates

Rust may only be considered for narrow spikes in these areas:

1. `ModelImporter` and mesh preprocessing, if profiling shows CPU-bound preprocessing that is both hot and stable.
2. Software rasterizer internals, only after profiling proves the hot path remains CPU-bound after the current architectural fixes.
3. A macOS native safety shim, only if the post-hardening C# boundary still shows unacceptable crash or ownership risk.

## Explicitly Prohibited Boundaries

Rust is not approved for:

1. `VideraEngine` or the broader engine core.
2. Avalonia host glue, view composition, or render-session orchestration.
3. A broad rewrite of existing D3D11, Vulkan, or Metal backends.
4. Multi-platform native interop replacement without boundary-specific evidence.

## Evidence Gate For Any Rust Spike

A Rust spike requires concrete evidence before implementation starts:

1. At least one profiler or benchmark capture that isolates a stable hotspot and shows why C# changes are insufficient.
2. Or at least one reproducible native-safety failure pattern with clear ownership, lifetime, or memory-corruption evidence that remains after C# hardening.
3. A narrow boundary proposal with measured inputs/outputs, rollback plan, and interoperability surface.
4. A comparison against a C#-only alternative attempted first or rejected with written rationale.

Without that evidence, the default decision stands: keep improving the C# architecture and native contracts in place.

## Consequences

- Near-term work stays focused on seam extraction, render-path cleanup, and deterministic native failure handling.
- Architecture remains simpler to debug and ship in the current repo.
- Any future Rust discussion must start from measurement or incident evidence, not preference.
