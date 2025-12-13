# 📝 ZZZ-Pipeline V2.5 开发日志 (Day 01)
**日期：** 2025-12-13
**里程碑：** v2.5-alpha (Native Awakening)
**开发周期：** V2.5 Day 01
**核心代号：** **Operation Crossing the Bridge (跨越桥梁)**

---

### 🏛️ 一、 架构演进：原生互操作性 (Native Interoperability)

**战略目标：** 为了突破 Unity C# 层的性能瓶颈与 API 限制，构建一套能够直接调用系统底层资源（内存、显卡驱动）的混合架构。

#### 1.1 C++ 开发环境搭建 (Environment Setup)
*   **工程结构重组:** 在项目根目录下建立了 `NativeSource/` 独立目录，用于管理 Visual Studio C++ 工程，与 Unity `Assets/` 目录物理隔离，保证 Git 结构清晰。
*   **VS 编译管线配置:**
    *   创建 `ZZZ_Native_Core` 动态链接库 (DLL) 项目。
    *   **架构锁定:** 强制配置为 `x64` 平台（适配 Unity Editor 架构）。
    *   **优化级别:** 配置为 `Release` 模式，移除调试符号以追求最大执行效率。
    *   **去噪:** 禁用了“预编译头” (`Precompiled Headers / pch.h`)，确保代码文件的独立性与纯净度，解决 `C1010` 编译错误。

#### 1.2 自动化构建流水线 (Automated Build Pipeline)
*   **痛点:** 每次 C++ 编译后需手动复制 DLL 到 Unity `Plugins` 目录，效率低下且容易出错（DLL 版本不一致）。
*   **解决方案:** 修改 Visual Studio 项目属性中的 **“输出目录 (Output Directory)”**。
    *   *路径:* `$(SolutionDir)..\..\Assets\Plugins\`
*   **成果:** 实现了 **"One-Click Deploy"**。按下 `Ctrl+Shift+B` 即可完成编译、链接、部署全流程，Unity 端只需重新聚焦即可加载最新底层逻辑。

---

### 🧬 二、 核心技术实现：内存指针与软件光栅化

#### 2.1 托管与非托管的握手 (Managed-Native Bridge)
*   **C# 端 (`NativeBridge.cs`):**
    *   使用 `System.Runtime.InteropServices.DllImport` 声明外部函数。
    *   **内存钉住 (Memory Pinning):** 使用 `GCHandle.Alloc(textureData, GCHandleType.Pinned)`。
        *   *技术原理:* 告知 C# 垃圾回收器 (GC) 在当前帧**禁止移动**该数组的内存地址，从而获取一个固定的、安全的物理内存地址 (`IntPtr`) 传递给 C++。
*   **C++ 端 (`NativeEntry.cpp`):**
    *   使用 `extern "C"` 禁用 C++ 的 Name Mangling (名称修饰)，确保函数名在 DLL 导出表中保持原样，使 Unity 能够通过字符串索引找到函数入口。
    *   使用 `__declspec(dllexport)` 标记导出函数。

#### 2.2 软件光栅化原型 (Software Rasterizer Prototype)
*   **功能:** 在 C++ 中直接操作像素内存，绘制动态图形。
*   **实现细节:**
    *   直接访问 `unsigned char*` 原始指针。
    *   通过 `(y * width + x) * 4` 映射二维坐标到一维内存偏移。
    *   实现了基础的图形学算法：UV 归一化、数学函数 (`sin/fabs`) 控制的动态 SDF 图形生成。
*   **验证:** 成功在 Unity 的 `RawImage` 上渲染出了一个由 C++ 全权控制的、高帧率刷新的动态绿色三角形。

---

### 🐛 三、 遇到的挑战与 Debug 记录 (Troubleshooting)

1.  **DLL 锁定问题 (DLL Locking Issue):**
    *   *现象:* Unity 打开时无法覆盖更新 DLL。
    *   *原因:* Unity Editor 加载 DLL 后会持有文件句柄不释放。
    *   *对策:* 确立了开发流规范——修改 C++ 逻辑时若涉及函数签名变更，需重启 Unity；仅修改内部逻辑时，有时可通过刷新解决（或使用将来可能引入的 DLL 热加载插件）。

2.  **C++ 严格类型检查 (Strict Typing):**
    *   *现象:* 报错 `C4244` 或函数重载错误。
    *   *原因:* C++ 标准库中的 `abs()` 在旧标准中仅支持整数。
    *   *修复:* 替换为 `fabs()` (float absolute) 并引入 `<math.h>`。

3.  **入口点未找到 (EntryPointNotFound):**
    *   *现象:* C# 报错找不到函数，尽管 DLL 已更新。
    *   *原因:* Unity 缓存了旧版 DLL 的符号表。
    *   *修复:* 配合自动化构建流程，重启 Unity 强制刷新符号表。

---

### 🔮 四、 下一步计划 (Next Steps)

*   **DirectX 12 上下文劫持:** 尝试在 C++ 中获取 Unity 的 D3D12 Device 指针。
*   **原生渲染:** 尝试跳过 CPU 内存写入，直接在 GPU 显存中分配资源。

---