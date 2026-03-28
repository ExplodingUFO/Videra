# Domain Pitfalls

**Domain:** .NET 3D Rendering Engine / Cross-Platform Graphics
**Researched:** 2026-03-28

## Critical Pitfalls

Mistakes that cause rewrites or major issues.

### Pitfall 1: Testing Implementation Instead of Behavior

**What goes wrong:** Writing tests that verify internal implementation details (e.g., specific method calls, internal state transitions) rather than observable behavior. This creates brittle tests that break during refactoring even when functionality remains correct.

**Why it happens:** Testing graphics code is difficult—GPU state is hard to assert against, and internal abstractions seem more "testable" than end-to-end rendering results.

**Consequences:**
- Tests break during legitimate refactoring (e.g., changing resource cleanup strategy)
- False sense of security—tests pass but rendering is broken
- Reluctance to improve code architecture due to test maintenance burden

**Prevention:**
- Test through public APIs only (e.g., `CreateBuffer`, `Draw`, `Present`)
- Use black-box testing for graphics operations—verify outputs, not implementation
- For rendering logic: test shader compilation succeeds, resources are created correctly
- Use integration tests that render to offscreen buffers and validate results

**Detection:** Tests require intimate knowledge of internal class structure; test names describe "when X calls Y" rather than "when rendering Z"

**Phase to Address:** Phase 1 (Testing Infrastructure)—establish testing philosophy before writing tests

---

### Pitfall 2: Cross-Platform Assumptions in Platform Backends

**What goes wrong:** Hardcoding platform-specific assumptions (library paths, display server availability, feature support) that break on different distributions or OS versions.

**Why it happens:** Developing on one platform and assuming others work the same; not testing on real target platforms.

**Consequences:**
- Linux backend assumes X11 is always available (breaks on Wayland-only systems)
- macOS backend uses hardcoded Objective-C selectors that fail on OS updates
- Windows backend assumes specific D3D feature level availability
- Silent failures or cryptic native error messages

**Prevention:**
- Use dynamic library loading (`dlopen`/`LoadLibrary`) instead of hardcoded paths
- Implement graceful fallbacks (e.g., software renderer when GPU backend fails)
- Test on real hardware for all three platforms before releases
- Document platform-specific requirements and limitations

**Detection:** Platform-specific code blocks without `#if` guards; hardcoded library paths; no fallback logic

**Phase to Address:** Phase 1 (Build Validation)—verify all platforms build and initialize

---

### Pitfall 3: Unsafe Code Without Bounds Verification

**What goes wrong:** Extensive pointer manipulation without bounds checking leads to memory corruption vulnerabilities and sporadic crashes.

**Why it happens:** Graphics APIs require unsafe code for performance; pressure to optimize hot paths; .NET's safety culture is relaxed in unsafe contexts.

**Consequences:**
- Buffer overflows corrupting memory
- Type confusion vulnerabilities
- Crashes that only occur under specific GPU/driver combinations
- Security vulnerabilities (memory disclosure, code execution)

**Prevention:**
- Add bounds checking before every pointer dereference
- Use `Span<T>` and `Memory<T>` instead of raw pointers where possible
- Create safe wrapper abstractions for unsafe operations
- Run fuzzing tools on buffer handling code
- Document safety invariants for each unsafe block

**Detection:** Direct pointer arithmetic without length checks; cast operations without size validation

**Phase to Address:** Phase 1 (Security Review)—audit all unsafe code before adding tests

---

### Pitfall 4: Debug Code in Hot Paths

**What goes wrong:** Console.WriteLine, debug counters, and conditional debug statements left in release code, particularly in per-frame rendering loops.

**Why it happens:** Quick debugging during development; forgetting to clean up; using debug output as logging.

**Consequences:**
- Performance degradation from console I/O and string formatting (60 FPS = millions of calls/minute)
- Console noise in production applications
- Modulo operations on every frame even when logging is infrequent
- Inconsistent behavior between debug and release builds

**Prevention:**
- Use structured logging (`ILogger`) with configurable levels
- Gate all debug code behind `#if DEBUG`
- Remove debug counters entirely or move to proper profiling tools
- Establish code review checklist for hot path code

**Detection:** `Console.WriteLine` in rendering methods; static debug counters; modulo operations in loops

**Phase to Address:** Phase 1 (Code Cleanup)—remove all debug code before adding features

---

### Pitfall 5: NotImplementedException in Public APIs

**What goes wrong:** Interface methods throw `NotImplementedException` because functionality isn't ready, creating false API surface.

**Why it happens:** Designing complete interfaces before implementing all platforms; wanting to "design APIs first"; pressure to show progress.

**Consequences:**
- Code appears complete via IntelliSense but crashes at runtime
- Unclear which methods are actually supported per platform
- Users discover unsupported features through crashes instead of documentation
- Breaking changes later when removing unimplemented methods

**Prevention:**
- Either implement methods fully or remove from interface until ready
- Use separate interfaces for platform-specific capabilities
- Document platform support clearly in XML docs
- Consider `NotSupportedException` for truly optional features

**Detection:** Method signatures that throw immediately; platform-specific code with NotImplementedException

**Phase to Address:** Phase 1 (API Cleanup)—remove or implement all NotImplementedException methods

---

## Moderate Pitfalls

### Pitfall 1: Generic Exceptions Without Diagnostics

**What goes wrong:** Throwing `Exception()` or `Exception("Failed to X")` without structured diagnostic information.

**Why it happens:** Quick error handling; not considering debugging needs; wanting to avoid exception type proliferation.

**Consequences:**
- Poor error diagnostics—cannot distinguish failure modes
- Difficult to debug platform-specific issues (no HRESULT, no Vulkan error codes)
- No programmatic error handling possible
- Users see cryptic messages with no actionable information

**Prevention:**
- Define domain-specific exception types (`GraphicsInitializationException`, `ShaderCompilationException`)
- Include relevant diagnostics (HRESULT, Vulkan error codes) in exception properties
- Provide user-friendly error messages with remediation steps
- Log full diagnostic details while showing simplified messages to users

**Detection:** Generic `Exception()` throws; error messages without platform-specific diagnostic codes

**Phase to Address:** Phase 1 (Error Handling)—define exception hierarchy before stabilization

---

### Pitfall 2: Per-Frame Allocations in Rendering Loop

**What goes wrong:** Creating new objects, arrays, or strings in per-frame rendering code, causing GC pressure and stuttering.

**Why it happens:** Convenience of LINQ and string interpolation; not profiling allocations; "clean code" patterns in hot paths.

**Consequences:**
- Gen0/Gen1 collections causing frame stuttering
- Unpredictable frame time spikes
- Poor scaling as scene complexity increases
- Difficult to profile due to allocation noise

**Prevention:**
- Pre-allocate buffers and reuse them
- Use `ArrayPool<T>` for temporary arrays
- Avoid LINQ and string operations in hot paths
- Profile with dotMemory or similar tools
- Document which methods are allocation-sensitive

**Detection:** `new` in rendering methods; LINQ in loops; string concatenation in per-frame code

**Phase to Address:** Phase 2 (Performance)—profile allocations before optimization work

---

### Pitfall 3: Missing Resource Cleanup Validation

**What goes wrong:** Native resources (buffers, textures, pipelines) are allocated but not verified to be properly released, causing leaks in long-running applications.

**Why it happens:** .NET's GC masks resource leaks; not testing long-running scenarios; assuming `Dispose()` is enough.

**Consequences:**
- GPU memory exhaustion over time
- Native handle leaks
- Crash after loading/unloading many models
- Difficult to debug without graphics debugging tools

**Prevention:**
- Implement RAII pattern using `SafeHandle` derivatives
- Add resource leak detection in debug builds (track allocations)
- Create integration tests that load/unload resources repeatedly
- Use graphics debugging tools (RenderDoc, PIX) to verify cleanup
- Document expected resource lifetimes

**Detection:** Unmatched create/destroy calls; no `using` statements; manual `Dispose()` without finally blocks

**Phase to Address:** Phase 2 (Resource Management)—implement RAII and leak detection

---

### Pitfall 4: Cross-Platform Shader Inconsistency

**What goes wrong:** Each platform uses different shader source (HLSL, GLSL, MSL) with feature drift, causing rendering differences.

**Why it happens:** Different shading languages; no unified shader system; quick fixes applied to one platform only.

**Consequences:**
- Visual differences between platforms
- Features work on Windows but not Linux/macOS
- 3x maintenance burden for shader changes
- Difficult to ensure consistent lighting/material behavior

**Prevention:**
- Evaluate cross-platform shader solutions (SPIRV-Cross, shaderc)
- Extract common shader metadata to centralized definition
- Add shader compilation to CI/CD
- Create regression tests that capture reference images
- Document platform-specific shader limitations

**Detection:** Inline shader strings in multiple files; different shader versions between platforms

**Phase to Address:** Phase 3 (Shader Unification)—evaluate cross-platform shader tools

---

### Pitfall 5: Inadequate Error Recovery

**What goes wrong:** Errors are logged but not surfaced to users; applications continue in broken state; no clear error boundaries.

**Why it happens:** Gaming culture of "ignore errors and keep going"; not designing error handling upfront; treating all errors as fatal.

**Consequences:**
- Silent failures (black screen, missing models)
- Users don't know why rendering failed
- No way to recover from transient failures (GPU reset, driver crash)
- Poor user experience for graphics issues

**Prevention:**
- Define error recovery strategies for each error type
- Use Result types instead of exceptions for expected failures
- Provide user-facing error messages with troubleshooting steps
- Implement graceful degradation (software renderer fallback)
- Test error paths, not just happy paths

**Detection:** `try-catch` with empty catch blocks; errors only logged to console; no user error UI

**Phase to Address:** Phase 2 (Error UX)—design error handling and user messaging

---

## Minor Pitfalls

### Pitfall 1: Outdated Documentation

**What goes wrong:** XML comments, README, and architecture docs drift from code implementation, misleading contributors and users.

**Why it happens:** Documentation updated separately from code; treating docs as afterthought; not reviewing docs in PRs.

**Consequences:**
- Contributors waste time understanding actual behavior
- Users encounter errors following outdated examples
- Increased support burden
- Loss of trust in documentation

**Prevention:**
- Update XML docs when changing APIs (make it part of definition of done)
- Include docs review in PR checklist
- Generate API docs from XML comments automatically
- Document what IS supported, not just what isn't
- Add "version added" tags to API docs

**Detection:** XML comments describing different behavior than implementation; README examples that don't compile

**Phase to Address:** Phase 4 (Documentation)—make doc maintenance part of workflow

---

### Pitfall 2: Implicit Platform Dependencies

**What goes wrong:** Code subtly depends on platform-specific behavior (endianess, path separators, line endings) not obvious from reading.

**Why it happens:** Developing primarily on one platform; not considering differences; assuming .NET abstracts everything.

**Consequences:**
- Bugs that only appear on non-development platforms
- Path handling failures on macOS/Linux
- Confusing error messages from platform-specific code
- Contributors can't test fixes without target hardware

**Prevention:**
- Use `Path.Combine`, `Path.DirectorySeparatorChar` consistently
- Avoid hardcoded path separators
- Test file I/O on all platforms
- Document platform-specific behavior in XML docs
- Use `#if` guards for truly platform-specific code

**Detection:** Hardcoded `\` or `/` in paths; byte order assumptions; platform API calls without guards

**Phase to Address:** Phase 1 (Cross-Platform)—audit platform assumptions early

---

### Pitfall 3: Monolithic Test Projects

**What goes wrong:** Single test project containing unit, integration, and E2E tests mixed together, making slow test suite.

**Why it happens:** Quick project setup; not distinguishing test types; default template structure.

**Consequences:**
- Slow test execution (minutes instead of seconds)
- Developers skip running tests due to slowness
- Difficult to run quick unit tests during development
- CI/CD takes longer than necessary

**Prevention:**
- Separate test projects by type (UnitTests, IntegrationTests, E2ETests)
- Use test traits/categories for selective execution
- Make unit tests fast (< 0.1s each)
- Run integration tests separately or in parallel
- Document test execution strategy

**Detection:** Test project name doesn't indicate type; tests take > 5 minutes to run all

**Phase to Address:** Phase 1 (Test Structure)—design test organization upfront

---

## Phase-Specific Warnings

| Phase Topic | Likely Pitfall | Mitigation |
|-------------|---------------|------------|
| **Phase 1: Testing** | Testing implementation instead of behavior | Establish testing philosophy; use black-box approach; document test strategy |
| **Phase 1: Build** | Cross-platform assumptions fail on real hardware | Test on all three platforms; use dynamic library loading; implement fallbacks |
| **Phase 1: Cleanup** | Debug code in hot paths persists | Remove all Console.WriteLine; use structured logging; gate debug code with `#if DEBUG` |
| **Phase 1: Security** | Unsafe code without bounds checking | Audit all unsafe blocks; add bounds verification; use Span<T> where possible |
| **Phase 2: Performance** | Per-frame allocations cause stuttering | Profile with memory tools; pre-allocate buffers; use ArrayPool<T> |
| **Phase 2: Resources** | GPU resources leak without verification | Implement RAII; add leak detection; test long-running scenarios |
| **Phase 2: Errors** | Generic exceptions without diagnostics | Define exception hierarchy; include platform error codes; provide user-friendly messages |
| **Phase 3: Shaders** | Cross-platform shader drift | Evaluate unified shader solution; extract common metadata; add shader tests |
| **Phase 4: Documentation** | Docs drift from implementation | Update XML docs with API changes; include docs in PR review; auto-generate docs |

## Videra-Specific Pitfalls

### Current Codebase Issues to Address

1. **Metal Objective-C Runtime Interop** (CONCERNS.md)
   - **Risk:** No compile-time safety; selector typos crash at runtime
   - **Phase:** Phase 1 (Platform Cleanup)
   - **Action:** Migrate to typed Silk.NET.Metal bindings when available

2. **Linux X11-Only Backend** (CONCERNS.md)
   - **Risk:** Fails on Wayland-only systems; hardcoded library path
   - **Phase:** Phase 1 (Platform Support)
   - **Action:** Add Wayland backend; use dlopen for library loading

3. **Inconsistent Depth Buffer Management** (CONCERNS.md)
   - **Risk:** Different behavior per platform; potential resource leaks
   - **Phase:** Phase 2 (Resource Consistency)
   - **Action:** Extract to interface; document platform differences

4. **Wireframe Color Update Inefficiency** (CONCERNS.md)
   - **Risk:** O(n) allocation per color change; GC pressure
   - **Phase:** Phase 2 (Performance)
   - **Action:** Update in-place; use sub-range updates; push to shader

5. **No GPU Resource Pooling** (CONCERNS.md)
   - **Risk:** Fragmentation; overhead from many small allocations
   - **Phase:** Phase 3 (Scaling)
   - **Action:** Implement BufferPool; pipeline cache; shader bytecode cache

## Sources

### Research Sources

- [Common Open Source Software Testing Mistakes - SourceForge](https://sourceforge.net/blog/common-open-source-software-testing-mistakes/)
- [Best practices for writing unit tests - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
- [Understanding the Full Impact of Breaking Changes - InfoQ](https://www.infoq.com/articles/breaking-changes-are-broken-semver/)
- [Versioning limitations in .NET - Jon Skeet](https://codeblog.jonskeet.uk/)
- [7 Hidden Allocations in C# That Quietly Hurt Performance - Medium](https://medium.com/@anderson.buenogod/7-hidden-allocations-in-c-that-quietly-hurt-performance-fea3074cdd43)
- [Reducing Garbage Collector (GC) Pressure in .NET - dev.to](https://dev.to/adrianbailador/reducing-garbage-collector-gc-pressure-in-net-practical-patterns-and-tools-5al3)
- [Avoid memory allocations and data copies - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/performance/)
- [Contribute to the .NET documentation - Contributor guide](https://learn.microsoft.com/en-us/contribute/content/dotnet/dotnet-contribute)
- [Contributing to .NET for Dummies - Rion Williams](https://rion.io/2017/04/28/contributing-to-net-for-dummies/)
- [NuGet Management Over Transitive Dependency Versions - GitHub](https://github.com/NuGet/Home/discussions/12106)

### Confidence Levels

| Area | Confidence | Notes |
|------|------------|-------|
| Testing Pitfalls | HIGH | Multiple authoritative sources (Microsoft Learn, SourceForge) |
| Cross-Platform Issues | HIGH | Direct observation from Videra codebase + research |
| Unsafe Code Security | HIGH | Well-documented security research + .NET security patterns |
| Performance/GC Pressure | HIGH | Multiple recent sources (2024-2025) + Microsoft docs |
| API Design/Semver | MEDIUM | Research-based, but Videra can break compatibility (greenfield) |
| Documentation Issues | MEDIUM | Common knowledge, fewer specific sources |
| Contributor Experience | MEDIUM | General OSS sources, fewer .NET-specific examples |
