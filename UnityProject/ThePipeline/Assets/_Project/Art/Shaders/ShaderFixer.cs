using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class ShaderFixer : EditorWindow
{
    // 这一步会在 Unity 顶部菜单栏增加一个按钮
    [MenuItem("ZZZ-Pipeline/🚑 强制修复 Compute Shader")]
    public static void ForceFix()
    {
        // 1. 确定文件路径 (生成在 Assets 根目录)
        string path = Application.dataPath + "/ParticleSys.compute";

        // 2. 准备绝对纯净的代码 (无任何中文，无任何特殊符号)
        string content =
@"#pragma kernel CSInit
#pragma kernel CSUpdate

struct ParticleData { float4 position; float4 velocity; };
RWStructuredBuffer<ParticleData> _ParticleBuffer;
float _Time; float _DeltaTime;

float rand(float2 co) { return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453); }

[numthreads(64, 1, 1)]
void CSInit(uint3 id : SV_DispatchThreadID) {
    uint idx = id.x;
    float range = 50.0;
    float3 startPos;
    startPos.x = (rand(float2(idx, 1.0)) * 2.0 - 1.0) * range;
    startPos.y = (rand(float2(idx, 2.0)) * 2.0 - 1.0) * range * 0.2 + 5.0; 
    startPos.z = (rand(float2(idx, 3.0)) * 2.0 - 1.0) * range;
    float scale = rand(float2(idx, 4.0)) * 0.8 + 0.2;
    
    _ParticleBuffer[idx].position = float4(startPos, scale);
    _ParticleBuffer[idx].velocity = float4(0,0,0,0);
}

[numthreads(64, 1, 1)]
void CSUpdate(uint3 id : SV_DispatchThreadID) {
    uint idx = id.x;
    ParticleData p = _ParticleBuffer[idx];
    
    float3 pos = p.position.xyz;
    float speed = 0.5;
    float c = cos(speed * _DeltaTime);
    float s = sin(speed * _DeltaTime);
    float3x3 rot = float3x3(c, 0, s, 0, 1, 0, -s, 0, c);
    
    pos = mul(rot, pos);
    pos.y += sin(_Time + idx * 0.1) * 0.02;
    
    p.position.xyz = pos;
    _ParticleBuffer[idx] = p;
}";

        // 3. 强制用 UTF8 (No BOM) 格式写入，杀死所有乱码
        File.WriteAllText(path, content, new UTF8Encoding(false));

        // 4. 刷新 Unity
        AssetDatabase.Refresh();
        Debug.Log("✅✅✅ 修复完成！纯净版 Shader 已生成在 Assets 根目录！");
    }
}