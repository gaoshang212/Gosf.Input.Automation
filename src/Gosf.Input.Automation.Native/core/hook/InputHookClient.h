#pragma once

#include <Windows.h>

namespace gosf::hook::input_hook_client {

long Bind(HWND hwnd, int mode);
long UnBind(HWND hwnd);
long LockInput(HWND hwnd, int lock);
bool GetCursorShape(HWND hwnd, unsigned long long &hash, unsigned long long &meta);

} // namespace gosf::hook::input_hook_client
