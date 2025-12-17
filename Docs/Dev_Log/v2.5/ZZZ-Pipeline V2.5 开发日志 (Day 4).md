# 📜 ZZZ-Pipeline 技术日志 | Day 4：内核觉醒与架构重塑

**📅 日期**：2025-12-17
**🏷️ 版本**：v2.5 (Native Awakening)
**📍 核心议题**：从“Unity 插件”向“独立计算内核”的架构转型

---

## 🚨 1. 里程碑：架构范式转移 (Paradigm Shift)

**🔴 变更前 (V1.0 - V2.0)**
* **模式**：典型的 Unity Monolithic 模式。
* **痛点**：逻辑与表现深度耦合，大量 C# GC 开销，无法支撑海量实体（Unity Transform 性能瓶颈）。
* **定义**：`MotionCore` 仅仅是一个辅助计算的 DLL 插件。

**🟢 变更后 (V2.5 当前)**
* **模式**：**ECS (Entity Component System) + Zero-Copy Data Driven**。
* **优势**：逻辑在 C++ 堆内存闭环，表现层（Unity）仅作为渲染视口。
* **定义**：**`ZZZKernel`** 升级为独立的高性能计算引擎，Unity 降级为单纯的“播放器”。

---

## 🛠️ 2. 核心实施细节 (Implementation)

### A. 内核层重构 (C++ Kernel)
我们对原有的文件结构进行了彻底的**重命名与职能重划**，以符合引擎级标准：

1.  **`MotionData.h` ➔ `ZZZKernelTypes.h`**
    * **修改点**：引入 `alignas(16)` 进行内存对齐。
    * **复盘理由**：为了适配 CPU Cache Line（缓存行）读取机制，并为未来 SIMD（AVX2 指令集）暴力计算预留通道。如果不常用 `alignas`，后续大规模计算会导致 CPU 跨行读取，性能暴跌。
2.  **`MotionCore.h` ➔ `ZZZKernelAPI.h`**
    * **修改点**：只暴露 `extern "C"` 接口，隐藏内部类。
    * **复盘理由**：实现 **Pimpl (Pointer to Implementation)** 思想的变种，保持 ABI（二进制接口）稳定性。
3.  **`MotionCore.cpp` ➔ `ZZZKernel.cpp`**
    * **修改点**：实现了 `std::vector<EntityTransform>` 线性内存池。
    * **关键代码**：
        ```cpp
        // 确保数据紧凑排列，对 CPU 极其友好
        std::vector<EntityTransform> g_EntityPool; 
        ```

### B. 桥接层重写 (C# Native Bridge)
重写了 `NativeBridge.cs`，核心技术点在于**“零拷贝 (Zero-Copy)”**：

* **指针直访**：不再使用 `Marshal.Copy`（这会产生内存复制开销）。
* **Unsafe Context**：
    ```csharp
    NativeEntity* ptr = _nativeBuffer + i; // 指针算术偏移
    transform.localPosition = new Vector3(ptr->x, ptr->y, ptr->z);
    ```
    * **复盘重点**：这是性能提升 100 倍的关键。C# 直接读取 C++ 的内存地址，中间没有任何中间商赚差价。
* **Padding 陷阱**：C++ 的 `alignas(16)` 导致结构体大小变为 48 字节，而 C# 默认是紧凑的 40 字节。我们在 C# 结构体末尾手动增加了 `private long _padding;` 来对齐。**（这是一个巨大的坑，复盘时务必注意！）**

### C. 自动化构建管线 (Build Pipeline)
为了解决跨设备路径报错问题，我们废弃了绝对路径，采用了 VS 宏：

* **操作**：VS 项目属性 -> 生成后事件 (Post-Build Event)。
* **指令**：
    ```cmd
    copy /y "$(OutDir)$(TargetName).dll" "$(SolutionDir)..\UnityProject\Assets\Plugins\$(TargetName).dll"
    ```
* **复盘价值**：利用 `$(SolutionDir)` 宏和相对路径 `..\`，实现了“代码拷到哪，DLL 就飞到哪”的自动化闭环。

---

## 📂 3. 工程管理与 Git 规范

### A. 目录结构大清洗 (Refactoring)
将项目从混乱状态整理为 **Monorepo（单体仓库）** 结构：

```text
ZZZ-Production-Pipeline/      <-- Git 根目录
├── NativeSource/             <-- C++ 核心
│   ├── ZZZKernel.sln         <-- 解决方案入口 (已归位)
│   └── ZZZKernel/            <-- 源码
├── UnityProject/             <-- Unity 客户端
│   └── Assets/Plugins/       <-- DLL 自动输出地
└── .gitignore                <-- 守门员