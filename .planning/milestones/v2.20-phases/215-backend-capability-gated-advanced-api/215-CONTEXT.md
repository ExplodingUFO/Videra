# Phase 215 Context

## Goal

Make advanced backend seams visibly capability-gated so consumers can detect unsupported shader/resource-set behavior before runtime exceptions, without widening backend architecture.

## Scope

- Do not remove or rename existing `IResourceFactory` / `ICommandExecutor` methods.
- Do not add compatibility fallback behavior for unsupported advanced APIs.
- Expose capability truth through existing render capability and diagnostics surfaces.
- Keep the normal viewer rendering minimum contract unchanged.

## Success Criteria

- Advanced backend operations are visible through explicit capability flags.
- Public docs and diagnostics explain the `CreateShader(...)`, `CreateResourceSet(...)`, and `SetResourceSet(...)` boundary.
- Native backends continue to throw `UnsupportedOperationException` for unsupported advanced calls while reporting capability flags.

