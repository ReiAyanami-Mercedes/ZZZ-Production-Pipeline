#include <vector>
#include <random>
#include "MotionCore.h" // 引入 MotionCore

#define DLLExport __declspec(dllexport)

// --- 旧的粒子系统代码 (保留) ---
struct ParticleData
{
    float position[4];
    float color[4];
};

extern "C"
{
    // --- 粒子系统接口 (保留) ---
    DLLExport void InitNativeWorld(ParticleData* data, int count, float spread, unsigned int seed)
    {
        std::mt19937 gen(seed);
        std::uniform_real_distribution<float> dis(-spread / 2.0f, spread / 2.0f);
        for (int i = 0; i < count; i++) {
            data[i].position[0] = dis(gen);
            data[i].position[1] = 0;
            data[i].position[2] = dis(gen);
            data[i].position[3] = 1.0f;
            data[i].color[0] = 1.0f; data[i].color[1] = 1.0f; data[i].color[2] = 1.0f; data[i].color[3] = 1.0f;
        }
    }

    DLLExport void UpdateNativeWorld(ParticleData* data, int count, float deltaTime)
    {
        // 简单的更新逻辑保留
        for (int i = 0; i < count; ++i) {
            data[i].position[1] += 1.0f * deltaTime;
            if (data[i].position[1] > 5.0f) data[i].position[1] = 0.0f;
        }
    }

    // --- 🔥🔥🔥 新增：Motion Matching 接口 🔥🔥🔥 ---

    DLLExport MotionDataset* InitMotionSystem(int frameCount)
    {
        return CreateTestMotionData(frameCount);
    }

    DLLExport void ReleaseMotionSystem(MotionDataset* dataset)
    {
        if (dataset != nullptr)
        {
            if (dataset->Frames != nullptr) delete[] dataset->Frames;
            delete dataset;
        }
    }

    // 新增：查询接口
    DLLExport int QueryMotionSystem(MotionDataset* dataset, MotionQuery query)
    {
        if (dataset == nullptr) return -1;
        return SearchBestFrame(dataset, query);
    }
}