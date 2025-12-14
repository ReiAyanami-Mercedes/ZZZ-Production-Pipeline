using UnityEngine;
using System.Runtime.InteropServices; // 🔥 核心：与 C++ 交互

// =================================================================
// ⚔️ ZZZ-Pipeline Module A: G-ECS Native 驱动器 (V2.5 - C++ Core)
// -----------------------------------------------------------------
// 核心升级：
// 1. 完全移除 ComputeShader 的数据生成部分。
// 2. 将 10,000 个单位的位置计算逻辑，下放到 Native C++ DLL。
// 3. Unity C# 只负责创建内存、传递指针、并将 C++ 修改后的数据上传 GPU。
// =================================================================

public class ComputeShaderRunner : MonoBehaviour
{
    // --- 🔥🔥🔥 C++ 函数声明 🔥🔥🔥 ---
    // 告诉 Unity：去 "ZZZ_Native_Core" 这个 DLL 里找这些函数
    [DllImport("ZZZ_Native_Core")]
    private static extern void InitNativeWorld(System.IntPtr data, int count, float spread, uint seed);

    [DllImport("ZZZ_Native_Core")]
    private static extern void UpdateNativeWorld(System.IntPtr data, int count, float time, float frequency, float amplitude);

    // --- C# 中的数据结构 (必须与 C++ 的 ParticleData 结构和内存对齐方式一致！) ---
    // float[4] 在 C++ 是 4个float，在C#对应Vector4，但为了安全和直接访问，这里用struct模拟
    [StructLayout(LayoutKind.Sequential)] // 确保内存顺序一致
    private struct ParticleData
    {
        public Vector4 position;
        public Vector4 color; // 虽然C++只设了白色，但留着接口
    }

    [Header("渲染配置")]
    public Mesh instanceMesh;
    public Material instanceMaterial;

    [Header("军团配置")]
    public int population = 10000;
    public float spread = 100.0f;

    [Header("动态参数")]
    public float globalScale = 1.0f;
    [Range(0.1f, 10f)] public float frequency = 1.0f;
    [Range(0.1f, 10f)] public float amplitude = 2.0f;

    // --- 内部数据 ---
    private ParticleData[] _cpuData;      // 🔥 CPU 端数组，C++ 会直接修改这块内存
    private GCHandle _cpuDataHandle;      // 🔥 内存句柄，防止 GC 移动 _cpuData
    private System.IntPtr _cpuDataPtr;    // 🔥 指向 _cpuData 内存的原始指针

    private ComputeBuffer _gpuBuffer;     // GPU 端 Buffer，用于渲染
    private ComputeBuffer _argsBuffer;    // 绘制参数 Buffer

    void Start()
    {
        InitBuffers();
    }

    void InitBuffers()
    {
        if (instanceMesh == null || instanceMaterial == null)
        {
            Debug.LogError("❌ 缺少必要的渲染组件！请检查 Inspector 中的 Mesh 和 Material！");
            return;
        }

        // 1. 【C#】创建 CPU 端数组，并获取其内存指针
        _cpuData = new ParticleData[population];
        _cpuDataHandle = GCHandle.Alloc(_cpuData, GCHandleType.Pinned); // 钉住内存
        _cpuDataPtr = _cpuDataHandle.AddrOfPinnedObject();              // 获取指针

        // 2. 【C++ 调用】初始化数据
        // 将内存指针传递给 C++，C++ 会直接在这块内存上写入初始位置
        InitNativeWorld(_cpuDataPtr, population, spread, (uint)Random.Range(0, int.MaxValue)); // 传入随机种子

        // 3. 【C#】创建 GPU 端 ComputeBuffer
        _gpuBuffer = new ComputeBuffer(population, Marshal.SizeOf(typeof(ParticleData)));
        // 将 C++ 初始化好的数据，第一次上传到 GPU
        _gpuBuffer.SetData(_cpuData);

        // 4. 将 GPU Buffer 绑定到材质球
        instanceMaterial.SetBuffer("_ParticleBuffer", _gpuBuffer);

        // 5. 初始化绘制参数 ComputeBuffer (Indirect Args)
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = (uint)instanceMesh.GetIndexCount(0);
        args[1] = (uint)population;
        args[2] = (uint)instanceMesh.GetIndexStart(0);
        args[3] = (uint)instanceMesh.GetBaseVertex(0);

        _argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        _argsBuffer.SetData(args);

        Debug.Log($"⚔️ Native G-ECS 核心已启动！单位数: {population}");
    }

    void Update()
    {
        if (_cpuDataPtr == System.IntPtr.Zero || _gpuBuffer == null) return;

        // 1. 【C++ 调用】更新数据 (所有游戏逻辑计算在这里进行！)
        UpdateNativeWorld(_cpuDataPtr, population, Time.time, frequency, amplitude);

        // 2. 【C#】将 C++ 修改后的 CPU 数据，上传到 GPU
        // 这一步是CPU到GPU的拷贝，如果想优化，未来可以考虑DX12直接写显存
        _gpuBuffer.SetData(_cpuData);

        // 3. 实时同步缩放参数给材质球
        instanceMaterial.SetFloat("_Scale", globalScale);

        // 4. 绘制所有实例
        UnityEngine.Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial,
            new Bounds(Vector3.zero, Vector3.one * (spread + amplitude) * 2), _argsBuffer);
    }

    void OnDestroy()
    {
        // 释放所有资源，防止内存泄漏
        if (_gpuBuffer != null) _gpuBuffer.Release();
        if (_argsBuffer != null) _argsBuffer.Release();
        // 🔥 释放内存句柄，允许 GC 再次管理 _cpuData
        if (_cpuDataHandle.IsAllocated) _cpuDataHandle.Free();
    }
}