Shader "ZZZ/Character_CelShaded"
{
    Properties
    {
        // --- 基础属性 ---
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor]   _BaseColor ("Base Color", Color) = (1,1,1,1)
        
        // --- 卡通渲染专用设置 ---
        [Header(Cel Shading Settings)]
        _ShadowColor ("Shadow Color", Color) = (0.7, 0.6, 0.8, 1) // 默认给个淡紫色阴影，比黑色好看！
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.5 // 阴影面积：越小阴影越少
        _ShadowSmoothness ("Shadow Smoothness", Range(0.001, 0.1)) = 0.02 // 边缘软硬：越小越硬
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Geometry" 
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // 引入 URP 核心库
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
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                // 新增的变量
                float4 _ShadowColor;
                float _ShadowThreshold;
                float _ShadowSmoothness;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            Varyings vert(Attributes input)
            {
                Varyings output;
                // 顶点位置变换
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                // 法线变换
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                output.normalWS = normalInput.normalWS;
                // UV 变换
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. 采样贴图颜色
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;

                // 2. 光照准备
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 normal = normalize(input.normalWS);

                // 3. 计算半兰伯特 (Half-Lambert)
                // 把光照范围从 -1~1 映射到 0~1，让背光面不那么死黑
                float NdotL = dot(normal, lightDir);
                float halfLambert = NdotL * 0.5 + 0.5;

                // 4. --- 核心：二次元阶梯化 (Cel Shading) ---
                // 使用 smoothstep 把平滑的过渡变成“硬边缘”
                // 如果光照强度 < 阈值，就显示阴影色；如果 > 阈值，就显示亮面色
                float ramp = smoothstep(_ShadowThreshold - _ShadowSmoothness, _ShadowThreshold + _ShadowSmoothness, halfLambert);

                // 5. 混合颜色
                // 亮面是白色(1,1,1)，阴影面是 _ShadowColor
                float3 lightIntensity = lerp(_ShadowColor.rgb, float3(1,1,1), ramp);

                // 6. 最终合成
                // 贴图颜色 * 光照强度 * 灯光颜色
                half3 finalColor = texColor.rgb * lightIntensity * mainLight.color;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
