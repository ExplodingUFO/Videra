# 故障排查

[English](../troubleshooting.md) | [中文](troubleshooting.md)

本文档汇总 Videra 常见的构建、运行和平台后端问题。

## 快速诊断

优先运行仓库统一验证入口：

```bash
./verify.sh --configuration Release
pwsh -File ./verify.ps1 -Configuration Release
```

如果问题与 Linux 或 macOS 原生后端有关，请显式启用：

```bash
./verify.sh --configuration Release --include-native-linux
./verify.sh --configuration Release --include-native-macos

pwsh -File ./verify.ps1 -Configuration Release -IncludeNativeLinux
pwsh -File ./verify.ps1 -Configuration Release -IncludeNativeMacOS
```

如果你需要匹配宿主执行步骤，或想使用 GitHub Actions 上的手动入口，请查看[原生宿主验证](native-validation.md)。

## 安装与后端偏好边界

- Avalonia 应用请安装 `Videra.Avalonia` 加匹配平台包：`Videra.Platform.Windows`、`Videra.Platform.Linux` 或 `Videra.Platform.macOS`
- 只需要核心渲染抽象时再单独安装 `Videra.Core`
- 软件回退适合诊断，但不会安装缺失的平台包
- `VIDERA_BACKEND` 只影响后端选择偏好，不会安装缺失的平台包，也不会替代 matching-host 原生验证

## 常见问题

- `Failed to create D3D11 device`：检查 Windows GPU 和驱动
- `Failed to create Vulkan instance`：检查 Linux Vulkan 运行库与 X11
- `Failed to create Metal device`：确认 macOS 主机支持 Metal
- 渲染区域空白：优先尝试 `VIDERA_BACKEND=software`
- Demo 无内容：等待 `VideraView` 完成后端初始化

## 相关文档

- [英文故障排查](../troubleshooting.md)
- [原生宿主验证](native-validation.md)
- [中文首页](README.md)
- [架构说明](ARCHITECTURE.md)
