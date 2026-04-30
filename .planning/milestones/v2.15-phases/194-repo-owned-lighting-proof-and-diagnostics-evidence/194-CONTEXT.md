# Phase 194 Context

Phase `194` proves the `v2.15` direct-lighting baseline on repository-owned viewer proof surfaces and captures bounded diagnostics evidence.

Key context:

- `Phase 193` closed the Vulkan native-path direct-lighting contract gap.
- The proof needs to stay on viewer proof apps, not SurfaceCharts.
- The user added one extra hard requirement for this milestone execution: each desktop proof app used in validation must be able to stay alive for `10` seconds without crashing.
- Existing smoke apps already own launch, diagnostics, and shutdown, so the smallest honest hook is an opt-in proof hold rather than changing default behavior.
