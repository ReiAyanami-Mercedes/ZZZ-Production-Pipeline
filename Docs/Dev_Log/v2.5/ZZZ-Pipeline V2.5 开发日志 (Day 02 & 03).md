# 📝 ZZZ-Pipeline V2.5 深度开发日志 (Day 02 & 03)

**日期：** 2025-12-14
**版本里程碑：** v2.5-alpha (Motion Core Awakening)
**开发周期：** Phase 2 - Native Core Implementation
**核心代号：** **Operation Pulse (脉冲行动)**
**涉及文件：** `MotionCore.h`, `MotionCore.cpp`, `NativeEntry.cpp`, `MotionMatchingSystem.cs`

---

## 🏛️ 一、 核心架构：Native Motion Engine (原生动作引擎)

**战略目标：** 构建 Motion Matching (运动匹配) 的底层 C++ 核心，利用非托管内存 (Unmanaged Memory) 实现数据的零拷贝传输与毫秒级搜索，突破 C# GC 性能瓶颈。

### 1.1 内存布局与数据结构 (Memory Layout & Data Structures)
* **Design Principle (设计原则):** **AoSoA (Array of Structures of Arrays)** 思想的初步实践，优先考虑缓存命中率 (Cache Locality) 和 SIMD 指令集兼容性。
* **`MotionFrame` 结构体设计 (`MotionCore.h`):**
    * **16-Byte Alignment:** 使用 `alignas(16)` 关键字修饰结构体。
        * *目的:* 确保每个 `MotionFrame` 的内存起始地址是 16 的倍数，这是 CPU **AVX/SSE 指令集** 进行并行计算的硬性要求。
        * *布局:* 将原本的 `float3` 向量扩展为 `float4` (增加 `w` 分量作为 Padding)，避免跨缓存行读取 (Cache Line Splitting)，提升读取效率。
    * **Trajectory Prediction:** 存储未来 0.33s, 0.66s, 1.0s 的预测点与方向，作为搜索时的“高权重特征”。
* **`MotionQuery` 结构体设计:**
    * **镜像对齐:** 在 C++ 与 C# 端定义了完全一致的内存布局。
    * **Input Data:** 包含“当前帧的姿态”以及“玩家输入的期望轨迹”，作为搜索算法的输入 Key。

### 1.2 核心算法实现 (Core Algorithms)
* **Procedural Data Generation (程序化数据生成):**
    * *函数:* `CreateTestMotionData`
    * *逻辑:* 在没有 Houdini 真实资产的情况下，利用数学公式 (`sin`/`cos`) 生成标准的 **螺旋线轨迹 (Spiral Trajectory)**。
    * *作用:* 验证从 C++ 堆内存到 Unity Gizmos 的数据通路是否畅通，排除资产导入流程对核心逻辑的干扰。
* **Brute-Force Linear Search (暴力线性搜索):**
    * *函数:* `SearchBestFrame`
    * *逻辑:* 遍历 `MotionDataset` 中的每一帧，计算其特征与 `MotionQuery` 的 **欧几里得距离平方 (Squared Euclidean Distance)**。
    * *Cost Function (代价函数):* `Cost = ||Trajectory_Query - Trajectory_Frame||² + Weight * ||Pose_Query - Pose_Frame||²`
    * *性能考量:* 在数据量 < 5000 帧时，由于内存连续，CPU 的 **预取机制 (Prefetching)** 使得线性扫描速度极快，暂不需要 KD-Tree 优化。

### 1.3 互操作性层 (Interop Layer)
* **DLL Export (动态链接库导出):**
    * 使用 `extern "C"` 防止 C++ Name Mangling，导出 `InitMotionSystem`, `ReleaseMotionSystem`, `QueryMotionSystem` 三大标准接口。
* **C# Unsafe Context (不安全上下文):**
    * *文件:* `MotionMatchingSystem.cs`
    * *技术:* 启用 `unsafe` 关键字，使用 `MotionDataset*` 指针直接访问 C++ 堆内存。
    * *优势:* 实现了 **Zero-Copy (零拷贝)**。Unity 渲染线程直接读取 C++ 计算结果，无需 `Marshal.PtrToStructure` 的昂贵开销，运行时 **GC Allocation 为 0**。

---

## 🐛 二、 深度故障排查记录 (Deep Troubleshooting Log)

### 🔴 Critical Error 1: C++ Narrowing Conversion (收缩转换)
* **错误代码:** `C2397`, `C4244`
* **现象:** 编译失败，提示 `从 double 转换到 float 需要收缩转换`。
* **根因分析:** C++11 标准在列表初始化 (List Initialization `{}`) 时禁止隐式降级转换。标准库函数 `sin()`, `cos()`, `fabs()` 默认返回 `double` (8字节)，而目标容器是 `float` (4字节)。
* **解决方案:** 显式添加 `(float)` 强制类型转换，或改用 `sinf()` 等 float 专用函数。
    ```cpp
    // Fix:
    frame.BonePositions[0] = { 0.5f, (float)fabs(sin(t)), ... };
    ```

### 🔴 Critical Error 2: DLL File Locking (文件锁定)
* **错误代码:** `LNK1104`
* **现象:** Visual Studio 无法写入 `ZZZ_Native_Core.dll`。
* **根因分析:** Unity Editor 进程在后台持续加载并锁定了该 DLL 文件句柄，导致链接器无法覆盖。
* **解决方案:** 确立标准开发工作流 —— **修改 C++ -> 关闭 Unity -> 编译生成 -> 打开 Unity**。

### 🟠 Warning 3: EntryPointNotFound (入口点丢失)
* **现象:** C# 运行时报错 `EntryPointNotFoundException: QueryMotionSystem`。
* **根因分析:** Unity 为了加快启动速度，缓存了旧版 DLL 的符号表 (Symbol Table)。即使 DLL 文件已更新，Unity 内存中的镜像仍是旧版。
* **解决方案:** 重启 Unity Editor 强制刷新内存镜像。

### 🟠 Warning 4: Unsafe Code Restriction (不安全代码限制)
* **错误代码:** `CS0227`
* **现象:** Unity 编译器报错 `不安全代码只会在使用 /unsafe 编译的情况下出现`。
* **解决方案:** 在 `Project Settings` -> `Player` -> `Other Settings` 中勾选 **[x] Allow 'unsafe' Code**。

---

## 📊 三、 验证与性能评估 (Verification)

### 3.1 可视化验证 (Visualization)
* **轨迹渲染:** Unity Scene 窗口成功绘制出 **连续的绿色螺旋线**，证明 `MotionDataset` 内存布局在 C++ 与 C# 之间完全对齐。
* **实时交互:** 拖动目标球 (Target) 时，**红色匹配球 (Matched Frame)** 能够实时、平滑地吸附到螺旋线上距离目标最近的点，证明 `SearchBestFrame` 算法逻辑正确。

### 3.2 性能概览 (Performance Profile)
* **Memory:** 堆内存分配稳定，无 C# 侧的内存泄漏。
* **CPU:** 线性搜索 1000 帧数据的耗时在微秒级 (Microseconds)，远低于 16ms 的帧预算。

---

## 🔮 四、 下一步计划 (Next Steps)

* **[Asset]** 引入 Houdini KineFX 流程，导出真实的 `.bin` 动作数据文件。
* **[Native]** 实现二进制文件加载器 (`LoadMotionData`) 替换程序化生成代码。
* **[Unity]** 将匹配到的 `MotionFrame` 数据应用到角色骨骼上，实现真正的动作驱动。