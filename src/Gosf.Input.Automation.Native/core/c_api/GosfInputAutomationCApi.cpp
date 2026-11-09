#include "GosfInputAutomationCApi.h"

#include "../../include/GosfInputAutomation.h"
#include "../base/Environment.h"
#include "../base/AutomationModes.h"

#include <Windows.h>
#include <cstdint>
#include <string>
#include <utility>

#undef FindWindow
#undef FindWindowEx
#undef SetWindowText

extern "C" BOOL WINAPI DllMain(HINSTANCE hInstance, DWORD reason, LPVOID) {
    if (reason == DLL_PROCESS_ATTACH) {
        RuntimeEnvironment::setInstance(hInstance);
    }
    return TRUE;
}

#define GOSF_WIDEN_TEXT2(text) L##text
#define GOSF_WIDEN_TEXT(text) GOSF_WIDEN_TEXT2(text)

struct gosf_c_context {
    gosf_c_context() = default;
    ~gosf_c_context() = default;

    gosf_c_context(const gosf_c_context &) = delete;
    gosf_c_context &operator=(const gosf_c_context &) = delete;

    gosf::AutomationCore core;
    std::wstring string_result;
};

namespace {

template <typename Func>
int call_int(gosf_handle handle, Func &&func) {
    gosf::AutomationCore *core = handle ? &handle->core : nullptr;
    if (!core)
        return 0;

    try {
        return static_cast<int>(std::forward<Func>(func)(*core));
    } catch (...) {
        return 0;
    }
}

template <typename Func>
int call_ret(gosf_handle handle, Func &&func) {
    return call_int(handle, [&](gosf::AutomationCore &core) {
        long ret = 0;
        std::forward<Func>(func)(core, &ret);
        return ret;
    });
}

template <typename Func>
intptr_t call_intptr(gosf_handle handle, Func &&func) {
    if (!handle)
        return 0;

    try {
        LONG_PTR ret = 0;
        std::forward<Func>(func)(handle->core, &ret);
        return static_cast<intptr_t>(ret);
    } catch (...) {
        return 0;
    }
}

template <typename Func>
const wchar_t *call_string(gosf_handle handle, Func &&func) {
    if (!handle)
        return L"";

    try {
        handle->string_result.clear();
        std::forward<Func>(func)(handle->core, handle->string_result);
        return handle->string_result.c_str();
    } catch (...) {
        return L"";
    }
}

const wchar_t *safe_text(const wchar_t *text) { return text ? text : L""; }
void out_int(int *target, long value) { if (target) *target = static_cast<int>(value); }
} // namespace

// Lifecycle

gosf_handle GOSF_CALL GosfCreate(void) {
    try {
        return new gosf_c_context();
    } catch (...) {
        return nullptr;
    }
}

void GOSF_CALL GosfDestroy(gosf_handle handle) {
    delete handle;
}

const wchar_t *GOSF_CALL GosfVer(void) {
    return GOSF_WIDEN_TEXT(GOSF_VERSION);
}

// Background binding

int GOSF_CALL GosfBindWindow(gosf_handle handle, intptr_t hwnd, const wchar_t *display, const wchar_t *mouse,
                         const wchar_t *keypad, int mode) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) {
        core.BindWindow(static_cast<LONG_PTR>(hwnd), safe_text(display), safe_text(mouse), safe_text(keypad), mode, ret);
    });
}

int GOSF_CALL GosfBindWindowEx(gosf_handle handle, intptr_t display_hwnd, intptr_t input_hwnd, const wchar_t *display,
                           const wchar_t *mouse, const wchar_t *keypad, int mode) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) {
        core.BindWindowEx(static_cast<LONG_PTR>(display_hwnd), static_cast<LONG_PTR>(input_hwnd), safe_text(display),
                        safe_text(mouse), safe_text(keypad), mode, ret);
    });
}

int GOSF_CALL GosfUnBindWindow(gosf_handle handle) {
    return call_ret(handle, [](gosf::AutomationCore &core, long *ret) { core.UnBindWindow(ret); });
}

int GOSF_CALL GosfUnbindWindow(gosf_handle handle) {
    return GosfUnBindWindow(handle);
}

int GOSF_CALL GosfLockInput(gosf_handle handle, int lock) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) { core.LockInput(lock, ret); });
}

intptr_t GOSF_CALL GosfGetBindWindow(gosf_handle handle) {
    return call_intptr(handle, [](gosf::AutomationCore &core, LONG_PTR *ret) { core.GetBindWindow(ret); });
}

int GOSF_CALL GosfIsBind(gosf_handle handle) {
    return call_ret(handle, [](gosf::AutomationCore &core, long *ret) { core.IsBind(ret); });
}

// Mouse

int GOSF_CALL GosfGetCursorPos(gosf_handle handle, int *x, int *y) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) {
        long lx = 0, ly = 0;
        core.GetCursorPos(&lx, &ly, ret);
        out_int(x, lx);
        out_int(y, ly);
    });
}

const wchar_t *GOSF_CALL GosfGetCursorShape(gosf_handle handle) {
    return call_string(handle, [](gosf::AutomationCore &core, std::wstring &ret) { core.GetCursorShape(ret); });
}

int GOSF_CALL GosfMoveR(gosf_handle handle, int x, int y) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) { core.MoveR(x, y, ret); });
}

int GOSF_CALL GosfMoveTo(gosf_handle handle, int x, int y) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) { core.MoveTo(x, y, ret); });
}

const wchar_t *GOSF_CALL GosfMoveToEx(gosf_handle handle, int x, int y, int w, int h) {
    return call_string(handle, [&](gosf::AutomationCore &core, std::wstring &ret) { core.MoveToEx(x, y, w, h, ret); });
}

int GOSF_CALL GosfMoveToSmooth(gosf_handle handle, int x, int y, int duration) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) { core.MoveToSmooth(x, y, duration, ret); });
}

const wchar_t *GOSF_CALL GosfMoveToExSmooth(gosf_handle handle, int x, int y, int w, int h, int duration) {
    return call_string(handle, [&](gosf::AutomationCore &core, std::wstring &ret) { core.MoveToExSmooth(x, y, w, h, duration, ret); });
}

int GOSF_CALL GosfMovePath(gosf_handle handle, const wchar_t *path, int duration) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) { core.MovePath(safe_text(path), duration, ret); });
}

int GOSF_CALL GosfDragPath(gosf_handle handle, const wchar_t *path, int duration) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) { core.DragPath(safe_text(path), duration, ret); });
}

int GOSF_CALL GosfSetMouseTrajectory(gosf_handle handle, int mode, int min_duration, int max_duration, int jitter,
                                 int start_delay, int end_delay) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) {
        core.SetMouseTrajectory(mode, min_duration, max_duration, jitter, start_delay, end_delay, ret);
    });
}

#define GOSF_MOUSE_RET(name, method) \
    int GOSF_CALL name(gosf_handle handle) { return call_ret(handle, [](gosf::AutomationCore &core, long *ret) { core.method(ret); }); }

GOSF_MOUSE_RET(GosfLeftClick, LeftClick)
GOSF_MOUSE_RET(GosfLeftDoubleClick, LeftDoubleClick)
GOSF_MOUSE_RET(GosfLeftDown, LeftDown)
GOSF_MOUSE_RET(GosfLeftUp, LeftUp)
GOSF_MOUSE_RET(GosfMiddleClick, MiddleClick)
GOSF_MOUSE_RET(GosfMiddleDoubleClick, MiddleDoubleClick)
GOSF_MOUSE_RET(GosfMiddleDown, MiddleDown)
GOSF_MOUSE_RET(GosfMiddleUp, MiddleUp)
GOSF_MOUSE_RET(GosfRightClick, RightClick)
GOSF_MOUSE_RET(GosfRightDoubleClick, RightDoubleClick)
GOSF_MOUSE_RET(GosfRightDown, RightDown)
GOSF_MOUSE_RET(GosfRightUp, RightUp)
GOSF_MOUSE_RET(GosfXButton1Click, XButton1Click)
GOSF_MOUSE_RET(GosfXButton1DoubleClick, XButton1DoubleClick)
GOSF_MOUSE_RET(GosfXButton1Down, XButton1Down)
GOSF_MOUSE_RET(GosfXButton1Up, XButton1Up)
GOSF_MOUSE_RET(GosfXButton2Click, XButton2Click)
GOSF_MOUSE_RET(GosfXButton2DoubleClick, XButton2DoubleClick)
GOSF_MOUSE_RET(GosfXButton2Down, XButton2Down)
GOSF_MOUSE_RET(GosfXButton2Up, XButton2Up)
GOSF_MOUSE_RET(GosfWheelDown, WheelDown)
GOSF_MOUSE_RET(GosfWheelUp, WheelUp)

#undef GOSF_MOUSE_RET

int GOSF_CALL GosfWheel(gosf_handle handle, int delta) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) { core.Wheel(delta, ret); });
}

int GOSF_CALL GosfHWheel(gosf_handle handle, int delta) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) { core.HWheel(delta, ret); });
}

int GOSF_CALL GosfSetMouseDelay(gosf_handle handle, const wchar_t *type, int delay) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) { core.SetMouseDelay(safe_text(type), delay, ret); });
}

// Keyboard

int GOSF_CALL GosfGetKeyState(gosf_handle handle, int vk_code) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) { core.GetKeyState(vk_code, ret); });
}

int GOSF_CALL GosfKeyDown(gosf_handle handle, int vk_code) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) { core.KeyDown(vk_code, ret); });
}

int GOSF_CALL GosfKeyDownChar(gosf_handle handle, const wchar_t *vk_code) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) { core.KeyDownChar(safe_text(vk_code), ret); });
}

int GOSF_CALL GosfKeyUp(gosf_handle handle, int vk_code) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) { core.KeyUp(vk_code, ret); });
}

int GOSF_CALL GosfKeyUpChar(gosf_handle handle, const wchar_t *vk_code) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) { core.KeyUpChar(safe_text(vk_code), ret); });
}

int GOSF_CALL GosfWaitKey(gosf_handle handle, int vk_code, int time_out) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) { core.WaitKey(vk_code, time_out, ret); });
}

int GOSF_CALL GosfKeyPress(gosf_handle handle, int vk_code) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) { core.KeyPress(vk_code, ret); });
}

int GOSF_CALL GosfKeyPressChar(gosf_handle handle, const wchar_t *vk_code) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) { core.KeyPressChar(safe_text(vk_code), ret); });
}

int GOSF_CALL GosfSetKeypadDelay(gosf_handle handle, const wchar_t *type, int delay) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) { core.SetKeypadDelay(safe_text(type), delay, ret); });
}

int GOSF_CALL GosfKeyPressStr(gosf_handle handle, const wchar_t *key_str, int delay) {
    return call_ret(handle, [&](gosf::AutomationCore &core, long *ret) { core.KeyPressStr(safe_text(key_str), delay, ret); });
}

