// ⚠️ 必须在第一行
#define _CRT_SECURE_NO_WARNINGS 

// ⚠️ 注意：这里引用名字变了！
#include "ZZZKernelAPI.h" 

#include <cstdio>
#include <vector>
#include <cmath>
#include <cstring>
#include <cfloat>

// =========================================================
// 全局内存池
// =========================================================
std::vector<RuntimeFrame> g_MotionDatabase;
MotionDataset g_DatasetWrapper;
std::vector<EntityTransform> g_EntityPool;
KernelSettings g_Settings;

// =========================================================
// 接口实现
// =========================================================
extern "C" {

    // --- 动作部分 ---
    ZZZ_API MotionDataset* CreateTestMotionData(int count) {
        g_MotionDatabase.resize(count);
        for (int i = 0; i < count; i++) {
            float t = i * 0.1f;
            g_MotionDatabase[i].Pos[0] = sinf(t) * 5.0f;
            g_MotionDatabase[i].Pos[1] = 0.0f;
            g_MotionDatabase[i].Pos[2] = cosf(t) * 5.0f;
            g_MotionDatabase[i].Pos[3] = 1.0f;
            g_MotionDatabase[i].Rot[3] = 1.0f;
        }
        g_DatasetWrapper.Frames = g_MotionDatabase.data();
        g_DatasetWrapper.Count = count;
        return &g_DatasetWrapper;
    }

    ZZZ_API bool GenerateDummyBinFile(const char* filepath, int count) {
        FILE* file = fopen(filepath, "wb");
        if (!file) return false;
        MotionFileHeader header = { 0x5A5A5A31, 1, (uint32_t)count, 60.0f };
        fwrite(&header, sizeof(MotionFileHeader), 1, file);
        CreateTestMotionData(count);
        fclose(file);
        return true;
    }

    ZZZ_API int LoadMotionData(const char* filepath) {
        FILE* file = fopen(filepath, "rb");
        if (!file) return -1;
        MotionFileHeader header;
        fread(&header, sizeof(MotionFileHeader), 1, file);
        if (header.Magic != 0x5A5A5A31) { fclose(file); return -2; }

        g_MotionDatabase.resize(header.FrameCount);
        std::vector<DiskFrame> temp(header.FrameCount);
        fread(temp.data(), sizeof(DiskFrame), header.FrameCount, file);

        for (uint32_t i = 0; i < header.FrameCount; i++) {
            g_MotionDatabase[i].Pos[0] = temp[i].Pos[0];
            g_MotionDatabase[i].Pos[1] = temp[i].Pos[1];
            g_MotionDatabase[i].Pos[2] = temp[i].Pos[2];
            g_MotionDatabase[i].Pos[3] = 1.0f;
        }
        fclose(file);
        g_DatasetWrapper.Frames = g_MotionDatabase.data();
        g_DatasetWrapper.Count = (int)header.FrameCount;
        return (int)header.FrameCount;
    }

    ZZZ_API int SearchBestFrame(MotionDataset* dataset, MotionQuery query) {
        return 0; // 占位
    }

    ZZZ_API RuntimeFrame* GetDatabasePtr() {
        return g_MotionDatabase.data();
    }


    // --- 实体部分 ---
    ZZZ_API void InitializeEntities(int count) {
        g_EntityPool.resize(count);
        for (int i = 0; i < count; ++i) {
            g_EntityPool[i].entityID = i;
            g_EntityPool[i].isActive = 1;
            g_EntityPool[i].scale = 1.0f;
            int row = i / 100;
            int col = i % 100;
            g_EntityPool[i].position[0] = (float)col * 1.0f;
            g_EntityPool[i].position[1] = 0.0f;
            g_EntityPool[i].position[2] = (float)row * 1.0f;
            g_EntityPool[i].rotation[0] = 0;
            g_EntityPool[i].rotation[1] = 0;
            g_EntityPool[i].rotation[2] = 0;
            g_EntityPool[i].rotation[3] = 1;
        }
    }

    ZZZ_API void UpdateEntities(float totalTime) {
        float speed = g_Settings.waveSpeed;
        float height = g_Settings.waveHeight;
        float freq = g_Settings.waveFrequency;

        for (auto& e : g_EntityPool) {
            float x = e.position[0];
            float z = e.position[2];
            e.position[1] = std::sin(totalTime * speed + x * freq + z * freq) * height;

            float angle = totalTime * 0.5f;
            e.rotation[1] = std::sin(angle);
            e.rotation[3] = std::cos(angle);
        }
    }

    ZZZ_API EntityTransform* GetEntityPtr() {
        return g_EntityPool.data();
    }

    ZZZ_API void UpdateSettings(float speed, float height, float freq) {
        g_Settings.waveSpeed = speed;
        g_Settings.waveHeight = height;
        g_Settings.waveFrequency = freq;
    }
}