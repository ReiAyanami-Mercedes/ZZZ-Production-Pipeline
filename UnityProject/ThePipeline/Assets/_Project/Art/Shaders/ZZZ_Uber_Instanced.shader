// =======================================================================
// 🌌 ZZZ-Pipeline Module A: 军团专用 Uber Shader (GPU Instanced)
// -----------------------------------------------------------------------
// 结合了 SDF/视差渲染 和 ComputeBuffer 读取功能
// =======================================================================

Shader "ZZZ/Uber_Instanced_Legion"
{
    Properties
    {
        [Header(Core)]
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
        _SDFMap ("SDF Map", 2D) = "white" {}
        _Scale ("Global Scale", Float) = 1.0 // 缩放控制

        [Header(Lighting)]
        _ShadowColor ("Shadow Color", Color) = (0.6, 0.5, 0.6, 1)
        _ShadowSmoothness ("Shadow Softness", Range(0.001, 0.5)) = 0.05
        
        [Header(Rim)]
        [HDR] _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0.1, 10)) = 4.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            Name "ZZZ_Legion_Pass"
            Tags { "LightMode"="UniversalForward" }
            
            Blend Off 
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // 🔥 开启 Instancing 魔法
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID // 身份证
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float4 color : COLOR; // 接收来自 Compute Shader 的颜色
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor, _ShadowColor, _RimColor;
                float4 _MainTex_ST;
                float _ShadowSmoothness, _RimPower, _Scale;
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_SDFMap); SAMPLER(sampler_SDFMap);

            // --- GPU 数据读取 ---
            struct ParticleData { float4 position; float4 color; };
            StructuredBuffer<ParticleData> _ParticleBuffer;

            // --- 核心：手动构建矩阵 ---
            void setup()
            {
                #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                    uint id = unity_InstanceID;
                    ParticleData data = _ParticleBuffer[id];
                    float3 pos = data.position.xyz;
                    float s = _Scale;

                    unity_ObjectToWorld._11_21_31_41 = float4(s, 0, 0, 0);
                    unity_ObjectToWorld._12_22_32_42 = float4(0, s, 0, 0);
                    unity_ObjectToWorld._13_23_33_43 = float4(0, 0, s, 0);
                    unity_ObjectToWorld._14_24_34_44 = float4(pos.x, pos.y, pos.z, 1);

                    float invS = 1.0f / s;
                    unity_WorldToObject._11_21_31_41 = float4(invS, 0, 0, 0);
                    unity_WorldToObject._12_22_32_42 = float4(0, invS, 0, 0);
                    unity_WorldToObject._13_23_33_43 = float4(0, 0, invS, 0);
                    unity_WorldToObject._14_24_34_44 = float4(-pos.x * invS, -pos.y * invS, -pos.z * invS, 1);
                #endif
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input); // 激活 ID

                // 1. 坐标变换 (使用 setup 后的矩阵)
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;

                // 2. 法线变换
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionOS.xyz);
                
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                // 3. 传递粒子颜色 (用于 Debug 或者染色)
                #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                    uint id = unity_InstanceID;
                    output.color = _ParticleBuffer[id].color;
                #else
                    output.color = float4(1,1,1,1);
                #endif

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // A. 基础采样
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _BaseColor;
                float sdf_threshold = SAMPLE_TEXTURE2D(_SDFMap, sampler_SDFMap, input.uv).r;
                
                // B. 光照计算
                float3 normalWS = normalize(input.normalWS);
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                
                float NdotL = dot(normalWS, lightDir);
                float halfLambert = NdotL * 0.5 + 0.5; // 强制半兰伯特

                // C. SDF 阴影
                float shadowMask = smoothstep(sdf_threshold - _ShadowSmoothness, sdf_threshold + _ShadowSmoothness, halfLambert);
                float3 finalDiffuse = lerp(albedo.rgb * _ShadowColor.rgb, albedo.rgb, shadowMask);
                float3 finalColor = finalDiffuse * mainLight.color;

                // D. 边缘光
                float3 viewDirWS = normalize(input.viewDirWS);
                float fresnel = 1.0 - saturate(dot(normalWS, viewDirWS));
                float rim = pow(fresnel, _RimPower);
                finalColor += _RimColor.rgb * rim;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}