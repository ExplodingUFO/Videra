---
summary_date: 2026-04-16T19:20:00+08:00
phase: 30
plan: 30-01
status: complete
requirements-completed:
  - FEED-01
---

# 30-01 Summary

- 删除旧 publish-nuget 命名，拆成 publish-public 与 publish-github-packages 两条 workflow。
- 公开 workflow 以 tagged release 为中心，preview workflow 保留手动 GitHub Packages 推送。
