using UnityEngine;
using UnityEngine.UI; // 👈 我们需要显示在 UI 上
using System.Runtime.InteropServices;

public class NativeBridge : MonoBehaviour
{
    // 引用 C++ 函数
    [DllImport("ZZZ_Native_Core")]
    private static extern void DrawNativeTriangle(System.IntPtr data, int width, int height, float time);

    [Header("显示屏幕")]
    public RawImage displayScreen; // 用来显示结果的屏幕

    private Texture2D _texture;
    private int _width = 512;
    private int _height = 512;
    private Color32[] _pixelBuffer; // 像素缓存数组
    private GCHandle _pixelHandle;  // 内存句柄 (钉住内存不让 GC 挪动)
    private System.IntPtr _pixelPtr; // 指针

    void Start()
    {
        // 1. 创建一张空的贴图 (RGBA32 格式)
        _texture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
        _texture.filterMode = FilterMode.Point; // 像素风，更硬核

        // 2. 把这张贴图贴到 UI 上
        if (displayScreen != null) displayScreen.texture = _texture;

        // 3. 初始化像素数组
        _pixelBuffer = _texture.GetPixels32();

        // 4. 【关键】获取内存指针！
        // 我们要告诉 C# 垃圾回收器(GC)："别动这个数组！我要把它的地址给 C++ 用！"
        _pixelHandle = GCHandle.Alloc(_pixelBuffer, GCHandleType.Pinned);
        _pixelPtr = _pixelHandle.AddrOfPinnedObject();
    }

    void Update()
    {
        if (_pixelPtr == System.IntPtr.Zero) return;

        // 5. 每帧调用 C++ 绘制！
        // 把指针传过去，C++ 会直接修改 _pixelBuffer 里的数据
        DrawNativeTriangle(_pixelPtr, _width, _height, Time.time);

        // 6. 把修改后的数据应用回贴图
        _texture.SetPixels32(_pixelBuffer);
        _texture.Apply();
    }

    void OnDestroy()
    {
        // 7. 记得释放内存句柄，否则会内存泄漏！
        if (_pixelHandle.IsAllocated) _pixelHandle.Free();
    }
}