# Phase 190 Context: Repo-Only WPF Golden Path and Diagnostics Parity

## Why now

`Phase 189` closed the minimum shared host-sync seam. The next bounded move is to prove one honest repository-only WPF path on Windows and make its diagnostics artifact speak the same support vocabulary as the existing Avalonia viewer line.

## Scope

- keep `smoke/Videra.WpfSmoke` repository-only
- seed one bounded scene so the smoke path proves an actual rendered frame
- emit a richer diagnostics artifact aligned with current viewer/runtime terminology
- add one repository-native invocation path so Windows-native validation can exercise the WPF smoke proof

## Non-goals

- no second public UI package or host-adapter line
- no runtime/chart/backend/import breadth work
- no broader diagnostics framework or generic multi-shell abstraction
- no compatibility shims or transitional adapters
