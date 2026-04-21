# Phase 54 Research

## Key Context

1. Single and batch import behavior was already reasonable, but the orchestration still lived directly in the runtime scene partial.
2. Import and upload had to stay separate concerns: import produces CPU-side assets and deferred objects, while upload remains queue-driven frame work.
3. The right service needed to preserve bounded concurrency, stable ordering, and all-or-nothing batch replacement semantics.
