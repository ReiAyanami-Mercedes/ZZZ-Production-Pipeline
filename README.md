# 🌌 ZZZ-Pipeline V1.0 - 工业级风格化渲染管线原型
**A Data-Driven & Automated Pipeline Prototype for Stylized Real-time Rendering.**

---

## 1. 核心理念 (Core Philosophy)
本管线旨在解决现代二次元游戏开发中的两大核心矛盾：
-   **【艺术表现 vs. 技术限制】**：如何通过算法（而非手绘）实现具有体积感、可呼吸的次世代卡通光影。
-   **【协作效率 vs. 资产混乱】**：如何通过自动化工具链，在多人协作中保证数据流的纯净与标准化，降低“人祸”导致的 Bug 率。

本项目不仅仅是一个渲染 Demo，它是一套遵循 **工业化标准**、具备 **架构思维** 的微型生产系统。

---

## 2. 核心模块与技术壁垒 (Core Modules & Features)

### 🎨 A. 渲染核心 (Rendering Core)
*   **SDF 面部光影算法：** 摒弃传统 Step 硬阴影，基于预计算的 **SDF (Signed Distance Field)** 数据图，实现了在动态光照下依然柔和、平滑的面部阴影过渡。
    *   *![SDF效果对比](Docs/Images/SDF_Compare.png)*  <-- (这里换成你的 Before/After 对比图)
*   **Uber Shader 架构：** 将半兰伯特、菲涅尔边缘光等常用渲染技术封装进一个高度可配置的 `Uber_Character_V1.shader`，通过宏开关管理，兼顾效果与性能。

### ⚙️ B. 自动化工具链 (Automation Toolchain)
*   **资产守门人 (AssetProcessor):** 基于 C# `AssetPostprocessor` 开发，实现了资产的 **“导入即标准化”**。
    -   **功能：** 自动识别 `_sdf` 等后缀，强制关闭 sRGB/压缩，从源头杜绝了 **线性工作流 (Linear Workflow)** 中的色彩空间错误。
    -  ![alt text](资产守门员.png)
*   **DCC 资产海关 (Python Validator):** 基于 Maya Python API 开发，实现了导出前的 **强制校验**。
    -   **功能：** 拦截不符合命名规范 (`SK_`/`SM_`) 和性能预算 (面数超标) 的资产，将错误阻挡在 DCC 软件内部。
    -  ![alt text](命名规范.png)

### 🏛️ C. 数据驱动架构 (Data-Driven Architecture)
*   **渲染配置资产化：** 使用 **ScriptableObject** 将渲染参数（阴影色、边缘光强度等）从材质中解耦，变为可独立配置、可热更的 `Render Profile` 资产。
 - ![alt text](ZZZ_Module_C_DataDriven_Architecture-1.png)
*   **可观测性系统 (Observability):** 实现了简易的 **Runtime Profiler**，可在运行时实时监控 FPS、内存等关键性能指标。
    ![alt text](性能监视.png)

---

## 3. 技术栈 (Tech Stack)
-   **引擎:** Unity 2022.3 (URP)
-   **渲染:** HLSL, 色彩科学 (Linear Space, Tone Mapping)
-   **工具:** C# (Editor Extension), Python (Maya API)
-   **DCC:** Houdini (VDB & HDA), Maya
-   **架构思想:** DOD (Data-Oriented Design), 软件工程自动化

---

## 4. 未来展望 (V2.0 Roadmap)
本 V1.0 版本已验证了核心管线的可行性。V2.0 将在以下方向进行深度挖掘：
-   **[渲染]** 引入 **Compute Shader** 与 **视差映射 (Parallax)**，实现更高级的 GPU Driven 特效与眼球渲染。
-   **[动画]** 搭建 **Houdini KineFX** 流程，为 **Motion Matching** 提供自动化数据清洗管线。
-   **[架构]** 探索 **Unity 6** 的 **GPU Driven Rendering** 新特性，并预研 **D3D12 Native Plugin** 的可能性。




















