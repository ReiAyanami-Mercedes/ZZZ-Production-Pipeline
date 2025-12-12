using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using ZZZ.Runtime;
using UnityEngine.Profiling;

namespace ZZZ.Editor
{
    public class ZZZPipelineWindow : EditorWindow
    {
        private SerializedObject _serializedSettings;
        private Label _fpsLabel;
        private Label _memLabel;

        private double _lastUpdateTime = 0;
        private const double UPDATE_INTERVAL = 0.5;

        [MenuItem("ZZZ-Pipeline/Open Control Center")]
        public static void ShowWindow()
        {
            ZZZPipelineWindow wnd = GetWindow<ZZZPipelineWindow>();
            wnd.titleContent = new GUIContent("ZZZ Cockpit");
            wnd.minSize = new Vector2(450, 600);
        }

        private void OnEnable()
        {
            EditorApplication.update += TimedUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= TimedUpdate;
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.Clear();

            DrawHeader(root);
            DrawPerformanceHUD(root);
            LoadAndBindSettings(root); // 🔥 它回来了！
            DrawFooter(root);
        }

        private void DrawHeader(VisualElement root)
        {
            Label title = new Label("ZZZ Pipeline V2.0");
            title.style.fontSize = 24;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = new StyleColor(new Color(0.2f, 0.8f, 1.0f));
            title.style.alignSelf = Align.Center;
            title.style.marginTop = 15;
            title.style.marginBottom = 5;
            root.Add(title);

            Label subtitle = new Label("Industrial Render Control System");
            subtitle.style.fontSize = 12;
            subtitle.style.color = Color.gray;
            subtitle.style.alignSelf = Align.Center;
            subtitle.style.marginBottom = 15;
            root.Add(subtitle);
        }

        private void DrawPerformanceHUD(VisualElement root)
        {
            Box hudBox = new Box();
            hudBox.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.1f));
            hudBox.style.paddingTop = 10;
            hudBox.style.paddingBottom = 10;
            hudBox.style.paddingLeft = 10;
            hudBox.style.paddingRight = 10;
            hudBox.style.marginTop = 15;
            hudBox.style.marginBottom = 10;

            Label header = new Label("Runtime Profiler");
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.color = new Color(0.8f, 0.8f, 0.8f);
            hudBox.Add(header);

            _fpsLabel = new Label("FPS: N/A");
            hudBox.Add(_fpsLabel);

            _memLabel = new Label("Memory: N/A");
            hudBox.Add(_memLabel);

            root.Add(hudBox);
        }

        private void LoadAndBindSettings(VisualElement root)
        {
            string[] guids = AssetDatabase.FindAssets("t:GlobalRenderSettings");
            if (guids.Length == 0)
            {
                root.Add(new HelpBox("[!] No Global Settings found!", HelpBoxMessageType.Error));
                return;
            }
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            GlobalRenderSettings settings = AssetDatabase.LoadAssetAtPath<GlobalRenderSettings>(path);
            if (settings != null)
            {
                _serializedSettings = new SerializedObject(settings);
                Box settingsBox = new Box();
                InspectorElement inspector = new InspectorElement(_serializedSettings);
                settingsBox.Add(inspector);
                root.Add(settingsBox);

                // 显示链接路径
                Label pathLabel = new Label($"Linked: {path}");
                pathLabel.style.fontSize = 10;
                pathLabel.style.color = Color.gray;
                pathLabel.style.marginTop = 5;
                root.Add(pathLabel);
            }
        }

        private void DrawFooter(VisualElement root)
        {
            VisualElement footer = new VisualElement();
            footer.style.flexDirection = FlexDirection.Row;
            footer.style.justifyContent = Justify.SpaceBetween;
            footer.style.marginTop = 20;
            Button refreshBtn = new Button(() => { CreateGUI(); }) { text = "[Refresh Link]" };
            refreshBtn.style.flexGrow = 1;
            Button testBtn = new Button(() => { Debug.Log("Running Diagnostics..."); }) { text = "[Diagnostics]" };
            testBtn.style.flexGrow = 1;
            footer.Add(refreshBtn);
            footer.Add(testBtn);
            root.Add(footer);
        }

        private void TimedUpdate()
        {
            double currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - _lastUpdateTime < UPDATE_INTERVAL) return;
            _lastUpdateTime = currentTime;
            UpdatePerformanceHUD();
        }

        private void UpdatePerformanceHUD()
        {
            if (_fpsLabel == null || _memLabel == null) return;

            long memoryBytes = Profiler.GetTotalAllocatedMemoryLong();
            string memoryMB = (memoryBytes / (1024f * 1024f)).ToString("F1") + " MB";
            _memLabel.text = $"Total Memory: {memoryMB}";

            if (Application.isPlaying)
            {
                float fps = 1.0f / Time.smoothDeltaTime;
                _fpsLabel.text = $"FPS: {fps:F1}";
                _fpsLabel.style.color = (fps < 30) ? Color.red : (fps < 50 ? Color.yellow : Color.green);
            }
            else
            {
                _fpsLabel.text = "FPS: (Only in Play Mode)";
                _fpsLabel.style.color = Color.gray;
            }
        }
    }
}