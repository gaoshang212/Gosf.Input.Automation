#include "session/AutomationContext.h"

#include <GosfInputAutomation.h>

std::atomic<int> gosf::AutomationCore::s_id(0);

gosf::AutomationCore::AutomationCore() : m_context(std::make_unique<internal::AutomationContext>(s_id++)) {
}

gosf::AutomationCore::~AutomationCore() {
}
