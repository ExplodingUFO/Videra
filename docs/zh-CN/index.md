# 文档导航

[English](../index.md) | [中文](index.md)

本页汇总 Videra 的中文文档入口，便于手动切换后继续阅读。中文导航页与英文首页保持一致：viewer/runtime 核心真相围绕 `SceneDocument` 与 imported asset catalog，当前边界保持 static-scene-only，但 material/runtime truth 还包括 per-primitive non-Blend material participation、occlusion texture binding/strength，以及 `KHR_texture_transform` 的 offset/scale/rotation 和 texture-coordinate override；当前 renderer path 已经消费 baseColor 纹理采样与 occlusion / texture-transform 相关真相，而 emissive 和 normal-map-ready 仍保持为 retained runtime truth。

## 安装与分发入口

- [项目首页](README.md)：`nuget.org` 默认安装路径、`GitHub Packages` preview 说明、`Videra.Avalonia` + `Videra.Platform.*` 组合安装、`Videra.Core` 单独消费，以及默认 viewer 路径对 `Videra.Import.*` 的传递依赖说明
- [包矩阵](../package-matrix.md)：published packages、repository-only demos/samples 与公开安装边界
- [支持矩阵](../support-matrix.md)：平台、验证方式与支持级别
- [发布策略](../release-policy.md)：公开 feed、preview feed 与 repository-only demo/sample 边界
- [发布运行手册](../releasing.md)：tag 发布、release notes、package assets，以及只读 `Release Dry Run` / `release-dry-run-evidence` 验证路径
- [Benchmark Gates](../benchmark-gates.md)：viewer / surface charts benchmark 的自动 PR 阈值门禁与手动运行入口
- [Alpha Feedback](../alpha-feedback.md)：alpha 集成反馈、诊断快照和支持边界
- [故障排查](troubleshooting.md)：`VIDERA_BACKEND`、软件回退和 matching-host 原生验证的边界
- [原生宿主验证](native-validation.md)：GitHub Actions 与本地 matching-host 验证入口

## 开始使用

- [项目首页](README.md)
- [Videra.MinimalSample](../../samples/Videra.MinimalSample/README.md)
- [扩展合同](extensibility.md)：`VideraView.Engine` 流程、`samples/Videra.ExtensibilitySample`、`RegisterPassContributor(...)` / `RegisterFrameHook(...)`、`RenderCapabilities`、`BackendDiagnostics`
- [架构说明](ARCHITECTURE.md)
- [故障排查](troubleshooting.md)
- [原生宿主验证](native-validation.md)
- [贡献指南](CONTRIBUTING.md)

## 模块文档

- [Videra.Core](modules/videra-core.md)
- [Videra.Avalonia](modules/videra-avalonia.md)
- [Videra.Platform.Windows](modules/platform-windows.md)
- [Videra.Platform.Linux](modules/platform-linux.md)
- [Videra.Platform.macOS](modules/platform-macos.md)
- [Videra.Demo](modules/demo.md)

## 其他

- [英文文档首页](../index.md)
- 英文版为准
- `package discovery` 与 `plugin loading` 仍然不在当前公开扩展范围内，详情见 [扩展合同](extensibility.md)
