Shader "ZZZ/FluxShader_Toon_Global"
{
    Properties { 
        _BaseColor ("Base Color (Local)", Color) = (1,1,1,1) // 粒子自带颜色
    }
    SubShader {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass {
            Name "ToonLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma target 4.5
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // -------------------------------------
            // 1. 数据结构
            // -------------------------------------
            struct ParticleData { float4 pos; float4 vel; };
            // 强制读取 Buffer
            StructuredBuffer<ParticleData> _Buffer; 

            struct Attributes { 
                float4 positionOS : POSITION; 
                float3 normalOS : NORMAL;
                uint instanceID : SV_InstanceID; 
            };

            struct Varyings { 
                float4 positionCS : SV_POSITION; 
                float3 normalWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
            CBUFFER_END

            // 🔥 2. 接收来自 RenderManager 的全局变量
            float4 _GlobalShadowColor;   // 全局阴影色
            float _GlobalSDFThreshold;   // 阴影阈值
            float _GlobalSDFSmoothness;  // 阴影软硬度

            void setup() {} 

            // -------------------------------------
            // 3. 顶点着色器
            // -------------------------------------
            Varyings vert(Attributes input) {
                Varyings output;
                
                uint id = input.instanceID;
                ParticleData p = _Buffer[id];

                float3 pos = input.positionOS.xyz;
                pos = pos * p.pos.w + p.pos.xyz; // 缩放 + 位移

                output.positionCS = TransformWorldToHClip(pos);
                // 法线转世界空间
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);

                return output;
            }

            // -------------------------------------
            // 4. 片元着色器 (二次元卡通渲染逻辑)
            // -------------------------------------
            half4 frag(Varyings input) : SV_Target {
                
                // A. 光照准备
                Light mainLight = GetMainLight();
                float3 lightDir = mainLight.direction;
                float3 lightColor = mainLight.color;
                
                // B. 计算 N dot L (受光面)
                float nDotL = dot(normalize(input.normalWS), lightDir);
                // 把范围从 [-1, 1] 映射到 [0, 1] 方便计算 SDF
                float halfLambert = nDotL * 0.5 + 0.5;

                // C. 卡通阶梯计算 (SDF Logic)
                // 使用 smoothstep 根据全局阈值切分明暗
                // 如果 halfLambert > Threshold，就是亮面，否则是暗面
                float ramp = smoothstep(_GlobalSDFThreshold, _GlobalSDFThreshold + _GlobalSDFSmoothness, halfLambert);

                // D. 颜色混合
                // 亮部 = 粒子本色 * 光色
                float3 litColor = _BaseColor.rgb * lightColor;
                // 暗部 = 粒子本色 * 全局阴影色 (让粒子融入环境！)
                float3 shadowColor = _BaseColor.rgb * _GlobalShadowColor.rgb;

                // 最终颜色 = 在亮部和暗部之间插值
                float3 finalColor = lerp(shadowColor, litColor, ramp);

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}