# Phase 33 Research

## Goal

把 release 页面做成可消费的发行入口，包含 notes 分类、资产和 maintainer runbook。

## Findings

1. 只改 workflow 不足以形成 release surface；还需要 release.yml 和 releasing.md。
2. GitHub generated release notes 可以消费 .github/release.yml 的分类。
3. public workflow 必须同时附带 nupkg/snupkg 资产。
