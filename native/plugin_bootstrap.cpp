#include <mutex>

#include <boost_plugin_loader/utils.h>
#include <tesseract/collision/bullet/bullet_factories.h>
#include <tesseract/collision/fcl/fcl_factories.h>
#include <tesseract/kinematics/kdl/kdl_factories.h>
#include <tesseract/kinematics/opw/opw_factory.h>
#include <tesseract/kinematics/ur/ur_factory.h>

#if defined(_WIN32)
#define DARP_TESSERACT_EXPORT __declspec(dllexport)
#else
#define DARP_TESSERACT_EXPORT __attribute__((visibility("default")))
#endif

namespace
{
std::once_flag plugin_registration;
}

extern "C" DARP_TESSERACT_EXPORT int darp_tesseract_initialize_plugins()
{
  try
  {
    std::call_once(plugin_registration, []() {
      boost_plugin_loader::addSymbolLibraryToSearchLibrariesEnv(
          tesseract::collision::BulletFactoriesAnchor(), "TESSERACT_CONTACT_MANAGERS_PLUGINS");
      boost_plugin_loader::addSymbolLibraryToSearchLibrariesEnv(
          tesseract::collision::FCLFactoriesAnchor(), "TESSERACT_CONTACT_MANAGERS_PLUGINS");
      boost_plugin_loader::addSymbolLibraryToSearchLibrariesEnv(
          tesseract::kinematics::KDLFactoriesAnchor(), "TESSERACT_KINEMATICS_PLUGINS");
      boost_plugin_loader::addSymbolLibraryToSearchLibrariesEnv(
          tesseract::kinematics::OPWFactoriesAnchor(), "TESSERACT_KINEMATICS_PLUGINS");
      boost_plugin_loader::addSymbolLibraryToSearchLibrariesEnv(
          tesseract::kinematics::URFactoriesAnchor(), "TESSERACT_KINEMATICS_PLUGINS");
    });
    return 0;
  }
  catch (...)
  {
    return -1;
  }
}
