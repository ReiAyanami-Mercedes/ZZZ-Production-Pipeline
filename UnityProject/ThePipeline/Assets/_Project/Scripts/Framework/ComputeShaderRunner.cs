using UnityEngine;
using System.Runtime.InteropServices;

namespace ZZZ.Runtime.Graphic
{
    public class ComputeShaderRunner : MonoBehaviour
    {
        [Header("🔥 核心资源 (千万别空着！)")]
        public ComputeShader computeShader; // <--- 记得把 .compute 文件拖回来！
        public Mesh mesh;                   // <--- 记得把 Sphere 拖回来！
        public Material material;           // <--- 记得把 Material 拖回来！

        [Header("🎛️ 实时控制 (随便调)")]
        [Range(100, 500000)]
        public int objectCount = 10000;     // 数量滑块

        [Range(0.01f, 2.0f)]
        public float particleSize = 0.1f;   // 大小滑块 (新增的！)

        // 内部数据
        private ComputeBuffer resultBuffer;
        private ComputeBuffer argsBuffer;
        private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        private int kernelHandle;
        private int groupCount;
        private int currentCount; // 记录当前数量，防止你拖滑块时报错

        void Start()
        {
            // 初始化
            InitBuffers();
        }

        void InitBuffers()
        {
            // 🛑 安全检查：如果东西没拖全，就不跑，防止报错
            if (computeShader == null || mesh == null || material == null)
            {
                Debug.LogError("[ZZZ] 老公！检查一下 Inspector，是不是有东西没拖进去？");
                return;
            }

            kernelHandle = computeShader.FindKernel("CSMain");

            // 记录一下现在的数量
            currentCount = objectCount;

            // 1. 创建数据 Buffer (float4 = 16字节)
            if (resultBuffer != null) resultBuffer.Release();
            resultBuffer = new ComputeBuffer(currentCount, 16);

            // 2. 创建间接绘制参数 Buffer
            if (argsBuffer != null) argsBuffer.Release();
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

            // 设置参数
            args[0] = (uint)mesh.GetIndexCount(0);
            args[1] = (uint)currentCount;
            args[2] = (uint)mesh.GetIndexStart(0);
            args[3] = (uint)mesh.GetBaseVertex(0);
            argsBuffer.SetData(args);

            // 3. 传给 Shader
            computeShader.SetBuffer(kernelHandle, "Result", resultBuffer);
            material.SetBuffer("positionBuffer", resultBuffer);

            // 计算线程组
            uint threadX, threadY, threadZ;
            computeShader.GetKernelThreadGroupSizes(kernelHandle, out threadX, out threadY, out threadZ);
            groupCount = Mathf.CeilToInt((float)currentCount / threadX);
        }

        void Update()
        {
            if (computeShader == null || material == null) return;

            // 🔄 如果你拖动了数量滑块，我们得重启一下 Buffer
            if (currentCount != objectCount)
            {
                InitBuffers();
            }

            // ✨ 实时更新大小 (假设你的 Shader 里有个叫 _Scale 或 _Size 的属性)
            // 试着传这几个常见名字，总有一个是对的
            material.SetFloat("_Scale", particleSize);
            material.SetFloat("_Size", particleSize);
            material.SetFloat("_ParticleSize", particleSize);

            // ⚡ 运行 Compute Shader
            computeShader.SetFloat("Time", Time.time);
            computeShader.Dispatch(kernelHandle, groupCount, 1, 1);

            // 🎨 绘制
            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, new Bounds(Vector3.zero, Vector3.one * 1000.0f), argsBuffer);
        }

        void OnDestroy()
        {
            if (resultBuffer != null) resultBuffer.Release();
            if (argsBuffer != null) argsBuffer.Release();
        }
    }
}