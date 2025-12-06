using UnityEngine;
using UnityEditor;
using System.IO;

// ----------------------------------------------------------------
// ?? ZZZ Pipeline Tool - Genesis Module
// ----------------------------------------------------------------

public class ZZZPipelineTool : EditorWindow
{
    // 在 Unity 顶部菜单栏添加一个 "ZZZ-Pipeline" 选项
    [MenuItem("ZZZ-Pipeline/Open Dashboard ??")]
    public static void ShowWindow()
    {
        // 弹出窗口，标题叫 "ZZZ Control"
        GetWindow<ZZZPipelineTool>("ZZZ Control");
    }

    // 窗口里的 UI 绘制代码
    void OnGUI()
    {
        // 1. 标题区
        GUILayout.Space(10);
        GUILayout.Label("? ZZZ PIPELINE SETUP ?", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // 装饰性线条
        EditorGUILayout.HelpBox("Project Initialization Module // Day 05", MessageType.Info);
        GUILayout.Space(10);

        // 2. 核心功能按钮：一键生成目录
        if (GUILayout.Button("?? Initialize Project Structure (一键建目录)", GUILayout.Height(40)))
        {
            CreateFolders();
        }

        // 3. 附加功能：打开持久化数据目录 (方便找存档)
        if (GUILayout.Button("?? Open Persistent Data Path", GUILayout.Height(30)))
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
    }

    // ----------------------------------------------------------------
    // ?? 文件夹生成逻辑 (工业级目录结构)
    // ----------------------------------------------------------------
    void CreateFolders()
    {
        // 定义我们要创建的根目录 (加下划线是为了让它在 Project 里置顶)
        string root = "_Project";

        // 定义一套标准的目录结构
        string[] folders = new string[]
        {
            // ?? 美术资产 (Art Assets)
            root + "/Art/Characters",   // 角色
            root + "/Art/Environment",  // 场景
            root + "/Art/Effects",      // 特效 (VFX)
            root + "/Art/Animation",    // 动画
            root + "/Art/Materials",    // 材质
            root + "/Art/Textures",     // 贴图
            root + "/Art/Shaders",      // 着色器 (我们后面要写的)

            // ?? 核心代码 (Codebase)
            root + "/Scripts/Framework", // 框架
            root + "/Scripts/Gameplay",  // 玩法
            root + "/Scripts/Editor",    // 编辑器扩展

            // ?? 预制体与配置 (Prefabs & Data)
            root + "/Prefabs",
            root + "/Resources",         // 动态加载资源
            root + "/Scenes",            // 场景
            root + "/Settings",          // 渲染管线配置
            
            // ?? 第三方插件 (把它隔离开，别跟我们的代码混在一起)
            "_ThirdParty"
        };

        // 循环创建
        foreach (string path in folders)
        {
            string fullPath = Path.Combine(Application.dataPath, path);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                Debug.Log($"<color=green>[ZZZ-Pipeline]</color> ? Created: {path}");
            }
        }

        // 刷新 Unity 数据库，让文件夹立刻显示出来
        AssetDatabase.Refresh();

        // 弹窗告诉老婆搞定了
        EditorUtility.DisplayDialog("System Report", "Pipeline Structure Initialized! \n\n所有文件夹已按标准建立完毕。", "Nice! ??");
    }
}

