// ⚠️ 必须在第一行，解决 fopen 报错
#define _CRT_SECURE_NO_WARNINGS 

#include "MotionData.h" 
#include <cstdio>       
#include <vector>       
#include <cmath>        
#include <cstring>
#include <cfloat> // for FLT_MAX

// =========================================================
// 全局变量定义 (整个 DLL 只有这一份)
// =========================================================
std::vector<RuntimeFrame> g_MotionDatabase; // 真正的内存池
MotionDataset g_DatasetWrapper;             // 给 C# 看的包装壳

// 宏定义：导出接口
#define ZZZ_API __declspec(dllexport)

extern "C" {

    // =========================================================
    // 1. 生成假数据 (Legacy Support)
    // =========================================================
    ZZZ_API MotionDataset* CreateTestMotionData(int count) {
        g_MotionDatabase.resize(count);

        for (int i = 0; i < count; i++) {
            float t = i * 0.1f;
            // 造一点螺旋线数据，方便你在 Unity 里看到东西
            g_MotionDatabase[i].Pos[0] = sinf(t) * 5.0f;
            g_MotionDatabase[i].Pos[1] = 0.0f;
            g_MotionDatabase[i].Pos[2] = cosf(t) * 5.0f;
            g_MotionDatabase[i].Pos[3] = 1.0f;

            g_MotionDatabase[i].Rot[0] = 0.0f; g_MotionDatabase[i].Rot[1] = 0.0f;
            g_MotionDatabase[i].Rot[2] = 0.0f; g_MotionDatabase[i].Rot[3] = 1.0f;

            g_MotionDatabase[i].Velocity[0] = cosf(t);
            g_MotionDatabase[i].Velocity[1] = 0.0f;
            g_MotionDatabase[i].Velocity[2] = -sinf(t);
            g_MotionDatabase[i].Velocity[3] = 0.0f;
        }

        // 更新包装器
        g_DatasetWrapper.Frames = g_MotionDatabase.data();
        g_DatasetWrapper.Count = count;
        return &g_DatasetWrapper;
    }

    // =========================================================
    // 2. 生成二进制文件 (工具函数)
    // =========================================================
    ZZZ_API bool GenerateDummyBinFile(const char* filepath, int count) {
        FILE* file = fopen(filepath, "wb");
        if (!file) return false;

        MotionFileHeader header = { 0x5A5A5A31, 1, (uint32_t)count, 60.0f };
        fwrite(&header, sizeof(MotionFileHeader), 1, file);

        // 借用上面的逻辑生成数据
        CreateTestMotionData(count);

        // 转换并写入
        std::vector<DiskFrame> diskBuffer(count);
        for (int i = 0; i < count; i++) {
            diskBuffer[i].Pos[0] = g_MotionDatabase[i].Pos[0];
            diskBuffer[i].Pos[1] = g_MotionDatabase[i].Pos[1];
            diskBuffer[i].Pos[2] = g_MotionDatabase[i].Pos[2];
            // ... 简化起见只拷贝位置，实际项目要拷贝全 ...
        }
        fwrite(diskBuffer.data(), sizeof(DiskFrame), count, file);
        fclose(file);
        return true;
    }

    // =========================================================
    // 3. 加载二进制文件 (Core Feature)
    // =========================================================
    ZZZ_API int LoadMotionData(const char* filepath) {
        FILE* file = fopen(filepath, "rb");
        if (!file) return -1;

        MotionFileHeader header;
        fread(&header, sizeof(MotionFileHeader), 1, file);

        if (header.Magic != 0x5A5A5A31) { fclose(file); return -2; }

        // 准备内存
        g_MotionDatabase.resize(header.FrameCount);
        std::vector<DiskFrame> tempBuffer(header.FrameCount);
        fread(tempBuffer.data(), sizeof(DiskFrame), header.FrameCount, file);

        // 解压 (Unpack)
        for (uint32_t i = 0; i < header.FrameCount; i++) {
            g_MotionDatabase[i].Pos[0] = tempBuffer[i].Pos[0];
            g_MotionDatabase[i].Pos[1] = tempBuffer[i].Pos[1];
            g_MotionDatabase[i].Pos[2] = tempBuffer[i].Pos[2];
            g_MotionDatabase[i].Pos[3] = 1.0f;
            // 这里记得把 Rot 和 Velocity 也加上，逻辑同上
        }

        fclose(file);

        // 关键：必须更新 wrapper，否则 Unity 拿不到指针
        g_DatasetWrapper.Frames = g_MotionDatabase.data();
        g_DatasetWrapper.Count = (int)header.FrameCount;

        return (int)header.FrameCount;
    }

    // =========================================================
    // 4. 搜索算法 (Motion Matching Logic)
    // =========================================================
    ZZZ_API int SearchBestFrame(MotionDataset* dataset, MotionQuery query) {
        // 如果传入空指针，就用全局的兜底
        if (!dataset) dataset = &g_DatasetWrapper;
        if (!dataset->Frames || dataset->Count == 0) return -1;

        float minCost = FLT_MAX;
        int bestIndex = 0;

        // 暴力搜索
        for (int i = 0; i < dataset->Count; i++) {
            RuntimeFrame& frame = dataset->Frames[i];

            float dx = frame.Pos[0] - query.Target.Pos[0];
            float dy = frame.Pos[1] - query.Target.Pos[1];
            float dz = frame.Pos[2] - query.Target.Pos[2];

            float cost = (dx * dx + dy * dy + dz * dz);

            if (cost < minCost) {
                minCost = cost;
                bestIndex = i;
            }
        }
        return bestIndex;
    }

    // 辅助接口
    ZZZ_API RuntimeFrame* GetDatabasePtr() {
        return g_MotionDatabase.data();
    }
}