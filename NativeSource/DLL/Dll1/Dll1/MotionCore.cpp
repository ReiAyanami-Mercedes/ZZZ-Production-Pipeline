#include "MotionCore.h"
#include <math.h> 
#include <float.h> // for FLT_MAX

MotionDataset* CreateTestMotionData(int frameCount)
{
    MotionDataset* dataset = new MotionDataset();
    dataset->FrameCount = frameCount;
    dataset->Frames = new MotionFrame[frameCount];

    for (int i = 0; i < frameCount; ++i)
    {
        MotionFrame& frame = dataset->Frames[i];

        // 强转 float，消除警告
        float t = (float)i * 0.033f;

        for (int k = 0; k < 3; ++k)
        {
            float futureTime = t + (k + 1) * 0.3f;

            // 🔧 修复点 1：这里的 cos/sin 返回 double，必须强转 (float)
            frame.TrajectoryPositions[k].x = (float)cos(futureTime) * 5.0f;
            frame.TrajectoryPositions[k].y = 0.0f;
            frame.TrajectoryPositions[k].z = (float)sin(futureTime) * 5.0f;
            frame.TrajectoryPositions[k].w = 1.0f;
        }

        // 🔧 修复点 2 (关键报错点 C2397)：
        // 在花括号初始化 { } 内部，C++ 禁止隐式转换。
        // 所以这里必须把 fabs(...) 的结果显式转为 (float)，否则直接报错！
        frame.BonePositions[(int)BoneIndex::LeftFoot] = { 0.5f, (float)fabs(sin(t * 5.0f)), 0.0f, 1.0f };

        frame.Time = t;
        frame.ClipID = 1001;
        frame.FrameIndex = i;
    }

    return dataset;
}

// 辅助：计算距离平方
float DistanceSqr(float4 a, float4 b)
{
    float dx = a.x - b.x;
    float dy = a.y - b.y;
    float dz = a.z - b.z;
    return dx * dx + dy * dy + dz * dz;
}

// 暴力搜索实现
int SearchBestFrame(MotionDataset* dataset, MotionQuery query)
{
    int bestIndex = -1;
    float minCost = FLT_MAX;

    for (int i = 0; i < dataset->FrameCount; ++i)
    {
        MotionFrame& frame = dataset->Frames[i];

        float currentCost = 0.0f;

        // 1. 比较轨迹
        currentCost += DistanceSqr(query.TrajectoryPositions[0], frame.TrajectoryPositions[0]);

        // 2. 比较左脚
        currentCost += DistanceSqr(query.BonePositions[(int)BoneIndex::LeftFoot], frame.BonePositions[(int)BoneIndex::LeftFoot]);

        if (currentCost < minCost)
        {
            minCost = currentCost;
            bestIndex = i;
        }
    }

    return bestIndex;
}