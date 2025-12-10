Shader "ZZZ/Instanced_Legion"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1,1,1,1)
        // 🔧 新增：全局缩放控制 (默认 1.0)
        _Scale ("Global Scale", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // 1. 开启 GPU Instancing
            #pragma multi_compile_instancing
            // 2. 开启 Procedural Setup (自定义位置)
            #pragma instancing_options procedural:setup 

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                // 🔑 必须加这个！否则 Setup 无法获取 ID
                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _BaseColor;
            
            // 🔧 新增：缩放变量
            float _Scale;

            // 粒子数据结构 (必须与 C# 对应)
            struct ParticleData
            {
                float4 position;
                float4 color;
            };
            
            // 只读缓冲区
            StructuredBuffer<ParticleData> _ParticleBuffer;

            // --- ⚙️ 核心魔法：摆放模型 ---
            void setup()
            {
                #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                    // 1. 获取当前士兵的 ID
                    uint id = unity_InstanceID;
                    
                    // 2. 读数据
                    ParticleData data = _ParticleBuffer[id];
                    float3 pos = data.position.xyz;

                    // 3. 构建【模型 -> 世界】矩阵 (包含缩放和位移)
                    // 矩阵结构：
                    // Scale  0      0      PosX
                    // 0      Scale  0      PosY
                    // 0      0      Scale  PosZ
                    // 0      0      0      1
                    
                    unity_ObjectToWorld._11_21_31_41 = float4(_Scale, 0, 0, 0);
                    unity_ObjectToWorld._12_22_32_42 = float4(0, _Scale, 0, 0);
                    unity_ObjectToWorld._13_23_33_43 = float4(0, 0, _Scale, 0);
                    unity_ObjectToWorld._14_24_34_44 = float4(pos.x, pos.y, pos.z, 1);

                    // 4. 构建【世界 -> 模型】逆矩阵 (数学修正版)
                    // Inverse(M) = Inverse(Scale) * Inverse(Translate)
                    float invScale = 1.0f / _Scale;
                    
                    unity_WorldToObject._11_21_31_41 = float4(invScale, 0, 0, 0);
                    unity_WorldToObject._12_22_32_42 = float4(0, invScale, 0, 0);
                    unity_WorldToObject._13_23_33_43 = float4(0, 0, invScale, 0);
                    // 逆矩阵的位移项 = -Pos * (1/Scale)
                    unity_WorldToObject._14_24_34_44 = float4(-pos.x * invScale, -pos.y * invScale, -pos.z * invScale, 1);
                #endif
            }

            v2f vert (appdata v)
            {
                v2f o;
                
                // 5. 初始化 ID (必须调用！)
                UNITY_SETUP_INSTANCE_ID(v); 
                
                // 此时 unity_ObjectToWorld 已经被 setup() 替换，可以直接使用
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                o.vertex = vertexInput.positionCS;
                o.uv = v.uv;
                
                #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                    uint id = unity_InstanceID;
                    o.color = _ParticleBuffer[id].color; // 读取 Compute Shader 算的颜色
                #else
                    o.color = float4(1,1,1,1);
                #endif
                
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                // 混合 基础色 * 贴图 * 粒子颜色
                return col * _BaseColor * i.color; 
            }
            ENDHLSL
        }
    }
}