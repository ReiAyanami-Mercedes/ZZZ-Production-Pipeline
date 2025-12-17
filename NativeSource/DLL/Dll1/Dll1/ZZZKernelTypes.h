#pragma once
#include <cstdint>

// =========================================================
// [PART 1] 动作模块数据 (Motion Matching Data)
// =========================================================

#pragma pack(push, 1)
struct MotionFileHeader {
    uint32_t Magic;
    uint32_t Version;
    uint32_t FrameCount;
    float    FrameRate;
};

struct DiskFrame {
    float Pos[3];
    float Rot[4];
    float Velocity[3];
};
#pragma pack(pop)

struct alignas(16) RuntimeFrame {
    float Pos[4];
    float Rot[4];
    float Velocity[4];
};

struct MotionQuery {
    RuntimeFrame Target;
    float Responsiveness;
};

struct MotionDataset {
    RuntimeFrame* Frames;
    int Count;
};

// =========================================================
// [PART 2] 核心实体数据 (Core Entity Data)
// =========================================================

struct alignas(16) EntityTransform {
    float position[3];
    float scale;
    float rotation[4];
    int32_t entityID;
    int32_t isActive;
};

struct KernelSettings {
    float waveSpeed = 2.0f;
    float waveHeight = 2.0f;
    float waveFrequency = 0.1f;
};