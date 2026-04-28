# Videra.SurfaceCharts.Rendering

`Videra.SurfaceCharts.Rendering` provides the rendering-runtime layer for the SurfaceCharts package family.

`nuget.org` is the default public feed for this package. `GitHub Packages` remains `preview` / internal validation only. The current support level is `alpha`.

Most consumers should start with `Videra.SurfaceCharts.Avalonia`, which depends on this package transitively. This package exists so the public chart package line can stay truthful about the current assembly/runtime split without pretending `VideraChartView` owns the entire rendering pipeline by itself.

That chart-local runtime split is also where the current efficiency story lives: tighter interactive residency under camera movement and lower probe-path churn stay on the existing chart-local path.

## Responsibilities

- chart-local render-state orchestration
- `GPU-first` / chart-local `software fallback` backend runtime
- resident tile state and incremental render snapshots
- shared rendering helpers used by `VideraChartView`

## Boundaries

- This package remains independent from `VideraView`.
- It does not define chart-domain contracts; those stay in `Videra.SurfaceCharts.Core`.
- It keeps renderer fallback local to SurfaceCharts contracts and does not imply downshift to viewer/backend modes.
- It is not the primary chart control entrypoint; use `Videra.SurfaceCharts.Avalonia` for that.
