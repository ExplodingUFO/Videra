# Phase 47 Research

## Key Context

1. Scene pipeline work in v1.7 needed lower-level coverage than the existing demo and broad integration suites could provide.
2. The safest seam was a dedicated `Videra.Avalonia.Tests` project that could exercise runtime-only behavior without inventing new public APIs.
3. Render-session invalidation and the new scene services had to be verifiable without depending on viewer demo wiring.
