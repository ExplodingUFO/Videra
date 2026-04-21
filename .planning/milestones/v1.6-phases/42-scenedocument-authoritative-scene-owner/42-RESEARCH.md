# Phase 42 Research

## Key Context

1. `SceneDocument` existed after v1.5, but it still behaved too much like a mirror of `Engine.SceneObjects`, which kept scene ownership ambiguous.
2. The risky paths were `AddObject`, `ReplaceScene`, `ClearScene`, and `Items` binding changes, because they still depended on engine-first mutation and post-hoc document rebuilds.
3. The strongest guardrail already existed in viewer integration tests, so the phase could be judged by whether those flows still passed after moving to document-first ownership.
