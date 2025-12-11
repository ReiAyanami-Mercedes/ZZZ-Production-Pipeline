using UnityEngine;

namespace ZZZ.Runtime
{
    // [CreateAssetMenu] 让我们可以右键创建这个资产
    [CreateAssetMenu(fileName = "ZZZ_GlobalSettings", menuName = "ZZZ Pipeline/Global Render Settings")]
    public class GlobalRenderSettings : ScriptableObject
    {
        [Header("🎨 Toon Shading (风格化渲染)")]

        [Tooltip("全局阴影颜色 (Shadow Color)")]
        public Color shadowColor = new Color(0.6f, 0.6f, 0.8f, 1.0f);

        [Range(0f, 1f)]
        [Tooltip("SDF 光影阈值 (控制阴影面积)")]
        public float sdfThreshold = 0.5f;

        [Range(0f, 1f)]
        [Tooltip("SDF 羽化程度 (软硬阴影过渡)")]
        public float sdfSmoothness = 0.05f;

        [Header("🖋️ Outline (描边配置)")]
        public bool enableOutline = true;

        [Range(0f, 0.1f)]
        public float outlineWidth = 0.02f;

        public Color outlineColor = Color.black;

        [Header("⚙️ System (系统配置)")]
        public bool gpuInstancing = true;
        public bool debugMode = false;
    }
}