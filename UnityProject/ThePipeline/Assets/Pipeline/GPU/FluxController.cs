using UnityEngine;

public class FluxController : MonoBehaviour
{
    public ComputeShader computeShader; // 拖 FluxCore
    public Material material;           // 拖 Mat_Flux
    public Mesh mesh;                   // 拖 Sphere

    ComputeBuffer buffer;
    ComputeBuffer argsBuffer;
    uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    int count = 10000; // 1万个球

    void Start()
    {
        // 初始化 Buffer
        buffer = new ComputeBuffer(count, 32); // float4 pos, float4 vel
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        // 告诉 Shader
        int kInit = computeShader.FindKernel("CSInit");
        computeShader.SetBuffer(kInit, "_Buffer", buffer);
        computeShader.Dispatch(kInit, Mathf.CeilToInt(count / 64f), 1, 1);

        // 设置 Mesh 参数
        if (mesh != null)
        {
            args[0] = (uint)mesh.GetIndexCount(0);
            args[1] = (uint)count;
            args[2] = (uint)mesh.GetIndexStart(0);
            args[3] = (uint)mesh.GetBaseVertex(0);
            argsBuffer.SetData(args);
        }
    }

    void Update()
    {
        // 每一帧更新位置
        int kUpd = computeShader.FindKernel("CSUpdate");
        computeShader.SetFloat("_Time", Time.time);
        computeShader.SetFloat("_DeltaTime", Time.deltaTime);
        computeShader.SetBuffer(kUpd, "_Buffer", buffer);
        computeShader.Dispatch(kUpd, Mathf.CeilToInt(count / 64f), 1, 1);

        // 渲染
        material.SetBuffer("_Buffer", buffer);
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, new Bounds(Vector3.zero, Vector3.one * 1000), argsBuffer);
    }

    void OnDestroy()
    {
        if (buffer != null) buffer.Release();
        if (argsBuffer != null) argsBuffer.Release();
    }
}