cmake_minimum_required(VERSION 3.21)
cmake_policy(SET CMP0091 NEW)
project(GosfInputAutomationNative LANGUAGES C CXX ASM_MASM)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
set(BUILD_SHARED_LIBS OFF)

include(FetchContent)
FetchContent_Declare(blackbone
  GIT_REPOSITORY https://github.com/DarthTon/Blackbone.git
  GIT_TAG 5ede6ce50cd8ad34178bfa6cae05768ff6b3859b
  SOURCE_SUBDIR src/BlackBone)
FetchContent_Declare(minhook
  GIT_REPOSITORY https://github.com/TsudaKageyu/minhook.git
  GIT_TAG c3fcafdc10146beb5919319d0683e44e3c30d537)
FetchContent_MakeAvailable(blackbone minhook)
target_compile_definitions(BlackBone PRIVATE BLACKBONE_STATIC)

if(CMAKE_SIZEOF_VOID_P EQUAL 8)
  set(GOSF_TARGET Gosf.Input.Automation.Native.x64)
else()
  set(GOSF_TARGET Gosf.Input.Automation.Native.x86)
endif()

add_library(${GOSF_TARGET} SHARED
  core/AutomationCore.cpp
  core/base/AutomationModes.cpp
  core/base/DiaGuids.cpp
  core/base/Environment.cpp
  core/base/Utils.cpp
  core/binding/BindingSession.cpp
  core/c_api/GosfInputAutomationCApi.cpp
  core/hook/ApiResolver.cpp
  core/hook/HookExport.cpp
  core/hook/InputHook.cpp
  core/hook/WpfInputState.cpp
  core/hook/InputHookClient.cpp
  core/hook/MinHookRuntime.cpp
  core/input/keyboard/KeyboardBackend.cpp
  core/input/keyboard/WinKeyboard.cpp
  core/input/keyboard/DxKeyboard.cpp
  core/input/mouse/CursorShape.cpp
  core/input/mouse/WinMouse.cpp
  core/input/mouse/DxMouse.cpp
  core/session/AutomationContext.cpp
  core/session/AutomationInput.cpp)

target_include_directories(${GOSF_TARGET} PRIVATE include core "${blackbone_SOURCE_DIR}/src")
target_compile_definitions(${GOSF_TARGET} PRIVATE
  UNICODE _UNICODE U_STATIC_IMPLEMENTATION GOSF_C_API_EXPORTS BLACKBONE_STATIC)
target_compile_options(${GOSF_TARGET} PRIVATE /utf-8 /EHa)
target_link_options(${GOSF_TARGET} PRIVATE /MAP)
target_link_libraries(${GOSF_TARGET} PRIVATE BlackBone minhook shlwapi dinput8 dxguid)

if(CMAKE_SIZEOF_VOID_P EQUAL 4)
  target_sources(${GOSF_TARGET} PRIVATE hook-exports-x86.def)
endif()
