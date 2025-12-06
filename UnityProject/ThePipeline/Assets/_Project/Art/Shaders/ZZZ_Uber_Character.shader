// =======================================================================
// 🌌 ZZZ-Pipeline Module A: 核心渲染 Shader (Uber V1.2 - 终极注释版)
// -----------------------------------------------------------------------
// 目标：实现半兰伯特光照、SDF面部阴影、菲涅尔边缘光。
// =======================================================================

Shader "ZZZ/Uber_Character_V1"
{
    // ===================================================================
    // 1. 【菜单】(Properties) - 所有暴露给美术的参数
    // ===================================================================
    Properties
    {
        [Header(Core Textures)]
        _BaseColor ("角色主色 (Base Color)", Color) = (1,1,1,1)
        _MainTex ("基础贴图 (Albedo)", 2D) = "white" {}
        _SDFMap ("SDF数据图 (Linear)", 2D) = "white" {} // 核心：SDF 阴影阈值图

        [Space(10)]
        [Header(Lighting Settings)]
        [Toggle(_HALF_LAMBERT)] _UseHalfLambert ("开启 半兰伯特", Float) = 1
        _ShadowColor ("阴影颜色", Color) = (0.6, 0.5, 0.6, 1)
        _ShadowSmoothness ("阴影柔和度", Range(0.001, 0.5)) = 0.05

        [Space(10)]
        [Header(Rim Light)]
        [Toggle(_RIM_LIGHT)] _UseRimLight ("开启 边缘光", Float) = 1
        [HDR] _RimColor ("边缘光颜色 (HDR)", Color) = (1,1,1,1)
        _RimPower ("边缘光宽度 (Power)", Range(0.1, 10)) = 4.0
    }

    // ===================================================================
    // 2. 【渲染通道】(SubShader & Pass) - 定义渲染流程
    // ===================================================================
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "RenderPipeline"="UniversalPipeline" 
            "Queue"="Geometry" 
        }

        Pass
        {
            Name "ZZZ_Forward"
            Tags { "LightMode"="UniversalForward" }
            Blend Off 
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma shader_feature _HALF_LAMBERT
            #pragma shader_feature _RIM_LIGHT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // ===================================================================
            // 3. 【数据结构】(Structs & Buffers) - 定义数据格式
            // ===================================================================
            struct Attributes
            {
                float4 positionOS : POSITION; // 顶点在模型空间的位置
                float2 uv : TEXCOORD0;       // UV 坐标
                float3 normalOS : NORMAL;      // 顶点在模型空间的法线
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION; // 顶点在裁剪空间的位置
                float2 uv : TEXCOORD0;         // UV 坐标
                float3 normalWS : TEXCOORD1;     // 法线在世界空间的方向
                float3 viewDirWS : TEXCOORD2;    // 视线在世界空间的方向
            };
            
            CBUFFER_START(UnityPerMaterial)
                 float4 _BaseColor;
                 float4 _MainTex_ST;
                 float4 _ShadowColor;
                 float4 _RimColor;
                 float _ShadowSmoothness;
                 float _RimPower;
            CBUFFER_END
            
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_SDFMap);  SAMPLER(sampler_SDFMap);

            // ===================================================================
            // 4. 【顶点着色器】(Vertex Shader) - 负责空间变换
            // ===================================================================
            Varyings vert (Attributes input)
            {
                Varyings output = (Varyings)0;
                
                // 获取顶点在世界空间的位置、裁剪空间的位置等
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                
                // 获取顶点法线在世界空间的方向
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                output.normalWS = normalInput.normalWS;
                
                // 获取从摄像机到顶点的视线方向
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                
                // 处理贴图的平铺和偏移
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                return output;

            }

            // ===================================================================
            // 5. 【片元着色器】(Fragment Shader) - 负责光影计算
            // ===================================================================
            half4 frag (Varyings input) : SV_Target
            {
                // A. 【数据准备】
                // ----------------
                // 规范化，确保向量长度为 1，计算才准确
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                
                // 获取场景中的主光源信息
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 lightColor = mainLight.color;
                
                // 采样贴图
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _BaseColor;
                float sdf_threshold = SAMPLE_TEXTURE2D(_SDFMap, sampler_SDFMap, input.uv).r;

                // B. 【光照计算】
                // ----------------
                // NdotL 是光线和法线的点积，值越大，说明越正对着光
                float NdotL = dot(normalWS, lightDir);

                #if _HALF_LAMBERT
                    NdotL = NdotL * 0.5 + 0.5; // 半兰伯特：将 [-1, 1] 映射到 [0, 1]
                #else
                    NdotL = saturate(NdotL);   // 普通兰伯特：将 [-1, 0] 截断为 0
                #endif

                // C. 【SDF 阴影混合】 - 核心壁垒
                // --------------------------------
                // smoothstep(min, max, x) 会在 min 和 max 之间平滑地插值
                float shadowMask = smoothstep(sdf_threshold - _ShadowSmoothness, sdf_threshold + _ShadowSmoothness, NdotL);
                
                // lerp(A, B, t) - 如果 t=0 返回 A，t=1 返回 B
                float3 finalDiffuse = lerp(albedo.rgb * _ShadowColor.rgb, albedo.rgb, shadowMask);
                
                // 乘以主光源颜色
                float3 finalColor = finalDiffuse * lightColor;

                // D. 【边缘光叠加】
                // -----------------
                #if _RIM_LIGHT
                    // 菲涅尔公式：视线和法线越垂直，值越大
                    float fresnel = 1.0 - saturate(dot(normalWS, viewDirWS));
                    float rim = pow(fresnel, _RimPower);
                    
                    // 将边缘光叠加到最终颜色上
                    finalColor += _RimColor.rgb * rim * albedo.rgb;
                #endif
             // ===================================================================
             // 最终返回：将所有计算结果混合，并保证 Alpha 为 1.0 (不透明)
             // ===================================================================
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
