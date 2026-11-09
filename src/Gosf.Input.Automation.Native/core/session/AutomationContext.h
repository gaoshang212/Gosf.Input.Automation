#pragma once

#include "binding/BindingSession.h"
#include <map>
#include <string>

namespace gosf::internal {

struct AutomationContext {
    explicit AutomationContext(int client_id);

    gosf::binding::BindingSession bkproc;
    std::map<std::wstring, long> vkmap;
    int id;
};

} // namespace gosf::internal
