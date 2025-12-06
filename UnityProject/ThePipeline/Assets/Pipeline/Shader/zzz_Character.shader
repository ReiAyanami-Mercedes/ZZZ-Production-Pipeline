Shader "ZZZ/Character_Base"
{
    Properties
    {
        // --- 属性面板：这里的东西会显示在 Unity 材质球上 ---
        [MainTexture] _BaseMap ("Base Map (Color)", 2D) = "white" {}
        [MainColor]   _BaseColor ("Base Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        // --- 标签：告诉 Unity 这是个什么类型的物体 ---
        Tags 
        { 
            "RenderType" = "Opaque"           // 不透明物体
            "RenderPipeline" = "UniversalPipeline" // 必须是 URP 管线！
            "Queue" = "Geometry"              // 渲染顺序
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" } // 指定这是主渲染 Pass

            HLSLPROGRAM
            // 声明我们要写顶点(vert)和片元(frag)着色器
            #pragma vertex vert
            #pragma fragment frag

            // --- 引入 URP 核心库 (没这些就没法渲染) ---
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // --- 1. 输入结构 (从模型拿数据) ---
            struct Attributes
            {
                float4 positionOS   : POSITION; // 模型空间位置
                float2 uv           : TEXCOORD0; // UV
                float3 normalOS     : NORMAL;    // 法线 (用来算光照)
            };

            // --- 2. 传输结构 (顶点算完给片元) ---
            struct Varyings
            {
                float4 positionCS   : SV_POSITION; // 屏幕位置
                float2 uv           : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;   // 世界空间法线
            };

            // --- 3. 变量声明 (对应 Properties) ---
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST; // 贴图的缩放偏移
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            // --- 顶点着色器 (Vertex Shader) ---
            // 负责把模型的点，画到屏幕正确的位置上
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // 1. 计算顶点位置 (MVP变换)
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;

                // 2. 计算法线 (转到世界空间)
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                output.normalWS = normalInput.normalWS;

                // 3. 传递 UV
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

                return output;
            }

            // --- 片元着色器 (Fragment Shader) ---
            // 负责给每个像素上色
            half4 frag(Varyings input) : SV_Target
            {
                // A. 采样贴图 (读取衣服/皮肤的颜色)
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;

                // B. 简单的光照计算 (Lambert)
                // 获取主光源 (太阳)
                Light mainLight = GetMainLight();
                float3 lightDir = mainLight.direction;
                float3 normal = normalize(input.normalWS);
                
                // N dot L (法线 点乘 光方向) -> 算出受光强度
                // 结果是 -1(背光) 到 1(向光)。
                // 我们把它映射到 0.5 到 1.0，这样背光面不会死黑，这叫 Half-Lambert，二次元常用的底色。
                float NdotL = dot(normal, lightDir);
                float halfLambert = NdotL * 0.5 + 0.5;

                // C. 最终组合
                // 颜色 = 贴图颜色 * 光照强度 * 灯光颜色
                half3 finalColor = texColor.rgb * halfLambert * mainLight.color;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}