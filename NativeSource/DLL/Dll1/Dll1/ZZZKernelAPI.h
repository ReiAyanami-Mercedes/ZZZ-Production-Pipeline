#pragma once
// ⚠️ 注意：这里引用名字变了！
#include "ZZZKernelTypes.h" 

#ifndef ZZZ_API
#define ZZZ_API __declspec(dllexport)
#endif

extern "C" {
    // [Legacy] 动作接口
    ZZZ_API MotionDataset* CreateTestMotionData(int count);
    ZZZ_API bool GenerateDummyBinFile(const char* filepath, int count);
    ZZZ_API int LoadMotionData(const char* filepath);
    ZZZ_API int SearchBestFrame(MotionDataset* dataset, MotionQuery query);
    ZZZ_API RuntimeFrame* GetDatabasePtr();

    // [Core] 实体接口
    ZZZ_API void InitializeEntities(int count);
    ZZZ_API void UpdateEntities(float totalTime);
    ZZZ_API EntityTransform* GetEntityPtr();
    ZZZ_API void UpdateSettings(float speed, float height, float freq);
}