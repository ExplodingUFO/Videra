# 贡献指南

[English](../../CONTRIBUTING.md) | [中文](CONTRIBUTING.md)

感谢你关注 Videra。本文档说明如何在本仓库中进行开发、验证和提交变更。

## 欢迎的贡献

- Bug 修复
- 文档改进
- 测试补充与稳定性提升
- 平台适配和渲染后端改进
- Demo 体验优化

## 本地启动

```bash
git clone https://github.com/ExplodingUFO/Videra.git
cd Videra
dotnet restore
dotnet build Videra.slnx
dotnet run --project samples/Videra.Demo/Videra.Demo.csproj
```

## 提交前验证

```bash
./verify.sh --configuration Release
pwsh -File ./scripts/verify.ps1 -Configuration Release
```

如改动涉及 Linux 或 macOS 原生宿主，请补充对应平台验证。

## 文档要求

以下变更需要同步更新文档：

- 公共 API 或使用方式变化
- 平台依赖、环境变量或验证命令变化
- 模块新增、弃用或归档

## 相关文档

- [英文贡献指南](../../CONTRIBUTING.md)
- [中文文档导航](index.md)
- [故障排查](troubleshooting.md)
