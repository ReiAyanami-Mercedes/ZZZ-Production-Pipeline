using UnityEngine;
using System.Collections.Generic;

public class ComputeShaderRunner : MonoBehaviour
{
    [Header("核心组件")]
    public UnityEngine.ComputeShader computeShader;

    [System.Serializable]
    public class BodyPart
    {
        public string name;
        public Mesh mesh;
        public UnityEngine.Material material;
        [HideInInspector] public ComputeBuffer argsBuffer;
    }

    [Header("角色部件组装")]
    public List<BodyPart> characterParts = new List<BodyPart>();

    [Header("设置")]
    public int population = 10000;
    public float spread = 50.0f;
    [Range(0.01f, 10f)] public float globalScale = 1.0f;

    // 🔧 新增：在这里选颜色！
    [Header("军团外观")]
    public Color legionColor = Color.cyan; // 默认青色

    struct ParticleData
    {
        public Vector4 position;
        public Color color;
    }

    private ComputeBuffer _particleBuffer;
    private int _kernelHandle;

    void Start()
    {
        InitBuffers();
    }

    void InitBuffers()
    {
        ParticleData[] data = new ParticleData[population];
        for (int i = 0; i < population; i++)
        {
            Vector2 circle = Random.insideUnitCircle * spread;
            data[i].position = new Vector4(circle.x, 0, circle.y, 1);
            data[i].color = Color.white;
        }

        _particleBuffer = new ComputeBuffer(population, 32);
        _particleBuffer.SetData(data);

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

            part.material.SetBuffer("_ParticleBuffer", _particleBuffer);
            part.material.SetFloat("_Scale", globalScale);
        }

        if (computeShader != null)
        {
            _kernelHandle = computeShader.FindKernel("CSMain");
            computeShader.SetBuffer(_kernelHandle, "ResultBuffer", _particleBuffer);
        }
    }

    void Update()
    {
        if (computeShader == null) return;

        computeShader.SetFloat("_Time", Time.time);

        // 🔧 核心：把面板上的颜色传给 GPU！
        computeShader.SetVector("_LegionColor", legionColor);

        computeShader.Dispatch(_kernelHandle, Mathf.CeilToInt(population / 64.0f), 1, 1);

        foreach (var part in characterParts)
        {
            if (part.mesh == null || part.material == null) continue;

            part.material.SetFloat("_Scale", globalScale);
            UnityEngine.Graphics.DrawMeshInstancedIndirect(part.mesh, 0, part.material, new Bounds(Vector3.zero, Vector3.one * 1000), part.argsBuffer);
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