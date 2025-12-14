#pragma once

// --- 基础数学类型 ---
struct float3
{
    float x, y, z;
};

struct float4
{
    float x, y, z, w;
};

// --- 骨骼索引枚举 ---
enum class BoneIndex : int
{
    Root = 0,
    LeftFoot = 1,
    RightFoot = 2,
    Hips = 3,
    Count = 4
};

// --- 动作帧数据 (数据集里的每一帧) ---
struct alignas(16) MotionFrame
{
    // 轨迹 (Trajectory)
    float4 TrajectoryPositions[3];
    float4 TrajectoryDirections[3];

    // 姿态 (Pose)
    float4 BonePositions[4];
    float4 BoneVelocities[4];

    // 元数据
    float Time;
    int ClipID;
    int FrameIndex;
};

// --- 🔥 新增：搜索请求 (Query) ---
// 用于描述 "我想找什么样的动作"
struct alignas(16) MotionQuery
{
    float4 TrajectoryPositions[3];
    float4 TrajectoryDirections[3];
    float4 BonePositions[4];
    float4 BoneVelocities[4];
};

// --- 数据集容器 ---
struct MotionDataset
{
    MotionFrame* Frames;
    int FrameCount;
};

// --- 函数声明 ---
MotionDataset* CreateTestMotionData(int frameCount);

// 🔥 新增：搜索函数
int SearchBestFrame(MotionDataset* dataset, MotionQuery query);