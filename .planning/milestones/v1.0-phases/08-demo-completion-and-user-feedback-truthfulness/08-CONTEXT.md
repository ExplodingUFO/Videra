# Phase 8: Demo 完整度与用户反馈真实性 - Context

**Gathered:** 2026-04-08
**Status:** Ready for planning
**Source:** Repo audit after Phase 7 closure

<domain>
## Phase Boundary

本阶段聚焦于三件事：
1. 把 Demo 的等待中 / 就绪 / 降级可用 / 初始化失败状态收口成单一、可测试、对用户可见的真相。
2. 让 Demo 的交互 gating 与实际 capability 一致，去掉会误导外部用户的 residual sample-only 路径。
3. 让 Demo 文档、中文镜像和仓库守卫与真实运行行为一致，不再沿用过时叙事。

本阶段**不**包含：
- 新的模型导入格式或新的编辑能力
- 对底层 backend selection / rendering contract 的再次重构（这些已经在 Phase 6/7 收口）
- Wayland 支持
- broad Avalonia UI redesign
- demo 之外的包模型或发布流程变化

</domain>

<decisions>
## Implementation Decisions

### 用户可见真相优先
- **D-01:** Demo 的状态栏必须表达用户真正关心的运行状态，不能只把关键信息留在日志里。
- **D-02:** 同一个执行路径里不能同时写出“backend ready”与“默认场景创建失败”两套互相打架的结论；最终可见状态必须只有一份真相。
- **D-03:** 初始化失败时，Demo 必须保持 `IsBackendReady == false`；默认场景创建失败时可以保持 backend ready，但要明确告知“降级可用，导入仍可用”。

### 交互 gating 规则
- **D-04:** 已经通过的工作不要重做。`Import Model` / `Frame All` / `Reset Camera` 的 gating 已经存在，但不能继续只靠 `IsBackendReady` 这个单 bit 推断所有 capability。
- **D-05:** backend-dependent 交互应由显式 capability 或 command can-execute 驱动，而不是“按钮可点，命令里再被动拒绝”。
- **D-06:** residual sample-only 交互路径要么删除，要么被明确标注为 sample shortcut；当前优先处理 `Test Wireframe` 这类与正式 UI 控件重复、但会扭曲用户理解的入口。

### 验证策略
- **D-07:** Phase 8 不能只靠 grep/XAML 文本守卫；至少要补一层 sample 行为测试，覆盖 ViewModel/status/bootstrapper contract。
- **D-08:** 公共文档必须描述 Demo 的真实启动流程、默认场景、backend diagnostics、导入失败反馈和 capability gating。

### the agent's Discretion
- 可以在 sample 层引入小型 status/capability helper 或显式 ViewModel 方法，只要目标是让状态转移和测试更清晰。
- 可以扩展现有测试项目以直接引用 `samples/Videra.Demo`，前提是验证面更真实且不会破坏三平台 CI。

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Roadmap and project constraints
- `.planning/ROADMAP.md` — Phase 8 的目标、依赖、需求与成功标准
- `.planning/STATE.md` — 当前项目阶段、Phase 7 已完成、Phase 8 为当前计划阶段
- `.planning/REQUIREMENTS.md` — `ERROR-03`, `DOC-02`, `DOC-03` requirement 定义
- `AGENTS.md` — 仓库工作规则

### Current demo behavior
- `samples/Videra.Demo/ViewModels/MainWindowViewModel.cs`
- `samples/Videra.Demo/Services/DemoSceneBootstrapper.cs`
- `samples/Videra.Demo/Services/AvaloniaModelImporter.cs`
- `samples/Videra.Demo/Views/MainWindow.axaml`
- `samples/Videra.Demo/Views/MainWindow.axaml.cs`
- `samples/Videra.Demo/README.md`
- `docs/zh-CN/modules/demo.md`
- `README.md`

### Existing library-facing diagnostics surface
- `src/Videra.Avalonia/Controls/VideraView.cs`
- `src/Videra.Avalonia/Controls/VideraBackendDiagnostics.cs`
- `src/Videra.Avalonia/Controls/VideraBackendFailureEventArgs.cs`
- `src/Videra.Avalonia/Controls/VideraBackendStatusChangedEventArgs.cs`

### Existing verification surface
- `tests/Videra.Core.Tests/Samples/DemoConfigurationTests.cs`
- `tests/Videra.Core.Tests/Repository/RepositoryLocalizationTests.cs`
- `tests/Videra.Core.IntegrationTests/Rendering/VideraViewSceneIntegrationTests.cs`

</canonical_refs>

<code_context>
## Existing Code Insights

### Already solved or partially solved
- `MainWindow.axaml` 已经把 `Import Model`、`Frame All`、`Reset Camera` 绑定到了 Demo UI 中，并且有基础 gating。
- `MainWindowViewModel` 已经维护 `StatusMessage`, `IsBackendReady`, `BackendDisplay`, `BackendDetails`，以及导入结果汇总逻辑。
- `AvaloniaModelImporter` 会把多文件导入失败收集为 `ModelLoadBatchResult`，并在成功时自动 `FrameAll()`。

### Current truth gaps
- `DemoSceneBootstrapper.TryInitialize` 在默认场景创建失败时，先写了 “backend is ready” 再写失败消息，状态来源分裂。
- `MainWindow.axaml.cs` 的 `OnInitializationFailed` 只写 `SetStatusMessage(...)`，没有走统一的 backend-failure contract。
- `MainWindow.axaml` 同时提供 `WireframeMode` 下拉框和 `Test Wireframe` 按钮，这让 sample UI 暴露出一条重复而模糊的输入路径。
- `DemoConfigurationTests` 目前主要是源码文本守卫，缺少对 sample status / capability contract 的行为级验证。

### Reusable assets
- `MainWindowViewModel.BuildImportStatus(...)` 已经是可复用的导入反馈汇总点。
- `BackendDiagnostics` 与 `InitializationFailed` 事件已经提供足够的底层信息，不需要在 Phase 8 重新发明 diagnostics API。

</code_context>

<specifics>
## Specific Ideas

- 推荐把 Phase 8 拆成三波：
  1. status/init/default-scene truth contract
  2. explicit capability gating and residual sample path cleanup
  3. demo/docs/repository truth closure
- `Import Model` 更适合改成显式 `CanImportModels` 或等价的 command can-execute，而不是继续让 XAML 直接猜 `IsBackendReady`。
- 默认场景失败时的推荐用户文案应表达：“渲染后端已就绪，但默认 demo cube 创建失败；仍可继续导入模型。”

</specifics>

<deferred>
## Deferred Ideas

- 新 demo 功能
- 新 UI 皮肤或布局 redesign
- backend selection 机制变化
- logging overhaul

</deferred>

---

*Phase: 08-demo-completion-and-user-feedback-truthfulness*
*Context gathered: 2026-04-08*
