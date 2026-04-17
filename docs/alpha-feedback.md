# Alpha Feedback

Videra is still an early alpha embeddable viewer stack. Good feedback is not just "it failed" — it needs enough integration context to reproduce the host, backend, and package path that produced the failure.

## Before filing

1. Confirm the install path:
   - `nuget.org` public packages
   - `GitHub Packages` preview validation
   - repository source build
2. Capture the exact package version or commit SHA.
3. Reproduce the issue with the smallest flow you can manage:
   - [Videra.MinimalSample](../samples/Videra.MinimalSample/README.md) for the default happy path
   - [Videra.ExtensibilitySample](../samples/Videra.ExtensibilitySample/README.md) for `VideraView.Engine`
   - [Videra.InteractionSample](../samples/Videra.InteractionSample/README.md) for controlled interaction
   - [Videra.Demo](../samples/Videra.Demo/README.md) for backend diagnostics and scene-pipeline visibility

## What to include in a bug report

- Operating system and version
- GPU and driver details when native rendering is involved
- Package install path and package version
- `PreferredBackend` or `VIDERA_BACKEND` value, if you overrode backend preference
- `BackendDiagnostics` snapshot
- Linux display-server context when relevant:
  - `ResolvedDisplayServer`
  - `DisplayServerFallbackUsed`
  - `DisplayServerFallbackReason`
- Whether the issue reproduces in:
  - your host app
  - `Videra.MinimalSample`
  - `consumer smoke`
- Smallest asset or scene that still reproduces the problem

## Support boundary reminders

- `Videra.Avalonia` + one matching platform package is the default public install path.
- Linux native hosting is still `X11` plus `XWayland` compatibility, not compositor-native Wayland embedding.
- `Videra.SurfaceCharts.*` remains source-first and is not part of the public package promise.

## Where to send feedback

- **Bug reports:** GitHub Issues with the bug template
- **Feature/API/docs ideas:** GitHub Issues with the feature template
- **Usage questions / design discussion:** GitHub Discussions
- **Private vulnerabilities:** [SECURITY.md](../SECURITY.md) only
