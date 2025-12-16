using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class FluxGenerator : EditorWindow
{
    [MenuItem("ZZZ-Pipeline/⚡ 生成 Flux 核心文件")]
    public static void GenerateFiles()
    {
        // 1. 生成 Compute Shader (绝对纯净版)
        string computePath = Application.dataPath + "/FluxCore.compute";
        string computeCode =
@"#pragma kernel CSInit
#pragma kernel CSUpdate

struct ParticleData { float4 pos; float4 vel; };
RWStructuredBuffer<ParticleData> _Buffer;
float _Time; float _DeltaTime;

float rand(float2 co) { return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453); }

[numthreads(64,1,1)]
void CSInit(uint3 id : SV_DispatchThreadID) {
    uint idx = id.x;
    float range = 20.0;
    float3 p;
    p.x = (rand(float2(idx, 1.0)) * 2.0 - 1.0) * range;
    p.y = (rand(float2(idx, 2.0)) * 2.0 - 1.0) * range * 0.5 + 5.0;
    p.z = (rand(float2(idx, 3.0)) * 2.0 - 1.0) * range;
    float s = rand(float2(idx, 4.0)) * 0.5 + 0.1;
    _Buffer[idx].pos = float4(p, s);
    _Buffer[idx].vel = float4(0,0,0,0);
}

[numthreads(64,1,1)]
void CSUpdate(uint3 id : SV_DispatchThreadID) {
    uint idx = id.x;
    ParticleData pd = _Buffer[idx];
    float3 p = pd.pos.xyz;
    
    // 螺旋运动
    float speed = 0.5;
    float c = cos(speed * _DeltaTime);
    float s = sin(speed * _DeltaTime);
    float3x3 m = float3x3(c,0,s, 0,1,0, -s,0,c);
    p = mul(m, p);
    p.y += sin(_Time + idx * 0.01) * 0.02;
    
    pd.pos.xyz = p;
    _Buffer[idx] = pd;
}";
        // 强制写入纯净 UTF8
        File.WriteAllText(computePath, computeCode, new UTF8Encoding(false));

        // 2. 刷新资源
        AssetDatabase.Refresh();
        Debug.Log("✅ FluxCore.compute 已生成在 Assets 根目录！");
    }
}