---
requirements_completed:
  - BACK-01
  - BACK-02
  - BACK-03
---

# Phase 80 Summary 02

- Routed `MetalResourceFactory.CreatePipeline(PipelineDescription)` through the existing built-in pipeline path so `Metal` now matches the minimum shipped viewer contract.
- Left `D3D11` as the reference implementation for the minimum contract instead of widening abstractions further.
- Tightened backend smoke tests so contract drift shows up as a deliberate failure instead of doc-only guidance.
