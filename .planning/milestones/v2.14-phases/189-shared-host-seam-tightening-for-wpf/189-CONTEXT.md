# Phase 189 Context: Shared Host-Seam Tightening for WPF

## Milestone

`v2.14 WPF Hosted Adapter Validation`

## Requirement

`WPF-02`: Tighten only the minimum host seams needed for the existing viewer/runtime line to host cleanly through the WPF proof path while avoiding a general multi-shell abstraction layer or a second public UI package line.

## Scope

- `RenderSessionOrchestrator` host-input synchronization only.
- Existing Avalonia session/bridge reuse only where it can adopt the same shared host-sync path.
- `smoke/Videra.WpfSmoke` host proof only where it stops hand-assembling the same attach/bind/resize sequence.
- Focused integration/configuration tests only.

## Starting Point

- Avalonia and WPF already share `RenderSessionOrchestrator`, but the WPF smoke path still manually sequences `Attach(...)`, `BindHandle(...)`, and `Resize(...)` in `MainWindow.xaml.cs`.
- `VideraViewSessionBridge` already owns the Avalonia host bridge, but it still composes the same host-input pieces across separate calls.
- The milestone scope lock explicitly forbids turning this into a general shell framework or public multi-host API.
