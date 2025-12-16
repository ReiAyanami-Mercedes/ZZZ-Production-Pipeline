#pragma once
#include <cstdint>

// =========================================================
// 1. 硬盘存储格式 (紧凑，1字节对齐)
// =========================================================
#pragma pack(push, 1)

struct MotionFileHeader {
    uint32_t Magic;         // 0x5A5A5A31 ('ZZZ1')
    uint32_t Version;       // 1
    uint32_t FrameCount;    // 总帧数
    float    FrameRate;     // 60.0f
};

struct DiskFrame {
    float Pos[3];      // x, y, z
    float Rot[4];      // x, y, z, w
    float Velocity[3]; // x, y, z
};

#pragma pack(pop)

// =========================================================
// 2. 内存运行时格式 (16字节对齐，为了 SIMD)
// =========================================================
struct alignas(16) RuntimeFrame {
    float Pos[4];      // x, y, z, pad
    float Rot[4];      // x, y, z, w
    float Velocity[4]; // x, y, z, pad
};

// =========================================================
// 3. 交互结构体 (用于 C++ 和 C# 传参)
// =========================================================
struct MotionDataset {
    RuntimeFrame* Frames; // 指向 C++ 堆内存的指针
    int Count;            // 数组长度
};

struct MotionQuery {
    RuntimeFrame Target;  // 目标姿态
    float Responsiveness; // 响应权重
};