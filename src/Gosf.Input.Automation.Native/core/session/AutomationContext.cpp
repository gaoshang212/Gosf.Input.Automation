#include "AutomationContext.h"
#include <Windows.h>

gosf::internal::AutomationContext::AutomationContext(int client_id) : id(client_id) {
    ::SetProcessDPIAware();
    // 初始化键码表
    vkmap[L"back"] = VK_BACK;
    vkmap[L"ctrl"] = VK_CONTROL;
    vkmap[L"lctrl"] = VK_LCONTROL;
    vkmap[L"rctrl"] = VK_RCONTROL;
    vkmap[L"alt"] = VK_MENU;
    vkmap[L"lalt"] = VK_LMENU;
    vkmap[L"ralt"] = VK_RMENU;
    vkmap[L"shift"] = VK_SHIFT;
    vkmap[L"lshift"] = VK_LSHIFT;
    vkmap[L"rshift"] = VK_RSHIFT;
    vkmap[L"win"] = VK_LWIN;
    vkmap[L"lwin"] = VK_LWIN;
    vkmap[L"rwin"] = VK_RWIN;
    vkmap[L"space"] = VK_SPACE;
    vkmap[L"cap"] = VK_CAPITAL;
    vkmap[L"tab"] = VK_TAB;
    vkmap[L"esc"] = VK_ESCAPE;
    vkmap[L"enter"] = VK_RETURN;
    vkmap[L"up"] = VK_UP;
    vkmap[L"down"] = VK_DOWN;
    vkmap[L"left"] = VK_LEFT;
    vkmap[L"right"] = VK_RIGHT;
    vkmap[L"menu"] = VK_APPS;
    vkmap[L"print"] = VK_SNAPSHOT;
    vkmap[L"insert"] = VK_INSERT;
    vkmap[L"delete"] = VK_DELETE;
    vkmap[L"pause"] = VK_PAUSE;
    vkmap[L"scroll"] = VK_SCROLL;
    vkmap[L"home"] = VK_HOME;
    vkmap[L"end"] = VK_END;
    vkmap[L"pgup"] = VK_PRIOR;
    vkmap[L"pgdn"] = VK_NEXT;
    vkmap[L"f1"] = VK_F1;
    vkmap[L"f2"] = VK_F2;
    vkmap[L"f3"] = VK_F3;
    vkmap[L"f4"] = VK_F4;
    vkmap[L"f5"] = VK_F5;
    vkmap[L"f6"] = VK_F6;
    vkmap[L"f7"] = VK_F7;
    vkmap[L"f8"] = VK_F8;
    vkmap[L"f9"] = VK_F9;
    vkmap[L"f10"] = VK_F10;
    vkmap[L"f11"] = VK_F11;
    vkmap[L"f12"] = VK_F12;

    // Numpad keys
    vkmap[L"num0"] = VK_NUMPAD0;
    vkmap[L"num1"] = VK_NUMPAD1;
    vkmap[L"num2"] = VK_NUMPAD2;
    vkmap[L"num3"] = VK_NUMPAD3;
    vkmap[L"num4"] = VK_NUMPAD4;
    vkmap[L"num5"] = VK_NUMPAD5;
    vkmap[L"num6"] = VK_NUMPAD6;
    vkmap[L"num7"] = VK_NUMPAD7;
    vkmap[L"num8"] = VK_NUMPAD8;
    vkmap[L"num9"] = VK_NUMPAD9;
    vkmap[L"numlock"] = VK_NUMLOCK;
    vkmap[L"num."] = VK_DECIMAL;
    vkmap[L"num*"] = VK_MULTIPLY;
    vkmap[L"num+"] = VK_ADD;
    vkmap[L"num-"] = VK_SUBTRACT;
    vkmap[L"num/"] = VK_DIVIDE;

}
