Shader "ZZZ/ParticleLit"
{
    Properties
    {
        _Color ("Color", Color) = (0,0.5,1,1) // 默认骚蓝色
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // 🔥 核心指令：使用标准光照模型(Standard)，并启用阴影投射(addshadow)
        // vertex:vert 告诉它我们要自定义顶点位置
        #pragma surface surf Standard addshadow fullforwardshadows vertex:vert
        
        // 🔥 核心指令：启用 GPU Instancing
        #pragma multi_compile_instancing
        
        // 告诉编译器我们要用 Shader Model 4.5 以上特性(StructuredBuffer)
        #pragma target 4.5

        sampler2D _MainTex;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // 与 Compute Shader 对应的数据结构
        struct ParticleData
        {
            float4 position; // xyz=pos, w=scale
            float4 velocity;
        };

        // 只读缓冲区 (从 Compute Shader 拿数据)
        #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
            StructuredBuffer<ParticleData> _ParticleBuffer;
        #endif

        struct Input
        {
            float2 uv_MainTex;
        };

        // ========================================================
        // 顶点着色器：在这里把粒子“瞬移”到正确位置并缩放
        // ========================================================
        void vert(inout appdata_full v)
        {
            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
                // 获取当前实例的 ID
                uint id = unity_InstanceID;
                ParticleData p = _ParticleBuffer[id];

                float3 pos = p.position.xyz;
                float scale = p.position.w;

                // 应用位置和缩放
                // 把模型空间的顶点 * 缩放 + 世界空间位置
                v.vertex.xyz = v.vertex.xyz * scale + pos;
                // 法线也要缩放，不然光照会错
                v.normal = normalize(v.normal); 
            #endif
        }

        // ========================================================
        // 表面着色器：处理材质和光照 (标准的 PBR 流程)
        // ========================================================
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}