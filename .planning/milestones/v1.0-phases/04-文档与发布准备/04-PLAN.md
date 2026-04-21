---
phase: 04-文档与发布准备
plan: 01
type: execute
wave: 8
depends_on: ["03-05"]
requirements: [DOC-01, DOC-02, DOC-03]
autonomous: true
---

<objective>
完成 Videra 项目所有面向用户的文档，包括 XML API 注释、README 扩展、贡献指南和架构文档。项目达到开源发布标准。
</objective>

<execution_context>
@F:/CodeProjects/DotnetCore/Videra/.planning/REQUIREMENTS.md
@F:/CodeProjects/DotnetCore/Videra/.planning/STATE.md
@F:/CodeProjects/DotnetCore/Videra/README.md
@F:/CodeProjects/DotnetCore/Videra/ARCHITECTURE.md
</execution_context>

<guardrails>
- 不伪造"已在 Linux/macOS 验证"的事实。
- 所有改动必须通过 `dotnet build Videra.slnx`（0 errors）。
- 所有现有测试必须继续通过（不引入回归）。
- 文档内容基于实际代码，不虚构功能。
- XML 注释用英文撰写（国际开源标准）。
</guardrails>

<tasks>

<task id="04-01" type="auto" tdd="false">
  <name>补全核心公共 API XML 注释</name>
  <files>
    src/Videra.Core/Graphics/VideraEngine.cs
    src/Videra.Core/Graphics/Object3D.cs
    src/Videra.Core/Graphics/GridRenderer.cs
    src/Videra.Core/Cameras/OrbitCamera.cs
    src/Videra.Core/IO/ModelImporter.cs
    src/Videra.Core/Graphics/Abstractions/DepthBufferConfiguration.cs
    src/Videra.Core/NativeLibrary/NativeLibraryHelper.cs
  </files>
  <action>
    1. 为 VideraEngine 的所有公共方法和属性添加 XML 注释。
    2. 为 Object3D 添加 XML 注释（类、属性、方法）。
    3. 为 OrbitCamera 添加 XML 注释（Rotate、Zoom、Pan、UpdateProjection）。
    4. 为 ModelImporter 添加 XML 注释（Load、SupportedFormats）。
    5. 为 GridRenderer 添加 XML 注释。
    6. 为 DepthBufferConfiguration 和 NativeLibraryHelper 确保注释完整。
    7. 在 Videra.Core.csproj 中添加 `<GenerateDocumentationFile>true</GenerateDocumentationFile>` 并处理所有 CS1591 警告。
  </action>
  <acceptance_criteria>
    - Videra.Core 所有公共 API 有 XML 注释
    - 构建通过，无 CS1591 警告
  </acceptance_criteria>
</task>

<task id="04-02" type="auto" tdd="false">
  <name>补全平台和 Avalonia 公共 API XML 注释</name>
  <files>
    src/Videra.Avalonia/Controls/VideraView.cs
    src/Videra.Avalonia/Controls/VideraNativeHost.cs
    src/Videra.Platform.Windows/D3D11Backend.cs
    src/Videra.Platform.Linux/VulkanBackend.cs
    src/Videra.Platform.macOS/MetalBackend.cs
  </files>
  <action>
    1. 为 VideraView 的所有公共属性和事件添加 XML 注释。
    2. 为 VideraNativeHost 添加 XML 注释。
    3. 为 D3D11Backend、VulkanBackend、MetalBackend 的公共方法添加 XML 注释。
    4. 在各平台 .csproj 中启用 `<GenerateDocumentationFile>`。
    5. 对任何 internal/private 成员用 `#pragma warning disable CS1591` 或添加注释。
  </action>
  <acceptance_criteria>
    - 所有平台和 Avalonia 公共 API 有 XML 注释
    - 构建通过
  </acceptance_criteria>
</task>

<task id="04-03" type="auto" tdd="false">
  <name>创建 CONTRIBUTING.md 贡献指南</name>
  <files>
    CONTRIBUTING.md (new)
  </files>
  <action>
    1. 基于 .planning/codebase/CONVENTIONS.md 的编码规范创建用户友好的 CONTRIBUTING.md。
    2. 包含：开发环境搭建、代码风格要求、PR 流程、提交消息格式、测试要求。
    3. 英文撰写。
  </action>
  <acceptance_criteria>
    - CONTRIBUTING.md 存在且覆盖所有要求
  </acceptance_criteria>
</task>

<task id="04-04" type="auto" tdd="false">
  <name>增强 README.md 和架构文档</name>
  <files>
    README.md
    ARCHITECTURE.md
  </files>
  <action>
    1. README.md：添加 Features 列表、扩展快速开始、添加故障排查段落、链接到 CONTRIBUTING.md。
    2. ARCHITECTURE.md：确保渲染管线文档完整，添加平台特定注意事项段落。
    3. 保持中文（与现有文档语言一致）。
  </action>
  <acceptance_criteria>
    - README 有 Features、快速开始、故障排查
    - ARCHITECTURE.md 有渲染管线和平台特定说明
  </acceptance_criteria>
</task>

<task id="04-05" type="auto" tdd="false">
  <name>Phase 4 验证与状态更新</name>
  <files>
    .planning/phases/04-文档与发布准备/04-SUMMARY.md
    .planning/phases/04-文档与发布准备/04-VERIFICATION.md
    .planning/STATE.md
    .planning/ROADMAP.md
  </files>
  <action>
    1. 验证所有 XML 注释生成无警告。
    2. 验证构建和测试通过。
    3. 更新 STATE.md 和 ROADMAP.md。
    4. 创建 summary 和 verification 文档。
  </action>
  <acceptance_criteria>
    - Phase 4 所有 deliverable 完成
    - 状态文件与实际一致
  </acceptance_criteria>
</task>

</tasks>

<verification>
- `dotnet build F:/CodeProjects/DotnetCore/Videra/Videra.slnx` — 0 errors
- `dotnet test F:/CodeProjects/DotnetCore/Videra/Videra.slnx` — 0 failures
- CONTRIBUTING.md exists
- README.md has Features, Quick Start, Troubleshooting sections
- All public APIs have XML doc comments (no CS1591 on public members)
</verification>
