# 📝 ZZZ-Pipeline 开发日志 (Day 13)
**日期：** 2025-12-11
**当前版本：** v2.0-alpha (Infrastructure Phase)
**作战代号：** **Operation Ironclad (铁甲行动)**
**核心目标：** 在低算力环境下，构建自动化测试框架与数据驱动架构。

---

### 🔧 一、 自动化质量保障 (Automated QA)
**任务目标：** 建立 TDD (测试驱动开发) 流程，确保资产管线逻辑的健壮性。

1.  **单元测试框架集成 (NUnit Integration):**
    *   引入 **Unity Test Runner** 模块。
    *   解决了 **Assembly Definition (AsmDef)** 的引用隔离问题（最终采用 `Editor` 文件夹的特殊规则打通了测试脚本与主工程的连接）。
    *   解决了 `UnityEngine.Assertions` 与 `NUnit.Framework.Assert` 的命名空间冲突，统一使用 **NUnit** 标准。

2.  **资产守门人测试 (Asset Processor Tests):**
    *   编写了 `ZZZAssetProcessorTests.cs`。
    *   **核心逻辑：** 模拟创建一个带 `_sdf` 后缀的临时贴图 -> 触发导入器 -> 断言 (Assert) 检查 `sRGB` 属性是否自动关闭 (Linear Space)。
    *   **成果：** 实现了“无人工干预”的管线逻辑验证。

---

### 🎛️ 二、 工具链与可视化 (Toolchain & Visualization)
**任务目标：** 抛弃旧式 IMGUI，使用最新的 UI Toolkit 构建可视化管线控制台。

1.  **驾驶舱构建 (Cockpit Window):**
    *   创建 `ZZZPipelineWindow` (继承自 `EditorWindow`)。
    *   使用 **UI Toolkit (CreateGUI)** 替代 `OnGUI`，提升了编辑器性能与可扩展性。
    *   实现了基础 UI 布局：标题、系统状态监控 (System Status)、以及预留的功能按钮。

2.  **编码规范治理 (Encoding Standardization):**
    *   遭遇 VS 默认 GB2312 编码导致的 Inspector 中文/Emoji 乱码问题。
    *   **解决方案：** 确立了项目级编码规范，强制统一为 **UTF-8 (with signature)**。
    *   **清理工作：** 移除了代码中不兼容的特殊 Unicode 字符，确保跨平台/跨IDE的兼容性。

---

### 🧠 三、 数据驱动架构 (Data-Oriented Architecture)
**任务目标：** 解耦渲染逻辑与配置数据，实现 DOD (面向数据设计)。

1.  **全局渲染配置 (Global Render Settings):**
    *   创建 `GlobalRenderSettings` (**ScriptableObject**)。
    *   **设计哲学：** 将散落在 Shader/Material 中的参数（阴影色、SDF阈值、描边宽）收敛到一个数据资产中。
    *   **优势：** 支持热更新，策划/美术可独立配置，无需触碰代码。

2.  **运行时管理器 (Runtime Manager):**
    *   重构 `ZZZRenderManager` (单例模式)。
    *   **核心技术：** 使用 `Shader.SetGlobalColor/Float` 直接修改 GPU 全局变量区。
    *   **性能优化：** 使用 `Shader.PropertyToID` 缓存属性 ID，避免 Update 循环中的 String Hashing 开销。

3.  **代码重构 (Refactoring):**
    *   修正了变量命名规范（PascalCase -> **camelCase**）。
    *   修复了命名空间缺失问题 (`using ZZZ.Runtime`)。
    *   解决了 `ScriptableObject` 资源与 `class` 定义的对应冲突。

---

### 🩹 四、 问题修复记录 (Troubleshooting)
*   [FIXED] **Namespace Error:** `ZZZRenderManager` 无法识别 `GlobalRenderSettings` -> *Solution: 添加 `using ZZZ.Runtime;`*。
*   [FIXED] **Ambiguous Reference:** `Assert` 指向不明 -> *Solution: 显式调用 `NUnit.Framework.Assert`*。
*   [FIXED] **Encoding Warning:** Unity 报 Error Code 4 或 Unicode 警告 -> *Solution: 转码 UTF-8 并清理 Emoji*。
*   [PENDING] **Rim Light Integration:** 边缘光逻辑暂时注释屏蔽，待 V2.1 阶段 Shader 联调时恢复。

---

