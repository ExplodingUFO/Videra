# Phase 64 Research

## Problem

The public alpha story still mixed the shortest `VideraView` integration path with advanced extensibility seams. New users could reach the right APIs, but the first-scene path was not yet isolated enough from `Engine`, frame hooks, and contributor-oriented docs.

## Findings

- `Videra.Avalonia` already exposed the right happy-path surface: `VideraViewOptions`, `LoadModelAsync(...)`, `LoadModelsAsync(...)`, `FrameAll()`, `ResetCamera()`, and `BackendDiagnostics`.
- The repository still lacked a narrow minimal sample that proved those APIs without exposing `VideraView.Engine`.
- Repository guards were already the normal mechanism for locking sample/docs truth across README, module README, and samples.

## Decision

Phase 64 should add a dedicated minimal sample, rewrite the default README/Avalonia README flow around the happy path, and lock the vocabulary with repository tests instead of leaving it as prose drift.
