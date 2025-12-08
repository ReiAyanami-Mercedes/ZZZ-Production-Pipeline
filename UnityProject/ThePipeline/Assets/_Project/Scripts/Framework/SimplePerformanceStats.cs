using UnityEngine;
using UnityEngine.Profiling; // 引入性能分析库

// =================================================================
// 📊 ZZZ-Pipeline Module C: 可观测性系统 (Runtime Profiler)
// -----------------------------------------------------------------
// 核心思想：SRE 思维。实时监控渲染管线的健康状态。
// =================================================================

public class SimplePerformanceStats : MonoBehaviour
{
    [Header("UI 配置")]
    public Color textColor = Color.green;
    public int fontSize = 20;

    // 缓存数据
    private float deltaTime = 0.0f;
    private float fps = 0.0f;
    private long totalMemory = 0;

    void Update()
    {
        // 1. 计算 FPS (平滑处理)
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        fps = 1.0f / deltaTime;

        // 2. 获取内存 (每 60 帧更新一次，节省性能)
        if (Time.frameCount % 60 == 0)
        {
            totalMemory = Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024; // MB
        }
    }

    // 3. 绘制 UI (最简单的 IMGUI，不用创建 Canvas，随插随用)
    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = fontSize;
        style.normal.textColor = textColor;

        // 构造显示文本
        string text = string.Format(
            "⚡ ZZZ Pipeline Status ⚡\n" +
            "------------------------\n" +
            "FPS: {0:0.0} ms ({1:0.} fps)\n" +
            "Memory: {2} MB\n" +
            "System: {3}\n",
            deltaTime * 1000.0f, fps, totalMemory, SystemInfo.graphicsDeviceType);

        // 绘制在左上角
        GUI.Label(new Rect(10, 10, w, h * 2 / 100), text, style);
    }
}