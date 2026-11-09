#pragma once
#include <atomic>
#include <cstddef>
#include <cstdint>
#include <memory>
#include <string>
#include <vector>
#include <BaseTsd.h>
#ifdef U_STATIC_IMPLEMENTATION
#define GOSF_API
#else
#ifndef GOSF_API
#if defined(GOSF_EXPORTS)
#define GOSF_API __declspec(dllexport)
#else
#define GOSF_API __declspec(dllimport)
#endif
#endif
#endif
// gosf::AutomationCore
#undef FindWindow
#undef FindWindowEx
#undef SetWindowText

#ifdef _MSC_VER
#pragma warning(push)
#pragma warning(disable : 4251)
#endif

namespace gosf {

namespace internal {
struct AutomationContext;
}

class GOSF_API AutomationCore {
  public:
    AutomationCore();
    ~AutomationCore();
    AutomationCore(const AutomationCore &) = delete;
    AutomationCore &operator=(const AutomationCore &) = delete;
  private:
    std::unique_ptr<internal::AutomationContext> m_context;
    static std::atomic<int> s_id;
  public:
    // 兼容旧接口的单句柄绑定。显示和输入都使用同一个 hwnd。
    void BindWindow(_In_ LONG_PTR hwnd, _In_ const wchar_t *display, _In_ const wchar_t *mouse, _In_ const wchar_t *keypad,
                    _In_ long mode, _Out_ long *ret);
    // 扩展绑定接口。显示截图使用 display_hwnd，鼠标和键盘输入使用 input_hwnd。
    void BindWindowEx(_In_ LONG_PTR display_hwnd, _In_ LONG_PTR input_hwnd, _In_ const wchar_t *display,
                      _In_ const wchar_t *mouse, _In_ const wchar_t *keypad, _In_ long mode, _Out_ long *ret);
    // 解绑窗口
    void UnBindWindow(_Out_ long *ret);
    // 临时锁定目标窗口的外部输入。只对 dx 鼠标、dx 键盘有效。
    void LockInput(_In_ long lock, _Out_ long *ret);
    // 获取当前对象已经绑定的显示窗口句柄. 无绑定返回0
    void GetBindWindow(_Out_ LONG_PTR *ret);
    // 判定当前对象是否已绑定窗口.
    void IsBind(_Out_ long *ret);
    //--------------------mouse & keyboard------------------
    // 获取鼠标位置.
    void GetCursorPos(_Out_ long *x, _Out_ long *y, _Out_ long *ret);
    // 获取当前鼠标形状: visible,hash,width,height,hotX,hotY.
    void GetCursorShape(_Out_ std::wstring &ret);
    // 鼠标相对于上次的位置移动rx,ry.
    void MoveR(_In_ long x, _In_ long y, _Out_ long *ret);
    // 把鼠标移动到目的点(x,y)
    void MoveTo(_In_ long x, _In_ long y, _Out_ long *ret);
    // 把鼠标移动到目的范围内的任意一点
    void MoveToEx(_In_ long x, _In_ long y, _In_ long w, _In_ long h, _Out_ std::wstring &ret);
    // 按模拟轨迹移动到指定点
    void MoveToSmooth(_In_ long x, _In_ long y, _In_ long duration, _Out_ long *ret);
    // 在目标范围内随机取点，并按模拟轨迹移动过去
    void MoveToExSmooth(_In_ long x, _In_ long y, _In_ long w, _In_ long h, _In_ long duration,
                        _Out_ std::wstring &ret);
    // 按 x,y|x,y 格式提供的路径移动
    void MovePath(_In_ const wchar_t *path, _In_ long duration, _Out_ long *ret);
    // 按 x,y|x,y 格式提供的路径拖拽
    void DragPath(_In_ const wchar_t *path, _In_ long duration, _Out_ long *ret);
    // 设置模拟轨迹参数，只影响 Smooth 和 Path 系列鼠标移动
    void SetMouseTrajectory(_In_ long mode, _In_ long min_duration, _In_ long max_duration, _In_ long jitter,
                            _In_ long start_delay, _In_ long end_delay, _Out_ long *ret);
    // 按下鼠标左键
    void LeftClick(_Out_ long *ret);
    // 双击鼠标左键
    void LeftDoubleClick(_Out_ long *ret);
    // 按住鼠标左键
    void LeftDown(_Out_ long *ret);
    // 弹起鼠标左键
    void LeftUp(_Out_ long *ret);
    // 按下鼠标中键
    void MiddleClick(long *ret);
    // 双击鼠标中键
    void MiddleDoubleClick(long *ret);
    // 按住鼠标中键
    void MiddleDown(_Out_ long *ret);
    // 弹起鼠标中键
    void MiddleUp(_Out_ long *ret);
    // 按下鼠标右键
    void RightClick(long *ret);
    // 双击鼠标右键
    void RightDoubleClick(long *ret);
    // 按住鼠标右键
    void RightDown(_Out_ long *ret);
    // 弹起鼠标右键
    void RightUp(long *ret);
    // 按下鼠标侧键1
    void XButton1Click(long *ret);
    // 双击鼠标侧键1
    void XButton1DoubleClick(long *ret);
    // 按住鼠标侧键1
    void XButton1Down(long *ret);
    // 弹起鼠标侧键1
    void XButton1Up(long *ret);
    // 按下鼠标侧键2
    void XButton2Click(long *ret);
    // 双击鼠标侧键2
    void XButton2DoubleClick(long *ret);
    // 按住鼠标侧键2
    void XButton2Down(long *ret);
    // 弹起鼠标侧键2
    void XButton2Up(long *ret);
    // 垂直滚轮滚动指定 delta
    void Wheel(long delta, long *ret);
    // 水平滚轮滚动指定 delta
    void HWheel(long delta, long *ret);
    // 滚轮向下滚
    void WheelDown(_Out_ long *ret);
    // 滚轮向上滚
    void WheelUp(_Out_ long *ret);
    // 设置鼠标单击或者双击时,鼠标按下和弹起的时间间隔
    void SetMouseDelay(_In_ const wchar_t *type, _In_ long delay, _Out_ long *ret);
    // 获取指定的按键状态.(前台信息,不是后台)
    void GetKeyState(_In_ long vk_code, _Out_ long *ret);
    // 按住指定的虚拟键码
    void KeyDown(_In_ long vk_code, _Out_ long *ret);
    // 按住指定的虚拟键码
    void KeyDownChar(_In_ const wchar_t *vk_code, _Out_ long *ret);
    // 弹起来虚拟键vk_code
    void KeyUp(_In_ long vk_code, _Out_ long *ret);
    // 弹起来虚拟键vk_code
    void KeyUpChar(_In_ const wchar_t *vk_code, _Out_ long *ret);
    // 等待指定的按键按下 (前台,不是后台)
    void WaitKey(_In_ long vk_code, _In_ long time_out, _Out_ long *ret);
    // 发送字符串
    // long SendString(long HWND)
    // 弹起来虚拟键vk_code
    void KeyPress(_In_ long vk_code, _Out_ long *ret);
    void KeyPressChar(_In_ const wchar_t *vk_code, _Out_ long *ret);
    // 设置按键时,键盘按下和弹起的时间间隔
    void SetKeypadDelay(_In_ const wchar_t *type, _In_ long delay, _Out_ long *ret);
    // 根据指定的字符串序列，依次按顺序按下其中的字符
    void KeyPressStr(_In_ const wchar_t *key_str, _In_ long delay, _Out_ long *ret);
};
} // namespace gosf
#ifdef _MSC_VER
#pragma warning(pop)
#endif
