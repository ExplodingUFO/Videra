# Phase 28 Research

## Observed Gaps

### Axis Presenter

- [SurfaceAxisOverlayPresenter.cs](F:/CodeProjects/DotnetCore/Videra/src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisOverlayPresenter.cs) 当前把 tick generation、visible-edge selection、title formatting、tick layout 全部塞在一个 presenter 里。
- `CreateTickValues(...)` 只有 major ticks，没有 label collision handling，也没有 host-provided formatter。

### Legend Presenter

- [SurfaceLegendOverlayPresenter.cs](F:/CodeProjects/DotnetCore/Videra/src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceLegendOverlayPresenter.cs) 当前 title 固定为 `Value`，formatter 固定 `0.###`，swatch layout 不支持 host customization。

### Control Surface

- [SurfaceChartView.Properties.cs](F:/CodeProjects/DotnetCore/Videra/src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Properties.cs) 当前只有 `Source` / `Viewport` / `ViewState` / `ColorMap` / `InteractionQuality` 相关合同，没有 overlay options。
- [SurfaceChartView.Overlay.cs](F:/CodeProjects/DotnetCore/Videra/src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs) 直接按固定 presenter 合同构建 axis/legend state。

## Chosen Direction

1. **Chart-local overlay options**
   - 在 `SurfaceChartView` 暴露少量 chart-local styled properties/options。
   - 首批包含 formatter、axis title/unit override、minor ticks、grid-plane、axis-side mode。

2. **Presenter split**
   - 把 tick generation、visible-edge selection、label layout/collision 从单一 presenter 里拆开。
   - 保持 `SurfaceAxisOverlayPresenter` 作为组装层。

3. **Professional layout baseline**
   - 先做 deterministic collision culling、自然 edge switching、grid-plane visibility。
   - 不在本 phase 引入复杂文本避让系统或 viewer-level annotation framework。

## Verification Plan

- integration tests covering:
  - label overlap reduction
  - axis side switching across yaw changes
  - formatter/unit/title override truth
  - grid-plane selection and legend/axis formatter consistency
- repository/demo/docs truth updates after behavior lands
