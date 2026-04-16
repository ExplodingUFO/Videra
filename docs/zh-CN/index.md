# 文档导航

[English](../index.md) | [中文](index.md)

本页汇总 Videra 的中文文档入口，便于手动切换后继续阅读。

## 安装与分发入口

- [项目首页](README.md)：`nuget.org` 默认安装路径、`GitHub Packages` preview 说明、`Videra.Avalonia` + `Videra.Platform.*` 组合安装、`Videra.Core` 单独消费
- [包矩阵](../package-matrix.md)：published packages、source-only modules 与 demos/samples 的边界
- [支持矩阵](../support-matrix.md)：平台、验证方式与支持级别
- [发布策略](../release-policy.md)：公开 feed、preview feed 和 source-only 边界
- [故障排查](troubleshooting.md)：`VIDERA_BACKEND`、软件回退和 matching-host 原生验证的边界
- [原生宿主验证](native-validation.md)：GitHub Actions 与本地 matching-host 验证入口

## 开始使用

- [项目首页](README.md)
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
