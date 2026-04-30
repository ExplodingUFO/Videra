# Phase 175 Context: Primitive-Centric Runtime Truth and Guardrails

## Goal

Align docs, diagnostics wording, and repository guardrails with the primitive-centric imported-runtime story shipped in Phases 173 and 174.

## Problem

- Several docs and module READMEs still teach the older object-centric runtime truth where mixed `Blend` and non-`Blend` imports remain guarded outright.
- Repository tests still encode parts of that stale wording, so the docs/guardrail layer no longer matches the shipped runtime hot path.
- Scene-delta and upload-queue docs still describe a coarse add/remove/reupload or FIFO mental model instead of typed retained-entry changes plus coalesced attached-first draining.

## Scope

- Update repo docs and README surfaces that describe imported runtime truth, mixed opaque/transparent participation, and scene upload vocabulary.
- Update repository tests that enforce those docs so the new wording is guarded.
- Keep the milestone boundary and deferred renderer breadth explicit.

## Non-Goals

- No new runtime behavior, renderer feature, importer breadth, or transparency-system expansion.
- No localization-wide rewrite beyond files needed to keep repository guards honest.
- No package/release workflow or milestone closeout work in this phase.

## Assumption

Phase 175 should describe the current shipped truth honestly: imported runtime entries may expand into multiple internal runtime objects, while broader primitive-level transparency rendering breadth and future static glTF/PBR metadata consumption remain future work.
