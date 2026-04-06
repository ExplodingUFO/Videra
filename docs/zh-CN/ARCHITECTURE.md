# Videra 架构说明

[English](../../ARCHITECTURE.md) | [中文](ARCHITECTURE.md)

本文档描述 Videra 的公开架构边界、模块职责和运行方式。

## 分层结构

- `Videra.Core`：平台无关的渲染核心、抽象接口、导入与软件回退
- `Videra.Avalonia`：Avalonia 控件层、渲染会话与原生宿主桥接
- `Videra.Platform.Windows`：Direct3D 11 后端
- `Videra.Platform.Linux`：Vulkan 后端
- `Videra.Platform.macOS`：Metal 后端
- `Videra.Demo`：示例应用

## 后端选择

Videra 支持两条后端选择路径：

1. `VideraView.PreferredBackend`
2. `VIDERA_BACKEND` 环境变量

`Auto` 默认按平台选择：

- Windows: `D3D11`
- Linux: `Vulkan`
- macOS: `Metal`

## 验证策略

标准验证入口：

```bash
./verify.sh --configuration Release
pwsh -File ./verify.ps1 -Configuration Release
```

Linux 和 macOS 的原生宿主闭环验证需要显式启用：

```bash
./verify.sh --configuration Release --include-native-linux
./verify.sh --configuration Release --include-native-macos

pwsh -File ./verify.ps1 -Configuration Release -IncludeNativeLinux
pwsh -File ./verify.ps1 -Configuration Release -IncludeNativeMacOS
```

## 相关文档

- [英文架构文档](../../ARCHITECTURE.md)
- [中文文档导航](index.md)
- [故障排查](troubleshooting.md)
