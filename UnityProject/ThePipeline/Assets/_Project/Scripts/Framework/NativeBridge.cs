using UnityEngine;
using System.Runtime.InteropServices;

// ---------------------------------------------------------
// 1. 定义数据结构 (必须严丝合缝地对齐 C++ 内存)
// ---------------------------------------------------------
// C++: struct alignas(16) EntityTransform (总大小 48 字节)
[StructLayout(LayoutKind.Sequential)]
public struct NativeEntity
{
    // [Block 1] 16 Bytes
    public float x, y, z;   // position[3]
    public float scale;     // scale

    // [Block 2] 16 Bytes
    public float qx, qy, qz, qw; // rotation[4]

    // [Block 3] 16 Bytes (数据 + 填充)
    public int entityID;    // 4 bytes
    public int isActive;    // 4 bytes

    // ⚠️ 关键细节：C++ 的 alignas(16) 强制把 40 字节补齐到了 48 字节
    // 所以这里必须手动补 8 个字节 (long 是 8 字节)，否则指针偏移会错位！
    private long _padding;
}

public unsafe class NativeBridge : MonoBehaviour
{
    // ---------------------------------------------------------
    // 2. 召唤 DLL (名字已经统一为 ZZZKernel)
    // ---------------------------------------------------------
    private const string DLL_NAME = "ZZZKernel";

    // 实体接口
    [DllImport(DLL_NAME)] private static extern void InitializeEntities(int count);
    [DllImport(DLL_NAME)] private static extern void UpdateEntities(float time);
    [DllImport(DLL_NAME)] private static extern NativeEntity* GetEntityPtr();

    // 热重载接口
    [DllImport(DLL_NAME)] private static extern void UpdateSettings(float speed, float height, float freq);

    // ---------------------------------------------------------
    // 3. Unity Inspector 参数 (支持热重载调节)
    // ---------------------------------------------------------
    [Header("Simulation Config")]
    [Range(100, 20000)]
    public int entityCount = 10000; // 默认一万个！
    public GameObject meshPrefab;   // 这里记得拖一个 Cube Prefab

    [Header("Hot Reload (实时调节 C++ 内核)")]
    [Range(0, 10)] public float waveSpeed = 2.0f;
    [Range(0, 10)] public float waveHeight = 2.0f;
    [Range(0, 2)] public float waveFrequency = 0.1f;

    // ---------------------------------------------------------
    // 4. 内部数据
    // ---------------------------------------------------------
    private NativeEntity* _nativeBuffer;  // 指向 C++ 堆内存的“钥匙”
    private Transform[] _unityTransforms; // Unity 里的表现层对象 (View Layer)
    private bool _isInitialized = false;

    void Start()
    {
        if (meshPrefab == null)
        {
            Debug.LogError("🔴 既然要造世界，你也得给我个砖块啊！请在 Inspector 里拖拽一个 Cube Prefab。");
            return;
        }

        // A. 启动 C++ 内核 (分配内存)
        InitializeEntities(entityCount);

        // B. 拿到内存指针
        _nativeBuffer = GetEntityPtr();

        // C. 生成 Unity 物体 (这是唯一的 GC 耗时，仅一次)
        // 我们把所有方块都放在一个父节点下，保持 Hierarchy 整洁
        var root = new GameObject("Native_World_Root").transform;
        _unityTransforms = new Transform[entityCount];

        Debug.Log($"🚀 正在生成 {entityCount} 个实体...");

        for (int i = 0; i < entityCount; i++)
        {
            var obj = Instantiate(meshPrefab, root);
            _unityTransforms[i] = obj.transform;
        }

        _isInitialized = true;
    }

    void Update()
    {
        if (!_isInitialized || _nativeBuffer == null) return;

        // 1. 【热重载】把面板上的参数每一帧都传给 C++
        // 这样你在 Unity 拖动滑条，C++ 那边立马响应
        UpdateSettings(waveSpeed, waveHeight, waveFrequency);

        // 2. 【内核解算】让 C++ 跑完这一帧的所有逻辑 (Job System)
        UpdateEntities(Time.time);

        // 3. 【零拷贝同步】直接读取 C++ 内存，刷新 Unity 物体
        // 指针运算极快，比 C# 数组快得多，且 0 GC
        NativeEntity* ptr = _nativeBuffer; // 获取基地址

        for (int i = 0; i < entityCount; i++)
        {
            // 这里的 ptr[i] 等价于 *(ptr + i)，是最高效的内存访问方式
            // 直接把 C++ 算好的坐标赋值给 Unity
            _unityTransforms[i].localPosition = new Vector3(ptr[i].x, ptr[i].y, ptr[i].z);

            // 同步旋转 (C++ 算出来的四元数)
            _unityTransforms[i].localRotation = new Quaternion(ptr[i].qx, ptr[i].qy, ptr[i].qz, ptr[i].qw);
        }
    }
}