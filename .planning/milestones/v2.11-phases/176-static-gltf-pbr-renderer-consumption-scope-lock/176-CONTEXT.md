# Phase 176 Context: Static glTF/PBR Renderer Consumption Scope Lock

## Goal

Freeze `v2.11` to one narrow renderer-consumption slice: make already-imported static glTF/PBR metadata visible on the shipped renderer path without widening product scope.

## Problem

- The repo already imports and retains `KHR_texture_transform`, texture-coordinate override, occlusion texture binding/strength, tangent-aware mesh data, and related static glTF/PBR metadata.
- Docs currently describe much of that data as imported-asset/runtime truth only, not renderer-consumed truth.
- After `v2.10` tightened the imported-asset/runtime bridge, the next highest-value gap is renderer consumption of existing metadata, not broader importer/runtime/chart/platform expansion.

## Scope

- Phase `177` may consume existing texture-transform and UV-set metadata on the current static-scene renderer path.
- Phase `178` may consume existing occlusion metadata and add bounded golden-scene evidence.
- Phase `179` may align docs, support wording, and repository guardrails with the newly consumed renderer truth.

## Non-Goals

- No animation, skeleton, morph, lights, shadows, environment maps, post-processing, or broad advanced transparency work.
- No new importer format/extension breadth beyond metadata already retained in runtime truth.
- No new chart family, chart-kernel widening, extra UI adapter, backend/API expansion, compatibility shim, or migration adapter.
- No package publishing or repository tag creation.

## Assumption

This milestone should consume retained metadata through the current material/runtime/render line, not by introducing a generic material-system rewrite or a broader renderer architecture change.
