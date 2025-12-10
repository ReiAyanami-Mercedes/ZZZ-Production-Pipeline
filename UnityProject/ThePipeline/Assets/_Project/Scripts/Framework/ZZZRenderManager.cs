using UnityEngine;

// 加上这个，在编辑器里拖动参数也能实时看到效果！
[ExecuteAlways]
public class ZZZRenderManager : MonoBehaviour
{
    // 单例模式 (Singleton)：世界只有一个总管
    public static ZZZRenderManager Instance;

    [Header(" 数据源")]
    public GlobalRenderSettings settings;

    // --- 缓存 ID (DOD 性能优化) ---
    private static readonly int Id_GlobalShadowColor = Shader.PropertyToID("_GlobalShadowColor");
    private static readonly int Id_GlobalShadowSmooth = Shader.PropertyToID("_GlobalShadowSmoothness");
    private static readonly int Id_GlobalRimColor = Shader.PropertyToID("_GlobalRimColor");
    private static readonly int Id_GlobalRimPower = Shader.PropertyToID("_GlobalRimPower");

    void OnEnable()
    {
        Instance = this;
    }

    void Update()
    {
        if (settings == null) return;

        // --- 核心魔法：Shader.SetGlobal ---
        // 我们不需要找任何材质球！
        // 我们直接修改 GPU 的"全局变量区"！
        // 场景里 100 万个物体，只要用了对应的 Shader，都会瞬间同步！

        Shader.SetGlobalColor(Id_GlobalShadowColor, settings.GlobalShadowColor);
        Shader.SetGlobalFloat(Id_GlobalShadowSmooth, settings.GlobalShadowSmoothness);
        Shader.SetGlobalColor(Id_GlobalRimColor, settings.GlobalRimColor);
        Shader.SetGlobalFloat(Id_GlobalRimPower, settings.GlobalRimPower);
    }
}