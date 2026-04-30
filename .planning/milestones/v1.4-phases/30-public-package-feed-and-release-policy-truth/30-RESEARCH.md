# Phase 30 Research

## Goal

把公开稳定安装入口收敛到 nuget.org，并把 GitHub Packages 降到 preview/internal 通道。

## Findings

1. 旧的 publish-nuget workflow 名称与真实目标不一致，会误导维护者和消费者。
2. README 与 package readmes 需要同样讲 public vs preview 的 feed truth。
3. release-policy 文档是 feed 边界的长期合同。
