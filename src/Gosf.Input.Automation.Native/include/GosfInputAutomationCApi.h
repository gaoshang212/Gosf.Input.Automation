#pragma once

#include <stddef.h>
#include <stdint.h>

#if defined(_WIN32)
#define GOSF_CALL __stdcall
#if defined(GOSF_C_API_STATIC)
#define GOSF_C_API
#elif defined(GOSF_C_API_EXPORTS)
#define GOSF_C_API __declspec(dllexport)
#else
#define GOSF_C_API __declspec(dllimport)
#endif
#else
#define GOSF_CALL
#if defined(GOSF_C_API_STATIC)
#define GOSF_C_API
#elif defined(GOSF_C_API_EXPORTS)
#define GOSF_C_API __attribute__((visibility("default")))
#else
#define GOSF_C_API
#endif
#endif

#ifdef __cplusplus
extern "C" {
#endif

typedef struct gosf_c_context gosf_c_context;
typedef gosf_c_context *gosf_handle;

// Lifecycle
GOSF_C_API gosf_handle GOSF_CALL GosfCreate(void);
GOSF_C_API void GOSF_CALL GosfDestroy(gosf_handle handle);

GOSF_C_API const wchar_t *GOSF_CALL GosfVer(void);

// Background binding
GOSF_C_API int GOSF_CALL GosfBindWindow(gosf_handle handle, intptr_t hwnd, const wchar_t *display, const wchar_t *mouse,
                                  const wchar_t *keypad, int mode);
GOSF_C_API int GOSF_CALL GosfBindWindowEx(gosf_handle handle, intptr_t display_hwnd, intptr_t input_hwnd,
                                    const wchar_t *display, const wchar_t *mouse, const wchar_t *keypad, int mode);
GOSF_C_API int GOSF_CALL GosfUnBindWindow(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfLockInput(gosf_handle handle, int lock);
GOSF_C_API intptr_t GOSF_CALL GosfGetBindWindow(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfIsBind(gosf_handle handle);

// Mouse
GOSF_C_API int GOSF_CALL GosfGetCursorPos(gosf_handle handle, int *x, int *y);
GOSF_C_API const wchar_t *GOSF_CALL GosfGetCursorShape(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfMoveR(gosf_handle handle, int x, int y);
GOSF_C_API int GOSF_CALL GosfMoveTo(gosf_handle handle, int x, int y);
GOSF_C_API const wchar_t *GOSF_CALL GosfMoveToEx(gosf_handle handle, int x, int y, int w, int h);
GOSF_C_API int GOSF_CALL GosfMoveToSmooth(gosf_handle handle, int x, int y, int duration);
GOSF_C_API const wchar_t *GOSF_CALL GosfMoveToExSmooth(gosf_handle handle, int x, int y, int w, int h, int duration);
GOSF_C_API int GOSF_CALL GosfMovePath(gosf_handle handle, const wchar_t *path, int duration);
GOSF_C_API int GOSF_CALL GosfDragPath(gosf_handle handle, const wchar_t *path, int duration);
GOSF_C_API int GOSF_CALL GosfSetMouseTrajectory(gosf_handle handle, int mode, int min_duration, int max_duration, int jitter,
                                          int start_delay, int end_delay);
GOSF_C_API int GOSF_CALL GosfLeftClick(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfLeftDoubleClick(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfLeftDown(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfLeftUp(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfMiddleClick(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfMiddleDoubleClick(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfMiddleDown(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfMiddleUp(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfRightClick(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfRightDoubleClick(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfRightDown(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfRightUp(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfXButton1Click(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfXButton1DoubleClick(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfXButton1Down(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfXButton1Up(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfXButton2Click(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfXButton2DoubleClick(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfXButton2Down(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfXButton2Up(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfWheel(gosf_handle handle, int delta);
GOSF_C_API int GOSF_CALL GosfHWheel(gosf_handle handle, int delta);
GOSF_C_API int GOSF_CALL GosfWheelDown(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfWheelUp(gosf_handle handle);
GOSF_C_API int GOSF_CALL GosfSetMouseDelay(gosf_handle handle, const wchar_t *type, int delay);

// Keyboard
GOSF_C_API int GOSF_CALL GosfGetKeyState(gosf_handle handle, int vk_code);
GOSF_C_API int GOSF_CALL GosfKeyDown(gosf_handle handle, int vk_code);
GOSF_C_API int GOSF_CALL GosfKeyDownChar(gosf_handle handle, const wchar_t *vk_code);
GOSF_C_API int GOSF_CALL GosfKeyUp(gosf_handle handle, int vk_code);
GOSF_C_API int GOSF_CALL GosfKeyUpChar(gosf_handle handle, const wchar_t *vk_code);
GOSF_C_API int GOSF_CALL GosfWaitKey(gosf_handle handle, int vk_code, int time_out);
GOSF_C_API int GOSF_CALL GosfKeyPress(gosf_handle handle, int vk_code);
GOSF_C_API int GOSF_CALL GosfKeyPressChar(gosf_handle handle, const wchar_t *vk_code);
GOSF_C_API int GOSF_CALL GosfSetKeypadDelay(gosf_handle handle, const wchar_t *type, int delay);
GOSF_C_API int GOSF_CALL GosfKeyPressStr(gosf_handle handle, const wchar_t *key_str, int delay);


#ifdef __cplusplus
}
#endif
