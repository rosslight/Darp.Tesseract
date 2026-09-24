cmake_minimum_required(VERSION 3.21)

if(POLICY CMP0167)
  cmake_policy(SET CMP0167 OLD)
endif()
set(CMAKE_POLICY_VERSION_MINIMUM 3.5 CACHE STRING "Minimum CMake policy version")

project(darp_tesseract_native LANGUAGES CXX)

include(CheckIPOSupported)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

if(NOT DEFINED TESSERACT_CPP_PREFIX OR NOT IS_DIRECTORY "${TESSERACT_CPP_PREFIX}")
  message(FATAL_ERROR "TESSERACT_CPP_PREFIX must point to the private Tesseract installation.")
endif()
if(NOT DEFINED TESSERACT_SOURCE_DIR OR NOT EXISTS "${TESSERACT_SOURCE_DIR}/package.xml")
  message(FATAL_ERROR "TESSERACT_SOURCE_DIR must point to the staged Tesseract source tree.")
endif()
if(NOT DEFINED TESSERACT_PLANNING_PREFIX OR NOT IS_DIRECTORY "${TESSERACT_PLANNING_PREFIX}")
  message(FATAL_ERROR "TESSERACT_PLANNING_PREFIX must point to the private Tesseract Planning installation.")
endif()

list(PREPEND CMAKE_PREFIX_PATH "${TESSERACT_PLANNING_PREFIX}" "${TESSERACT_CPP_PREFIX}")
if(WIN32 AND IS_DIRECTORY "$ENV{CONDA_PREFIX}/Library")
  list(APPEND CMAKE_PREFIX_PATH "$ENV{CONDA_PREFIX}/Library")
else()
  list(APPEND CMAKE_PREFIX_PATH "$ENV{CONDA_PREFIX}")
endif()

find_package(tesseract 0.35.0 REQUIRED COMPONENTS common)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS geometry)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS scene_graph)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS srdf)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS urdf)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS collision)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS collision_bullet)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS collision_fcl)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS state_solver)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS kinematics)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS kinematics_kdl)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS kinematics_opw)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS kinematics_ur)
find_package(tesseract 0.35.0 REQUIRED COMPONENTS environment)
find_package(
  tesseract_planning 0.35.0 REQUIRED
  COMPONENTS
    command_language
    motion_planners
    motion_planners_simple
    motion_planners_descartes
    motion_planners_trajopt
    task_composer
    task_composer_planning
    task_composer_taskflow
    time_parameterization
    time_parameterization_isp)
find_package(console_bridge REQUIRED)

find_library(
  task_composer_factories
  NAMES tesseract_task_composer_factories
  PATHS "${TESSERACT_PLANNING_PREFIX}/lib"
  NO_DEFAULT_PATH
  REQUIRED)
find_library(
  task_composer_planning_factories
  NAMES tesseract_task_composer_planning_factories
  PATHS "${TESSERACT_PLANNING_PREFIX}/lib"
  NO_DEFAULT_PATH
  REQUIRED)
find_library(
  task_composer_taskflow_factories
  NAMES tesseract_task_composer_taskflow_factories
  PATHS "${TESSERACT_PLANNING_PREFIX}/lib"
  NO_DEFAULT_PATH
  REQUIRED)

add_library(
  tesseract_csharp
  SHARED
  "${CMAKE_CURRENT_SOURCE_DIR}/../bindings/generated/TesseractNative_wrap.cxx"
  "${CMAKE_CURRENT_SOURCE_DIR}/plugin_bootstrap.cpp"
  "${TESSERACT_SOURCE_DIR}/collision/bullet/src/bullet_factories.cpp"
  "${TESSERACT_SOURCE_DIR}/collision/fcl/src/fcl_factories.cpp"
  "${TESSERACT_SOURCE_DIR}/kinematics/kdl/src/kdl_factories.cpp"
  "${TESSERACT_SOURCE_DIR}/kinematics/opw/src/opw_factory.cpp"
  "${TESSERACT_SOURCE_DIR}/kinematics/ur/src/ur_factory.cpp")

set(darp_tesseract_libraries
  tesseract::common
  tesseract::geometry
  tesseract::scene_graph
  tesseract::srdf
  tesseract::urdf
  tesseract::state_solver_kdl
  tesseract::state_solver_ofkt
  tesseract::state_solver
  tesseract::kinematics
  tesseract::kinematics_kdl
  tesseract::kinematics_opw
  tesseract::kinematics_ur
  tesseract::environment
  tesseract::collision
  tesseract::collision_bullet
  tesseract::collision_fcl
  tesseract::command_language
  tesseract::motion_planners
  tesseract::motion_planners_simple
  tesseract::motion_planners_descartes
  descartes::descartes_light
  tesseract::motion_planners_trajopt
  trajopt::trajopt
  trajopt::trajopt_common
  trajopt::trajopt_sco
  tesseract::visualization
  tesseract::task_composer
  tesseract::task_composer_nodes
  tesseract::task_composer_planning_nodes
  tesseract::task_composer_taskflow
  tesseract::time_parameterization
  tesseract::time_parameterization_isp
  console_bridge::console_bridge)
set(darp_task_composer_factory_libraries
  "$<LINK_LIBRARY:WHOLE_ARCHIVE,${task_composer_factories}>"
  "$<LINK_LIBRARY:WHOLE_ARCHIVE,${task_composer_planning_factories}>"
  "$<LINK_LIBRARY:WHOLE_ARCHIVE,${task_composer_taskflow_factories}>")

if(UNIX AND NOT APPLE)
  # GNU ld only visits a static archive once. Tesseract core and planning have
  # circular references, so rescan them until all symbols are resolved.
  target_link_libraries(
    tesseract_csharp
    PRIVATE
      ${darp_task_composer_factory_libraries}
      "$<LINK_GROUP:RESCAN,${darp_tesseract_libraries}>")
else()
  target_link_libraries(
    tesseract_csharp
    PRIVATE
      ${darp_tesseract_libraries}
      ${darp_task_composer_factory_libraries})
endif()

if(MSVC)
  target_compile_definitions(tesseract_csharp PRIVATE NOMINMAX _USE_MATH_DEFINES)
  target_compile_options(tesseract_csharp PRIVATE /W4 /permissive- /EHsc /Zc:__cplusplus)
else()
  target_compile_options(tesseract_csharp PRIVATE -Wall -Wextra -Wpedantic)
endif()

if(APPLE)
  set_target_properties(
    tesseract_csharp
    PROPERTIES
      BUILD_RPATH "@loader_path"
      INSTALL_NAME_DIR "@rpath"
      INSTALL_RPATH "@loader_path"
      INSTALL_RPATH_USE_LINK_PATH FALSE
      INSTALL_REMOVE_ENVIRONMENT_RPATH TRUE)
elseif(UNIX)
  set_target_properties(
    tesseract_csharp
    PROPERTIES
      BUILD_RPATH "$ORIGIN"
      INSTALL_RPATH "$ORIGIN"
      INSTALL_RPATH_USE_LINK_PATH FALSE
      INSTALL_REMOVE_ENVIRONMENT_RPATH TRUE)
endif()

set_target_properties(
  tesseract_csharp
  PROPERTIES
    OUTPUT_NAME "tesseract_csharp"
    CXX_VISIBILITY_PRESET hidden
    VISIBILITY_INLINES_HIDDEN YES)

check_ipo_supported(RESULT ipo_supported OUTPUT ipo_error)
if(ipo_supported)
  set_property(TARGET tesseract_csharp PROPERTY INTERPROCEDURAL_OPTIMIZATION_RELEASE ON)
else()
  message(STATUS "Interprocedural optimization is unavailable: ${ipo_error}")
endif()

if(MSVC)
  target_compile_options(tesseract_csharp PRIVATE $<$<CONFIG:Release>:/Gy /Gw>)
  target_link_options(tesseract_csharp PRIVATE $<$<CONFIG:Release>:/OPT:REF /OPT:ICF>)
elseif(APPLE)
  target_compile_options(tesseract_csharp PRIVATE $<$<CONFIG:Release>:-ffunction-sections -fdata-sections>)
  target_link_options(tesseract_csharp PRIVATE $<$<CONFIG:Release>:-Wl,-dead_strip>)
elseif(UNIX)
  target_compile_options(tesseract_csharp PRIVATE $<$<CONFIG:Release>:-ffunction-sections -fdata-sections>)
  target_link_options(tesseract_csharp PRIVATE $<$<CONFIG:Release>:-Wl,--gc-sections>)
endif()

install(TARGETS tesseract_csharp RUNTIME DESTINATION . LIBRARY DESTINATION .)
