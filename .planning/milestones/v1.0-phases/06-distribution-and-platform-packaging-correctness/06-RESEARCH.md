# Phase 6: 分发与平台打包正确性 - Research

**Researched:** 2026-04-07
**Domain:** .NET 8 / GitHub Packages alpha distribution / platform-specific package semantics
**Confidence:** HIGH

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

### 分发形态
- **D-01:** `Videra.Avalonia` 不再承担“单包隐式带齐全部平台原生后端”的承诺；它是 Avalonia UI/control 入口。
- **D-02:** 本阶段优先采用“base entry package + explicit platform backend packages”的真实分发语义。
- **D-03:** 平台后端包可以继续分片存在，但文档必须明确“安装成功 ≠ native path 已充分验证”。

### 后端发现与依赖模型
- **D-04:** `Videra.Avalonia` 不能继续依赖编译时 `VIDERA_*_BACKEND` 常量裁剪包能力集合。
- **D-05:** 要把 `Videra.Avalonia` 从宿主 OS 条件 `ProjectReference` 中解耦，转为运行时可发现、可缺失、可显式报告的平台能力模型。
- **D-06:** 如有取舍，优先真实、可验证、易文档化的模型。

### 发布与验证策略
- **D-07:** 发布工作流不能再以 `windows-latest` 单一宿主作为三平台包正确性的唯一证据。
- **D-08:** 包验证 gate 必须覆盖依赖关系、平台语义和元数据，不止是数量/版本/README。
- **D-09:** `v0.1.x` 阶段继续只用 GitHub Packages。

### 文档与对外承诺
- **D-10:** 根 README、模块 README、troubleshooting 必须使用同一套分发叙事。
- **D-11:** 每个 package README 都应独立说明安装前置条件。
- **D-12:** 不保留文档与包结构冲突的灰区。

### Deferred Ideas (OUT OF SCOPE)
- NuGet.org 迁移
- 新图形后端或 Wayland 能力扩展
- 更重的分发增强（meta-package、profile package、大规模 symbols/release engineering）
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| BUILD-01 | 跨平台构建验证 | 当前发布和 CI 都是 Windows-only；Phase 6 需要把“构建证明”升级为 matching-host evidence。 |
| DOCS-02 | 用户文档 | 当前 README 已有 alpha 说明，但 package README 还不是完整安装闭环。 |
| DOCS-03 | 架构/平台文档 | 当前平台边界已写出一部分，但分发语义、平台包责任和 `VIDERA_BACKEND` 规则未统一。 |
</phase_requirements>

## Summary

Phase 6 的关键不是“把更多包发出去”，而是让当前已经公开的包结构说真话。现在 `Videra.Avalonia` 同时承担 UI 入口和平台后端引用，但它的 `.csproj` 用宿主 OS 条件常量与 `ProjectReference` 决定最终依赖闭包；再加上发布工作流只在 Windows 上打包，结果就是用户看到的是“统一入口包”，实际得到的是“由构建机 OS 决定能力集合的半统一包”。这正是 Phase 6 需要收口的核心缺陷。

对当前仓库而言，最稳妥的分发模型不是继续追求“一个包自动带齐全部后端”，而是明确采用三层语义：
1. `Videra.Core` 负责纯核心能力；
2. `Videra.Avalonia` 负责 UI/control 层；
3. `Videra.Platform.*` 负责平台原生后端。

在这个模型下，`Videra.Avalonia` 仍可保留为推荐入口包，但它不能再静态承诺所有原生后端都已内含。正确方向是：把原生后端变成**显式平台依赖 + 运行时可发现能力**，让 resolver 能在包缺失时给出确定性、可文档化的行为。

发布侧同样需要从“打出 5 个 `.nupkg` 文件”升级为“证明每个平台包的语义正确”。当前 `publish-nuget.yml` 只校验包数量、版本和 README，这不足以证明 `Videra.Avalonia` 的依赖模型、平台包的 metadata、以及 README 承诺和实际包内容一致。Phase 6 应把验证拆成两层：一层验证包结构（nuspec dependency graph、README/license/metadata）；另一层验证平台语义（matching-host build/test evidence 或明确的验证边界）。

**Primary recommendation:** 保持 `Videra.Avalonia` 为入口包，但改成不再由宿主 OS 条件静态锁定平台能力；同时把发布与文档全部改成“显式平台后端 + matching-host evidence + package-level install truth”的模型。

## Option Analysis

### Option A: Keep a “unified” `Videra.Avalonia` package by retaining host-OS conditional references
**Pros**
- 表面上最省改动
- 用户仍只看到一个推荐入口包

**Cons**
- 与当前实际行为冲突，Windows 构建得到的 `Videra.Avalonia` 并不对 Linux/macOS 等价
- 难以通过 workflow gate 证明正确性
- 文档只能不断打补丁解释例外

**Verdict**
- 不推荐。它延续了 Phase 6 要解决的根问题。

### Option B: Keep `Videra.Avalonia` as entry package, but make platform backends explicit and runtime-discovered
**Pros**
- 与当前 `Videra.Platform.*` 项目结构一致
- 不需要重写核心架构
- 最容易做到“包语义真实 + 文档可讲清 + 发布可验证”
- 允许 alpha 阶段继续走 GitHub Packages 而不引入更大的分发承诺

**Cons**
- 用户安装步骤比“一个包全包”多一步
- 需要调整 resolver、包引用和 README 叙事

**Verdict**
- 推荐。它是当前仓库最稳、最实用、最可验证的模型。

### Option C: Introduce separate platform-specific entry packages immediately
**Pros**
- 分发语义最清晰
- 用户几乎不可能误解平台边界

**Cons**
- 会显著改变公开入口和包心智
- 相比当前 alpha 阶段收益不如 Option B

**Verdict**
- 可作为后续 backlog，不适合成为 Phase 6 的第一步。

## Recommended Technical Direction

### 1. Package model
- `Videra.Core` 继续保持平台无关。
- `Videra.Avalonia` 只承担 Avalonia UI/control 层与后端发现逻辑，不再通过宿主 OS 条件 `ProjectReference` 决定最终平台能力。
- `Videra.Platform.Windows` / `Videra.Platform.Linux` / `Videra.Platform.macOS` 继续作为显式平台包。
- resolver 以“平台包是否已安装/可加载”为输入，而不是“编译时是否带了某个 `VIDERA_*_BACKEND` 常量”为输入。

### 2. Release workflow gates
- 把 `publish-nuget.yml` 拆成至少两类验证：
  - matching-host build/test or pack evidence；
  - packed artifact semantic checks（dependency graph / metadata / README/license）。
- `ci.yml` 不必一口气变成完整 E2E matrix，但至少需要能证明平台分发相关改动不会只在 Windows 上自证。
- `verify.ps1` / `verify.sh` 已有 `-IncludeNativeLinux` / `-IncludeNativeMacOS` 开关，可以作为 Phase 6 文档与 workflow 设计的基础，而不是另起一套验证故事。

### 3. Documentation truth model
- 根 README：保留“alpha + GitHub Packages + 当前最稳路径”的总入口叙事。
- package README：每个都重复最小安装前置，包括 package source、alpha 状态、平台边界、推荐组合方式。
- troubleshooting：解释平台 host 验证与包安装之间的关系，尤其是“native validation still requires matching hosts”。
- `VIDERA_BACKEND` 需要在根 README 和 `Videra.Avalonia` README 中统一表达：它选择后端偏好，不等于自动补齐缺失的平台包。

## Planning Implications

Phase 6 最适合拆成 3 个计划波次：

### Wave 1: 包结构与运行时能力模型
- 目标：收掉 `Videra.Avalonia` 的宿主 OS 条件依赖与编译时能力裁剪。
- 风险：这是最容易影响现有安装/运行行为的地方，需要测试先行。

### Wave 2: 发布与验证 gate
- 目标：让 GitHub Packages 发布真正证明包语义正确，而不是只证明“打出来了”。
- 风险：需要修改 Actions workflow，并可能引入 repository-level semantic tests。

### Wave 3: 文档与安装叙事
- 目标：让 README/package README/troubleshooting 统一反映新的分发模型。
- 风险：如果前两波的实际结构没有先稳定，文档会再次漂移。

## Risks and Tradeoffs

### Risk 1: Runtime discovery becomes too magical
如果只移除条件引用，却没有给出清晰的缺失平台包行为，用户会从“编译时不完整”切换到“运行时静默失败”。

**Mitigation:** 把日志/异常/README 一起设计，明确“缺哪个包时会发生什么”。

### Risk 2: Workflow matrix grows faster than the repo can truly validate
如果直接把发布链路扩成完整三平台高强度矩阵，但 Linux/macOS native verification 仍依赖 matching hosts 与额外前提，workflow 可能先变成脆弱噪声。

**Mitigation:** 先区分“pack/build correctness”与“native end-to-end validation”两类 gate，不要把它们混成一个通过/失败按钮。

### Risk 3: Docs fix wording but not semantics
如果只改 README 而不改包结构，Phase 6 会退化成“更会解释现有缺陷”，不是修复缺陷。

**Mitigation:** 文档计划必须依赖前两波落地后的真实结构。

## Environment Availability

| Dependency | Required By | Available | Notes |
|------------|------------|-----------|-------|
| Windows host | 当前本机开发与本地 pack 验证 | ✓ | 可本地验证 Windows 侧打包与 workflow 逻辑 |
| Linux host | Linux native/runtime evidence | ✗ | 仍需 matching host 才能闭合原生验证 |
| macOS host | macOS native/runtime evidence | ✗ | 仍需 matching host 才能闭合原生验证 |
| GitHub Actions | 发布/CI workflow 设计与最终远端证据 | ✓ | 当前仓库已在使用 CI 与 publish workflows |

**Implication:** Phase 6 可以先把包结构、workflow 语义检查和文档模型在当前环境做完；真正的 Linux/macOS runtime evidence 仍需对应宿主完成。

## Sources

### Primary (HIGH confidence)
- `.planning/phases/06-distribution-and-platform-packaging-correctness/06-CONTEXT.md`
- `.planning/ROADMAP.md`
- `.planning/STATE.md`
- `Directory.Build.props`
- `.github/workflows/ci.yml`
- `.github/workflows/publish-nuget.yml`
- `src/Videra.Avalonia/Videra.Avalonia.csproj`
- `src/Videra.Avalonia/Composition/AvaloniaGraphicsBackendResolver.cs`
- `src/Videra.Core/Videra.Core.csproj`
- `src/Videra.Platform.Windows/Videra.Platform.Windows.csproj`
- `src/Videra.Platform.Linux/Videra.Platform.Linux.csproj`
- `src/Videra.Platform.macOS/Videra.Platform.macOS.csproj`
- `README.md`
- `docs/troubleshooting.md`
- `src/Videra.Avalonia/README.md`
- `tests/Videra.Core.Tests/Repository/RepositoryReleaseReadinessTests.cs`

### Secondary (MEDIUM confidence)
- `src/Videra.Core/README.md`
- `src/Videra.Platform.Windows/README.md`
- `src/Videra.Platform.Linux/README.md`
- `src/Videra.Platform.macOS/README.md`
- `verify.ps1`

## Metadata

**Confidence breakdown:**
- Package/dependency model: HIGH
- Workflow gating direction: HIGH
- Documentation alignment needs: HIGH
- Exact runtime-discovery implementation shape: MEDIUM

**Research date:** 2026-04-07
**Valid until:** 2026-05-07
