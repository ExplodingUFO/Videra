# Phase 6: 分发与平台打包正确性 - Context

**Gathered:** 2026-04-07
**Status:** Ready for planning
**Source:** Auto-captured from post-release packaging/distribution audit

<domain>
## Phase Boundary

本阶段聚焦于三件事：
1. 修复当前“构建机 OS 决定包能力集合”的打包设计缺陷。
2. 让发布链路能够真实证明各平台包的依赖、元数据和安装语义是正确的。
3. 把安装与分发文档改成对外真实表达，不再让用户误以为单一入口包已经等价覆盖全部平台原生后端。

本阶段**不**包含：
- 切换到 NuGet.org 公开分发
- 引入新的图形后端能力或平台功能
- 重新设计整个渲染架构或 native host 结构
- 把 Linux/macOS 的 runtime 验证 gap 伪装成“只要能 pack 就算完成”

</domain>

<decisions>
## Implementation Decisions

### 分发形态
- **D-01:** `Videra.Avalonia` 在本阶段不再承担“单包隐式带齐全部平台原生后端”的承诺；它应被定义为 Avalonia UI/control 入口，而平台原生后端继续通过显式平台包表达。
- **D-02:** 本阶段优先采用“base entry package + explicit platform backend packages”的真实分发语义，而不是继续维持构建宿主决定依赖闭包的伪统一包。
- **D-03:** 平台后端包在 alpha 阶段允许继续分片存在，但安装文档必须明确“安装成功 ≠ 该平台 native path 已充分验证”。

### 后端发现与依赖模型
- **D-04:** `Videra.Avalonia` 的运行时后端解析不能再依赖编译时 `VIDERA_*_BACKEND` 常量来裁剪可用能力集合。
- **D-05:** 本阶段优先把平台后端从 `Videra.Avalonia` 的宿主 OS 条件 `ProjectReference` 中解耦，改为可由运行时发现、可缺失、可显式报告的平台能力模型。
- **D-06:** 若需要在“统一入口体验”和“真实能力边界”之间取舍，优先选择真实、可验证、易文档化的模型，而不是保留隐式魔法行为。

### 发布与验证策略
- **D-07:** 发布工作流不能继续以 `windows-latest` 单一宿主机作为三平台包正确性的唯一证据；至少要在匹配宿主上验证对应平台产物。
- **D-08:** 包验证 gate 不能只检查 `.nupkg` 数量、版本和 README；还必须检查依赖关系、平台语义和元数据是否与设计一致。
- **D-09:** `v0.1.x` 阶段继续以 GitHub Packages 作为唯一分发渠道，不在本阶段切换 NuGet.org。

### 文档与对外承诺
- **D-10:** 根 `README`、模块级 README、故障排查文档必须使用同一套分发叙事：当前 alpha 最稳路径、平台边界、source 配置、以及 `VIDERA_BACKEND` 的真实含义。
- **D-11:** 所有 package README 都应能独立说明安装前置条件，不再假设读者一定先读根 README。
- **D-12:** 若文档与当前包结构存在冲突，优先修改包结构或文档之一来消除冲突，不保留“内部知道、外部猜测”的灰区。

### the agent's Discretion
- 可由研究/计划阶段决定采用何种技术实现可选后端发现，例如反射、组合注册、显式插件加载或条件引用重构，只要满足 D-04 ~ D-08。
- 可自行决定是否把本阶段拆成“包结构收口”“发布 gate 收口”“文档承诺收口”三波执行，但不能弱化 Phase 6 的成功标准。

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Roadmap and project constraints
- `.planning/ROADMAP.md` — Phase 6 的目标、依赖、需求与成功标准
- `.planning/PROJECT.md` — 项目核心价值、分发范围、out-of-scope 与约束
- `.planning/REQUIREMENTS.md` — BUILD / DOCS 相关 requirement 定义
- `.planning/STATE.md` — 当前项目阻塞、平台验证状态与已记录决策
- `AGENTS.md` — 仓库级工作约束与工具/编辑规则

### Current packaging and distribution mechanics
- `Directory.Build.props` — 当前统一版本、仓库元数据和打包基础属性
- `.github/workflows/ci.yml` — 当前 CI 验证边界
- `.github/workflows/publish-nuget.yml` — 当前 GitHub Packages 发布与打包验证逻辑
- `src/Videra.Avalonia/Videra.Avalonia.csproj` — 当前宿主 OS 条件常量和平台引用根源
- `src/Videra.Avalonia/Composition/AvaloniaGraphicsBackendResolver.cs` — 当前受编译常量影响的后端发现逻辑
- `src/Videra.Core/Videra.Core.csproj` — 核心包当前元数据与入口能力
- `src/Videra.Platform.Windows/Videra.Platform.Windows.csproj` — Windows 平台包打包边界
- `src/Videra.Platform.Linux/Videra.Platform.Linux.csproj` — Linux 平台包打包边界
- `src/Videra.Platform.macOS/Videra.Platform.macOS.csproj` — macOS 平台包打包边界

### Current user-facing install story
- `README.md` — 当前 alpha 安装入口、平台支持矩阵、GitHub Packages 说明与边界承诺
- `docs/index.md` — 当前公开文档入口
- `docs/troubleshooting.md` — 当前平台验证方式与运行时边界说明
- `src/Videra.Avalonia/README.md` — UI/control 入口包的独立安装叙事
- `src/Videra.Core/README.md` — 核心包的独立安装叙事
- `src/Videra.Platform.Windows/README.md` — Windows 平台包说明
- `src/Videra.Platform.Linux/README.md` — Linux 平台包说明
- `src/Videra.Platform.macOS/README.md` — macOS 平台包说明

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `IGraphicsBackendResolver` / `GraphicsBackendFactory` 这条 seam 已在 Phase 5 建好，可作为把编译时平台绑定改为运行时能力发现的承载点。
- `verify.ps1` / `verify.sh` 已经支持按开关显式包含 Linux/macOS native 验证，可被 Phase 6 复用为发布 gate 的基础。
- 仓库级 `Directory.Build.props` 已经集中管理版本、仓库和 license 元数据，适合继续收敛包元数据策略。

### Established Patterns
- 平台实现仍按 `Videra.Platform.*` 分项目隔离，这是正确的边界；Phase 6 不应把平台风险重新拉回 `Videra.Core`。
- 当前 `Videra.Avalonia` 既承担 UI 入口又承担平台后端引用，是造成分发语义混乱的主要原因。
- 当前工作流倾向于“先本地/Windows 验证，再用文档解释边界”；Phase 6 需要把这变成结构化的发布规则，而不是解释性补丁。

### Integration Points
- 包依赖模型的核心落点在 `src/Videra.Avalonia/Videra.Avalonia.csproj` 与 `src/Videra.Avalonia/Composition/AvaloniaGraphicsBackendResolver.cs`。
- 发布 gate 的核心落点在 `.github/workflows/publish-nuget.yml`、`.github/workflows/ci.yml` 和验证脚本。
- 文档收口的核心落点在根 `README.md` 与各 package README。

</code_context>

<specifics>
## Specific Ideas

- 当前最稳的 alpha 安装模型更接近“安装 `Videra.Avalonia` + 安装目标平台后端包”，而不是“只装一个包自动拥有所有原生后端”。
- 如果后端发现被改成运行时可选能力模型，日志和错误信息必须能明确告诉用户“缺哪个平台包/为何不可用”，不能只是静默不可达。
- 发布策略应优先证明“包语义正确”，而不是追求一次性引入更大的公开分发渠道或复杂 packaging trick。

</specifics>

<deferred>
## Deferred Ideas

- 切换到 NuGet.org 主分发
- 引入统一 meta-package 或按平台发行 profile package
- 引入 symbols/SourceLink 以外的更重发布增强项
- 把平台验证范围扩展到 Wayland、新图形后端或更多安装器故事

</deferred>

---

*Phase: 06-distribution-and-platform-packaging-correctness*
*Context gathered: 2026-04-07*
