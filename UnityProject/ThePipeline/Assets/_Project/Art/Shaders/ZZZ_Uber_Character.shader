// =======================================================================
// 🌌 ZZZ-Pipeline Module A: 核心渲染 Shader (Uber V2.8 - 全局接管版)
// -----------------------------------------------------------------------
// [V2.8 Global Control]
// 1. 阴影控制 (Shadow) 移交 GlobalRenderSettings 全局管理。
// 2. 保留 MatCap 高光、视差映射、边缘光等所有 V2.7 功能。
// =======================================================================

Shader "ZZZ/Uber_Character_V2"
{
    // ===================================================================
    // 1. Properties
    // ===================================================================
    Properties
    {
        [Header(Core Textures)]
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _MainTex ("Albedo Map", 2D) = "white" {}
        _SDFMap ("SDF Map (Linear)", 2D) = "white" {}

        [Space(10)]
        [Header(Parallax Eye)]
        _ParallaxMap ("Eye Depth Map (Linear)", 2D) = "black" {}
        _ParallaxScale ("Parallax Depth", Range(0, 1.0)) = 0.05      
        _PupilScale ("Pupil Scale", Range(0.5, 2.0)) = 1.0

        [Space(10)]
        [Header(MatCap Highlight)]
        [Toggle(_USE_MATCAP_HIGHLIGHT)] _UseMatCapHighlight ("Enable MatCap Highlight", Float) = 0
        [HDR] _HighlightColor ("Highlight Color", Color) = (1,1,1,1)
        _MatCapCenterX ("Center X (Calibration)", Range(-1.0, 1.0)) = 0.0
        _MatCapCenterY ("Center Y (Calibration)", Range(-1.0, 1.0)) = 0.0
        _HighlightX ("Pos X (Offset)", Range(-1.0, 1.0)) = 0.1
        _HighlightY ("Pos Y (Offset)", Range(-1.0, 1.0)) = 0.1
        _HighlightSize ("Size", Range(0.0, 0.5)) = 0.05 
        _HighlightSoftness ("Softness", Range(0.001, 0.2)) = 0.02

        [Space(10)]
        [Header(Lighting Settings)]
        [Toggle(_HALF_LAMBERT)] _UseHalfLambert ("Enable Half Lambert", Float) = 1
        
        // ❌ [Modified] 这些属性虽然留在这里为了占位(防止材质球报错)，但 Shader 内部不再使用它们
        _ShadowColor ("Shadow Color (Override by Global)", Color) = (0.6, 0.5, 0.6, 1)
        _ShadowSmoothness ("Shadow Softness (Override by Global)", Range(0.001, 0.5)) = 0.05

        [Space(10)]
        [Header(Rim Light)]
        [Toggle(_RIM_LIGHT)] _UseRimLight ("Enable Rim Light", Float) = 1
        [HDR] _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0.1, 10)) = 4.0
    }

    // ===================================================================
    // 2. SubShader & Pass
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
            
            // 变体开关
            #pragma shader_feature _HALF_LAMBERT
            #pragma shader_feature _RIM_LIGHT
            #pragma shader_feature _USE_MATCAP_HIGHLIGHT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes { float4 p:POSITION; float2 uv:TEXCOORD0; float3 n:NORMAL; float4 t:TANGENT; };
            struct Varyings { float4 p:SV_POSITION; float2 uv:TEXCOORD0; float3 n:TEXCOORD1; float3 v:TEXCOORD2; float3 vTS:TEXCOORD3;};
            
            // --- CBUFFER (材质本地属性) ---
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor, _MainTex_ST, _RimColor; // 移除了 _ShadowColor
                float _RimPower;                           // 移除了 _ShadowSmoothness
                float _ParallaxScale, _PupilScale;
                float4 _HighlightColor;
                float _MatCapCenterX, _MatCapCenterY;
                float _HighlightX, _HighlightY;
                float _HighlightSize, _HighlightSoftness;
                
                // 为了兼容 Properties 块，这里定义但不使用 (或者直接删掉)
                 float4 _ShadowColor;
                 float _ShadowSmoothness;
            CBUFFER_END

            // --- 🔥 全局变量定义 (Global Variables) ---
            // 这些变量由 ZZZRenderManager 统一控制
            float4 _GlobalShadowColor;
            float _GlobalShadowSmoothness;

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_SDFMap); SAMPLER(sampler_SDFMap);
            TEXTURE2D(_ParallaxMap); SAMPLER(sampler_ParallaxMap);

            Varyings vert(Attributes i) {
                Varyings o = (Varyings)0;
                o.p = GetVertexPositionInputs(i.p.xyz).positionCS;
                VertexNormalInputs ni = GetVertexNormalInputs(i.n, i.t);
                o.n = ni.normalWS;
                float3 vWS = GetWorldSpaceNormalizeViewDir(i.p.xyz);
                o.v = vWS;
                float3x3 tbn = float3x3(ni.tangentWS, ni.bitangentWS, ni.normalWS);
                o.vTS = mul(vWS, tbn);
                o.uv = TRANSFORM_TEX(i.uv, _MainTex);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                // === A. 视差映射 (Parallax) ===
                float3 vTS = normalize(i.vTS);
                float depth = SAMPLE_TEXTURE2D(_ParallaxMap, sampler_ParallaxMap, i.uv).g;
                float2 parallaxOffset = vTS.xy * (1.0 - depth) * _ParallaxScale;
                float2 finalUV = i.uv - parallaxOffset; 
                
                // === B. 基础渲染 (Base Rendering) ===
                float3 nWS = normalize(i.n);
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, finalUV) * _BaseColor;
                float sdf_threshold = SAMPLE_TEXTURE2D(_SDFMap, sampler_SDFMap, finalUV).r;
                Light light = GetMainLight();
                float3 lDir = normalize(light.direction);
                float NdotL = dot(nWS, lDir);
                #if _HALF_LAMBERT
                    NdotL = NdotL * 0.5 + 0.5; 
                #else
                    NdotL = saturate(NdotL);
                #endif
                
                // 🔥🔥🔥 [Modified] 使用全局变量 _Global... 🔥🔥🔥
                float shadowMask = smoothstep(sdf_threshold - _GlobalShadowSmoothness, sdf_threshold + _GlobalShadowSmoothness, NdotL);
                float3 finalDiffuse = lerp(albedo.rgb * _GlobalShadowColor.rgb, albedo.rgb, shadowMask);
                
                float3 finalColor = finalDiffuse * light.color;

                #if _RIM_LIGHT
                    float3 vWS = normalize(i.v);
                    float fresnel = 1.0 - saturate(dot(nWS, vWS));
                    float rim = pow(fresnel, _RimPower);
                    finalColor += _RimColor.rgb * rim * albedo.rgb;
                #endif

                // === C. MatCap 高光 ===
                #if _USE_MATCAP_HIGHLIGHT
                    float3 normalVS = TransformWorldToViewDir(nWS);
                    float2 matcapUV = normalVS.xy * 0.5 + 0.5;
                    matcapUV += float2(_MatCapCenterX, _MatCapCenterY);
                    float2 highlightCenter = float2(0.5, 0.5) + float2(_HighlightX, _HighlightY);
                    float dist = length(matcapUV - highlightCenter);
                    float highlightMask = 1.0 - smoothstep(_HighlightSize, _HighlightSize + _HighlightSoftness, dist);
                    finalColor += _HighlightColor.rgb * highlightMask;
                #endif

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}