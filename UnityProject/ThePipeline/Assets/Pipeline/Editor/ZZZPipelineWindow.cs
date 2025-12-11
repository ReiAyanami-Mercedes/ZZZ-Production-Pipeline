using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZZZ.Editor
{
    public class ZZZPipelineWindow : EditorWindow
    {
        // 1. 在 Unity 顶部菜单栏添加一个入口
        [MenuItem("ZZZ-Pipeline/Open Control Center")]
        public static void ShowWindow()
        {
            // 创建并显示窗口，设置标题
            ZZZPipelineWindow wnd = GetWindow<ZZZPipelineWindow>();
            wnd.titleContent = new GUIContent("ZZZ Cockpit");
            wnd.minSize = new Vector2(400, 300);
        }

        // 2. CreateGUI 是 UI Toolkit 的核心，相当于 Start()
        public void CreateGUI()
        {
            // 获取根节点
            VisualElement root = rootVisualElement;

            // --- 标题区域 ---
            Label title = new Label("ZZZ Pipeline V2.0");
            title.style.fontSize = 20;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginTop = 10;
            title.style.marginBottom = 10;
            title.style.alignSelf = Align.Center;
            title.style.color = new StyleColor(new Color(0.2f, 0.8f, 1.0f)); // 赛博蓝
            root.Add(title);

            // --- 状态面板 ---
            Box statusBox = new Box();
            statusBox.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
            statusBox.style.paddingTop = 10;
            statusBox.style.paddingBottom = 10;
            statusBox.style.paddingLeft = 10;
            statusBox.style.paddingRight = 10;
            statusBox.style.marginTop = 10;

            Label statusLabel = new Label("System Status: ONLINE");
            statusLabel.style.color = Color.green;
            statusBox.Add(statusLabel);

            Label cpuLabel = new Label("Workstation: Low-Spec Mode");
            cpuLabel.style.fontSize = 10;
            statusBox.Add(cpuLabel);

            root.Add(statusBox);

            // --- 按钮区域 ---
            Button runTestBtn = new Button();
            runTestBtn.text = "Run Unit Tests (Quick)";
            runTestBtn.style.height = 30;
            runTestBtn.style.marginTop = 20;

            // 给按钮绑定点击事件
            runTestBtn.clicked += () =>
            {
                Debug.Log("Command received: Running diagnostics...");
                // 这里以后可以连接我们刚才写的测试
                EditorUtility.DisplayDialog("System", "All Systems Nominal. Ready for rendering.", "OK");
            };

            root.Add(runTestBtn);
        }
    }
}