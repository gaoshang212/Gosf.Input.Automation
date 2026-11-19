#pragma once

#include "../base/Types.h"
#include "../hook/HookProtocol.h"

namespace gosf::input::message {

// 键鼠的窗口消息都用同一套超时发送，避免目标窗口卡住时脚本一起挂住。
inline long SendTimeout(HWND hwnd, UINT message, WPARAM wparam, LPARAM lparam) {
    DWORD_PTR result = 0;
    if (!::SendMessageTimeout(hwnd, message, wparam, lparam, SMTO_BLOCK, 2000, &result)) return 0;
    return result == GOSF_UNSUPPORTED_OLE_DRAG ? 0L : 1L;
}

} // namespace gosf::input::message
