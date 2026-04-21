# Phase 61 Research

## Problem

After `v1.8`, upload budgeting and residency transitions were correct, but there was still no scene-specific benchmark harness and no explicit contract around oversize-first upload behavior. The next performance move was still under-evidenced.

## Findings

- The repository already had BenchmarkDotNet infrastructure, but only for surface charts.
- The current upload queue deliberately allows a single oversize object at the front of the queue to upload even if it exceeds the nominal byte budget.
- Repository guards were the right place to lock the existence and intended scope of a new viewer-scene benchmark project.

## Decision

Phase 61 should add a dedicated viewer-scene benchmark project, benchmark the important scene pipeline seams, and treat queue drain semantics for both normal backlog and oversize-first cases as explicit tested contract.
