Shader "ZZZ/URP_ParticleLit"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0, 0.6, 1, 1) // 默认亮蓝色
    }

    SubShader
    {
        // 🔥 关键标签：告诉 Unity 这是 URP 的地盘
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            // -------------------------------------
            // URP 的核心引用
            // -------------------------------------
            #pragma vertex vert
            #pragma fragment frag

            // 开启 GPU Instancing
            #pragma multi_compile_instancing
            
            // 这一句是为了让 buffer 能在某些平台正常工作
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // -------------------------------------
            // 数据结构
            // -------------------------------------
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 normalWS     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // 粒子数据 (必须与 Compute Shader 一致)
            struct ParticleData
            {
                float4 position; // xyz=pos, w=scale
                float4 velocity;
            };

            // 缓冲区声明
            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
                StructuredBuffer<ParticleData> _ParticleBuffer;
            #endif

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
            CBUFFER_END

            // -------------------------------------
            // 必须有的 Setup 函数
            // -------------------------------------
            void setup()
            {
                #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
                    // 必须存在，虽然里面是空的
                #endif
            }

            // -------------------------------------
            // 顶点着色器 (Vertex)
            // -------------------------------------
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // 初始化 Instancing
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 pos = input.positionOS.xyz;
                float scale = 1.0;

                #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
                    // 从 Buffer 拿数据
                    // unity_InstanceID 是系统分配给每个球的 ID
                    ParticleData p = _ParticleBuffer[unity_InstanceID];
                    
                    // 应用缩放
                    scale = p.position.w;
                    pos = pos * scale;
                    
                    // 应用位移 (把球挪到 Compute Shader 算出来的地方)
                    // 注意：这里是直接加的世界坐标，所以后面不能再乘 Model 矩阵了
                    float3 worldPos = pos + p.position.xyz;
                #else
                    // 如果没跑起来，就默认在原地
                    float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                #endif

                // 转换到裁剪空间 (Clip Space)
                output.positionCS = TransformWorldToHClip(worldPos);

                // 处理法线 (简单的把法线转到世界空间，没考虑旋转，因为是球嘛)
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);

                return output;
            }

            // -------------------------------------
            // 片元着色器 (Fragment)
            // -------------------------------------
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // 1. 获取主光源 (太阳光)
                Light mainLight = GetMainLight();
                float3 lightDir = mainLight.direction;
                float3 lightColor = mainLight.color;

                // 2. 简单的兰伯特漫反射 (Lambert Diffuse)
                // 计算法线和光线的夹角，越垂直越亮
                float nDotL = saturate(dot(input.normalWS, lightDir));

                // 3. 混合颜色
                // 基础色 * 光照强度 * 光颜色
                // 稍微加一点环境光 (0.1)，不然背光面死黑
                float3 finalColor = _BaseColor.rgb * (nDotL + 0.1) * lightColor;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}