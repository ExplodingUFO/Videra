# Phase 72 Research

- `VideraView` 已经有稳定的 runtime / overlay / diagnostics seams，适合先从 viewer-first clipping 开始给这些边界施压。
- clipping 必须影响 scene truth、overlay truth、snapshot truth 和 diagnostics truth，不能只做某一条渲染路径的视觉效果。
- 这一步需要保持 viewer-first 范围，不能演化成 editor-style clipping gizmo 或 pass-level extensibility 教程。
