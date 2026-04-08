# Videra 路线图

**项目目标：** 跨平台 3D 渲染引擎的可靠性  
**当前活动里程碑：** `v1.1 Render Pipeline Architecture`

---

## Milestones

- ✅ **v1.0 Alpha Ready** — Phases `1-8`, shipped `2026-04-08`
  Archive: `.planning/milestones/v1.0-ROADMAP.md`
- ✅ **v1.1 Render Pipeline Architecture** — Phases `9-12`, execution complete on `2026-04-08`

## v1.1 Scope

**里程碑目标：** 检查并重构整个渲染管线，把 frame orchestration、host/backend/view 边界和用户扩展点收口成更清晰、更可维护、可对外开放的契约。

**Requirements:** `PIPE-01`, `PIPE-02`, `PIPE-03`, `EXT-01`, `EXT-02`, `EXT-03`, `MAIN-01`, `MAIN-02`, `MAIN-03`

## v1.1 Phases

### Phase 9: Render Pipeline Inventory and Contract Extraction

**Goal:** 把当前 end-to-end frame path 审计成显式的 pipeline contract，并抽出稳定的 stage/pass 语义。

**Depends on:** `v1.0` archived baseline

**Requirements:** `PIPE-01`, `PIPE-02`

**Success criteria:**
1. 一帧渲染的阶段顺序、关键输入输出、失败边界可以被代码和文档同时描述
2. 现有 render orchestration 被收口成显式的 stage/pass abstraction，而不是继续散落在 `VideraEngine` / `RenderSession` 的隐式流程里
3. 新 contract 可以被测试和 diagnostics 消费，而不是只存在于注释里

**Status:** Complete on `2026-04-08`

**Delivered:** `09-01`, `09-02`, `09-03`

**Plans:** 3 delivered

### Phase 10: Host-Agnostic Render Orchestration

**Goal:** 继续把纯渲染 orchestration 从 Avalonia view/session/native host 细节中解耦出来，并建立可回归验证。

**Depends on:** Phase 9

**Requirements:** `PIPE-03`, `MAIN-01`

**Success criteria:**
1. 纯渲染 orchestration 可以在不依赖 Avalonia view/native host 的条件下被组装和测试
2. host/session/backend glue 与 pure pipeline core 的职责边界清晰
3. 解耦后的行为有 unit/integration coverage，能证明没有功能回退

**Status:** Complete on `2026-04-08`

**Delivered:** `10-01`, `10-02`, `10-03`

**Plans:** 3 delivered

### Phase 11: Public Extensibility APIs

**Goal:** 向库使用者开放稳定、可理解的 pipeline 扩展接口，而不是要求修改引擎内部代码。

**Depends on:** Phase 10

**Requirements:** `EXT-01`, `EXT-02`, `EXT-03`

**Success criteria:**
1. 开发者可以注册或替换自定义 render pass contributor
2. 开发者可以在 frame begin/end 或 scene submit 生命周期插入自定义逻辑，并拿到稳定上下文
3. pipeline / backend / capability / diagnostics 信息能通过公开 API 查询

**Status:** Complete on `2026-04-08`

**Delivered:** `11-01`, `11-02`, `11-03`

**Plans:** 3 delivered

### Phase 12: Developer-Facing Samples, Docs, and Compatibility Guards

**Goal:** 用 sample、文档和 contract tests 固定新的扩展模型，防止它再次漂移成内部实现细节。

**Depends on:** Phase 11

**Requirements:** `MAIN-02`, `MAIN-03`

**Success criteria:**
1. 新的扩展点有最小 sample/reference usage，外部开发者可以照着使用
2. 文档明确说明新的 pipeline contract、扩展点和 unsupported/disposed/unavailable 语义
3. repository tests 会阻止公开扩展接口、文档和实现再次分叉

**Status:** Complete on `2026-04-08`

**Delivered:** `12-01`, `12-02`, `12-03`, `12-04`

**Plans:** 4 delivered

## Deferred From v1.0

- compositor-native Wayland embedding
- higher-level macOS safer binding replacement

## Progress

| Milestone | Scope | Status | Shipped |
|-----------|-------|--------|---------|
| v1.0 Alpha Ready | Phases 1-8 | Complete | 2026-04-08 |
| v1.1 Render Pipeline Architecture | Phases 9-12 | Execution complete | 2026-04-08 |

## References

- Current milestone requirements: `.planning/REQUIREMENTS.md`
- Archived v1.0 roadmap: `.planning/milestones/v1.0-ROADMAP.md`
- Project context: `.planning/PROJECT.md`

---
*Roadmap updated after Phase 12 execution on 2026-04-08*
