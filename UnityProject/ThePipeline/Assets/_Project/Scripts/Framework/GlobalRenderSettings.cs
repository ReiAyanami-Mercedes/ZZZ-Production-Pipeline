using UnityEngine;

[CreateAssetMenu(fileName = "GlobalRenderSettings", menuName = "ZZZ Pipeline/Global Settings")]
public class GlobalRenderSettings : ScriptableObject
{
    [Header(" 世界观光影配置")]
    // 阴影颜色 (全局统一，保证画风一致)
    public Color GlobalShadowColor = new Color(0.6f, 0.5f, 0.7f, 1f);

    // 阴影柔和度
    [Range(0.001f, 1f)] public float GlobalShadowSmoothness = 0.05f;

    [Header("☀ 边缘光配置")]
    [ColorUsage(true, true)] public Color GlobalRimColor = Color.white;
    [Range(0.1f, 10f)] public float GlobalRimPower = 4.0f;

    // 甚至可以控制全局的线条粗细
    [Range(0f, 0.05f)] public float GlobalOutlineWidth = 0.02f;
}