# Phase 193 Context

Phase `193` executes the minimum code slice for `v2.15`: close the shipped native-path direct-lighting gap without widening the runtime model.

Key context:

- Windows/D3D11 and macOS/Metal already consume the existing style-driven direct-lighting uniform contract.
- The remaining native-path gap is the Vulkan viewer pipeline, which did not bind or consume the style uniform for normal static-scene rendering.
- Software fallback remains out of scope for this phase; the phase only closes the native-path contract gap.
- The phase must stay bounded to the existing style/uniform seam and avoid introducing a generic lighting system, new runtime light abstractions, or broader renderer breadth.
