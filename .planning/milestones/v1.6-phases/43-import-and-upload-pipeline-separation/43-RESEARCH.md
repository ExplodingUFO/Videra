# Phase 43 Research

## Key Context

1. The pre-phase load path still imported and uploaded in one step, which meant `LoadModelAsync()` immediately depended on a resource factory and could silently fall back to software resources.
2. Batch loading still used serial `foreach await`, so throughput and cancellation semantics were weaker than they needed to be and partial failures could blur scene-replacement meaning.
3. The correct contract for this milestone was backend-neutral import first, then explicit runtime upload once the active backend path is known.
