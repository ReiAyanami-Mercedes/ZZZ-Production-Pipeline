Shader "ZZZ/Character_Stockings"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor]   _BaseColor ("Base Color", Color) = (1,1,1,1)
        
        // --- 丝袜专用设置 ---
        [Header(Stockings Settings)]
        _FresnelColor ("Fresnel Color", Color) = (0.1, 0.1, 0.1, 1) // 边缘的丝袜颜色（通常是深色）
        _FresnelPower ("Fresnel Power", Range(0.1, 5)) = 1.0        // 边缘范围
        _Opacity ("Base Opacity", Range(0, 1)) = 0.5                // 整体透明度
        
        // --- 卡通阴影设置 (保持风格统一) ---
        [Header(Cel Shading Settings)]
        _ShadowColor ("Shadow Color", Color) = (0.7, 0.6, 0.8, 1)
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSmoothness ("Shadow Smoothness", Range(0.001, 0.1)) = 0.02
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"      // 告诉 Unity 这是半透明的
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Transparent"           // 在透明队列渲染
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            // --- 关键混合模式 ---
            Blend SrcAlpha OneMinusSrcAlpha  // 开启透明混合
            ZWrite On                        // 开启深度写入 (防止腿部穿帮，这是做角色的秘诀！)

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float3 viewDirWS    : TEXCOORD3; // 我们需要视角方向来算菲涅尔
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                float4 _FresnelColor;
                float _FresnelPower;
                float _Opacity;
                float4 _ShadowColor;
                float _ShadowThreshold;
                float _ShadowSmoothness;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                output.normalWS = normalInput.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                
                // 计算视角方向 (相机在哪？)
                output.viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. 基础采样
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;

                // 2. 准备数据
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 normal = normalize(input.normalWS);
                float3 viewDir = normalize(input.viewDirWS);

                // 3. 卡通光照 (同上)
                float NdotL = dot(normal, lightDir);
                float halfLambert = NdotL * 0.5 + 0.5;
                float ramp = smoothstep(_ShadowThreshold - _ShadowSmoothness, _ShadowThreshold + _ShadowSmoothness, halfLambert);
                float3 lightIntensity = lerp(_ShadowColor.rgb, float3(1,1,1), ramp);

                // 4. --- 黑丝核心算法 (Fresnel) ---
                // 计算视线和法线的夹角 (1 = 面对面，0 = 边缘)
                float NdotV = saturate(dot(normal, viewDir));
                // 反转一下：边缘是 1，中心是 0
                float fresnel = pow(1.0 - NdotV, _FresnelPower);
                
                // 5. 颜色合成
                half3 finalRGB = texColor.rgb * lightIntensity * mainLight.color;

                // 6. 透明度计算
                // 最终透明度 = 贴图原本的透明度 * 基础透明度 + 边缘菲涅尔增强 (边缘更不透)
                // clamp 限制在 0-1 之间
                float finalAlpha = saturate((texColor.a * _Opacity) + fresnel);

                return half4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
}
