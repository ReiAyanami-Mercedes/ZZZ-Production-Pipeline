using UnityEngine;

// =================================================================
// 🎮 ZZZ-Pipeline Module C: 渲染控制器 (Runtime Controller)
// -----------------------------------------------------------------
// 核心思想：在游戏运行时，读取 Profile 数据，并实时应用到材质上。
// =================================================================

[ExecuteAlways] // 让它在编辑模式下也能跑！方便预览！
public class CharacterRenderController : MonoBehaviour
{
    [Header(" 数据驱动核心")]
    public CharacterRenderProfile renderProfile; // 插槽：放入你的配置文件

    [Header(" 目标渲染器")]
    public Renderer targetRenderer; // 插槽：放入角色的 MeshRenderer

    // 缓存 Shader 属性 ID (性能优化：DOD 思维，避免字符串查找)
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ShadowColorId = Shader.PropertyToID("_ShadowColor");
    private static readonly int ShadowSmoothId = Shader.PropertyToID("_ShadowSmoothness");
    private static readonly int RimColorId = Shader.PropertyToID("_RimColor");
    private static readonly int RimPowerId = Shader.PropertyToID("_RimPower");

    // 每一帧都更新 (为了演示方便，实际项目中可以用事件驱动)
    void Update()
    {
        if (renderProfile == null || targetRenderer == null) return;

        // 1. 获取所有材质球 (是个数组！)
        Material[] mats = targetRenderer.sharedMaterials;

        // 2. 遍历每一个材质球，挨个发指令！
        foreach (Material mat in mats)
        {
            if (mat != null && mat.shader.name.Contains("ZZZ/Uber")) // 只改 Uber Shader
            {
                mat.SetColor(BaseColorId, renderProfile.BaseColorTint);
                mat.SetColor(ShadowColorId, renderProfile.ShadowColor);
                mat.SetFloat(ShadowSmoothId, renderProfile.ShadowSmoothness);
                mat.SetColor(RimColorId, renderProfile.RimColor);
                mat.SetFloat(RimPowerId, renderProfile.RimPower);
            }
        }
    }
}