---
phase: 03-跨平台完善
plan: 01
type: execute
wave: 7
depends_on: ["02-05"]
requirements: [MAC-01, MAC-02, PLAT-01, PLAT-02, PLAT-03]
autonomous: true
---

<objective>
在不伪造跨平台验证结果的前提下，在 Windows 上完成所有可实现的 Phase 3 代码改进：统一深度缓冲管理、提取 ObjC 互操作辅助层、抽象动态库加载、以及 Vulkan 表面创建策略模式。将无法在当前环境验证的工作记录为后续执行包。
</objective>

<execution_context>
@F:/CodeProjects/DotnetCore/Videra/.planning/phases/03-跨平台完善/03-RESEARCH.md
@F:/CodeProjects/DotnetCore/Videra/.planning/REQUIREMENTS.md
@F:/CodeProjects/DotnetCore/Videra/.planning/STATE.md
</execution_context>

<guardrails>
- 不伪造"已在 Linux/macOS 验证"的事实。
- 所有改动必须通过 `dotnet build Videra.slnx`（0 errors）。
- 所有现有测试必须继续通过（不引入回归）。
- ObjC 互操作重构仅限提取共享代码，不改变外部行为。
- 深度缓冲统一仅影响配置，不改变渲染结果。
</guardrails>

<tasks>

<task id="03-01" type="auto" tdd="true">
  <name>统一三平台深度缓冲配置</name>
  <files>
    src/Videra.Platform.Windows/D3D11Backend.cs
    src/Videra.Platform.Linux/VulkanBackend.cs
    src/Videra.Platform.macOS/MetalBackend.cs
  </files>
  <action>
    1. 在 `Videra.Core/Graphics/Abstractions/` 添加 `DepthBufferConfiguration` 值类型，包含 Format、ClearValue、ComparisonFunc 枚举。
    2. D3D11: 保留 D24UnormS8Uint 但添加注释说明为何选择此格式（兼容性）。确保 CreateDepthStencil 和 CreateDepthStencilState 使用配置。
    3. Vulkan: 将 D32Sfloat 保留，统一注释。确保 CreateDepthResources 和 pipeline depth state 使用配置。
    4. Metal: 在 CreateDepthStencilState 中显式设置深度格式（MTLPixelFormatDepth32Float = 252），统一比较函数注释。
    5. 所有平台统一比较函数为 LessEqual，清除值为 1.0f。
    6. 添加测试验证 DepthBufferConfiguration 值类型。
  </action>
  <acceptance_criteria>
    - 三个后端深度比较函数一致（LessEqual）
    - 三个后端清除值一致（1.0f）
    - Metal 显式指定深度格式
    - 无渲染回归（构建通过）
  </acceptance_criteria>
</task>

<task id="03-02" type="auto" tdd="true">
  <name>提取 macOS ObjC 互操作辅助层</name>
  <files>
    src/Videra.Platform.macOS/ObjCRuntime.cs (new)
    src/Videra.Platform.macOS/MetalBackend.cs
    src/Videra.Platform.macOS/MetalCommandExecutor.cs
    src/Videra.Platform.macOS/MetalResourceFactory.cs
    src/Videra.Platform.macOS/MetalBuffer.cs
    src/Videra.Platform.macOS/MetalPipeline.cs
  </files>
  <action>
    1. 创建 `ObjCRuntime.cs` 静态类，集中所有 DllImport 声明：
       - `objc_getClass`, `sel_registerName`
       - `objc_msgSend` 的所有签名变体（IntPtr, int, bool, double, CGSize 等）
       - `MTLCreateSystemDefaultDevice`
    2. 提供类型安全的辅助方法：
       - `GetClass(name)`, `RegisterSelector(name)`
       - `SendMessage(receiver, sel)`, `SendMessageInt(receiver, sel, int)` 等
       - `AllocInit(className)`
    3. 将所有 Metal 文件中的内联 DllImport 替换为对 ObjCRuntime 的调用。
    4. 保留相同的外部行为 — 纯重构。
  </action>
  <acceptance_criteria>
    - 每个 Metal 文件中不再有重复的 DllImport 声明
    - ObjCRuntime.cs 是唯一的 P/Invoke 入口点
    - 所有调用点使用类型安全辅助方法
    - 构建通过，无行为变化
  </acceptance_criteria>
</task>

<task id="03-03" type="auto" tdd="true">
  <name>抽象动态库加载与路径回退</name>
  <files>
    src/Videra.Core/NativeLibrary/NativeLibraryHelper.cs (new)
    src/Videra.Platform.Linux/VulkanBackend.cs
    src/Videra.Avalonia/Controls/VideraLinuxNativeHost.cs
  </files>
  <action>
    1. 创建 `NativeLibraryHelper` 静态类：
       - `LoadLibrary(string[] candidatePaths)` — 按序尝试多个路径
       - `GetSymbol(IntPtr library, string symbolName)` — dlsym/GetProcAddress
       - 使用 `System.Runtime.InteropServices.NativeLibrary` API (.NET Core 3.0+)
    2. 在 `VulkanBackend.cs` 中将硬编码 `libX11.so.6` 替换为 `NativeLibraryHelper` 调用，支持回退路径：`libX11.so.6`, `libX11.so`, `libX11`。
    3. 在 `VideraLinuxNativeHost.cs` 中同样替换。
    4. 添加测试验证路径回退逻辑。
  </action>
  <acceptance_criteria>
    - 硬编码库路径替换为可配置回退
    - 使用 .NET Runtime NativeLibrary API（不是手动 dlopen）
    - 构建通过
  </acceptance_criteria>
</task>

<task id="03-04" type="auto">
  <name>抽象 Vulkan 表面创建策略（X11/Wayland 准备）</name>
  <files>
    src/Videra.Platform.Linux/VulkanBackend.cs
    src/Videra.Platform.Linux/ISurfaceCreator.cs (new)
    src/Videra.Platform.Linux/X11SurfaceCreator.cs (new)
  </files>
  <action>
    1. 创建 `ISurfaceCreator` 接口：`SurfaceKHR CreateSurface(Instance instance, IntPtr windowHandle)`
    2. 将 `VulkanBackend.CreateSurface` 中的 X11 特定代码提取到 `X11SurfaceCreator` 类
    3. `VulkanBackend` 通过构造函数/参数接受 `ISurfaceCreator`（默认为 X11）
    4. Wayland 实现留给 Phase 3B（环境阻塞）
  </action>
  <acceptance_criteria>
    - X11 表面创建逻辑封装在独立类中
    - VulkanBackend 不再直接依赖 X11 函数
    - Wayland 扩展点就绪
    - 构建通过
  </acceptance_criteria>
</task>

<task id="03-05" type="auto">
  <name>Phase 3 验证矩阵与状态更新</name>
  <files>
    .planning/phases/03-跨平台完善/03-SUMMARY.md
    .planning/phases/03-跨平台完善/03-VERIFICATION.md
    .planning/STATE.md
    .planning/ROADMAP.md
  </files>
  <action>
    1. 运行构建与测试矩阵。
    2. 以 requirement ID 回填证据。
    3. 记录哪些 requirement 完全关闭、哪些部分完成（代码就绪但未运行时验证）。
    4. 更新 STATE.md 和 ROADMAP.md。
  </action>
  <acceptance_criteria>
    - 每个 requirement 有可追踪证据或明确的阻塞说明
    - 状态文件与实际一致
  </acceptance_criteria>
</task>

</tasks>

<verification>
- `dotnet build F:/CodeProjects/DotnetCore/Videra/Videra.slnx`
- `dotnet test F:/CodeProjects/DotnetCore/Videra/tests/Videra.Core.Tests/`
- `dotnet test F:/CodeProjects/DotnetCore/Videra/tests/Videra.Core.IntegrationTests/`
- `dotnet test F:/CodeProjects/DotnetCore/Videra/tests/Videra.Platform.Windows.Tests/`
- grep: Metal 文件中不再有重复的 DllImport
- grep: 硬编码 libX11.so.6 替换为回退机制
</verification>
