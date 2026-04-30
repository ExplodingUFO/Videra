# Phase 173 Context: Primitive-Level Upload and Mixed Transparency Bridge

## Goal

Tighten the imported-asset/runtime bridge so imported runtime truth no longer collapses every asset into one guarded `Object3D`.

## Problem

- `SceneObjectFactory.CreateDeferred(...)` still compresses an imported asset into one deferred `Object3D`.
- Mixed `Blend` and non-`Blend` primitives still throw before the runtime can upload or render them.
- `SceneDocument` and residency state still assume one internal runtime object per imported entry even though imported truth is already node/primitive/material based.

## Scope

- Imported runtime entries may carry multiple internal runtime objects.
- Runtime scene flattening, residency, upload, and engine attachment may follow those multiple runtime objects.
- Mixed opaque and transparent primitive participation must survive the runtime bridge.

## Non-Goals

- No queue prioritization, change-kind expansion, or coalescing work from Phase 174.
- No renderer/material breadth expansion beyond what the existing transparent/opaque passes already support.
- No public API redesign for generic multi-object import helpers.
- No compatibility shim, fallback path, or transitional runtime contract.

## Assumption

The public single-object import/upload convenience path may remain as-is for now; Phase 173 only tightens the runtime hot path used by `SceneRuntimeCoordinator`.
