# Phase 74 Research

- `FrameAll()` / `ResetCamera()` 已经存在，但 inspection workflow 还缺一个可保存、可恢复、可导出的状态对象。
- snapshot export 必须与 on-screen overlay/clipping truth 一致，否则 support 和 sharing 价值会立刻下降。
- 生产路径不能依赖测试专用 headless 环境；导出实现需要在正常 runtime 环境里工作。
