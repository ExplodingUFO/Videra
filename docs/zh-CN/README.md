# Videra

[English](../../README.md) | [中文](README.md)

Videra 是一套面向 .NET 桌面应用的跨平台 3D 查看组件库，核心目标是在 Avalonia 应用中提供可复用、可嵌入、可扩展的 3D 查看能力。

## 项目状态

- 当前处于早期 `alpha`
- 当前默认版本线为 `0.1.0-alpha.1`
- API、包结构和部分平台行为在 `1.0` 前仍可能调整
- 当前 GitHub Packages 安装线更适合 Windows + Avalonia 评估；Linux/macOS 原生后端更建议按源码验证
- GitHub Actions 会在 pull requests 中自动执行跨平台原生验证；Linux 会同时覆盖 `X11` 原生路径和 Wayland 会话下的 `XWayland` 兼容路径；本地 matching-host 运行仍主要用于复现和排障

## 安装与包选择

以下中文说明用于快速索引，英文版为准。

先配置 GitHub Packages 源：

```bash
dotnet nuget add source "https://nuget.pkg.github.com/ExplodingUFO/index.json" \
  --name github-ExplodingUFO \
  --username YOUR_GITHUB_USER \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text
```

Avalonia 应用推荐安装 `Videra.Avalonia` 加一个匹配平台包：

```bash
dotnet add package Videra.Avalonia --version 0.1.0-alpha.1 --source github-ExplodingUFO
dotnet add package Videra.Platform.Windows --version 0.1.0-alpha.1 --source github-ExplodingUFO
# 或
dotnet add package Videra.Platform.Linux --version 0.1.0-alpha.1 --source github-ExplodingUFO
# 或
dotnet add package Videra.Platform.macOS --version 0.1.0-alpha.1 --source github-ExplodingUFO
```

如果只需要渲染抽象和导入管线，则直接安装 `Videra.Core`：

```bash
dotnet add package Videra.Core --version 0.1.0-alpha.1 --source github-ExplodingUFO
```

`VIDERA_BACKEND` 只影响后端选择偏好，不会安装缺失的平台包，也不会替代 matching-host 原生验证。

## 快速入口

- [英文首页](../../README.md)
- [扩展合同](extensibility.md)：`VideraView.Engine`、`RegisterPassContributor(...)`、`RegisterFrameHook(...)`、`RenderCapabilities`、`BackendDiagnostics` 与 `samples/Videra.ExtensibilitySample`
- [架构说明](ARCHITECTURE.md)
- [贡献指南](CONTRIBUTING.md)
- [故障排查](troubleshooting.md)
- [原生宿主验证](native-validation.md)
- [文档导航](index.md)

## 模块文档

- [Videra.Core](modules/videra-core.md)
- [Videra.Avalonia](modules/videra-avalonia.md)
- [Videra.Platform.Windows](modules/platform-windows.md)
- [Videra.Platform.Linux](modules/platform-linux.md)
- [Videra.Platform.macOS](modules/platform-macos.md)
- [Videra.Demo](modules/demo.md)

## 说明

默认公开入口现在以英文为主，英文版为准。中文文档保留为手动切换入口，便于中文读者快速查看项目定位、安装方式、验证方式和模块说明。

扩展入口的中文镜像集中在 [扩展合同](extensibility.md)。该页会把 `samples/Videra.ExtensibilitySample`、`disposed` 后注册调用的 `no-op` 语义，以及软件回退与 `BackendDiagnostics` / `FallbackReason` 的公开约定放在一起说明。
