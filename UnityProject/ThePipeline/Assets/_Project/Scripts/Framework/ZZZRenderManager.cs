using UnityEngine;
using ZZZ.Runtime; // ✅ 必须引用这个，才能找到 GlobalRenderSettings

// 加上这个，在编辑器里拖动参数也能实时看到效果！
[ExecuteAlways]
public class ZZZRenderManager : MonoBehaviour
{
    // 单例模式 (Singleton)：世界只有一个总管
    public static ZZZRenderManager Instance;

    [Header(" 数据源")]
    public GlobalRenderSettings settings;

    // --- 缓存 ID (DOD 性能优化) ---
    // 注意：这里字符串的名字("_ShadowColor")必须和 Shader 里的 Properties 名字一致
    // 这样 Shader.SetGlobal 才能生效
    private static readonly int Id_ShadowColor = Shader.PropertyToID("_ShadowColor");
    private static readonly int Id_ShadowSmoothness = Shader.PropertyToID("_ShadowSmoothness");

    // 暂存边缘光 ID (以后加回来的时候用)
    // private static readonly int Id_RimColor = Shader.PropertyToID("_RimColor");
    // private static readonly int Id_RimPower = Shader.PropertyToID("_RimPower");

    void OnEnable()
    {
        Instance = this;
    }

    void Update()
    {
        // 如果没有拖入配置文件，就什么都不做，防止报错
        if (settings == null) return;

        // --- 核心魔法：Shader.SetGlobal ---

        // 1. 设置阴影颜色
        // settings.shadowColor (小写) 是我们在新版配置里定义的名字
        Shader.SetGlobalColor(Id_ShadowColor, settings.shadowColor);

        // 2. 设置 SDF 柔和度
        // settings.sdfSmoothness 是新名字
        Shader.SetGlobalFloat(Id_ShadowSmoothness, settings.sdfSmoothness);

        // --- 边缘光部分暂时屏蔽 (注释掉) ---
        // 等明天我们在 GlobalRenderSettings 里把边缘光加回去，再解开这里的注释！
        // Shader.SetGlobalColor(Id_RimColor, settings.outlineColor); 
        // Shader.SetGlobalFloat(Id_RimPower, settings.outlineWidth); 
    }
}