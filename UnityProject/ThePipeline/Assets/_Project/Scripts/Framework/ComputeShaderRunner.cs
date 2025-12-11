using UnityEngine;
using System.Collections.Generic;

// =================================================================
// ⚔️ ZZZ-Pipeline Module A: 安比军团指挥官 (V3.0 - 多部件组装版)
// -----------------------------------------------------------------
// 核心升级：
// 1. 支持 List<BodyPart>，可以把头、身体、头发组装在一起。
// 2. 所有部件共享同一个 _particleBuffer (位置数据)，确保头不会飞出去。
// =================================================================

public class ComputeShaderRunner : MonoBehaviour
{
    [Header("核心组件")]
    public UnityEngine.ComputeShader computeShader;

    // 定义一个部件类
    [System.Serializable]
    public class BodyPart
    {
        public string partName;  // 名字 (比如 Hair)
        public Mesh mesh;        // 模型
        public UnityEngine.Material material; // 材质
        [HideInInspector] public ComputeBuffer argsBuffer; // 绘制参数
    }

    [Header("角色组装 (把安比的各个部件填进去)")]
    public List<BodyPart> characterParts = new List<BodyPart>();

    [Header("军团配置")]
    public int population = 10000;
    public float spread = 100.0f;

    [Header("动态参数")]
    public Color legionColor = Color.white; // 叠加色
    [Range(0.01f, 10f)] public float globalScale = 1.0f;
    [Range(0.1f, 10f)] public float frequency = 1.0f;
    [Range(0.1f, 10f)] public float amplitude = 2.0f;

    struct ParticleData
    {
        public Vector4 position;
        public Color color;
    }

    private ComputeBuffer _particleBuffer;
    private int _kernelHandle;

    void Start()
    {
        if (computeShader == null) return;
        InitBuffers();
    }

    void InitBuffers()
    {
        // 1. 准备位置数据 (所有人共用这一份数据！)
        ParticleData[] data = new ParticleData[population];
        for (int i = 0; i < population; i++)
        {
            Vector2 circle = Random.insideUnitCircle * spread;
            data[i].position = new Vector4(circle.x, 0, circle.y, 1);
            data[i].color = Color.white;
        }

        _particleBuffer = new ComputeBuffer(population, 32);
        _particleBuffer.SetData(data);

        _kernelHandle = computeShader.FindKernel("CSMain");
        computeShader.SetBuffer(_kernelHandle, "ResultBuffer", _particleBuffer);

        // 2. 为每一个身体部件，初始化它自己的参数
        foreach (var part in characterParts)
        {
            if (part.mesh == null || part.material == null) continue;

            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = (uint)part.mesh.GetIndexCount(0);
            args[1] = (uint)population;
            args[2] = (uint)part.mesh.GetIndexStart(0);
            args[3] = (uint)part.mesh.GetBaseVertex(0);
            args[4] = 0;

            part.argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            part.argsBuffer.SetData(args);

            // 关键：把位置数据喂给这个部件的材质球
            part.material.SetBuffer("_ParticleBuffer", _particleBuffer);
        }

        Debug.Log($"⚔️ 军团组装完毕！部件数: {characterParts.Count}, 单位数: {population}");
    }

    void Update()
    {
        if (computeShader == null || _particleBuffer == null) return;

        // 1. GPU 计算位置
        computeShader.SetFloat("_Time", Time.time);
        computeShader.SetVector("_Color", legionColor);
        computeShader.SetFloat("_Frequency", frequency);
        computeShader.SetFloat("_Amplitude", amplitude);
        computeShader.Dispatch(_kernelHandle, Mathf.CeilToInt(population / 64.0f), 1, 1);

        // 2. 循环绘制每一个部件
        foreach (var part in characterParts)
        {
            if (part.mesh == null || part.material == null) continue;

            // 实时同步缩放
            part.material.SetFloat("_Scale", globalScale);

            // 绘制！
            UnityEngine.Graphics.DrawMeshInstancedIndirect(part.mesh, 0, part.material, new Bounds(Vector3.zero, Vector3.one * 10000), part.argsBuffer);
        }
    }

    void OnDestroy()
    {
        if (_particleBuffer != null) _particleBuffer.Release();
        foreach (var part in characterParts)
        {
            if (part.argsBuffer != null) part.argsBuffer.Release();
        }
    }
}