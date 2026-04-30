# Phase 36 Research

## Key Context

1. The existing ready-state render loop ran every 16ms regardless of whether anything changed. That produced avoidable idle work and also forced software presentation copy to happen on a loop instead of only on dirty frames.
2. The right migration path is not a one-shot invalidate only. Interactive camera gestures still need a temporary continuous cadence, so the scheduler needs invalidation reasons plus an interactive lease model.
3. Diagnostics and fallback truth already lived in `RenderSession` and `RenderSessionOrchestrator`. The refactor therefore had to preserve runtime shell truth while changing the actual driver from permanent polling to invalidation-driven work.
