# 原生宿主验证

[English](../native-validation.md) | [中文](native-validation.md)

本文档说明 Videra 在 Linux、macOS 与 Windows 上的原生宿主验证入口。GitHub Actions 现在会在 pull requests 中把这条路径作为主要的 native validation gate。公开 viewer 路径以 Avalonia 为先，`smoke/Videra.WpfSmoke` 是 repository-only 的 Windows WPF smoke 证明，只用于验证和 support evidence；它不是第二条公开 UI 包线或发布路径。你可以用它查看 CI 行为，也可以通过 `workflow_dispatch` 手动重跑，或在本地匹配宿主上复现平台特定后端问题。

Phase 194 的 proof-mode hold 仍然是 repository-owned 且 opt-in 的：在选定的 smoke hosts 上设置 `VIDERA_LIGHTING_PROOF_HOLD_SECONDS` 时，smoke host 会在 shutdown 前把 direct-lighting proof 保持文档化的 10 秒 hold。这个 hold 只改变验证 evidence 的可见时间，不会扩大公开 package surface 或 shipped runtime 边界。

## 覆盖范围

- Linux X11：真实 X11 宿主上的 Vulkan 生命周期与 draw-path 测试
- Linux Wayland 会话：通过 `XWayland` 兼容路径执行的 Vulkan 生命周期与 draw-path 测试
- macOS：真实 `NSView` 宿主上的 Metal 生命周期与 draw-path 测试
- Windows 原生验证：真实 HWND 宿主上的 D3D11 生命周期与 draw-path 测试
- GitHub Actions 中对 pull requests 与 `master` push 自动执行的 gate
- GitHub Actions 手动触发入口
- 真实匹配宿主上的本地脚本入口

仓库默认验证入口仍然是：

```bash
./scripts/verify.sh --configuration Release
pwsh -File ./scripts/verify.ps1 -Configuration Release
```

但默认入口不会自动执行专用的匹配宿主 native jobs。要走和 GitHub Actions pull requests 相同的路径，请使用下面的专用入口。

## GitHub Actions 入口

仓库提供了一个托管 workflow：`.github/workflows/native-validation.yml`。

自动触发方式：

- `pull_request`
- push 到 `master`

手动触发方式：

- `workflow_dispatch`
- 目标：`all`、`linux-x11`、`linux-wayland-xwayland`、`macos`、`windows`

在 GitHub Actions 页面中，如需定向重跑可使用 `Run workflow`：

1. 打开 `Native Validation`
2. 点击 `Run workflow`
3. 选择 `all`、`linux-x11`、`linux-wayland-xwayland`、`macos` 或 `windows`

说明：

- Linux X11 job 会安装 `xvfb`、`libX11`、`libshaderc1` 和 Vulkan 运行库，然后在 `xvfb-run` 下执行验证
- Linux Wayland 会话 job 会安装 `xwayland-run`，通过 `xwfb-run` 启动无头 Wayland compositor + `XWayland`，并在 `DISPLAY` 与 `WAYLAND_DISPLAY` 同时存在时验证兼容路径
- macOS job 会直接执行托管 runner 上的 `NSView` / Metal 验证路径
- Windows job 会直接通过 PowerShell 包装脚本执行托管 runner 上的 HWND / D3D11 验证路径
- 如果托管 runner 不足以定位某个原生问题，仍应改用下面的真实宿主路径

## 本地匹配宿主入口

适合在以下场景使用本地匹配宿主路径：

- 复现只在 CI 中出现的原生失败
- 需要交互式查看日志、图形前置条件或驱动状态
- 某个平台问题超出了托管 runner 的可观测范围

### Linux X11

前置条件：

- .NET 8 SDK
- Linux 主机
- 可用的 X11 会话，或 `xvfb-run`
- Vulkan 驱动与运行库
- `libX11.so.6`
- `libshaderc.so.1`（Ubuntu 包名：`libshaderc1`）

Shell：

```bash
./scripts/run-native-validation.sh --platform linux --linux-display-server x11 --configuration Release
```

PowerShell：

```powershell
pwsh -File ./scripts/run-native-validation.ps1 -Platform Linux -LinuxDisplayServer X11 -Configuration Release
```

如果是无头主机，请在 Xvfb 下执行：

```bash
xvfb-run -a bash ./scripts/run-native-validation.sh --platform linux --linux-display-server x11 --configuration Release
```

### Linux Wayland 会话（XWayland 兼容路径）

前置条件：

- .NET 8 SDK
- Linux 主机
- 一个同时暴露 `WAYLAND_DISPLAY` 与 `DISPLAY` 的 Wayland 会话
- Vulkan 驱动与运行库
- `libX11.so.6`
- `libshaderc.so.1`（Ubuntu 包名：`libshaderc1`）
- `xwayland-run` 或等价的无头 Wayland compositor + `XWayland` 方案

Shell：

```bash
./scripts/run-native-validation.sh --platform linux --linux-display-server xwayland --configuration Release
```

PowerShell：

```powershell
pwsh -File ./scripts/run-native-validation.ps1 -Platform Linux -LinuxDisplayServer XWayland -Configuration Release
```

如果需要无头复现，可使用 Ubuntu `xwayland-run` 包中的 `xwfb-run`：

```bash
xwfb-run -- bash ./scripts/run-native-validation.sh --platform linux --linux-display-server xwayland --configuration Release
```

### macOS

前置条件：

- .NET 8 SDK
- macOS 主机
- 支持 Metal 的硬件
- 可用的 AppKit / Objective-C runtime

Shell：

```bash
./scripts/run-native-validation.sh --platform macos --configuration Release
```

PowerShell：

```powershell
pwsh -File ./scripts/run-native-validation.ps1 -Platform macOS -Configuration Release
```

### Windows

前置条件：

- .NET 8 SDK
- Windows 主机
- 支持 Direct3D 11 的硬件与驱动

PowerShell：

```powershell
pwsh -File ./scripts/run-native-validation.ps1 -Platform Windows -Configuration Release
```

托管 Windows 路径会运行 `pwsh -File ./scripts/verify.ps1 -Configuration Release -IncludeNativeWindows`，该命令会调用 `scripts/Invoke-WpfSmoke.ps1` 并写出 `wpf-smoke-diagnostics.txt`，作为 repository-only 的验证/support evidence，而不是公开包或发布产物。

## 脚本实际执行内容

- Linux X11：`./scripts/verify.sh --configuration Release --include-native-linux`
- Linux Wayland 会话 `XWayland`：`./scripts/verify.sh --configuration Release --include-native-linux-xwayland`
- macOS：`./scripts/verify.sh --configuration Release --include-native-macos`
- Windows：`pwsh -File ./scripts/verify.ps1 -Configuration Release -IncludeNativeWindows` -> `scripts/Invoke-WpfSmoke.ps1` -> `wpf-smoke-diagnostics.txt`

PowerShell 包装脚本会调用等价的 `scripts/verify.ps1` 入口。

## 何时算有效验证

只有同时满足下面几点，native validation 才算真正有意义：

- 对应平台测试项目在匹配宿主上实际运行
- 真实 native fixture 路径被执行
- 生命周期与 draw-path 测试通过
- Linux 诊断路径解析到预期显示服务器（`X11` 或 `XWayland`）
- 不是只靠构造函数断言、`IntPtr.Zero` 守卫或 placeholder 测试得出成功

从项目状态追踪角度看，第一次在 matching-host GitHub Actions 或等价环境中成功跑通，才是关闭旧的“仅靠本地人工执行”假设所需的证据。本地匹配宿主路径仍然保留为复现和排障入口。

相关测试项目：

- `tests/Videra.Platform.Linux.Tests`
- `tests/Videra.Platform.macOS.Tests`
- `tests/Videra.Platform.Windows.Tests`

## 相关文档

- [项目首页](../../README.md)
- [故障排查](troubleshooting.md)
- [文档导航](index.md)
- [英文原生验证文档](../native-validation.md)

