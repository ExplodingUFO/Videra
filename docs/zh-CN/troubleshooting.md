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

## 常见问题

- `Failed to create D3D11 device`：检查 Windows GPU 和驱动
- `Failed to create Vulkan instance`：检查 Linux Vulkan 运行库与 X11
- `Failed to create Metal device`：确认 macOS 主机支持 Metal
- 渲染区域空白：优先尝试 `VIDERA_BACKEND=software`
- Demo 无内容：等待 `VideraView` 完成后端初始化

## 相关文档

- [英文故障排查](../troubleshooting.md)
- [中文首页](README.md)
- [架构说明](ARCHITECTURE.md)
