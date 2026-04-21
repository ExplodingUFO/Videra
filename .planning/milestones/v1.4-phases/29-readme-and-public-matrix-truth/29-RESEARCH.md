# Phase 29 Research

## Goal

把 README、package matrix 和 support matrix 收敛成陌生人入口。

## Findings

1. 当前仓库骨架已经标准，主要缺口是 README 与 docs 对 published packages / source-only modules / demos 的边界表达。
2. English 与 Chinese 入口都需要同时更新，否则 repository guards 会继续锁定旧 truth。
3. 最稳妥的切法是先改 README/docs truth，再让 repository tests 反映新的公开 contract。
