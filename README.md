# ⚡ ZZZ-Pipeline V2.5: Native Awakening (原生觉醒)

> **Current Status:** 🏗️ V2.5-alpha (Day 01 - Native Foundation)
> **Branch:** `v2.5-development`
> **Architecture:** Hybrid (Unity Managed C# + Native C++)

![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black?logo=unity)
![Cpp](https://img.shields.io/badge/Native-C%2B%2B17-blue?logo=cplusplus)
![DirectX](https://img.shields.io/badge/Graphics-DirectX12-green)
![Status](https://img.shields.io/badge/Status-Active_R%26D-orange)

## 📖 愿景 (Vision)

**ZZZ-Pipeline V2.5** 标志着本项目从“应用层开发”向“引擎底层开发”的重大跃迁。
在 V2.0 完成了工业化工具链与数据驱动架构的构建后，V2.5 旨在突破 C# 脚本层的性能瓶颈。我们将通过 **Native C++ Plugin** 直接与硬件对话，引入 **DirectX 12** 底层图形能力与 **Motion Matching** 高性能动画解算，打造一套“超越引擎限制”的混合架构管线。

---

## 🏗️ 混合架构概览 (Hybrid Architecture)

本项目采用 **双层架构 (Dual-Layer Architecture)** 设计：

### 1. 🟢 Managed Layer (Unity C#)
> *负责：业务逻辑、工具链、资产管理、高层渲染调度*
*   **Cockpit Dashboard:** 基于 UI Toolkit 的可视化管线控制台 (V2.0 遗产)。
*   **Asset Processor:** 自动化资产导入与验证管线。
*   **Native Bridge:** 负责与底层 DLL 进行互操作 (Interop) 的桥接模块，管理 `IntPtr` 与 `GCHandle`。

### 2. 🔵 Native Layer (Visual Studio C++)
> *负责：高性能计算、底层图形 API 调用、内存直接操作*
*   **Memory Invasion:** 绕过 GC，直接操作纹理与 Buffer 的原始内存。
*   **DirectX 12 Hook (Planned):** 劫持 Unity 图形上下文，实现 Native 级渲染指令提交。
*   **High-Perf Algorithms:** 承载 Motion Matching 搜索算法与大规模 PCG 逻辑。

---

## 🗺️ 路线图 (Roadmap & Milestones)

### 📅 Phase 1: 内存与管线 (Memory & Pipeline) [✅ Current Stage]
*   [x] **Native Environment:** 搭建 VS2022 C++ 开发环境，配置 x64/Release 编译管线。
*   [x] **Automated Build:** 实现 DLL 自动部署到 Unity Plugins 目录 (`Post-Build Events`)。
*   [x] **Memory Interop:** 实现 C# `GCHandle` 内存钉住 (Pinning) 与 C++ 指针访问。
*   [x] **Software Rasterizer:** 在 C++ 端实现软光栅化三角形绘制，验证数据通路畅通。

### 📅 Phase 2: 图形底层 (Graphics & DX12) [🚧 Next Step]
*   [ ] **D3D12 Context Access:** 获取 Unity 的 ID3D12Device 指针。
*   [ ] **Hello Triangle (Hardware):** 使用原生 DX12 API 绘制第一个三角形。
*   [ ] **Shared Resources:** 实现 Unity `ComputeBuffer` 与 C++ 的资源共享。

### 📅 Phase 3: 动作与未来 (Motion & Future) [🔮 Planned]
*   [ ] **Motion Matching:** 基于数据驱动的下一代角色运动系统。
*   [ ] **Data Pipeline:** Houdini KineFX -> AssetProcessor -> Native Binary 格式转换。
*   [ ] **GPU Driven Culling:** 将剔除逻辑完全移交 C++ / Compute Shader。

---

## 🛠️ 技术栈 (Tech Stack)

*   **Engine:** Unity 2022.3 LTS (URP)
*   **Native Core:** Visual Studio 2022 (MSVC v143)
*   **Language:** C# 9.0 / C++ 17
*   **Graphics API:** DirectX 11 / DirectX 12
*   **Tools:** RenderDoc, Unity Profiler

---

## 🚀 快速开始 (Getting Started for Developers)

由于引入了 Native C++ 模块，环境配置比 V2.0 更为严格：

1.  **Clone Repository:**
    ```bash
    git clone -b v2.5-development https://github.com/YourRepo/ZZZ-Pipeline.git
    ```
2.  **Prerequisites:**
    *   Install **Visual Studio 2022**.
    *   Workload: **Desktop development with C++** (必须包含 MSVC 和 Windows SDK)。
3.  **Build Native Plugin:**
    *   Navigate to `NativeSource/ZZZ_Native_Core.sln`.
    *   Open in VS2022.
    *   Select **Release** configuration and **x64** platform.
    *   **Build Solution (Ctrl+Shift+B)**. (DLL will be auto-copied to `Assets/Plugins`).
4.  **Run in Unity:**
    *   Open project in Unity.
    *   Open scene `Scenes/Native_Test`.
    *   Press Play to see the C++ driven software rasterizer.

---

## 📄 目录结构 (Directory Structure)

```text
ZZZ-Pipeline/
├── Assets/
│   ├── Plugins/          # [Auto-Generated] 存放编译好的 ZZZ_Native_Core.dll
│   ├── Scripts/
│   │   └── Framework/    # 包含 NativeBridge.cs
│   └── ...
├── NativeSource/         # [New] C++ 原生项目源代码
│   ├── ZZZ_Native_Core/
│   │   ├── NativeEntry.cpp
│   │   └── ...
│   └── ZZZ_Native_Core.sln
├── Library/
└── README.md
```
