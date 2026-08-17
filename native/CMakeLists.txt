cmake_minimum_required(VERSION 3.21)

# Several transitive conda packages still publish configs using legacy policy
# behavior. Match the policy compatibility used by the maintained nanobind
# project that supplies our dependency lock.
if(POLICY CMP0167)
  cmake_policy(SET CMP0167 OLD)
endif()
set(CMAKE_POLICY_VERSION_MINIMUM 3.5 CACHE STRING "Minimum CMake policy version")

project(darp_tesseract_native LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

# Pixi/conda provides the official Tesseract 0.35 packages. Keep the same
# prefix resolution as tesseract_nanobind so local and CI builds consume the
# exact dependency graph recorded in its pinned pixi.lock.
if(NOT "$ENV{TESSERACT_CPP_PREFIX}" STREQUAL "")
  set(TESSERACT_CPP_PREFIX "$ENV{TESSERACT_CPP_PREFIX}")
elseif(NOT "$ENV{CONDA_PREFIX}" STREQUAL "")
  set(TESSERACT_CPP_PREFIX "$ENV{CONDA_PREFIX}")
endif()

if(DEFINED TESSERACT_CPP_PREFIX)
  list(PREPEND CMAKE_PREFIX_PATH "${TESSERACT_CPP_PREFIX}")
  link_directories("${TESSERACT_CPP_PREFIX}/lib")
  if(WIN32 AND IS_DIRECTORY "${TESSERACT_CPP_PREFIX}/Library")
    list(PREPEND CMAKE_PREFIX_PATH "${TESSERACT_CPP_PREFIX}/Library")
    link_directories("${TESSERACT_CPP_PREFIX}/Library/lib")
  endif()
endif()

find_package(tesseract 0.35.0 REQUIRED COMPONENTS common)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS geometry)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS scene_graph)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS srdf)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS urdf)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS collision)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS state_solver)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS kinematics)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS environment)
find_package(console_bridge REQUIRED)

add_library(
  tesseract_csharp
  SHARED
  "${CMAKE_CURRENT_SOURCE_DIR}/../bindings/generated/TesseractNative_wrap.cxx")

target_link_libraries(
  tesseract_csharp
  PRIVATE
    tesseract::common
    tesseract::geometry
    tesseract::scene_graph
    tesseract::srdf
    tesseract::urdf
    tesseract::state_solver_kdl
    tesseract::state_solver_ofkt
    tesseract::kinematics
    tesseract::environment
    tesseract::collision
    console_bridge::console_bridge)

if(MSVC)
  target_compile_definitions(tesseract_csharp PRIVATE NOMINMAX _USE_MATH_DEFINES)
  target_compile_options(tesseract_csharp PRIVATE /W4 /permissive- /EHsc)
else()
  target_compile_options(tesseract_csharp PRIVATE -Wall -Wextra -Wpedantic)
endif()

if(APPLE)
  set_target_properties(tesseract_csharp PROPERTIES INSTALL_RPATH "@loader_path")
elseif(UNIX)
  set_target_properties(tesseract_csharp PROPERTIES INSTALL_RPATH "$ORIGIN")
endif()

set_target_properties(
  tesseract_csharp
  PROPERTIES
    OUTPUT_NAME "tesseract_csharp"
    CXX_VISIBILITY_PRESET hidden
    VISIBILITY_INLINES_HIDDEN YES)
