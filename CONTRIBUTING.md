# 贡献指南

感谢你关注 Videra。本文档说明如何在本仓库中进行开发、验证和提交变更。

## 贡献类型

欢迎以下类型的贡献：

- Bug 修复
- 文档改进
- 测试补充与稳定性提升
- 新的平台适配与渲染后端改进
- Demo 体验优化

如果是较大范围的能力变更，建议先提交 Issue 讨论边界与方案。

## 开发环境

### 必要依赖

- .NET 8 SDK
- Git
- 一个常用的 C# IDE（Visual Studio、Rider 或 VS Code）

### 平台前提

| 平台 | 后端 | 依赖 |
| --- | --- | --- |
| Windows | Direct3D 11 | D3D11 兼容 GPU |
| Linux | Vulkan | Vulkan 驱动、X11 运行库 |
| macOS | Metal | Metal 兼容设备 |

## 本地启动

```bash
git clone https://github.com/<owner>/Videra.git
cd Videra
dotnet restore
dotnet build Videra.slnx
```

运行 Demo：

```bash
dotnet run --project samples/Videra.Demo/Videra.Demo.csproj
```

## 开发原则

- 变更范围保持聚焦，不混入无关重构
- 公共 API 变更必须同步更新文档
- 与平台相关的改动，优先在对应平台宿主机验证
- 不要把临时调试代码、注释掉的旧实现或本地实验文件带入 PR

## 验证要求

提交前至少运行一次统一验证入口：

```bash
# Unix shell
./verify.sh --configuration Release

# PowerShell
pwsh -File ./verify.ps1 -Configuration Release
```

如改动涉及原生宿主路径，请补充平台验证：

```bash
# Linux
./verify.sh --configuration Release --include-native-linux
pwsh -File ./verify.ps1 -Configuration Release -IncludeNativeLinux

# macOS
./verify.sh --configuration Release --include-native-macos
pwsh -File ./verify.ps1 -Configuration Release -IncludeNativeMacOS
```

必要时可直接运行测试：

```bash
dotnet test Videra.slnx
```

## 文档要求

以下场景需要同步更新文档：

- 调整了 `README.md` 中提到的能力、限制或使用方式
- 修改了 `VideraView` 的公开属性或示例用法
- 新增平台依赖、环境变量或验证命令
- 新增或废弃模块、设计决策或历史归档

建议优先维护以下文档：

- [README.md](README.md)
- [ARCHITECTURE.md](ARCHITECTURE.md)
- [docs/troubleshooting.md](docs/troubleshooting.md)
- 各模块 README

## 提交规范

建议使用 Conventional Commits：

```text
<type>(<scope>): <summary>
```

常见类型：

- `feat`
- `fix`
- `docs`
- `refactor`
- `test`
- `chore`

示例：

```text
feat(core): add style preset serialization
fix(linux): handle missing X11 runtime fallback
docs(readme): clarify platform validation status
test(macos): cover NSView host lifecycle
```

## Pull Request 要求

提交 PR 前请确认：

- 变更目标清晰，描述了问题与解决方案
- 统一验证已运行
- 原生平台改动已在对应宿主机验证，或明确说明未验证部分
- 文档与示例已同步更新
- 没有残留调试输出或临时代码

## 与平台有关的改动

### Windows

- 优先确认 D3D11 初始化、交换链与 resize 流程
- 如改动影响宿主窗口行为，至少重新跑一次标准验证

### Linux

- 当前正式原生路径基于 X11 + Vulkan
- 不要把 X11 验证结果直接外推为 Wayland 支持

### macOS

- 当前后端依赖 `NSView`、`CAMetalLayer` 和 Objective-C runtime 互操作
- 如改动涉及原生宿主或渲染路径，建议在真实 macOS 主机上验证

## 提问与反馈

- 使用 Issue 报告 Bug、兼容性问题或文档缺口
- 提交 PR 前如有较大设计分歧，优先在 Issue 中同步上下文

感谢你的贡献。
