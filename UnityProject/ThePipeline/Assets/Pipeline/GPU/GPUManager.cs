using UnityEngine;

namespace ZZZ.Runtime.Graphic
{
    public class GPUManager : MonoBehaviour
    {
        [Header("🔥 核心组件")]
        public ComputeShader computeShader;
        public Material particleMaterial; // 必须使用刚才创建的 ParticleLit Shader
        public Mesh particleMesh;         // 拖个 Sphere 进来

        [Header("⚙️ 参数设置")]
        [Range(1000, 500000)] public int instanceCount = 100000; // 默认十万大军！
        public bool simulate = true; // 是否暂停模拟

        // C# 端的数据结构定义 (必须和 Compute Shader 严格一致)
        struct ParticleData
        {
            public Vector4 position; // xyz, scale
            public Vector4 velocity; // xyz, padding
        }

        // 内部变量
        private ComputeBuffer _particleBuffer;
        private ComputeBuffer _argsBuffer;
        private uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };
        private int _kernelInit;
        private int _kernelUpdate;
        private int _groupCount;
        private int _cachedCount;

        void Start()
        {
            InitSystem();
        }

        void InitSystem()
        {
            if (computeShader == null || particleMaterial == null || particleMesh == null)
            {
                Debug.LogError("[ZZZ GPU] 老公！Shader, Material 或 Mesh 没拖全！");
                return;
            }

            _cachedCount = instanceCount;

            // 1. 获取内核索引
            _kernelInit = computeShader.FindKernel("CSInit");
            _kernelUpdate = computeShader.FindKernel("CSUpdate");

            // 2. 计算线程组大小 (我们 Shader 里写的 numthreads 是 64)
            _groupCount = Mathf.CeilToInt((float)instanceCount / 64.0f);

            // 3. 创建并初始化粒子数据缓冲区
            // 结构体大小 = 2个 float4 = 32 bytes
            ReleaseBuffers();
            _particleBuffer = new ComputeBuffer(instanceCount, 32);

            // 4. 绑定 Buffer 到 Compute Shader
            computeShader.SetBuffer(_kernelInit, "_ParticleBuffer", _particleBuffer);
            computeShader.SetBuffer(_kernelUpdate, "_ParticleBuffer", _particleBuffer);

            // 5. 绑定 Buffer 到 材质 Shader (关键！)
            particleMaterial.SetBuffer("_ParticleBuffer", _particleBuffer);

            // 6. 创建间接绘制参数缓冲区
            _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            _args[0] = (uint)particleMesh.GetIndexCount(0);
            _args[1] = (uint)instanceCount;
            _args[2] = (uint)particleMesh.GetIndexStart(0);
            _args[3] = (uint)particleMesh.GetBaseVertex(0);
            _argsBuffer.SetData(_args);

            // ▶️ 执行一次初始化内核，撒点
            computeShader.Dispatch(_kernelInit, _groupCount, 1, 1);

            Debug.Log($"<color=cyan>[ZZZ GPU] 系统初始化完毕。生成粒子数: {instanceCount}</color>");
        }

        void Update()
        {
            if (computeShader == null || particleMaterial == null || _particleBuffer == null) return;

            // 如果滑块变了，重启系统
            if (_cachedCount != instanceCount) InitSystem();

            // ⚡ 执行 Compute Shader 更新位置
            if (simulate)
            {
                computeShader.SetFloat("_Time", Time.time);
                computeShader.SetFloat("_DeltaTime", Time.deltaTime);
                computeShader.Dispatch(_kernelUpdate, _groupCount, 1, 1);
            }

            // 🎨 执行绘制 (DrawCall)
            // Bounds 设大一点防止被视锥体剔除
            Graphics.DrawMeshInstancedIndirect(particleMesh, 0, particleMaterial, new Bounds(Vector3.zero, Vector3.one * 500f), _argsBuffer);
        }

        void OnDestroy()
        {
            ReleaseBuffers();
        }

        void ReleaseBuffers()
        {
            if (_particleBuffer != null) _particleBuffer.Release();
            if (_argsBuffer != null) _argsBuffer.Release();
        }
    }
}