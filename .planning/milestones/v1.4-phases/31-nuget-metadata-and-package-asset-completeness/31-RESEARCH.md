# Phase 31 Research

## Goal

给五个公开包补齐 icon、SourceLink、symbols 和 pack validation。

## Findings

1. 当前公共包已有 README/description/tags，但还缺 icon、SourceLink 和 snupkg truth。
2. Directory.Build.props 是最小改动的共享 metadata 注入点。
3. scripts/Validate-Packages.ps1 需要真实校验 icon 与 symbol package，而不是只看 nupkg。
