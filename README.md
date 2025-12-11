# ⚡ ZZZ-Production-Pipeline (V2.0)

> **Current Status:** 🚧 Active Development (Day 13)
> **Branch:** `v2.0-development`
> **Codename:** Industrial Crystal (工业结晶)

![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black?logo=unity)
![RenderPipeline](https://img.shields.io/badge/RenderPipeline-URP-blue)
![Architecture](https://img.shields.io/badge/Architecture-DOD-green)
![Tests](https://img.shields.io/badge/Tests-Passing-success)

## 📖 简介 (Introduction)

**ZZZ-Production-Pipeline V2.0** 是基于 Unity URP 构建的 **二次元风格化渲染工业管线**。
与 V1.0 的纯视觉探索不同，V2.0 致力于解决 **“规模化生产”** 问题。本项目引入了 **DOD (面向数据设计)**、**自动化测试 (TDD)** 以及 **GPU Driven** 渲染技术，旨在构建一套可扩展、高性能、工具完善的次世代卡通渲染方案。

---

## 🏗️ 核心架构 (Core Architecture)

### 🎨 Module A: 渲染奇点 (Visual Singularity)
> *Goal: 极致的二次元面部表现与海量单位渲染能力。*

- [x] **Uber Shader V2:** 基于 SDF 面部阴影、视差遮蔽 (Parallax)、半兰伯特光照。
- [ ] **GPU Driven Legion:** 基于 `ComputeShader` + `DrawMeshInstancedIndirect` 实现 10,000+ 单位同屏 (开发中)。
- [ ] **Stylized Post-Processing:** 风格化后处理 (Bloom, Color Grading)。

### ⚙️ Module B: 工业化工具链 (Industrial Toolchain)
> *Goal: 用自动化工具解放人力，用 UI Toolkit 提升交互体验。*

- [x] **Cockpit Dashboard:** 基于 **UI Toolkit** 构建的管线可视化控制台。
- [x] **Asset Processor:** 自动化的资产导入管线（强制 Linear 空间、SDF 贴图自动识别）。
- [ ] **Art Validator:** 美术资产合规性自动检测工具 (Python/C#)。

### 🧠 Module C: 数据驱动核心 (Data Core)
> *Goal: 逻辑与数据分离，实现热更友好的配置管理。*

- [x] **Global Render Settings:** 基于 `ScriptableObject` 的全局渲染配置中心。
- [x] **Runtime Manager:** 使用 `Shader.SetGlobal` 实现高性能的参数同步。
- [ ] **Profile Switching:** 支持动态切换日夜/室内外渲染配置。

### 🛡️ Module F: 质量保障 (QA & Testing)
> *Goal: 引入 TDD 流程，确保管线逻辑健壮性。*

- [x] **Unit Testing:** 集成 **NUnit** 框架。
- [x] **Automation:** 针对 `AssetProcessor` 的自动化测试用例 (`ZZZAssetProcessorTests`)。

---

## 📅 开发日志 (DevLog)

### Day 13: 基础设施建设 (Infrastructure)
- **Feature:** 引入 Unity Test Runner，完成首个资产管线自动化测试。
- **Feature:** 搭建 UI Toolkit 编辑器窗口框架 (`ZZZPipelineWindow`)。
- **Refactor:** 重构全局渲染配置为 `GlobalRenderSettings` (ScriptableObject)。
- **Fix:** 统一全项目编码格式为 UTF-8，修复命名空间冲突。

### Day 12: 渲染底层重构
- **Tech:** 实现 `ComputeBuffer` 数据结构，为 GPU Instancing 铺路。
- **Shader:** 编写 `Uber_Instanced_Legion` Shader，支持手动矩阵构建。

---

## 🚀 快速开始 (Getting Started)

1. Clone 本仓库 (确保切换到 `v2.0-development` 分支)。
2. 打开 Unity (推荐 2022.3 LTS)。
3. 顶部菜单选择 `ZZZ-Pipeline` -> `Open Control Center` 打开管线控制台。
4. 打开 `Window` -> `General` -> `Test Runner` 运行单元测试。

---

*Made with ❤️ by [Your Name] & The AI Architect.*


---

## 🔮 未来展望 (Future Horizons: V2.5)
**Codename:** Hybrid Core (混合架构)
*目标：突破 C# 脚本层的性能天花板，构建“Unity + Native C++”的终极形态。*

### 💃 动作之魂：Motion Matching (运动匹配)
> *Status: R&D Phase (预研阶段)*
> *Tech Stack: Houdini KineFX -> Unity DOTS*

- 摒弃传统状态机，采用 **数据驱动** 的姿态搜索算法。
- 构建基于 **Houdini KineFX** 的自动化动捕数据清洗管线。
- 实现基于 **Job System** 的高并发动画解算。

### ⚡ 性能之骨：Native D3D12 Plugin
> *Status: Planned (架构规划中)*
> *Tech Stack: C++ / DirectX 12 / HLSL*

- **Bypass Unity:** 绕过引擎开销，直接通过 C++ DLL 调用底层 Graphics API。
- **Raw Performance:** 实现 `NativePluginBridge`，接管 Compute Shader 的调度。
- **Hello Triangle:** 也就是我们即将挑战的 "The First Triangle" —— 从底层画出的第一个三角形。

---

## 🏆 The GDC Ambition (技术宣言)
本项目不仅仅是一个游戏工程，更是一次对 **“独立游戏工业化”** 的极限探索。
我们的终极目标是将这套管线的开发经验整理成案，剑指 **GDC (Game Developers Conference)** 的 Technical Art / Programming 讲台。

**Core Philosophy:**
1.  **Industrialization:** 用规则对抗混乱，用自动化解放人力。
2.  **Democratization:** 让独立开发者也能驾驭 3A 级的生产管线。
3.  **Aesthetics:** 技术永远服务于艺术表达。

> *"We don't just write code; we forge the tools that build worlds."*