# Phase 66 Research

## Problem

The repository already had benchmark harnesses, but performance evidence still lived as ad hoc local runs. Without an explicit workflow gate, the new viewer-scene benchmarks and older surface-chart benchmarks were not yet part of a visible regression story.

## Findings

- Viewer benchmarks and surface-chart benchmarks could both be driven with BenchmarkDotNet and exported as JSON, CSV, and Markdown artifacts.
- Workflow cost was best controlled by making benchmark runs manual-by-default plus PR-label-triggered instead of mandatory on every push.
- The documentation gap was not how to run benchmarks locally, but how to interpret benchmark outputs as alpha evidence rather than binary pass/fail gates.

## Decision

Phase 66 should add a reusable benchmark runner script, a workflow-dispatch plus labeled-PR benchmark workflow, and supporting docs that explain how to interpret and compare the published artifacts.
