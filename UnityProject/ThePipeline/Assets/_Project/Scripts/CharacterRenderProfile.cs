using UnityEngine;

// =================================================================
// 🧠 ZZZ-Pipeline Module C: 渲染配置文件 (Data Asset)
// -----------------------------------------------------------------
// 核心思想：数据与逻辑分离。这是美术和策划可以直接编辑的"风格说明书"。
// =================================================================

[CreateAssetMenu(fileName = "NewRenderProfile", menuName = "ZZZ Pipeline/Render Profile")]
public class CharacterRenderProfile : ScriptableObject
{
    [Header("🎨 全局色彩风格")]
    public Color BaseColorTint = Color.white; // 基础色倾向

    [Header("🌑 阴影配置 (SDF Settings)")]
    public Color ShadowColor = new Color(0.6f, 0.5f, 0.6f, 1f); // 阴影色
    [Range(0f, 1f)]
    public float ShadowSmoothness = 0.05f; // 阴影软硬度

    [Header("✨ 边缘光配置 (Rim Light)")]
    [ColorUsage(true, true)] // 开启 HDR 颜色拾取
    public Color RimColor = Color.white;
    [Range(0.1f, 10f)]
    public float RimPower = 4.0f; // 边缘光宽度
}
