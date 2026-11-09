#include "WpfInputState.h"
#include "ApiResolver.h"
#include <cwchar>
#include <atomic>

namespace gosf::hook::wpf_input {
namespace {
HWND target = nullptr;
DWORD targetThread = 0;
std::atomic<bool> active{false};
UINT cleanupMessage = 0;
POINT cursor = {};
HWND focus = nullptr;
HWND capture = nullptr;
WNDPROC windowProcedure = nullptr;
decltype(&GetCursorPos) originalGetCursorPos = nullptr;
decltype(&GetMessagePos) originalGetMessagePos = nullptr;
decltype(&WindowFromPoint) originalWindowFromPoint = nullptr;
decltype(&GetFocus) originalGetFocus = nullptr;
decltype(&SetFocus) originalSetFocus = nullptr;
decltype(&GetActiveWindow) originalGetActiveWindow = nullptr;
decltype(&SetActiveWindow) originalSetActiveWindow = nullptr;
decltype(&SetForegroundWindow) originalSetForegroundWindow = nullptr;
decltype(&GetCapture) originalGetCapture = nullptr;
decltype(&SetCapture) originalSetCapture = nullptr;
decltype(&ReleaseCapture) originalReleaseCapture = nullptr;

bool TargetThread() {
    return target && GetCurrentThreadId() == targetThread;
}

bool OwnWindow(HWND hwnd) {
    return hwnd == target || (hwnd && IsChild(target, hwnd));
}

BOOL WINAPI VirtualGetCursorPos(LPPOINT point) {
    if (!ActiveThread() || !point) return originalGetCursorPos(point);
    *point = cursor;
    return TRUE;
}

DWORD WINAPI VirtualGetMessagePos() {
    return ActiveThread() ? MAKELONG(cursor.x, cursor.y) : originalGetMessagePos();
}

HWND WINAPI VirtualWindowFromPoint(POINT point) {
    if (ActiveThread() && point.x == cursor.x && point.y == cursor.y) return target;
    return originalWindowFromPoint(point);
}

HWND WINAPI VirtualGetFocus() {
    return ActiveThread() ? focus : originalGetFocus();
}

HWND WINAPI VirtualSetFocus(HWND hwnd) {
    if (!TargetThread() || (hwnd && !OwnWindow(hwnd))) return originalSetFocus(hwnd);
    // Guard activation from the moment hooks are installed, including rebind before first input.
    const auto previous = focus;
    focus = hwnd;
    return previous;
}

HWND WINAPI VirtualGetActiveWindow() {
    return ActiveThread() ? target : originalGetActiveWindow();
}

HWND WINAPI VirtualSetActiveWindow(HWND hwnd) {
    if (TargetThread() && OwnWindow(hwnd)) return target;
    return originalSetActiveWindow(hwnd);
}

BOOL WINAPI VirtualSetForegroundWindow(HWND hwnd) {
    if (TargetThread() && OwnWindow(hwnd)) return TRUE;
    return originalSetForegroundWindow(hwnd);
}

HWND WINAPI VirtualGetCapture() {
    return ActiveThread() ? capture : originalGetCapture();
}

HWND WINAPI VirtualSetCapture(HWND hwnd) {
    if (!ActiveThread() || !OwnWindow(hwnd)) return originalSetCapture(hwnd);
    const auto previous = capture;
    capture = hwnd;
    return previous;
}

BOOL WINAPI VirtualReleaseCapture() {
    if (!ActiveThread()) return originalReleaseCapture();
    const auto previous = capture;
    capture = nullptr;
    if (previous && windowProcedure) CallWindowProc(windowProcedure, previous, WM_CAPTURECHANGED, 0, 0);
    return TRUE;
}

template<typename T>
bool Add(HookInstaller install, const char* name, T detour, T& original) {
    return install(ResolveApi("user32.dll", name), reinterpret_cast<void*>(detour),
        reinterpret_cast<void**>(&original));
}

bool InstallCursorHooks(HookInstaller install) {
    return Add(install, "GetCursorPos", VirtualGetCursorPos, originalGetCursorPos)
        && Add(install, "GetMessagePos", VirtualGetMessagePos, originalGetMessagePos)
        && Add(install, "WindowFromPoint", VirtualWindowFromPoint, originalWindowFromPoint);
}

bool InstallFocusHooks(HookInstaller install) {
    return Add(install, "GetFocus", VirtualGetFocus, originalGetFocus)
        && Add(install, "SetFocus", VirtualSetFocus, originalSetFocus)
        && Add(install, "GetActiveWindow", VirtualGetActiveWindow, originalGetActiveWindow)
        && Add(install, "SetActiveWindow", VirtualSetActiveWindow, originalSetActiveWindow)
        && Add(install, "SetForegroundWindow", VirtualSetForegroundWindow, originalSetForegroundWindow);
}

bool InstallCaptureHooks(HookInstaller install) {
    return Add(install, "GetCapture", VirtualGetCapture, originalGetCapture)
        && Add(install, "SetCapture", VirtualSetCapture, originalSetCapture)
        && Add(install, "ReleaseCapture", VirtualReleaseCapture, originalReleaseCapture);
}
}

bool ActiveThread() {
    return active && TargetThread();
}

bool Install(HWND hwnd, HookInstaller install) {
    wchar_t className[256] = {};
    if (!GetClassNameW(hwnd, className, 256)) return false;
    if (wcsncmp(className, L"HwndWrapper[", 12) != 0) return true;
    cleanupMessage = RegisterWindowMessageW(L"Gosf.Input.Automation.WpfCleanup.v1");
    if (!cleanupMessage) return false;
    target = hwnd;
    targetThread = GetWindowThreadProcessId(hwnd, nullptr);
    return InstallCursorHooks(install) && InstallFocusHooks(install) && InstallCaptureHooks(install);
}

void PrepareMessage(HWND hwnd, UINT message, LPARAM lparam, WNDPROC original) {
    if (hwnd != target) return;
    windowProcedure = original;
    if (message >= WM_MOUSEFIRST && message <= WM_MOUSELAST) {
        cursor = POINT{static_cast<SHORT>(LOWORD(lparam)), static_cast<SHORT>(HIWORD(lparam))};
        if (message != WM_MOUSEWHEEL && message != WM_MOUSEHWHEEL) ClientToScreen(hwnd, &cursor);
    }
    if (active) return;
    active = true;
    focus = target;
    CallWindowProc(original, hwnd, WM_SETFOCUS, 0, 0);
}

// Synthetic messages call the original WndProc directly; only external messages reach here.
bool SuppressMessage(UINT message) {
    if (!ActiveThread()) return false;
    return (message >= WM_MOUSEFIRST && message <= WM_MOUSELAST)
        || message == WM_MOUSELEAVE || message == WM_CAPTURECHANGED || message == WM_KILLFOCUS;
}

void TranslateKey(WPARAM key, LPARAM lparam) {
    if (!ActiveThread()) return;
    BYTE state[256] = {};
    GetKeyboardState(state);
    if ((state[VK_CONTROL] | state[VK_MENU]) & 0x80) return;
    wchar_t text[16] = {};
    // Avoid modifying the user's kernel dead-key buffer while translating virtual keys.
    const int count = ToUnicodeEx(static_cast<UINT>(key), (lparam >> 16) & 0xFF,
        state, text, 16, 4, GetKeyboardLayout(targetThread));
    for (int i = 0; i < count && i < 16; ++i)
        CallWindowProc(windowProcedure, target, WM_CHAR, text[i], lparam);
}

bool HandleCleanup(UINT message) {
    if (!cleanupMessage || message != cleanupMessage) return false;
    if (!ActiveThread()) return true;
    VirtualReleaseCapture();
    focus = nullptr;
    CallWindowProc(windowProcedure, target, WM_KILLFOCUS, 0, 0);
    active = false;
    CallWindowProc(windowProcedure, target, WM_MOUSELEAVE, 0, 0);
    return true;
}

bool RequestCleanup() {
    if (!target || !active || !IsWindow(target)) return true;
    DWORD_PTR result = 0;
    return SendMessageTimeoutW(target, cleanupMessage, 0, 0,
        SMTO_ABORTIFHUNG | SMTO_BLOCK, 2000, &result) != 0;
}

void Reset() {
    active = false;
    target = focus = capture = nullptr;
    targetThread = 0;
    cursor = {};
    windowProcedure = nullptr;
    cleanupMessage = 0;
}
}

