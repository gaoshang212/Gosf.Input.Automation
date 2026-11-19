#pragma once

#include "../input/keyboard/KeyboardBackend.h"
#include "../input/mouse/WinMouse.h"
#include <functional>
#include <memory>
#include <string>
#include <utility>

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

    // A failed unbind can leave only one backend attached. Keep both for cleanup,
    // but never dispatch another operation to either until cleanup succeeds.
    template <typename Command, typename... Args>
    long Mouse(Command command, Args&&... args) {
        if (_cleanup_pending || !_mouse) return 0;
        return std::invoke(command, _mouse.get(), std::forward<Args>(args)...);
    }

    template <typename Command, typename... Args>
    long Keyboard(Command command, Args&&... args) {
        if (_cleanup_pending || !_keyboard) return 0;
        return std::invoke(command, _keyboard.get(), std::forward<Args>(args)...);
    }

  private:
    bool _cleanup_pending = false;
    std::unique_ptr<gosf::input::WinMouse> _mouse;
    std::unique_ptr<gosf::input::KeyboardBackend> _keyboard;

    long reset_bind_state(bool restore_default_input);
    void configure_backends(HWND displayWnd, HWND inputWnd, int mouseMode, int keypadMode);

    HWND _display_hwnd = nullptr;
    HWND _input_hwnd = nullptr;
    int _is_bind = 0;
    int _mouse_mode = 0;
    int _keypad_mode = 0;
};

} // namespace gosf::binding
