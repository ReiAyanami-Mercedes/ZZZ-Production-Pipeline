using UnityEngine;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public unsafe class MotionMatchingSystem : MonoBehaviour
{
    // --- 定义结构体 ---
    public enum BoneIndex { Root = 0, LeftFoot = 1, RightFoot = 2, Hips = 3, Count = 4 }

    [StructLayout(LayoutKind.Sequential)]
    public struct MotionFrame
    {
        public fixed float TrajectoryPositions[12];
        public fixed float TrajectoryDirections[12];
        public fixed float BonePositions[16];
        public fixed float BoneVelocities[16];
        public float Time;
        public int ClipID;
        public int FrameIndex;
        private int _padding;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MotionQuery
    {
        public fixed float TrajectoryPositions[12];
        public fixed float TrajectoryDirections[12];
        public fixed float BonePositions[16];
        public fixed float BoneVelocities[16];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MotionDataset
    {
        public MotionFrame* Frames;
        public int FrameCount;
    }

    // --- DLL 导入 ---
    [DllImport("ZZZ_Native_Core")]
    private static extern MotionDataset* InitMotionSystem(int frameCount);

    [DllImport("ZZZ_Native_Core")]
    private static extern void ReleaseMotionSystem(MotionDataset* dataset);

    [DllImport("ZZZ_Native_Core")]
    private static extern int QueryMotionSystem(MotionDataset* dataset, MotionQuery query);

    // --- 运行时逻辑 ---
    private MotionDataset* _datasetPtr;
    public int frameCountToGen = 1000;

    // 🔥 交互目标
    public Transform targetTransform;
    private int _matchedIndex = -1;

    void Start()
    {
        _datasetPtr = InitMotionSystem(frameCountToGen);
        Debug.Log($"✅ Motion System 初始化完成！C++ 生成了 {_datasetPtr->FrameCount} 帧数据。");
    }

    void OnDestroy()
    {
        if (_datasetPtr != null)
        {
            ReleaseMotionSystem(_datasetPtr);
            _datasetPtr = null;
        }
    }

    void Update()
    {
        if (_datasetPtr == null || targetTransform == null) return;

        // 1. 构建 Query：把 Target 的位置填进去
        MotionQuery query = new MotionQuery();
        Vector3 tPos = targetTransform.position;

        // 这里我们只填第0个轨迹点作为测试
        query.TrajectoryPositions[0] = tPos.x;
        query.TrajectoryPositions[1] = tPos.y;
        query.TrajectoryPositions[2] = tPos.z;

        // 2. 呼叫 C++ 搜索
        _matchedIndex = QueryMotionSystem(_datasetPtr, query);
    }

    void OnDrawGizmos()
    {
        if (_datasetPtr == null) return;

        // 画所有数据 (绿线)
        Gizmos.color = Color.green;
        for (int i = 0; i < _datasetPtr->FrameCount; i++)
        {
            MotionFrame* frame = _datasetPtr->Frames + i;
            float x = frame->TrajectoryPositions[0];
            float y = frame->TrajectoryPositions[1];
            float z = frame->TrajectoryPositions[2];

            // 画点
            if (i % 10 == 0) Gizmos.DrawSphere(new Vector3(x, y, z), 0.05f);

            // 画线
            if (i > 0)
            {
                MotionFrame* prev = frame - 1;
                Vector3 p1 = new Vector3(prev->TrajectoryPositions[0], prev->TrajectoryPositions[1], prev->TrajectoryPositions[2]);
                Vector3 p2 = new Vector3(x, y, z);
                Gizmos.DrawLine(p1, p2);
            }
        }

        // 🔥 画匹配结果 (红球)
        if (_matchedIndex >= 0 && _matchedIndex < _datasetPtr->FrameCount)
        {
            Gizmos.color = Color.red;
            MotionFrame* frame = _datasetPtr->Frames + _matchedIndex;

            float x = frame->TrajectoryPositions[0];
            float y = frame->TrajectoryPositions[1];
            float z = frame->TrajectoryPositions[2];

            Vector3 matchedPos = new Vector3(x, y, z);
            Gizmos.DrawSphere(matchedPos, 0.5f); // 红色大球

            if (targetTransform != null)
                Gizmos.DrawLine(targetTransform.position, matchedPos); // 连线
        }
    }
}