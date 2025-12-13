
---

# 📂 Project Log: ZZZ-Lite Character Pipeline
**日期:** Day 3 (Afternoon)
**当前阶段:** 风格化面部渲染 (Stylized Face Rendering) & Houdini 程序化流 (Procedural Workflow)
**项目状态:** 🟢 **Milestone Reached (里程碑达成)** - 成功实现 SDF 平滑面部阴影。

---

### ✅ 1. 已完成里程碑 (Achievements)

*   **Houdini 程序化管线 (Houdini Procedural Pipeline):**
    *   [x] **环境搭建:** 配置 Houdini Apprentice 20.5 + SideFX Labs 工具包。
    *   [x] **VDB 流水线:** 构建了基于 `VDB` 的体积平滑工作流，将原始头部模型转化为“光照代理模型 (Shadow Proxy)”。
    *   [x] **粒子重构法:** 针对 MMD 模型拓扑破碎（非闭合 Mesh）的问题，创新性地使用了 `Scatter` (撒点) + `VDB from Particles` 的重构方案，绕过了传统的 `VDB from Polygons` 限制。

*   **渲染技术落地 (Rendering Implementation):**
    *   [x] **法线传递 (Normal Transfer):** 放弃了复杂的贴图烘焙方案，采用 `Attribute Transfer` 节点，直接将平滑后的法线信息 (Vertex Normals) 传递给低模。
    *   [x] **格式规避:** 解决了 Apprentice 版无法导出 FBX 的限制，通过 `.obj` 格式成功将携带自定义法线的模型导入 Unity。
    *   [x] **Unity 验证:** 在 URP 环境下，成功验证了“平滑法线”对 Cel-Shading 阴影边缘的修正作用，消除了鼻翼和嘴角的锯齿阴影。

---

### 🐛 2. 遇到的问题与解决方案 (Issue Tracker)

| ID | 问题描述 (Issue) | 原因分析 (Root Cause) | 解决方案 (Solution) | 状态 |
| :--- | :--- | :--- | :--- | :--- |
| **011** | **Houdini 内存溢出/崩溃** | 原始模型尺寸过大 (160单位) 且 Voxel Size 设置过小 (0.005)，导致体素数量指数级爆炸。 | **参数优化：** 插入 `Transform` 缩小模型，或将 Voxel Size 上调至 `0.05`，配合高迭代次数的 `VDB Smooth` 达到平滑效果。 | ✅ 已解决 |
| **012** | **VDB 转换失败 (空/红)** | MMD 头发模型为单面片 (Open Mesh)，无法计算封闭体积 (Signed Distance)。 | **方案变更：** 改用 **粒子法** (`Scatter` -> `VDB from Particles`)，将模型视为粒子云进行体积化，忽略拓扑缺陷。 | ✅ 已解决 |
| **013** | **贴图烘焙失败 (Baker)** | `Labs Maps Baker` 在学徒版中不稳定，且模型 UV 状态未知，导致输出全黑或报错。 | **技术降级：** 放弃贴图烘焙，改用 **几何体法线替换** (`Attribute Transfer` 节点传递 `N` 属性)，直接导出模型。 | ✅ 已解决 |
| **014** | **无法导出 FBX** | Houdini Apprentice (免费版) 锁定 FBX 导出功能。 | **格式替换：** 使用 **OBJ** 格式导出 (`Right Click` -> `Save Geometry`)，该格式支持法线数据且未被锁定。 | ✅ 已解决 |
| **015** | **Unity 导入模型过大** | OBJ 格式缺失单位信息，Unity 默认按 1单位=1米 处理导致放大 100 倍。 | 在 Unity Inspector 中将 **Scale Factor** 调整为 `0.01`。 | ✅ 已解决 |

---

### 🧩 3. 关键节点逻辑 (Node Graph Snapshot)

本次 Houdini 核心节点流如下：

```mermaid
graph TD
    A[File (Import FBX)] --> B[Blast (Isolate Head)]
    B --> C[PolyFill (Close Holes)]
    C --> D[Scatter (20k points)]
    D --> E[VDB from Particles (Voxel 0.05)]
    E --> F[VDB Smooth (Iterations 100)]
    F --> G[Convert VDB (To Polygons)]
    B --> H[Attribute Transfer (Transfer N)]
    G --> H
    H --> I[ROP Output (Save OBJ)]
```

---

### 📝 个人总结 (Dev Note)

> "There is always a workaround."
> 今天最大的收获不是学会了某个节点，而是学会了 **“变通”**。
> 当 `VDB from Polygons` 失败时，我学会了用粒子重构；当烘焙贴图报错时，我学会了用法线传递；当 FBX 无法导出时，我学会了用 OBJ。
> **结果导向** 是 TA 最重要的思维模式。最终在 Unity 里看到的那个完美阴影，证明了这套“魔改”管线的有效性。

---

快把这个保存成 `Log_Day03_Houdini.md` (或者追加到之前的文档里)，然后 Push 上去！
这篇日志写得太漂亮了，以后面试官问你：“你遇到过什么困难？” 你直接甩出这张表格，瞬间秒杀！😎✨