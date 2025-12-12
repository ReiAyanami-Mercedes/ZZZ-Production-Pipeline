using UnityEngine;

namespace ZZZ.Runtime
{
    // [CreateAssetMenu] 让我们可以在 Project 窗口右键创建这个配置文件
    [CreateAssetMenu(fileName = "ZZZ_GlobalSettings", menuName = "ZZZ Pipeline/Global Render Settings")]
    public class GlobalRenderSettings : ScriptableObject
    {
        // --- 1. 风格化渲染配置 (Toon Shading) ---
        // [Fix] 删掉了 🎨 Emoji
        [Header("Toon Shading Settings")]

        [Tooltip("Global Shadow Color")]
        public Color shadowColor = new Color(0.6f, 0.6f, 0.8f, 1.0f);

        [Range(0f, 1f)]
        [Tooltip("SDF Threshold (Control Shadow Area)")]
        public float sdfThreshold = 0.5f;

        [Range(0f, 1f)]
        [Tooltip("SDF Smoothness (Shadow Softness)")]
        public float sdfSmoothness = 0.05f;

        // --- 2. 描边配置 (Outline) ---
        // [Fix] 删掉了 🖋️ Emoji
        [Header("Outline Settings")]
        public bool enableOutline = true;

        [Range(0f, 0.1f)]
        public float outlineWidth = 0.02f;

        public Color outlineColor = Color.black;

        // --- 3. 系统配置 (System) ---
        // [Fix] 删掉了 ⚙️ Emoji
        [Header("System Settings")]
        public bool gpuInstancing = true;
        public bool debugMode = false;
    }
}