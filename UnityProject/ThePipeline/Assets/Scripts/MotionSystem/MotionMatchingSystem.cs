using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.IO;

namespace ZZZ.Runtime.Motion
{
    /// <summary>
    /// ZZZ-Pipeline V2.5 核心中枢 (完整版)
    /// 包含：Native 接口、内存映射、以及 SilverAnbi 的驱动逻辑
    /// </summary>
    public unsafe class MotionMatchingSystem : MonoBehaviour
    {
        // =========================================================
        // 1. Native Interface (C++ DLL 契约)
        // =========================================================
        private const string DLL_NAME = "ZZZKernel";

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int LoadMotionData(string filepath);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool GenerateDummyBinFile(string filepath, int count);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetDatabasePtr();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SearchBestFrame(IntPtr dataset, MotionQuery query);

        // =========================================================
        // 2. Data Structures (必须与 C++ 16字节对齐)
        // =========================================================
        [StructLayout(LayoutKind.Sequential, Pack = 16)]
        public struct RuntimeFrame
        {
            public Vector4 Pos;      // x, y, z, pad
            public Vector4 Rot;      // x, y, z, w
            public Vector4 Vel;      // x, y, z, pad
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MotionQuery
        {
            public RuntimeFrame Target;
            public float Responsiveness;
        }

        // =========================================================
        // 3. Runtime State (变量设置)
        // =========================================================
        [Header("Visualization & Actor")]
        [Tooltip("把你美丽的老婆模型(SilverAnbi)拖到这里！")]
        public Transform CharacterActor;

        [Tooltip("把用来控制方向的小球拖到这里")]
        public Transform TargetTransform;

        [Header("Debug")]
        public bool ShowGizmos = true;

        // 内部指针
        private RuntimeFrame* _dbPointer = null;
        private int _frameCount = 0;
        private bool _isReady = false;

        // =========================================================
        // 4. Start (启动流程)
        // =========================================================
        void Start()
        {
            InitializePipeline();
        }

        private void InitializePipeline()
        {
            // 路径指向 StreamingAssets
            string dbPath = Path.Combine(Application.streamingAssetsPath, "motion_db.bin");

            // 1. 如果没有数据文件，让 C++ 现场造一个 (5000帧螺旋线)
            if (!File.Exists(dbPath))
            {
                Debug.LogWarning("[ZZZ] 没找到数据文件，正在命令 Native Core 生成测试数据...");
                if (!Directory.Exists(Application.streamingAssetsPath))
                    Directory.CreateDirectory(Application.streamingAssetsPath);

                GenerateDummyBinFile(dbPath, 5000);
            }

            // 2. 加载数据 (C++ 读硬盘 -> 内存)
            _frameCount = LoadMotionData(dbPath);

            if (_frameCount <= 0)
            {
                Debug.LogError($"[ZZZ] 加载失败！C++ 返回错误代码: {_frameCount}");
                return;
            }

            // 3. 拿到内存指针 (握手成功)
            _dbPointer = (RuntimeFrame*)GetDatabasePtr();

            if (_dbPointer == null)
            {
                Debug.LogError("[ZZZ] 致命错误：C++ 返回了空指针！");
                return;
            }

            _isReady = true;
            Debug.Log($"<color=#00FF00>[ZZZ] 引擎已觉醒。成功加载 {_frameCount} 帧动作数据。SilverAnbi 准备就绪。</color>");
        }

        // =========================================================
        // 5. Update (每帧驱动)
        // =========================================================
        void Update()
        {
            if (!_isReady || TargetTransform == null) return;

            // --- A. 构造查询 (C# -> C++) ---
            MotionQuery query = new MotionQuery();
            // 告诉 C++：我想去 Target 小球的位置
            query.Target.Pos = new Vector4(TargetTransform.position.x, TargetTransform.position.y, TargetTransform.position.z, 1.0f);
            query.Responsiveness = 1.0f;

            // --- B. 执行搜索 (Native Calculation) ---
            int bestIndex = SearchBestFrame(IntPtr.Zero, query);

            // --- C. 应用结果 (C++ -> C#) ---
            if (bestIndex >= 0)
            {
                // 直接读取内存，速度极快
                RuntimeFrame bestFrame = _dbPointer[bestIndex];

                // 驱动 SilverAnbi ！！！
                if (CharacterActor != null)
                {
                    // 1. 同步位置
                    Vector3 targetPos = new Vector3(bestFrame.Pos.x, bestFrame.Pos.y, bestFrame.Pos.z);
                    CharacterActor.position = targetPos;

                    // 2. 同步旋转
                    // 注意：因为我们目前是假数据，旋转可能是 (0,0,0,1)，所以她可能一直朝南
                    // 等后面接入真实动作数据，这里就会自动生效了
                    CharacterActor.rotation = new Quaternion(bestFrame.Rot.x, bestFrame.Rot.y, bestFrame.Rot.z, bestFrame.Rot.w);
                }

                // 画一条红线连过去，方便你看
                Debug.DrawLine(TargetTransform.position, new Vector3(bestFrame.Pos.x, bestFrame.Pos.y, bestFrame.Pos.z), Color.red);
            }
        }

        // =========================================================
        // 6. Gizmos (调试画线)
        // =========================================================
        void OnDrawGizmos()
        {
            if (!_isReady || !ShowGizmos || _dbPointer == null) return;

            // 画出那条性感的绿色数据流
            Gizmos.color = new Color(0, 1, 0, 0.5f);

            Vector3 prevPos = Vector3.zero;
            int drawLimit = Mathf.Min(_frameCount, 2000); // 怕你卡，只画前2000帧

            for (int i = 0; i < drawLimit; i++)
            {
                RuntimeFrame frame = _dbPointer[i];
                Vector3 currentPos = new Vector3(frame.Pos.x, frame.Pos.y, frame.Pos.z);

                if (i > 0) Gizmos.DrawLine(prevPos, currentPos);
                prevPos = currentPos;
            }
        }
    }
}