# 故障排查

本文档汇总 Videra 常见的构建、运行和平台后端问题。

## 快速诊断

优先运行仓库统一验证入口：

```bash
# Unix shell
./verify.sh --configuration Release

# PowerShell
pwsh -File ./verify.ps1 -Configuration Release
```

如果问题与原生 Linux 或 macOS 后端有关，请显式启用对应验证：

```bash
./verify.sh --configuration Release --include-native-linux
./verify.sh --configuration Release --include-native-macos

pwsh -File ./verify.ps1 -Configuration Release -IncludeNativeLinux
pwsh -File ./verify.ps1 -Configuration Release -IncludeNativeMacOS
```

## 常见问题

| 问题 | 平台 | 建议处理 |
| --- | --- | --- |
| `Failed to create D3D11 device` | Windows | 更新 GPU 驱动，确认显卡支持 Direct3D 11 |
| `Failed to create Vulkan instance` | Linux | 检查 Vulkan 驱动与运行库，确认主机可用 X11 |
| `Failed to create X11 Vulkan surface` | Linux | 确认 X11 会话有效，且系统存在可用的 `libX11` |
| `Failed to create Metal device` | macOS | 确认设备支持 Metal，并在真实 macOS 主机上验证 |
| 渲染区域空白 | 全平台 | 先尝试 `VIDERA_BACKEND=software`，确认是否为 GPU 或原生宿主问题 |
| 模型导入失败 | 全平台 | 当前仅支持 `.gltf`、`.glb`、`.obj`，请确认文件格式与内容有效 |
| Demo 启动但无法显示模型 | 全平台 | 等待 `VideraView` 完成后端初始化，再执行导入或场景变更 |

## 平台专项说明

### Windows

- 标准验证会覆盖 Windows 后端测试和真实 HWND 路径
- 如果改动涉及 D3D11 初始化、交换链或宿主窗口行为，建议重新执行：

```bash
pwsh -File ./verify.ps1 -Configuration Release
```

### Linux

- 当前正式原生路径基于 X11 + Vulkan
- `libX11.so.6` 缺失时，仓库已有回退解析逻辑，但系统仍必须安装可用 X11 运行库
- Wayland 目前不是正式支持目标

### macOS

- 当前原生路径基于 `NSView` + `CAMetalLayer` + Metal
- 后端通过 Objective-C runtime 互操作与系统框架通信
- 完整原生闭环验证必须在 macOS 主机上执行

## 环境变量

| 变量 | 作用 | 可选值 |
| --- | --- | --- |
| `VIDERA_BACKEND` | 强制指定渲染后端 | `software`, `d3d11`, `vulkan`, `metal`, `auto` |
| `VIDERA_FRAMELOG` | 启用帧日志 | `1`, `true` |
| `VIDERA_INPUTLOG` | 启用输入日志 | `1`, `true` |

建议先从 `VIDERA_BACKEND=software` 开始缩小问题范围，再切换回目标原生后端。

## 提交 Issue 时建议附带的信息

- 操作系统与版本
- GPU 与驱动信息
- 使用的后端偏好或 `VIDERA_BACKEND` 值
- 失败命令与完整报错
- 是否在对应原生宿主机上复现
- 是否能通过软件后端成功运行

## 相关文档

- [README.md](../README.md)
- [ARCHITECTURE.md](../ARCHITECTURE.md)
- [CONTRIBUTING.md](../CONTRIBUTING.md)
- [samples/Videra.Demo/README.md](../samples/Videra.Demo/README.md)
