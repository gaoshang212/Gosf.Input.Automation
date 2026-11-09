#include "BindingSession.h"
#include "../base/AutomationModes.h"
#include "../hook/InputHookClient.h"
#include "../input/keyboard/DxKeyboard.h"
#include "../input/keyboard/WinKeyboard.h"
#include "../input/mouse/DxMouse.h"

namespace gosf::binding {

BindingSession::BindingSession()
    : _mouse(std::make_unique<gosf::input::WinMouse>()),
      _keyboard(std::make_unique<gosf::input::WinKeyboard>()) {}

BindingSession::~BindingSession() {
    reset_bind_state(false);
}

long BindingSession::BindWindow(LONG_PTR hwnd, const std::wstring &display, const std::wstring &mouse,
                                const std::wstring &keypad, long mode) {
    return BindWindowEx(hwnd, hwnd, display, mouse, keypad, mode);
}

long BindingSession::BindWindowEx(LONG_PTR display_hwnd, LONG_PTR input_hwnd, const std::wstring &display,
                                  const std::wstring &mouse, const std::wstring &keypad, long mode) {
    reset_bind_state(false);
    (void)mode;

    HWND displayWnd = display_hwnd == 0 ? GetDesktopWindow() : reinterpret_cast<HWND>(display_hwnd);
    HWND inputWnd = input_hwnd == 0 ? displayWnd : reinterpret_cast<HWND>(input_hwnd);
    if (!::IsWindow(displayWnd) || !::IsWindow(inputWnd) || display != L"normal") {
        reset_bind_state(true);
        return 0;
    }

    int mouseMode = mouse == L"normal" ? IN_NORMAL : mouse == L"windows" ? IN_WINDOWS : mouse == L"dx" ? IN_DX : -1;
    int keypadMode = keypad == L"normal" ? IN_NORMAL : keypad == L"normal.hd" ? IN_NORMAL2 :
                     keypad == L"windows" ? IN_WINDOWS : keypad == L"dx" ? IN_DX : -1;
    if (mouseMode < 0 || keypadMode < 0) {
        reset_bind_state(true);
        return 0;
    }

    _display_hwnd = displayWnd;
    _input_hwnd = inputWnd;
    _mouse_mode = mouseMode;
    _keypad_mode = keypadMode;
    if (mouseMode == IN_DX)
        _mouse = std::make_unique<gosf::input::DxMouse>();
    else
        _mouse = std::make_unique<gosf::input::WinMouse>();
    if (keypadMode == IN_DX)
        _keyboard = std::make_unique<gosf::input::DxKeyboard>();
    else
        _keyboard = std::make_unique<gosf::input::WinKeyboard>();

    if (_mouse->Bind(inputWnd, mouseMode) != 1 || _keyboard->Bind(inputWnd, keypadMode) != 1) {
        reset_bind_state(true);
        return 0;
    }

    _is_bind = 1;
    return 1;
}

long BindingSession::UnBindWindow() {
    return reset_bind_state(true);
}

long BindingSession::LockInput(long lock) {
    if (lock < 0 || lock > 3)
        return 0;
    if (lock == 0)
        return gosf::hook::input_hook_client::LockInput(_input_hwnd, 0);
    if (!_is_bind || !_input_hwnd)
        return 0;

    const bool need_mouse = lock == 1 || lock == 2;
    const bool need_keyboard = lock == 1 || lock == 3;
    if ((need_mouse && _mouse_mode != IN_DX) || (need_keyboard && _keypad_mode != IN_DX))
        return 0;
    return gosf::hook::input_hook_client::LockInput(_input_hwnd, static_cast<int>(lock));
}

long BindingSession::reset_bind_state(bool restore_default_input) {
    if (_input_hwnd && (_mouse_mode == IN_DX || _keypad_mode == IN_DX))
        gosf::hook::input_hook_client::LockInput(_input_hwnd, 0);

    _display_hwnd = nullptr;
    _input_hwnd = nullptr;
    _is_bind = 0;
    _mouse_mode = IN_NORMAL;
    _keypad_mode = IN_NORMAL;
    if (_mouse) {
        _mouse->UnBind();
        _mouse.reset();
    }
    if (_keyboard) {
        _keyboard->UnBind();
        _keyboard.reset();
    }
    if (restore_default_input) {
        _mouse = std::make_unique<gosf::input::WinMouse>();
        _keyboard = std::make_unique<gosf::input::WinKeyboard>();
    }
    return 1;
}

LONG_PTR BindingSession::GetBindWindow() {
    return reinterpret_cast<LONG_PTR>(_display_hwnd);
}

long BindingSession::IsBind() {
    return _is_bind;
}

} // namespace gosf::binding
