#pragma once

#include <string>

namespace gosf::hook {

inline std::wstring ResolveHookModuleName(bool target_is64) {
    return target_is64 ? L"Gosf.Input.Automation.Native.x64.dll" : L"Gosf.Input.Automation.Native.x86.dll";
}

} // namespace gosf::hook
