#pragma once
#include <Windows.h>

// Native input state for the bound WPF UI thread; no managed control access.
namespace gosf::hook::wpf_input {
using HookInstaller = bool (*)(void*, void*, void**);
bool Install(HWND hwnd, HookInstaller install);
void Reset();
bool RequestCleanup();
bool HandleCleanup(UINT message);
void TranslateKey(WPARAM key, LPARAM lparam);
bool ActiveThread();
void PrepareMessage(HWND hwnd, UINT message, LPARAM lparam, WNDPROC original);
bool SuppressMessage(UINT message);
}
