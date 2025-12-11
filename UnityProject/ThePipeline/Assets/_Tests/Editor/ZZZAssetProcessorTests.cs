using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ZZZ.Tests
{
    public class ZZZAssetProcessorTests
    {
        // 定义一个临时的测试文件路径
        private const string TestTexturePath = "Assets/ZZZ_Temp_Test_sdf.png";

        [SetUp]
        public void Setup()
        {
            // 1. 创建一张 2x2 的临时红图
            var texture = new Texture2D(2, 2);
            byte[] bytes = texture.EncodeToPNG();

            // 2. 写入磁盘
            File.WriteAllBytes(TestTexturePath, bytes);

            // 3. 强制 Unity 刷新数据库
            AssetDatabase.Refresh();
        }

        [Test]
        public void Texture_With_SDF_Suffix_Should_Be_Linear()
        {
            // 1. 获取该图片的导入设置
            TextureImporter importer = AssetImporter.GetAtPath(TestTexturePath) as TextureImporter;

            // 这里我们需要明确告诉 Unity，我们要用 NUnit 的断言
            // 否则它会和 UnityEngine.Assertions.Assert 打架
            NUnit.Framework.Assert.IsNotNull(importer, "Fatal Error: Test texture was not created.");

            // 2. 核心断言
            // 检查 sRGBTexture 属性
            bool isSRGB = importer.sRGBTexture;

            // 同样使用 NUnit 的断言
            NUnit.Framework.Assert.IsFalse(isSRGB, "Error: Texture with _sdf suffix is still sRGB!");

            Debug.Log("[PASS] SDF Linear Check Passed.");
        }

        [TearDown]
        public void TearDown()
        {
            // 毁灭证据
            if (File.Exists(TestTexturePath))
            {
                AssetDatabase.DeleteAsset(TestTexturePath);
                AssetDatabase.Refresh();
            }
        }
    }
}