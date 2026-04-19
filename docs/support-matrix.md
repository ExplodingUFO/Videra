# Support Matrix

This matrix describes the current support boundary for the public viewer packages and the source-only chart modules.

## Viewer stack

| Surface | Platform | Backend | Package path | Validation truth | Support level |
| --- | --- | --- | --- | --- | --- |
| `Videra.Avalonia` + `Videra.Platform.Windows` | Windows 10+ | Direct3D 11 | Public package | Hosted and local matching-host validation | `alpha` |
| `Videra.Avalonia` + `Videra.Platform.Linux` | Linux | Vulkan | Public package | Hosted `X11` validation plus Wayland-session `XWayland` validation | `alpha` |
| `Videra.Avalonia` + `Videra.Platform.macOS` | macOS 10.15+ | Metal | Public package | Hosted and local matching-host validation | `alpha` |
| `Videra.Core` | Any .NET 8 host | Core abstractions only | Public package | Repository verification and integration tests | `alpha` |
| Software fallback | Any supported desktop host | Software | Runtime fallback | Covered by repository verification | Diagnostics/fallback only |

## Source-only chart stack

| Surface | Distribution | Validation truth | Support level | Notes |
| --- | --- | --- | --- | --- |
| `Videra.SurfaceCharts.Core` | Repository source only | Core tests | Source-first `alpha` | No public package promise yet |
| `Videra.SurfaceCharts.Avalonia` | Repository source only | Avalonia integration tests and demo validation | Source-first `alpha` | Independent from `VideraView` |
| `Videra.SurfaceCharts.Processing` | Repository source only | Processing tests and benchmarks | Source-first `alpha` | Data-preparation layer for offline matrices |
| `Videra.SurfaceCharts.Demo` | Repository source only | Demo verification and repository guards | Source-first `alpha` | Public reference for chart behavior |

## Compatibility notes

- `VIDERA_BACKEND` and `PreferredBackend` change backend preference only.
- They do not install missing platform packages.
- They do not replace matching-host native validation.
- The built-in backend minimum contract is buffer creation, current-viewer pipeline creation, direct buffer binding, draw calls, viewport/scissor, clear, and standard frame depth behavior with best-effort depth-state toggles.
- `CreateShader(...)`, `CreateResourceSet(...)`, and `SetResourceSet(...)` are not a cross-backend portability promise for the shipped native backends.
- Linux Wayland remains an `XWayland compatibility` story, not compositor-native embedding.
- This matrix does not imply an `OpenGL` backend promise.
- Use [Alpha Feedback](alpha-feedback.md) when reporting integration issues so the report carries package path, diagnostics, and display-server truth.
