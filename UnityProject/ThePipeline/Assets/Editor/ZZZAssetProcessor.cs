using UnityEngine;
using UnityEditor;
using System.IO;

// ZZZ-Pipeline Module B: 自动化工具链 / The Gatekeeper
// 核心思想：在资源导入前，拦截并强制设定符合工业化标准的参数。
public class ZZZAssetProcessor : AssetPostprocessor
{
    // -------------------------------------------------------------------
    // I. 纹理预处理： OnPreprocessTexture
    // -------------------------------------------------------------------
    void OnPreprocessTexture()
    {
        // 1. 获取导入器实例，这是我们修改参数的唯一入口
        TextureImporter importer = (TextureImporter)assetImporter;
        // 获取小写文件名，便于使用 Contains/EndsWith 进行模糊匹配
        string fileName = Path.GetFileNameWithoutExtension(assetPath).ToLower();

        // ** 【目标：解决 SDF 数据纯净性】 **
        // 识别关键数据图：SDF图、光照Mask、Ramp Map
        if (fileName.Contains("_sdf") || fileName.Contains("_ramp") || fileName.Contains("_mask"))
        {
            // ---------------------------------------------------
            // 1. 核心壁垒：线性空间！(非颜色数据)
            // 贴图不是颜色信息，而是数学数据，必须关闭 Gamma/sRGB！
            // 否则 GPU 计算光影时会得到错误结果。
            importer.sRGBTexture = false;

            // 2. 精度壁垒：无损压缩！
            // 关键数据不容有失，必须禁用压缩！
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            // 3. 性能壁垒：禁用 Mipmap！
            // 数据图不需要 Mipmap，禁用能节省显存，防止在远处被模糊。
            importer.mipmapEnabled = false;

            // 4. 通报系统：在控制台给出明确提示 (可观测性)
            Debug.Log($"<color=magenta>[ZZZ-Pipeline]</color> 🧬 资产守门人：已强制 SDF 数据图进入线性空间！ -> {fileName}");
        }

        // ** 【目标：解决法线贴图标准化】 **
        // 识别法线贴图
        else if (fileName.EndsWith("_n") || fileName.EndsWith("_normal"))
        {
            // 强制设为 Normal Map 类型
            importer.textureType = TextureImporterType.NormalMap;
            // 法线贴图自带的压缩模式最优化
            importer.textureCompression = TextureImporterCompression.Compressed;

            Debug.Log($"<color=cyan>[ZZZ-Pipeline]</color> ⚡ 资产守门人：已强制法线贴图标准化！ -> {fileName}");
        }

        // ---------------------------------------------------
        // II. 角色模型后处理：OnPostprocessModel (仅供参考，明日精写)
        // ---------------------------------------------------
    }

    // ** 【未来预留位：资产追踪 Log System】 **
    /* 
    void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        // 可以在这里写入 Log System，记录是哪个资产导入了（UID 溯源的基础）
    }
    */
}
