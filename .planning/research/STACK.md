# Technology Stack

**Project:** Videra 3D Rendering Engine - Open Source Preparation
**Researched:** 2026-03-28
**Overall confidence:** HIGH

## Recommended Stack

### Core Framework

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| .NET 8 | 8.0.x LTS | Primary runtime | **MUST KEEP** - Already adopted, LTS until 2026-11-10, cross-platform support, performance improvements |
| C# 12 | Default | Language version | **MUST KEEP** - Built into .NET 8, modern language features |

### Graphics (Existing - Keep)

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Silk.NET | 2.21.0+ | Graphics API bindings | **MUST KEEP** - Cross-platform D3D11/Vulkan/Metal bindings, actively maintained |
| Avalonia | 11.3.9+ | UI framework | **MUST KEEP** - Cross-platform XAML UI, already integrated |
| SharpGLTF.Toolkit | 1.0.6+ | Model loading | **MUST KEEP** - glTF 2.0 format support, already integrated |

---

## Test Infrastructure (NEW - Critical)

### Test Framework

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| **xUnit** | 2.9.x | Primary test framework | **RECOMMENDED** - Modern choice for .NET 8+, actively maintained, better analyzers, widely adopted in .NET 6+ ecosystem. Lightweight design, faster evolution than MSTest. |
| Microsoft.NET.Test.Sdk | 17.12.0+ | Test SDK | **REQUIRED** - Base testing platform, required for xUnit and Coverlet integration |

### Alternatives Considered (Test Framework)

| Category | Recommended | Alternative | Why Not |
|----------|-------------|-------------|---------|
| Test Framework | xUnit 2.9.x | NUnit 4.x | xUnit has better .NET 8 integration, more modern design, cleaner async support |
| Test Framework | xUnit 2.9.x | MSTest | MSTest is Microsoft's legacy option, slower evolution, fewer analyzers |

### Mocking & Assertions

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| **Moq** | 4.20.x | Mocking framework | **RECOMMENDED** - Industry standard for .NET mocking, LINQ-style syntax, strong support for generic interfaces |
| **FluentAssertions** | 7.0.x | Assertion library | **RECOMMENDED** - Fluent syntax improves test readability, rich failure messages, better than built-in Assert class |

### Test Data Generation

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| **AutoFixture** | 4.18.x | Test data generation | **OPTIONAL** - Generates test data automatically, reduces boilerplate in unit tests. Add after initial test infrastructure is established |

### Code Coverage

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| **Coverlet** | 6.0.x (coverlet.collector) | Code coverage | **RECOMMENDED** - Cross-platform, works with .NET 8, default coverage tool for .NET Core+ applications. Supports line/branch/method coverage |
| ReportGenerator | 5.4.x | Coverage reports | **OPTIONAL** - Generates HTML coverage reports from Coverlet output. Add when coverage visualization is needed |

### Coverlet Driver Choice

Use `coverlet.collector` (VSTest integration) rather than `coverlet.msbuild` or `coverlet.console`:
- **Why:** Prefer VSTest integration to avoid known MSBuild issues. Native .NET 8 SDK 8.0.112+ support.
- **Command:** `dotnet test --collect:"XPlat Code Coverage"`

### Integration Testing

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| **Respawn** | 6.x | Database/state reset | **OPTIONAL** - For integration tests requiring clean state between tests. Add when integration testing infrastructure is needed |

---

## Logging (NEW - Critical)

### Core Logging

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| **Serilog** | 4.2.x | Structured logging | **RECOMMENDED** - Powerful structured logging, 100+ sinks, advanced filtering, message templates |
| **Serilog.Extensions.Hosting** | 8.0.x | .NET hosting integration | **RECOMMENDED** - Integrates Serilog with Microsoft.Extensions.Logging via `AddSerilog()` |
| **Serilog.Sinks.Console** | 6.x | Console output | **RECOMMENDED** - Development logging, immediate visual feedback |
| **Serilog.Sinks.File** | 6.x | File output | **RECOMMENDED** - Persistent logs, configurable rolling, output templates |

### Serilog Configuration Pattern

```csharp
// Configure Serilog to integrate with Microsoft.Extensions.Logging
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Use with DI
services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
```

### Alternatives Considered (Logging)

| Category | Recommended | Alternative | Why Not |
|----------|-------------|-------------|---------|
| Structured Logging | Serilog | Microsoft.Extensions.Logging only | MEL alone has fewer built-in sinks, less powerful filtering. Serilog via MEL integration gives best of both |
| Sinks | Serilog.Sinks.* | log4net, NLog | Serilog has more modern architecture, better .NET 8 support, active development |

### Removing Console.WriteLine

**Current Issue:** Extensive `Console.WriteLine` debug statements throughout platform backends (CONCERNS.md)

**Approach:**
1. Add Serilog and Serilog.Sinks.Console
2. Replace all `Console.WriteLine` with `_logger.Log*()` calls
3. Remove all debug counters (`_frameDebugCount`, `_setBufferCallCount`, etc.)
4. Use `#if DEBUG` only for truly debug-only code

---

## Documentation (NEW - Critical)

### Documentation Generation

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| **DocFX** | 2.76.x | API documentation | **RECOMMENDED** - Static site generator for .NET, supports XML doc comments, markdown pages, API reference. Community-driven since 2022. |
| XML Documentation Comments | Built-in | API documentation source | **REQUIRED** - Enable `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in all projects |

### DocFX Installation & Usage

```bash
# Install as global tool
dotnet tool install -g docfx

# Initialize project
docfx init -y

# Build and serve locally
docfx build docfx_project/docfx.json --serve
```

### Documentation Requirements

**Before DocFX can generate documentation:**
1. Add XML documentation comments to all public APIs
2. Enable documentation generation in csproj files:
   ```xml
   <GenerateDocumentationFile>true</GenerateDocumentationFile>
   <NoWarn>$(NoWarn);1591</NoWarn>  <!-- Optional: suppress missing XML comment warnings -->
   ```

### Alternatives Considered (Documentation)

| Category | Recommended | Alternative | Why Not |
|----------|-------------|-------------|---------|
| API Docs | DocFX | Sandcastle, Sandcastle Help File Builder | DocFX is actively maintained, .NET Foundation supported, modern output |
| Static Site | DocFX | Hugo, Jekyll | DocFX has native .NET API documentation support |

---

## Static Analysis & Code Quality (NEW - Important)

### Analyzers

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| **Roslyn Analyzers** | Latest | Built-in code analysis | **RECOMMENDED** - Built into Visual Studio 2022 and .NET 8 SDK, real-time analysis, zero config |
| **Microsoft.CodeAnalysis.NetAnalyzers** | 9.x | .NET-specific rules | **RECOMMENDED** - Extended rules for .NET APIs, patterns, and security |
| **SonarAnalyzer.CSharp** | 10.x | Code quality & security | **RECOMMENDED** - Additional code quality rules, bug detection, security hotspots |

### Analyzer Configuration

Add to projects via `Directory.Build.props` or individual csproj files:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="9.0.0" PrivateAssets="all" />
  <PackageReference Include="SonarAnalyzer.CSharp" Version="10.0.0" PrivateAssets="all" />
</ItemGroup>
```

### Style Enforcement

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| **EditorConfig** | Built-in | Code style consistency | **RECOMMENDED** - .editorconfig file for consistent formatting across IDEs and editors |

### Alternatives Considered (Static Analysis)

| Category | Recommended | Alternative | Why Not |
|----------|-------------|-------------|---------|
| Analyzers | Roslyn + NetAnalyzers | ReSharper | ReSharper is paid product, Roslyn is built-in and free |
| Security | SonarAnalyzer.CSharp | Security Code Scan | SonarAnalyzer is more actively maintained, broader rule set |

---

## Cross-Platform Testing Strategy (NEW - Important)

### Platform Testing Approach

Given the requirement to support Windows (D3D11), Linux (Vulkan), and macOS (Metal) equally:

**Strategy 1: Matrix Testing (Recommended for future CI)**

When CI/CD is added, use GitHub Actions matrix strategy:

```yaml
strategy:
  matrix:
    os: [windows-latest, ubuntu-latest, macos-latest]
    dotnet: ['8.0.x']
runs-on: ${{ matrix.os }}
```

**Strategy 2: Local Build Verification (Current Phase)**

Since CI/CD is out of scope, use local platform testing:
- Windows developers verify Windows + software rendering
- Linux developers verify Linux + X11/Wayland detection
- macOS developers verify macOS + Metal

### Platform-Specific Test Considerations

| Platform | Test Focus | Mocking Strategy |
|----------|------------|------------------|
| Windows | D3D11 backend, shader compilation | Mock native D3D11 interfaces for unit tests |
| Linux | Vulkan backend, X11/Wayland | Mock Vulkan handles for unit tests |
| macOS | Metal backend, Objective-C interop | Mock Metal objects for unit tests |

### Graphics Testing

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| **Software Backend** | Existing | Platform-agnostic testing | **USE FOR TESTING** - Software renderer allows testing graphics logic without native GPU dependencies |
| **Headless Rendering** | To implement | CI-friendly testing | **FUTURE** - Implement headless rendering mode for automated testing |

---

## Supporting Libraries (Existing - Keep)

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| System.Numerics.Vectors | Built-in | Vector math | **MUST KEEP** - Hardware-accelerated vector operations, SIMD support |
| Microsoft.Extensions.DependencyInjection | 9.0.11+ | IoC container | **MUST KEEP** - Already integrated, standard .NET DI |
| Microsoft.Extensions.Hosting | 9.0.11+ | Application hosting | **MUST KEEP** - Generic host support, lifetime management |
| CommunityToolkit.Mvvm | 8.2.1+ | MVVM helpers | **MUST KEEP** - ViewModel base classes, commands, already integrated |

---

## Installation

### Testing

```bash
# Core testing packages
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Microsoft.NET.Test.Sdk --version 17.12.0

# Mocking and assertions
dotnet add package Moq --version 4.20.0
dotnet add package FluentAssertions --version 7.0.0

# Code coverage
dotnet add package coverlet.collector --version 6.0.0

# Optional: Test data generation
dotnet add package AutoFixture --version 4.18.0
dotnet add package AutoFixture.Xunit2 --version 4.18.0
```

### Logging

```bash
# Core Serilog packages
dotnet add package Serilog --version 4.2.0
dotnet add package Serilog.Extensions.Hosting --version 8.0.0

# Sinks
dotnet add package Serilog.Sinks.Console --version 6.0.0
dotnet add package Serilog.Sinks.File --version 6.0.0
```

### Documentation

```bash
# DocFX as global tool
dotnet tool install -g docfx

# Enable XML documentation in csproj:
# <GenerateDocumentationFile>true</GenerateDocumentationFile>
```

### Static Analysis

```bash
# Add to project (or Directory.Build.props for all projects)
dotnet add package Microsoft.CodeAnalysis.NetAnalyzers --version 9.0.0
dotnet add package SonarAnalyzer.CSharp --version 10.0.0
```

---

## Version Summary (Quick Reference)

| Category | Package | Version |
|----------|---------|---------|
| **Testing** | xUnit | 2.9.x |
| | Microsoft.NET.Test.Sdk | 17.12.0+ |
| | Moq | 4.20.x |
| | FluentAssertions | 7.0.x |
| | AutoFixture | 4.18.x (optional) |
| | coverlet.collector | 6.0.x |
| **Logging** | Serilog | 4.2.x |
| | Serilog.Extensions.Hosting | 8.0.x |
| | Serilog.Sinks.Console | 6.0.x |
| | Serilog.Sinks.File | 6.0.x |
| **Documentation** | DocFX | 2.76.x |
| **Analysis** | Microsoft.CodeAnalysis.NetAnalyzers | 9.0.x |
| | SonarAnalyzer.CSharp | 10.0.x |
| **Existing** | .NET 8 | 8.0.x (keep) |
| | Silk.NET | 2.21.0+ (keep) |
| | Avalonia | 11.3.9+ (keep) |
| | SharpGLTF.Toolkit | 1.0.6+ (keep) |

---

## Migration Priority

### Phase 1: Critical Infrastructure (MUST DO)
1. **Testing** - xUnit + Moq + FluentAssertions + Coverlet
   - Rationale: Zero test coverage is the highest risk area
   - Enables: Safe refactoring of complex graphics code

2. **Logging** - Serilog + Serilog.Extensions.Hosting
   - Rationale: Replace all Console.WriteLine debug statements
   - Enables: Production-ready logging, removes debug counters

### Phase 2: Code Quality (IMPORTANT)
3. **Static Analysis** - NetAnalyzers + SonarAnalyzer
   - Rationale: Catch bugs early, enforce consistent code style
   - Enables: Safer refactoring, better code quality

4. **Documentation** - DocFX + XML comments
   - Rationale: Open source requires documentation
   - Enables: API documentation site, contributor onboarding

### Phase 3: Advanced Features (OPTIONAL)
5. **Test Data** - AutoFixture
   - Rationale: Reduce test boilerplate
   - Enables: Faster test writing

6. **Coverage Reports** - ReportGenerator
   - Rationale: Visualize coverage trends
   - Enables: Coverage tracking over time

---

## Sources

### Testing (HIGH Confidence)
- [Best .NET 8 Testing Libraries: The Complete Guide for Developers](https://oussamasaidi.com/en/net-8-testing-libraries-complete-guide-for-developers/)
- [NET Unit Testing: Best Practices, Frameworks, and Tools in 2025](https://scand.com/company/blog/net-unit-testing/)
- [Microsoft Learn - Unit Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
- [NUnit vs XUnit vs MSTest: Differences Between Them](https://dev.to/kevinwalker/nunit-vs-xunit-vs-mstest-differences-between-them-33mm)

### Code Coverage (HIGH Confidence)
- [Coverlet GitHub Repository](https://github.com/coverlet-coverage/coverlet)
- [Coverlet for .NET: The Definitive 2026 Guide to Code Coverage](https://itnext.io/coverlet-for-net-the-definitive-2026-guide-to-code-coverage-4a95ed15a1b7)
- [Microsoft Learn - Use code coverage for unit testing](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage)

### Logging (HIGH Confidence)
- [Serilog Official Documentation](https://serilog.net/)
- [Logging And Structured Logging With Serilog The Definitive Guide](https://saigontechnology.com/blog/logging-and-structured-logging-with-serilog-the-definitive-guide/)
- [Serilog.Extensions.Logging NuGet](https://www.nuget.org/packages/Serilog.Extensions.Logging/)
- [Structured Logging in .NET 8 Isolated: A Comprehensive Guide](https://yisusvii.medium.com/structured-logging-in-net-8-isolated-a-comprehensive-guide-3da16ce62e4b)

### Documentation (HIGH Confidence)
- [DocFX GitHub Repository](https://github.com/dotnet/docfx)
- [DocFX NuGet](https://www.nuget.org/packages/docfx/)

### Static Analysis (HIGH Confidence)
- Search results confirmed Microsoft.CodeAnalysis.NetAnalyzers and SonarAnalyzer.CSharp as current best practices for .NET 8

### Cross-Platform (MEDIUM Confidence)
- Cross-platform testing strategy based on standard .NET practices, limited by search rate limiting during research
- Recommendations align with GitHub Actions and .NET 8 documentation patterns

---

*Stack research: 2026-03-28*
*Open source preparation phase*
