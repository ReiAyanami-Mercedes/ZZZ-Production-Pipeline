📝 Project ZZZ-Pipeline Development Log - Day 11
日期: 2025-12-10 (Day 11)
状态: 🟢 GPU 驱动渲染原型落地 (GPU Driven Prototype Achieved)
主题: Compute Shader, GPU Instancing Indirect, 并行计算架构
1. 核心技术突破 (Technical Breakthroughs)
GPGPU (通用计算) 落地: 成功编写并运行了首个 .compute 着色器 (HelloCompute)，实现了在 GPU 端并行处理 10,000 个 粒子的物理运动（正弦波浮动），彻底解放了 CPU 算力。
GPU Driven Rendering (GPU 驱动渲染): 摒弃了传统的 GameObject 实例化方案，采用 Graphics.DrawMeshInstancedIndirect API。
性能飞跃: 实现了 O(1) CPU 开销 渲染海量物体。无论渲染 1 个还是 10,000 个，CPU 只需要下达一条指令。
Procedural Shader (过程化着色器): 编写了专用 Shader (ZZZ/Instanced_Legion)，利用 setup() 函数和 StructuredBuffer，在顶点着色器阶段直接读取显存数据，手动构建 Local-to-World 矩阵。
2. 架构设计与实现 (Architecture Implementation)
数据结构对齐 (Memory Alignment): 严格遵循 HLSL 与 C# 的内存对齐规则 (Stride = 32 bytes)，确保数据在 CPU 与 GPU 之间无损传输。
数据驱动控制 (Data-Driven Control): 将渲染参数（如 LegionColor）暴露给 C# Inspector，实现了 运行时实时调参，验证了 DOD 架构下的灵活性。
3. 踩坑与故障排除 (Troubleshooting)
故障 A：命名空间二义性 (Ambiguity Error)
现象: 编译器报错 ComputeShaderRunner 定义重复。
解决: 这是一个典型的“CV 工程师之殇”。通过 IDE 全局搜索，删除了重复定义的脚本文件，并规范了类名。
故障 B：Shader 变体丢失 (Instance ID Missing)
现象: Shader 报错 no matching 1 parameter function。
解决: 在 Shader 结构体中显式添加 UNITY_VERTEX_INPUT_INSTANCE_ID 宏，为 GPU Instancing 开启了身份识别通道。
故障 C：文件编码灾难 (Encoding Issue)
现象: Compute Shader 报 INVALID_UTF8_STRING。
解决: 识别出这是由复制粘贴引入的隐形字符导致的。采用了“文件重建 + 无菌粘贴”的方案彻底修复。
4. 视觉成果 (Visual Results)
成功在 Unity 编辑器环境下，以 极高帧率 (48+ FPS) 渲染了 10,000 个动态起伏、颜色渐变的球体阵列。
验证了 “海量单位渲染” 的可行性，为后续 V2.0 的 “安比军团” 和 “植被系统” 奠定了坚实的底层基础。
5. 下一步计划 (Next Steps)
多 Mesh 组装: 升级脚本以支持由多个 Sub-Mesh (头、身、发) 组成的复杂角色 Instancing。
底层预研: 开始调研如何将此 Compute 逻辑迁移至 D3D12 Native Plugin，以进一步减少驱动开销。
Note: Today, we didn't just draw pixels; we orchestrated a legion.