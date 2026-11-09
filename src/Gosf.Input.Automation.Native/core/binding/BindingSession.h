#pragma once

#include "../input/keyboard/KeyboardBackend.h"
#include "../input/mouse/WinMouse.h"
#include <memory>
#include <string>

namespace gosf::binding {

class BindingSession {
  public:
    BindingSession();
    ~BindingSession();

    long BindWindow(LONG_PTR hwnd, const std::wstring &display, const std::wstring &mouse,
                    const std::wstring &keypad, long mode);
    long BindWindowEx(LONG_PTR display_hwnd, LONG_PTR input_hwnd, const std::wstring &display,
                      const std::wstring &mouse, const std::wstring &keypad, long mode);
    long UnBindWindow();
    long LockInput(long lock);
    LONG_PTR GetBindWindow();
    long IsBind();

    std::unique_ptr<gosf::input::WinMouse> _mouse;
    std::unique_ptr<gosf::input::KeyboardBackend> _keyboard;

  private:
    long reset_bind_state(bool restore_default_input);

    HWND _display_hwnd = nullptr;
    HWND _input_hwnd = nullptr;
    int _is_bind = 0;
    int _mouse_mode = 0;
    int _keypad_mode = 0;
};

} // namespace gosf::binding
