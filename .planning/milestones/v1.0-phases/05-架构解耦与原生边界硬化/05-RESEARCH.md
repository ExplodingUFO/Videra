# Phase 5: 架构解耦与原生边界硬化 - Research

**Researched:** 2026-04-02
**Domain:** .NET 8 / Avalonia 渲染组合解耦、原生互操作边界硬化、证据驱动的性能与 Rust 决策
**Confidence:** MEDIUM

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

### 组合层与后端解析
- **D-01:** `Videra.Core` 不应继续直接承担平台后端发现与反射装配职责；后端解析应迁移到组合层，并保留兼容适配器。
- **D-02:** Phase 5 优先引入显式 resolver/registry seam，避免一步到位重写现有 `GraphicsBackendFactory`。

### 渲染会话与 UI 边界
- **D-03:** `VideraView` 当前职责过重，Phase 5 要把 backend/session/timer/native-handle 生命周期从控件中抽离到单独的 render session/controller。
- **D-04:** native handle 的 destroy/recreate 必须被视为 backend rebind 事件，而不是简单 resize。

### 图形抽象与接口收口
- **D-05:** 先用 typed constants / enums 取代当前 binding slot 与 primitive magic numbers，再考虑更大的接口拆分。
- **D-06:** 不在本阶段做“全接口重设计”；先把 optional capability 与 always-supported contract 的边界识别清楚。

### Native Host 与平台边界
- **D-07:** `VideraView` 不应继续直接 new 各平台 native host；需要新增 `INativeHostFactory` 或等价 seam。
- **D-08:** Linux 侧继续沿用 `ISurfaceCreator` 作为可复用模式，并将其推广为更一般的 host/surface 组合边界。

### Native 安全硬化优先级
- **D-09:** macOS ObjC/Metal 路径是当前最高优先级 native 风险区，优先检查 zero-handle、`contents` 空指针、防止 `CAMetalLayer` 泄漏、收敛 `objc_msgSend` 变体。
- **D-10:** Linux Vulkan 路径第二优先，优先补齐 `MapMemory`、`BindBufferMemory`、提交/呈现相关 `Result` 检查，并收紧 X11 display/window 所有权模型。
- **D-11:** Windows D3D11 路径不作为本阶段重构中心，只做低风险修补（如 HRESULT/rollback/resize error handling）。

### 性能推进顺序
- **D-12:** 当前主要性能问题先按架构/API 使用方式处理，不以 Rust 为第一手优化手段。
- **D-13:** 优先排查 software backend 的 UI 线程整帧 copy、macOS `waitUntilCompleted`、每对象 uniform upload/rebind、线框颜色完整重写上传。
- **D-14:** 无 benchmark / profiler 证据前，不做“Rust 会更快”的推进决策。

### Rust 决策
- **D-15:** 本阶段默认结论是 **No Rust by default**。
- **D-16:** 若未来性能或安全证据要求继续评估，唯一可接受的第一批候选边界是：
  - `ModelImporter` / mesh preprocessing（首选）
  - 在确认 software fallback 为真实瓶颈后的软件栅格核心（次选）
  - 出于安全隔离目的的 macOS native boundary（仅在 C# 第一轮硬化后仍不满意时）
- **D-17:** 不接受把 Rust 用于跨平台 engine core、Avalonia host glue、或全平台 native interop 总重写。

### Claude's Discretion
- decoupling 方案的命名可以灵活，例如 `IGraphicsBackendResolver` / `RenderSession` / `ViewRendererController`，只要职责边界满足 D-01 ~ D-08。
- 研究/计划阶段可以自行决定是否把 Phase 5 切成“解耦波次”和“native hardening 波次”，前提是不弱化 D-09 ~ D-16。

### Deferred Ideas (OUT OF SCOPE)
- 把 `Videra.Core` 直接拆成多个 packages / assemblies
- 全量重写统一 graphics abstraction
- 广泛 Rust 化 native host / Vulkan-X11 / Avalonia glue
- 在无 profiling 证据前引入 Rust software rasterizer
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| PERF-01 | 性能优化：热路径收口、线框颜色更新优化、资源缓存 | 研究确认当前热点在 `Object3D.UpdateWireframeColor`、software copy、macOS `waitUntilCompleted`，且应先通过架构/API 收口而不是 Rust。 |
| PERF-02 | 可扩展性改进：GPU 资源池化、实例渲染、动态 uniform/ring buffer | 研究建议本阶段只建立 seam 与 typed contract，为后续 pooling/instancing/ring buffer 留接口，不把整套特性全部塞入 Phase 5。 |
| SEC-01 | 安全加固：指针/句柄边界、路径验证、库句柄/函数指针验证 | `ModelImporter` 的路径验证已存在；Phase 5 剩余重点是 native handle、函数指针、返回值和所有权模型硬化。 |
| CLEAN-01 | 资源清理验证：RAII、部分初始化回滚、泄漏检测 | 研究识别到 Metal layer 生命周期、Vulkan partial-init rollback、X11 display/window ownership 是主要清理风险。 |
| MACOS-01 | 重构 Metal 互操作：先做 C# 第一轮硬化，再决定是否需要更高层绑定 | 研究建议先收敛 `objc_msgSend` 变体、引入 owned-handle/typed wrapper、去掉 UI 线程 `waitUntilCompleted`，再写书面结论决定是否继续。 |
| LINUX-02 | 改进 X11 处理：验证库句柄、收紧 ownership、提供回退 | 研究建议延续 `ISurfaceCreator` 模式，把 host 与 surface 的 display ownership 对齐，并集中化 Vulkan `Result` 检查。 |
</phase_requirements>

## Summary

Phase 5 的核心不是“选不选 Rust”，而是把当前已经暴露出来的三个混杂区域拆开：`Videra.Core` 里的后端发现/反射装配、`VideraView` 里的 backend/session/timer/native handle 全生命周期、以及 macOS/Linux 原生边界里的未检查返回值与所有权漏洞。当前代码已经证明这些问题是结构性的，不是单点 bug：`GraphicsBackendFactory` 仍在 Core 中直接 `Assembly.Load` 平台后端，`VideraView` 仍直接 new 各平台 native host 并自己持有 render timer，Metal 路径仍在 UI 线程 `waitUntilCompleted`，Vulkan/X11 路径仍有多个关键返回值未检查。

最稳妥的推进方式是保持本阶段纯 C#，并明确拆成三个 planning wave。Wave 0 先做 requirement namespace 对齐，因为 `ROADMAP.md`/`PROJECT.md` 的 `MACOS-01`、`LINUX-02`、`PERF-*` 与 `REQUIREMENTS.md` 的 `MAC-01`、`PLAT-*`、`CLEAN-01` 含义并不一致；如果不先统一，后续 PLAN 会把“资源清理验证”和“tmp 文件清理”混为一谈。Wave 1 做组合 seam 与 render session seam，Wave 2 做 macOS/Linux native hardening，Wave 3 只做证据收集与 Rust 决策 memo，不默认包含 Rust 实现。

本机环境也强烈支持这种拆法。当前可执行的是 .NET/C# 结构改造和 Windows 本地验证；真正闭合 `MACOS-01` 和 `LINUX-02` 仍需要原生 macOS/Linux 主机。Rust 工具链当前不存在，因此任何“顺手加一个 Rust spike”的计划都会变成额外阻塞，而不是风险收敛。

**Primary recommendation:** 保持 Phase 5 纯 C#，按“Requirement 对齐 -> 组合/会话解耦 -> macOS/Linux 边界硬化 -> 证据化 Rust 决策”四段规划，不把依赖升级、Wayland 扩展或 Rust spike 混入主波次。

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| .NET / BCL interop APIs | `net8.0` target, SDKs `9.0.308` + `10.0.201` available | `LibraryImport`, `SafeHandle`, `NativeLibrary`, span-friendly interop | 这是当前仓库目标框架与官方推荐互操作工具链；Phase 5 不需要引入额外 FFI 框架。 |
| Avalonia | `11.3.9` repo pinned | UI shell、`NativeControlHost`、平台句柄接入 | 当前 UI 层已在用；官方 native interop 文档也推荐 shared interface + platform registration 模式。 |
| Silk.NET | `2.21.0` repo pinned | Vulkan / D3D11 绑定 | 当前平台后端已经基于 Silk.NET；Phase 5 应围绕现有 binding layer 做结果检查与 ownership 收口。 |
| `System.Runtime.InteropServices.ObjectiveC` | 官方 ABI support（.NET 8/9 文档可见） | Objective-C ABI 的官方方向性支持 | 这是后续评估 safer macOS binding 的官方方向，但 Phase 5 不应把它当成“现成 Metal wrapper”。 |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| SharpGLTF.Toolkit | `1.0.6` repo pinned/current | `ModelImporter` 与 mesh preprocessing | 继续保留；如果未来真的要试极小 Rust 边界，`ModelImporter` 是首选入口。 |
| BenchmarkDotNet | `0.15.8` latest stable | CPU 侧热点微基准 | 只在需要为 `PERF-01`/Rust memo 提供可重复证据时引入单独 benchmark 项目。 |
| `NativeLibraryHelper` | repo local helper | 多候选库名加载与符号查询 | Linux/X11 fallback 已部分存在；Phase 5 继续扩展但不要在每个 native 文件里重复 loader 逻辑。 |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| 现有 C# interop 收口 + hardening | 立即引入第三方 Metal wrapper / 更换平台 binding 方案 | 会把 Phase 5 变成依赖迁移和平台适配项目，风险高于收益。 |
| Avalonia 组合层 resolver/registry seam | 继续把 reflection 发现留在 `Videra.Core` | 保留了当前最不清晰、最难测的边界。 |
| 单独 benchmark 项目 | 手写 `Stopwatch` 循环 | 对 allocation、JIT、warmup 噪声不可靠，不足以支撑 Rust 决策。 |

**Installation:**
```bash
# Phase 5 主线不需要新增核心依赖。
# 仅当需要可重复 CPU 微基准时，再单独添加 benchmark 项目：
dotnet new console -n Videra.Benchmarks
dotnet add Videra.Benchmarks package BenchmarkDotNet --version 0.15.8
```

**Version verification:** 2026-04-02 已通过 NuGet registration API 验证当前稳定版。
- Avalonia latest stable: `11.3.13` published `2026-03-28`; repo pinned `11.3.9`
- Silk.NET.Vulkan latest stable: `2.23.0` published `2026-01-24`; repo pinned `2.21.0`
- SharpGLTF.Toolkit latest stable/current: `1.0.6` published `2025-12-30`
- BenchmarkDotNet latest stable: `0.15.8` published `2025-11-30`
- Microsoft.Extensions.Logging.Abstractions latest stable: `10.0.5` published `2026-03-13`; repo pinned `9.0.11`

**Package guidance:** Phase 5 不应夹带这些依赖升级。当前研究结论是“沿用 repo pinned 版本完成架构收口”，否则会把真实回归混进依赖噪声。

## Architecture Patterns

### Recommended Project Structure
```text
src/
├── Videra.Core/Graphics/Contracts/     # typed slots, primitive enums, always-supported contract
├── Videra.Avalonia/Composition/        # backend resolver, render session, native host factory
├── Videra.Avalonia/Controls/           # thin VideraView shell + input forwarding
├── Videra.Platform.macOS/Interop/      # ObjC owned handles, selector helpers, layer/device wrappers
└── Videra.Platform.Linux/Interop/      # X11 display/window ownership + Vulkan result guards
```

### Pattern 1: Composition-Root Backend Resolver
**What:** 把平台后端发现与实例化从 `Videra.Core` 挪到 Avalonia 组合层，通过 `IGraphicsBackendResolver` 或 registry 统一处理 `VIDERA_BACKEND`、平台判断、fallback 和兼容适配器。
**When to use:** 所有 backend 选择入口，包括 `Auto` 偏好、环境变量覆盖、native host 需要的 companion factory 绑定。
**Example:**
```csharp
// Source: https://docs.avaloniaui.net/docs/app-development/native-interop
public interface IGraphicsBackendResolver
{
    IGraphicsBackend Resolve(GraphicsBackendPreference preference);
}

public interface INativeHostFactory
{
    IVideraNativeHost Create(GraphicsBackendPreference preference);
}
```

### Pattern 2: Render Session State Machine Outside the Control
**What:** `VideraView` 只保留 Avalonia property/input/UI surface；`RenderSession` 或 `ViewRendererController` 负责 backend、engine、timer、bitmap/native handle、rebind 和 dispose。
**When to use:** 所有需要区分 software/native backend，或 native handle 可能 destroy/recreate 的路径。
**Example:**
```csharp
// Source: derived from current repo architecture + Avalonia native interop service pattern
public enum RenderSessionState
{
    WaitingForView,
    WaitingForNativeHandle,
    Ready,
    Rebinding,
    Disposed
}
```

### Pattern 3: Owned Native Boundary Wrappers
**What:** 为 `Display*`、X11 window、`CAMetalLayer`、Metal buffer contents、Vulkan `Result` 检查建立小而清晰的所有权/结果封装，而不是把 `IntPtr` 和 raw `Result` 到处散开。
**When to use:** 每一个 native call crossing，尤其是 create/bind/map/present 这些需要 rollback 的路径。
**Example:**
```csharp
// Source: https://learn.microsoft.com/en-us/dotnet/standard/native-interop/best-practices
// Source: https://learn.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke-source-generation
internal sealed class X11DisplayHandle : SafeHandle
{
    private X11DisplayHandle() : base(IntPtr.Zero, ownsHandle: true) { }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle() => NativeMethods.XCloseDisplay(handle) == 0;
}

internal static partial class NativeMethods
{
    [LibraryImport("libX11.so.6")]
    internal static partial int XCloseDisplay(IntPtr display);
}
```

### Anti-Patterns to Avoid
- **Core 继续持有平台反射装配:** `GraphicsBackendFactory` 当前直接 `Assembly.Load` + `Activator.CreateInstance`，这会让 Core 持续耦合平台发现逻辑。
- **`VideraView` 同时当控件、composition root、session owner、host factory、timer owner:** 当前 flags 和 lifecycle 已经分散在一个控件里，扩展任何平台特性都会继续恶化。
- **把 handle destroy 当 resize:** `OnNativeHandleDestroyed` 当前只把 `_renderHandle` 归零，未触发 backend/session rebind。
- **继续传播 magic numbers:** camera/style/world slot 和 primitive type 仍是 `1/2/3` 之类的裸数字。
- **把 native hardening 理解为“再多写几个 null check”:** 真正的问题是所有权、回滚和单一出口，而不是某个点状空指针。

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Native handle lifetime | 到处传 `IntPtr` + `_disposed` flag | `SafeHandle`/owned wrapper + 单一释放点 | .NET 官方 best practices 明确偏向 `SafeHandle`，能避免双释放和漏释放。 |
| X11/Vulkan result handling | 每个 call-site 各自忽略或日志打印 `Result` | 集中的 `VkResultGuard` / `ThrowIfFailed` / `RecreateRequired` helper | `vkMapMemory`、`vkBindBufferMemory`、`vkAcquireNextImageKHR`、`vkQueuePresentKHR` 都有明确返回码语义，分散处理很容易漏。 |
| Backend discovery | 继续在 Core 里做平台判断 + 反射装配 | Avalonia composition resolver/registry seam | 官方 Avalonia native interop 文档本身就是 shared interface + platform registration 模式。 |
| Performance evidence | 手工 `Stopwatch`、随机日志、肉眼观察 | `BenchmarkDotNet` + 平台 profiler | Phase 5 的 Rust 决策必须有可重复证据，不能靠体感。 |
| Binding slot / primitive conventions | 裸 `uint`/`int` 魔法数字 | `enum` / typed constants / small value object | 这一步的成本低，但能显著降低后端与 engine contract 误绑风险。 |

**Key insight:** 本阶段最容易“手搓出第二套问题”的地方，是 native lifetime 和 result handling。这里不需要大框架，但必须要有集中、可复用、可验证的小抽象，而不是把 `IntPtr`/`Result` 继续散在 UI、Core、Platform 三层。

## Runtime State Inventory

| Category | Items Found | Action Required |
|----------|-------------|------------------|
| Stored data | None — verified by repo scan; 项目没有数据库、缓存存储或持久化 backend/session state 线索。 | None |
| Live service config | None — verified by repo scan; 当前是库 + sample app，没有 UI-only service/workflow 配置。 | None |
| OS-registered state | None — verified by repo scan; 没有 systemd/launchd/task scheduler/service installer 资产。 | None |
| Secrets/env vars | `VIDERA_BACKEND`, `VIDERA_FRAMELOG`, `VIDERA_INPUTLOG` 是现有运行时开关。 | Code edit only — 新 resolver/session 层必须保留兼容读取或提供兼容 shim。 |
| Build artifacts | `bin/`, `obj/`, `coverage/` 以及测试输出会在 seam 重构后失效；不存在“旧名字数据迁移”。 | Rebuild only — `dotnet clean` + `dotnet build` + 重新执行相关测试。 |

## Common Pitfalls

### Pitfall 1: Requirement IDs Drift Before Planning Starts
**What goes wrong:** 计划把 `CLEAN-01` 当成临时文件清理，或把 `MACOS-01`/`LINUX-02` 直接映射到 `REQUIREMENTS.md` 的旧命名。
**Why it happens:** `PROJECT.md`、`ROADMAP.md`、`REQUIREMENTS.md` 当前存在双重命名体系。
**How to avoid:** Wave 0 先做 requirement normalization，并把 canonical mapping 写回 phase plan。
**Warning signs:** planner 无法给出单一 traceability table，或把 Phase 1/2 已完成的项重新规划进 Phase 5。

### Pitfall 2: Extracting a Class Without Extracting Ownership
**What goes wrong:** 新增了 `RenderSession` 名字，但 timer、handle、bitmap、backend dispose 仍旧 scattered 在 `VideraView`。
**Why it happens:** 只做“搬代码”而没有定义 state machine 和 owner。
**How to avoid:** 先列清 `VideraView` 当前 owner 列表，再把 owner 一次性迁移到 session/controller。
**Warning signs:** `VideraView` 仍直接 new host/backend 或直接持有 `_renderTimer`/`_renderHandle`。

### Pitfall 3: Treating Native Handle Destroy as a Resize Event
**What goes wrong:** backend 仍拿着旧 surface/layer/window，下一次 resize 或 draw 才在未知状态下崩。
**Why it happens:** 当前代码只把 `_renderHandle` 清零，没有显式 rebind protocol。
**How to avoid:** 把 handle create/destroy 升级成 session state transition：`WaitingForNativeHandle -> Rebinding -> Ready`。
**Warning signs:** 任何路径仍从 `HandleDestroyed` 直接回到普通 `Resize`。

### Pitfall 4: Hardening Metal/Vulkan Only at the Call Site
**What goes wrong:** 加了几处 null check，但 `contents` 空指针、Metal layer 泄漏、Vulkan `Result` 忽略和 X11 display ownership 仍然存在。
**Why it happens:** 原生 bug 往往是生命周期问题，不是单一 API 返回值问题。
**How to avoid:** 先建立 owned-handle/result-guard 层，再改具体 backend。
**Warning signs:** `IntPtr` 数量不降反升；`Dispose()` 仍看不出谁负责释放 layer/display/window。

### Pitfall 5: Using Performance Work to Smuggle in Scope
**What goes wrong:** 计划因为 `PERF-02` 直接扩展到 pooling、instancing、ring buffer、Rust spike 全做。
**Why it happens:** `PROJECT.md` 的性能条目比本阶段锁定边界更大。
**How to avoid:** 把 Phase 5 的性能目标限定为“识别热点、移除明显结构性瓶颈、为下一阶段建立 seam”，不是 feature-complete rendering rewrite。
**Warning signs:** plan 里出现新的 renderer architecture、跨平台 shader 统一、Rust crate scaffold 等与锁定边界无关的任务。

## Code Examples

Verified patterns from official sources:

### Shared Interface + Platform Registration
```csharp
// Source: https://docs.avaloniaui.net/docs/app-development/native-interop
public interface INativeHostFactory
{
    IVideraNativeHost Create(GraphicsBackendPreference preference);
}

builder.AfterSetup(_ =>
{
    services.AddSingleton<INativeHostFactory, PlatformNativeHostFactory>();
});
```

### Source-Generated P/Invoke + SafeHandle
```csharp
// Source: https://learn.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke-source-generation
// Source: https://learn.microsoft.com/en-us/dotnet/standard/native-interop/best-practices
internal static partial class NativeMethods
{
    [LibraryImport("libX11.so.6")]
    internal static partial IntPtr XOpenDisplay(IntPtr displayName);

    [LibraryImport("libX11.so.6")]
    internal static partial int XCloseDisplay(IntPtr display);
}

internal sealed class X11DisplayHandle : SafeHandle
{
    private X11DisplayHandle() : base(IntPtr.Zero, ownsHandle: true) { }
    public override bool IsInvalid => handle == IntPtr.Zero;
    protected override bool ReleaseHandle() => NativeMethods.XCloseDisplay(handle) == 0;
}
```

### Minimal BenchmarkDotNet Harness
```csharp
// Source: https://benchmarkdotnet.org/articles/guides/getting-started.html
[MemoryDiagnoser]
public class WireframeBenchmarks
{
    [Benchmark]
    public void UpdateWireframeColor()
    {
        _object.UpdateWireframeColor(_lineColor);
    }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Handwritten `DllImport` stubs everywhere | `LibraryImport` source-generated stubs | .NET 7+ current guidance | 更适合把 X11/Win32 的固定签名 native 方法集中化并交给 analyzers 检查。 |
| Raw `IntPtr` ownership + manual cleanup | `SafeHandle`-first unmanaged lifetime guidance | current .NET interop best practices | 对 Phase 5 的 X11 display/window、future Metal object wrapper 特别重要。 |
| Shared UI/core code直接分支到平台实现 | shared interface + platform registration | current Avalonia native interop docs | 正好匹配本阶段要做的 backend resolver / native host factory seam。 |
| 手工 Objective-C ABI 细节完全自管 | .NET 提供 `System.Runtime.InteropServices.ObjectiveC` ABI support | .NET 8/9 | 这是官方方向，但仍偏低层，不意味着 Phase 5 应立即重写成新 binding stack。 |

**Deprecated/outdated:**
- 在 UI 线程每帧 `waitUntilCompleted` 再提交下一帧。
- 继续在 Core 中承载平台 reflection composition。
- 继续传播 buffer slot / primitive type magic numbers。

## Open Questions

1. **哪一份文档是 Phase 5 requirement ID 的 canonical source？**
   - What we know: `PROJECT.md`/`ROADMAP.md` 使用 `MACOS-01`、`LINUX-02`、`PERF-*`，`REQUIREMENTS.md` 仍是 `MAC-01`、`PLAT-*`，且 `CLEAN-01` 含义不同。
   - What's unclear: planner 应该按哪一组 ID 写 traceability，还是需要先做一对一映射。
   - Recommendation: 把 requirement normalization 作为 Phase 5 的第一个 planning task，并同步更新状态文档。

2. **`SEC-01` 在 Phase 5 中是否只剩 native boundary regression guard？**
   - What we know: `ModelImporter` 已有 `Path.GetFullPath`、扩展名校验、目录/文件存在校验；相关单测也存在。
   - What's unclear: 是否还需要把“文件路径验证”重新规划进 Phase 5，还是只保留 regression coverage。
   - Recommendation: 默认只做 regression guard，把 Phase 5 的安全重点限定在 native handle、函数指针与返回值硬化。

3. **`PERF-02` 是“实现 pooling/instancing/ring buffer”，还是“为它们建立正确边界”？**
   - What we know: `PROJECT.md` 写的是完整特性集合，但 Phase 5 锁定决策明确反对大范围接口重写。
   - What's unclear: 本阶段是否真的要交付实例渲染和 ring buffer。
   - Recommendation: 先只计划 seam、typed contract 与 benchmark evidence；除非用户显式扩 scope，否则不要承诺 feature-complete instancing。

4. **是否允许引入 `net10.0-macos` 或平台特定 TFM 以采用 Avalonia 文档中的 macOS native-view 方案？**
   - What we know: Avalonia 官方 native interop 文档对 macOS 原生 view 创建写的是 `net10.0-macos` 路径；当前仓库全部是 `net8.0`。
   - What's unclear: Phase 5 是否允许 TFM 级别变更。
   - Recommendation: 本阶段不要改 TFM；先把现有 C# interop 边界收紧，等需要更高层 binding 时再单独评估。

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| .NET SDK | 构建、测试、重构验证 | ✓ | `10.0.201` (`9.0.308`, `10.0.201` installed) | Use current `net8.0` target |
| Windows host | 当前本机开发/回归验证 | ✓ | Windows 11 `10.0.26200` | — |
| Native Linux host with X11 + Vulkan | `LINUX-02` 真实 runtime hardening 与验证 | ✗ | — | None |
| Native macOS host with Metal | `MACOS-01` 真实 runtime hardening 与验证 | ✗ | — | None |
| Rust toolchain (`cargo`, `rustc`) | 未来可选的极小 Rust spike | ✗ | — | Keep `No Rust by default` |
| BenchmarkDotNet project | 需要重复 CPU 侧性能证据时 | ✗ (not installed in repo) | — | Optional future add |

**Missing dependencies with no fallback:**
- Native Linux host with X11 + Vulkan
- Native macOS host with Metal

**Missing dependencies with fallback:**
- Rust toolchain missing — fallback is the locked default path: do not introduce Rust in this phase
- BenchmarkDotNet project missing — fallback is 暂时先做结构性瓶颈修复；只有在需要书面证据时才创建 benchmark 项目

## Sources

### Primary (HIGH confidence)
- Repo code inspection:
  - `src/Videra.Core/Graphics/GraphicsBackendFactory.cs`
  - `src/Videra.Avalonia/Controls/VideraView.cs`
  - `src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs`
  - `src/Videra.Avalonia/Controls/VideraMacOSNativeHost.cs`
  - `src/Videra.Platform.macOS/MetalBackend.cs`
  - `src/Videra.Platform.macOS/MetalCommandExecutor.cs`
  - `src/Videra.Platform.macOS/MetalBuffer.cs`
  - `src/Videra.Platform.Linux/VulkanBackend.cs`
  - `src/Videra.Platform.Linux/VulkanResourceFactory.cs`
  - `src/Videra.Platform.Linux/VulkanBuffer.cs`
  - `src/Videra.Core/Graphics/Object3D.cs`
  - `src/Videra.Core/Graphics/VideraEngine.cs`
- Microsoft Learn: native interop best practices
  - https://learn.microsoft.com/en-us/dotnet/standard/native-interop/best-practices
- Microsoft Learn: P/Invoke source generation
  - https://learn.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke-source-generation
- Avalonia docs: native interop
  - https://docs.avaloniaui.net/docs/app-development/native-interop
- Avalonia API docs: `NativeControlHost`
  - https://api-docs.avaloniaui.net/docs/T_Avalonia_Controls_NativeControlHost
- Apple Metal Best Practices Guide: Drawables
  - https://developer.apple.com/library/archive/documentation/3DDrawing/Conceptual/MTLBestPracticesGuide/Drawables.html
- Khronos Vulkan refpages:
  - https://registry.khronos.org/vulkan/specs/latest/man/html/vkMapMemory.html
  - https://registry.khronos.org/vulkan/specs/latest/man/html/vkBindBufferMemory.html
  - https://registry.khronos.org/vulkan/specs/latest/man/html/vkBindImageMemory.html
  - https://registry.khronos.org/vulkan/specs/latest/man/html/vkAcquireNextImageKHR.html
  - https://registry.khronos.org/vulkan/specs/latest/man/html/vkQueueSubmit.html
  - https://registry.khronos.org/vulkan/specs/latest/man/html/vkQueuePresentKHR.html
- NuGet registration API (package version verification)
  - https://api.nuget.org/v3/registration5-semver1/avalonia/index.json
  - https://api.nuget.org/v3/registration5-semver1/silk.net.vulkan/index.json
  - https://api.nuget.org/v3/registration5-semver1/sharpgltf.toolkit/index.json
  - https://api.nuget.org/v3/registration5-semver1/benchmarkdotnet/index.json

### Secondary (MEDIUM confidence)
- Microsoft Learn: ABI support for Objective-C / C++ interop
  - https://learn.microsoft.com/en-us/dotnet/standard/native-interop/abi-support
- Microsoft Learn API docs: `ObjectiveCMarshal`
  - https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.objectivec.objectivecmarshal?view=net-9.0
- BenchmarkDotNet docs: getting started
  - https://benchmarkdotnet.org/articles/guides/getting-started.html

### Tertiary (LOW confidence)
- None

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - 基于仓库实际依赖、官方文档、NuGet 当前版本校验
- Architecture: MEDIUM - 代码证据清晰，但 Linux/macOS 原生 runtime 仍未在对应主机上完成闭环验证
- Pitfalls: HIGH - 由当前代码热点与官方 interop / Vulkan / Apple 文档共同支持

**Research date:** 2026-04-02
**Valid until:** 2026-05-02
