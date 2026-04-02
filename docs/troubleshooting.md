# Troubleshooting

常见渲染问题、平台依赖问题与验证建议。

## 快速诊断

优先执行仓库统一验证入口：

```bash
./verify.sh --configuration Release
# PowerShell
pwsh -File ./verify.ps1 -Configuration Release
```

如涉及原生 Linux / macOS 后端验证，使用显式开关：

```bash
./verify.sh --configuration Release --include-native-linux
./verify.sh --configuration Release --include-native-macos

pwsh -File ./verify.ps1 -Configuration Release -IncludeNativeLinux
pwsh -File ./verify.ps1 -Configuration Release -IncludeNativeMacOS
```

## 常见问题

| 问题 | 平台 | 建议处理 |
|------|------|----------|
| `Failed to create D3D11 device` | Windows | 更新 GPU 驱动，确认显卡支持 Direct3D 11 Feature Level 11_0 |
| `Failed to create Vulkan instance` / `Failed to create X11 Vulkan surface` | Linux | 安装 Vulkan 驱动并确认 X11 可用；先执行 `./verify.sh --configuration Release --include-native-linux` |
| `Failed to create Metal device` | macOS | 确认机器支持 Metal，并在原生 macOS 主机上执行 `./verify.sh --configuration Release --include-native-macos` |
| 渲染窗口空白 | 全平台 | 先确认 Demo 已成功初始化后端；再尝试 `VIDERA_BACKEND=software` 排除原生驱动问题 |
| Linux 下找不到 `libX11.so.6` | Linux | 仓库已支持回退到 `libX11.so` / `libX11`，但仍需安装可用的 X11 运行库 |
| 模型导入失败 | 全平台 | 当前仅支持 `.gltf`、`.glb`、`.obj`；确认文件存在且内容有效 |
| Demo 无法导入模型 | 全平台 | 等待 `VideraView` 后端完成初始化后再导入；查看 Demo 状态区消息 |

## 平台说明

### Windows

- Demo 默认优先固定到 D3D11 路径，便于验证真实原生渲染路径。
- 标准验证会覆盖解决方案构建、测试以及 Windows 后端测试。
- 如修改了 D3D11 或 Windows 原生宿主代码，优先重新运行：

```bash
pwsh -File ./verify.ps1 -Configuration Release
```

### Linux

- 当前正式原生路径基于 X11 + Vulkan。
- `libX11.so.6` 已支持仓库级 fallback 解析到 `libX11.so` / `libX11`。
- Wayland 仍是开放项；当前不能把 X11 支持等同于 Wayland 支持。
- 真实 Linux 原生闭环仍需在 Linux 宿主机上执行 `tests/Videra.Platform.Linux.Tests`。

### macOS

- 当前原生路径基于 NSView + CAMetalLayer + Metal。
- Objective-C runtime 互操作已集中到 `ObjCRuntime`，但更高层安全绑定替代仍未完成。
- 真实 macOS 原生闭环仍需在 macOS 宿主机上执行 `tests/Videra.Platform.macOS.Tests`。

## 环境变量

| 变量 | 作用 | 可选值 |
|------|------|--------|
| `VIDERA_BACKEND` | 强制指定渲染后端 | `software`, `d3d11`, `vulkan`, `metal`, `auto` |
| `VIDERA_FRAMELOG` | 启用帧日志 | `1`, `true` |
| `VIDERA_INPUTLOG` | 启用输入日志 | `1`, `true` |

## 如果问题仍然存在

1. 先执行对应验证入口并保留完整输出。
2. 确认问题是否只在特定平台原生宿主上出现。
3. 如果是平台后端问题，附上失败命令、平台信息、驱动信息和相关异常消息。
4. 若为文档或使用问题，可直接更新相关 README / CONTRIBUTING 说明。
