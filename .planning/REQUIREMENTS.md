# Requirements: Videra v1.1 Render Pipeline Architecture

**Defined:** 2026-04-08  
**Core Value:** 跨平台 3D 渲染引擎的可靠性

## v1.1 Requirements

### Pipeline Contract

- [x] **PIPE-01**: 开发者可以通过公开 contract 识别一帧渲染的阶段顺序、关键输入输出和错误边界
- [x] **PIPE-02**: 引擎内部使用显式 stage / pass abstraction 组织渲染流程，而不是把 frame orchestration 隐含在 `VideraEngine` / `RenderSession` 条件分支中
- [x] **PIPE-03**: 纯渲染 orchestration 可以在不依赖 Avalonia view 或 native host 的情况下被组装和测试

### Extensibility APIs

- [x] **EXT-01**: 开发者可以通过公开 API 注册或替换自定义 render pass contributor，而不需要修改 `VideraEngine` 源码
- [x] **EXT-02**: 开发者可以在 frame begin/end 或 scene submit 生命周期插入自定义逻辑，并拿到稳定上下文对象
- [x] **EXT-03**: 开发者可以通过公开 API 查询当前 pipeline、active backend、capabilities 和 diagnostics

### Maintainability

- [x] **MAIN-01**: 渲染管线关键阶段有 unit/integration coverage，能证明解耦后行为未回退
- [x] **MAIN-02**: 新的开发者扩展点有最小 sample/reference usage 和面向库使用者的文档
- [x] **MAIN-03**: 新的公开接口在 unsupported / disposed / unavailable 场景下有明确 contract，而不是隐式失败或行为漂移

## v1.2+ / Future Requirements

### Platform Deepening

- **PLATNEXT-01**: Linux 支持 compositor-native Wayland embedding
- **MACNEXT-01**: macOS Metal 互操作迁移到更高层 safer binding 模型

### Rendering Feature Expansion

- **RENDNEXT-01**: 在新管线 contract 稳定后，再考虑新增后处理或更复杂材质/光照特性
- **RENDNEXT-02**: 评估更完整的 plugin-style pipeline package model

## Out of Scope

| Feature | Reason |
|---------|--------|
| 新的大型渲染特性（PBR、阴影、后处理） | 当前里程碑聚焦管线解耦和接口设计，不扩张功能面 |
| 编辑器级 UI / inspector / scene authoring tools | 当前目标是引擎可维护性和扩展接口，不是做编辑器 |
| 替换 Avalonia 或整体宿主 UI 栈 | 当前问题核心是管线解耦，不是换框架 |
| compositor-native Wayland embedding | 已明确转为后续独立平台里程碑 |
| higher-level macOS safer binding replacement | 已明确转为后续独立平台里程碑 |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| PIPE-01 | Phase 9 | Complete |
| PIPE-02 | Phase 9 | Complete |
| PIPE-03 | Phase 10 | Complete |
| MAIN-01 | Phase 10 | Complete |
| EXT-01 | Phase 11 | Complete |
| EXT-02 | Phase 11 | Complete |
| EXT-03 | Phase 11 | Complete |
| MAIN-02 | Phase 12 | Complete |
| MAIN-03 | Phase 12 | Complete |

**Coverage:**
- v1.1 requirements: 9 total
- Mapped to phases: 9
- Unmapped: 0

---
*Requirements defined: 2026-04-08*  
*Last updated: 2026-04-08 after Phase 12 completion*
