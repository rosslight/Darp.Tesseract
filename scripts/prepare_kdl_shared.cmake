# KDL 1.5.3 chooses a static library on MSVC because it predates CMake's
# automatic symbol exports. Keep the LGPL library independently replaceable.
file(READ "${SOURCE_DIRECTORY}/orocos_kdl/src/CMakeLists.txt" kdl_cmake)
string(REPLACE "SET(LIB_TYPE STATIC)" "SET(LIB_TYPE SHARED)\n    SET(CMAKE_WINDOWS_EXPORT_ALL_SYMBOLS ON)" kdl_cmake "${kdl_cmake}")
file(WRITE "${SOURCE_DIRECTORY}/orocos_kdl/src/CMakeLists.txt" "${kdl_cmake}")
file(APPEND "${SOURCE_DIRECTORY}/orocos_kdl/src/CMakeLists.txt"
  "\nSET_TARGET_PROPERTIES(orocos-kdl PROPERTIES DEFINE_SYMBOL KDL_BUILDING_DLL)\n")

# Automatic exports cover functions; consumers must explicitly import global
# data used by KDL's inline headers (epsilon, PI and related constants).
set(kdl_utility_header "${SOURCE_DIRECTORY}/orocos_kdl/src/utilities/utility.h")
file(READ "${kdl_utility_header}" kdl_utility)
string(REPLACE "extern " "extern KDL_DATA_IMPORT " kdl_utility "${kdl_utility}")
file(WRITE "${kdl_utility_header}"
  "#if defined(_WIN32) && !defined(KDL_BUILDING_DLL)\n#define KDL_DATA_IMPORT __declspec(dllimport)\n#else\n#define KDL_DATA_IMPORT\n#endif\n${kdl_utility}")
