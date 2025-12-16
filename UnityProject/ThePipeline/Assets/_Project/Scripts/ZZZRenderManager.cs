using UnityEngine;
// 引用你的配置所在的命名空间，这样就不报 CS0246 错了！
using ZZZ.Runtime;

namespace ZZZ.Runtime
{
    [ExecuteAlways]
    public class ZZZRenderManager : MonoBehaviour
    {
        [Header("🔥 核心配置文件")]
        public GlobalRenderSettings settings;

        // --- 缓存 Shader 属性 ID ---
        private static readonly int _GlobalShadowColor = Shader.PropertyToID("_GlobalShadowColor");
        private static readonly int _GlobalSDFThreshold = Shader.PropertyToID("_GlobalSDFThreshold");
        private static readonly int _GlobalSDFSmoothness = Shader.PropertyToID("_GlobalSDFSmoothness");

        // 描边控制
        private static readonly int _GlobalOutlineWidth = Shader.PropertyToID("_GlobalOutlineWidth");
        private static readonly int _GlobalOutlineColor = Shader.PropertyToID("_GlobalOutlineColor");

        // 系统控制
        private static readonly int _GlobalInstancingEnabled = Shader.PropertyToID("_GlobalInstancingEnabled");

        void Update()
        {
            if (settings == null) return;

            // 1. 传输光照参数 (Toon Shading)
            Shader.SetGlobalColor(_GlobalShadowColor, settings.shadowColor);
            Shader.SetGlobalFloat(_GlobalSDFThreshold, settings.sdfThreshold);
            Shader.SetGlobalFloat(_GlobalSDFSmoothness, settings.sdfSmoothness);

            // 2. 传输描边参数 (Outline)
            if (settings.enableOutline)
            {
                Shader.SetGlobalColor(_GlobalOutlineColor, settings.outlineColor);
                Shader.SetGlobalFloat(_GlobalOutlineWidth, settings.outlineWidth);
            }
            else
            {
                Shader.SetGlobalFloat(_GlobalOutlineWidth, 0.0f);
            }

            // 3. 传输系统参数
            Shader.SetGlobalFloat(_GlobalInstancingEnabled, settings.gpuInstancing ? 1.0f : 0.0f);
        }
    }
}