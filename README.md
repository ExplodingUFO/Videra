# Videra 🧊

![License](https://img.shields.io/badge/license-MIT-blue.svg) ![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey) ![.NET](https://img.shields.io/badge/.NET-8.0-purple)

**Videra** 是一个基于 **Avalonia UI** 和 **Veldrid** 构建的高性能、跨平台 3D 模型查看器组件。

它旨在为 .NET 开发者提供一个轻量级、可嵌入的 3D 视口控件 (`VideraView`)，能够直接在 Avalonia 应用中无缝加载和渲染 3D 内容，同时利用 Metal (macOS)、DirectX (Windows) 和 Vulkan (Linux) 的底层图形能力。

> **当前状态**: 🚧 开发中 (WIP) - 已支持基础模型加载与交互。

## ✨ 特性 (Features)

* **跨平台原生渲染**:
    * 🍏 **macOS**: 完美适配 Apple Silicon (M1/M2/M3)，解决了 `CAMetalLayer` 尺寸同步与深度缓冲 ([0,1] Depth Clip) 问题。
    * 🪟 **Windows**: 支持 DirectX 11 后端。
    * 🐧 **Linux**: 支持 Vulkan 后端。
* **多格式模型支持**:
    * 内置 **SharpGLTF** 支持 `.gltf` / `.glb` (包含顶点色、法线)。
    * 集成 **Assimp** 支持 `.obj`, `.stl`, `.ply` 等通用格式。
* **MVVM 友好**:
    * 通过 `Items` 属性直接绑定 ViewModel 中的 3D 对象列表。
    * 支持属性绑定控制背景色、网格显隐等。
* **交互式轨道相机 (Orbit Camera)**:
    * 平滑的旋转、缩放和平移操作。
    * 支持轴向反转配置。
* **高性能**:
    * 基于 Veldrid 的底层 API 封装，极低的 CPU 占用。
    * 显式管理的 GPU 资源 (VertexBuffer/IndexBuffer)。

## 📸 截图 (Screenshots)

*(在此处放一张你的程序运行截图，例如显示那个红白相间的机械零件)*

> ![App Screenshot](./Assets/screenshot.png)

## 🛠️ 技术栈 (Tech Stack)

* **UI Framework**: [Avalonia UI](https://avaloniaui.net/) (11.x)
* **Graphics API**: [Veldrid](https://github.com/veldrid/veldrid)
* **Math Library**: System.Numerics
* **Importers**: SharpGLTF, AssimpNet

## 🚀 快速开始 (Getting Started)

### 环境要求
* .NET 8.0 SDK
* **macOS 用户**: 需要安装 Assimp (`brew install assimp`)

### 安装依赖

```bash
dotnet restore
```

### 在 XAML 中使用

Videra 的核心是一个名为 `VideraView` 的 Avalonia 控件。

XML

```
<UserControl xmlns="[https://github.com/avaloniaui](https://github.com/avaloniaui)"
             xmlns:controls="using:Videra.Avalonia.Controls"
             x:Class="YourApp.Views.MainView">

    <controls:VideraView 
        Name="Viewport"
        BackgroundColor="#1e1e1e"
        IsGridVisible="True"
        GridColor="#444444"
        Items="{Binding LoadedModels}" />

</UserControl>
```

### 鼠标控制 (Controls)

| **操作**     | **描述**         |
| ------------ | ---------------- |
| **左键拖拽** | 旋转视角 (Orbit) |
| **右键拖拽** | 平移视角 (Pan)   |
| **滚轮滚动** | 缩放视角 (Zoom)  |

## 