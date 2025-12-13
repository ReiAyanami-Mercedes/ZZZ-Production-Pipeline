#include <math.h> //我们需要数学库算坐标

#define DLLExport __declspec(dllexport)

extern "C"
{
    // 简单的加法保留着，当个吉祥物
    DLLExport int NativeAdd(int a, int b) { return a + b; }

    // 🔥🔥🔥 核心函数：绘制三角形 🔥🔥🔥
    // data: 这是一个指向 Unity 贴图原始内存的指针 (RGBA32格式，每个像素4个字节)
    // width, height: 贴图尺寸
    // time: 用来让三角形动起来
    DLLExport void DrawNativeTriangle(unsigned char* data, int width, int height, float time)
    {
        // 遍历每一个像素 (Y行 X列)
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 1. 计算当前像素在内存中的索引 (每个像素占 4 字节: R, G, B, A)
                int index = (y * width + x) * 4;

                // 2. 归一化坐标 (把 x,y 变成 0.0 到 1.0)
                float u = (float)x / width;
                float v = (float)y / height;

                // 3. 三角形数学逻辑 (Barycentric 或者简单的线性方程)
                // 这里我们用一个简单的动态波形模拟三角形的边缘
                // 让三角形随时间旋转/扭曲一点点
                float offset = sin(v * 10.0f + time * 5.0f) * 0.1f;

                // 简单的三角形判定：下半部分宽，上半部分尖
                // abs(u - 0.5) < v : 这是一个倒三角形
                // abs(u - 0.5) < (1.0 - v) : 这是一个正三角形
                bool isInside = fabs(u - 0.5f + offset) < (v * 0.8f);

                if (isInside)
                {
                    // --- 画三角形内部 (赛博绿色) ---
                    data[index + 0] = 0;    // R
                    data[index + 1] = 255;  // G (最亮)
                    data[index + 2] = 100;  // B
                    data[index + 3] = 255;  // Alpha
                }
                else
                {
                    // --- 画背景 (深灰色) ---
                    data[index + 0] = 30;   // R
                    data[index + 1] = 30;   // G
                    data[index + 2] = 30;   // B
                    data[index + 3] = 255;  // Alpha
                }
            }
        }
    }
}