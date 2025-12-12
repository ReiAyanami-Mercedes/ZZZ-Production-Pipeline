using UnityEngine;
using ZZZ.Runtime;

[ExecuteAlways] // 👈 确保这行还在！它让脚本在编辑模式也能跑
public class ZZZRenderManager : MonoBehaviour
{
    public static ZZZRenderManager Instance;

    [Header(" 数据源")]
    public GlobalRenderSettings settings;

    // 🔥🔥🔥 核心修正点：注意字符串里的 _Global 前缀！！！ 🔥🔥🔥
    private static readonly int Id_ShadowColor = Shader.PropertyToID("_GlobalShadowColor");
    private static readonly int Id_ShadowSmoothness = Shader.PropertyToID("_GlobalShadowSmoothness");

    void OnEnable()
    {
        Instance = this;
    }

    void Update()
    {
        if (settings == null) return;

        // 发送全局指令
        Shader.SetGlobalColor(Id_ShadowColor, settings.shadowColor);
        Shader.SetGlobalFloat(Id_ShadowSmoothness, settings.sdfSmoothness);

        // --- 🔍 调试代码 (验证脚本是否在运行) ---
        // 这一行在测试成功后可以删掉，不然会刷屏
        // if(Application.isPlaying) Debug.Log($"正在发送颜色: {settings.shadowColor}");
    }
}