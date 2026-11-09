#ifndef GOSF_BASE_ENVIRONMENT_H_
#define GOSF_BASE_ENVIRONMENT_H_
#include <string>
class RuntimeEnvironment {
  public:
    static void setInstance(void *instance);
    static void *getInstance();
    static std::wstring getBasePath();
    static int m_showErrorMsg;

  private:
    static void *m_instance;
    static std::wstring m_basePath;
};
#endif // GOSF_BASE_ENVIRONMENT_H_
