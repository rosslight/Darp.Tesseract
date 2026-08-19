cmake_minimum_required(VERSION 3.21)

project(darp_tesseract_native_superbuild NONE)

include(ExternalProject)

if(NOT CMAKE_BUILD_TYPE)
  set(CMAKE_BUILD_TYPE Release CACHE STRING "Superbuild configuration" FORCE)
endif()
if(NOT CMAKE_BUILD_TYPE STREQUAL "Release")
  message(FATAL_ERROR "Darp.Tesseract.Native supports Release native builds only.")
endif()
if("$ENV{CONDA_PREFIX}" STREQUAL "")
  message(FATAL_ERROR "CONDA_PREFIX is unavailable. Configure through 'pixi run build-native'.")
endif()

set(darp_tesseract_checkout "${CMAKE_CURRENT_SOURCE_DIR}/native/tesseract")
set(darp_boost_plugin_loader_checkout "${CMAKE_CURRENT_SOURCE_DIR}/native/boost_plugin_loader")
set(darp_ros_cmake_checkout "${CMAKE_CURRENT_SOURCE_DIR}/native/ros_industrial_cmake_boilerplate")
set(darp_opw_kinematics_checkout "${CMAKE_CURRENT_SOURCE_DIR}/native/opw_kinematics")
set(darp_tesseract_patch "${CMAKE_CURRENT_SOURCE_DIR}/patches/tesseract-runtime-dependencies.patch")
set(darp_ros_cmake_build "${CMAKE_BINARY_DIR}/ros-cmake-build")
set(darp_ros_cmake_install "${CMAKE_BINARY_DIR}/ros-cmake-install")
set(darp_boost_plugin_loader_build "${CMAKE_BINARY_DIR}/boost-plugin-loader-build")
set(darp_boost_plugin_loader_install "${CMAKE_BINARY_DIR}/boost-plugin-loader-install")
set(darp_opw_kinematics_build "${CMAKE_BINARY_DIR}/opw-kinematics-build")
set(darp_opw_kinematics_install "${CMAKE_BINARY_DIR}/opw-kinematics-install")
set(darp_tesseract_source "${CMAKE_BINARY_DIR}/source/tesseract")
set(darp_tesseract_build "${CMAKE_BINARY_DIR}/tesseract-build")
set(darp_tesseract_install "${CMAKE_BINARY_DIR}/tesseract-install")
set(darp_wrapper_build "${CMAKE_BINARY_DIR}/wrapper-build")
set(darp_wrapper_install "${CMAKE_BINARY_DIR}/wrapper-install")

if(NOT EXISTS "${darp_tesseract_checkout}/package.xml")
  message(FATAL_ERROR "The Tesseract submodule is missing. Run 'git submodule update --init native/tesseract'.")
endif()
if(NOT EXISTS "${darp_boost_plugin_loader_checkout}/package.xml")
  message(FATAL_ERROR
    "The boost_plugin_loader submodule is missing. Run 'git submodule update --init native/boost_plugin_loader'.")
endif()
if(NOT EXISTS "${darp_ros_cmake_checkout}/package.xml")
  message(FATAL_ERROR
    "The ros_industrial_cmake_boilerplate submodule is missing. Run 'git submodule update --init native/ros_industrial_cmake_boilerplate'.")
endif()
if(NOT EXISTS "${darp_opw_kinematics_checkout}/package.xml")
  message(FATAL_ERROR
    "The opw_kinematics submodule is missing. Run 'git submodule update --init native/opw_kinematics'.")
endif()
if(NOT EXISTS "${darp_tesseract_patch}")
  message(FATAL_ERROR "The required Tesseract patch is missing: ${darp_tesseract_patch}")
endif()

# Configure from a disposable source tree. The pinned checkout remains pristine.
file(REMOVE_RECURSE "${darp_tesseract_source}")
file(MAKE_DIRECTORY "${darp_tesseract_source}")
file(COPY "${darp_tesseract_checkout}/" DESTINATION "${darp_tesseract_source}" PATTERN ".git" EXCLUDE)
file(RELATIVE_PATH darp_staged_source_relative "${CMAKE_CURRENT_SOURCE_DIR}" "${darp_tesseract_source}")
execute_process(
  COMMAND git apply --check --ignore-space-change --ignore-whitespace --unsafe-paths
          --directory=${darp_staged_source_relative} "${darp_tesseract_patch}"
  WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
  RESULT_VARIABLE darp_patch_check_result
  OUTPUT_VARIABLE darp_patch_check_output
  ERROR_VARIABLE darp_patch_check_error)
if(NOT darp_patch_check_result EQUAL 0)
  message(FATAL_ERROR "The Tesseract point-cloud patch does not apply cleanly:\n${darp_patch_check_output}${darp_patch_check_error}")
endif()
execute_process(
  COMMAND git apply --ignore-space-change --ignore-whitespace --unsafe-paths
          --directory=${darp_staged_source_relative} "${darp_tesseract_patch}"
  WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
  COMMAND_ERROR_IS_FATAL ANY)

if(WIN32)
  set(darp_dependency_prefix "$ENV{CONDA_PREFIX}/Library")
else()
  set(darp_dependency_prefix "$ENV{CONDA_PREFIX}")
endif()

set(darp_common_cmake_args
  -DCMAKE_BUILD_TYPE=Release
  -DCMAKE_POSITION_INDEPENDENT_CODE=ON
  -DCMAKE_PROJECT_INCLUDE=${CMAKE_CURRENT_SOURCE_DIR}/cmake/reproducible_paths.cmake)
file(TO_CMAKE_PATH "$ENV{CONDA_PREFIX}" darp_dependency_source_path)
list(APPEND darp_common_cmake_args
  -DDARP_DEPENDENCY_SOURCE_PATH=${darp_dependency_source_path})
if(APPLE)
  list(APPEND darp_common_cmake_args -DCMAKE_OSX_DEPLOYMENT_TARGET=11.0)
endif()

ExternalProject_Add(
  ros_industrial_cmake_boilerplate_upstream
  SOURCE_DIR "${darp_ros_cmake_checkout}"
  BINARY_DIR "${darp_ros_cmake_build}"
  DOWNLOAD_COMMAND "${CMAKE_COMMAND}" -E true
  UPDATE_COMMAND "${CMAKE_COMMAND}" -E true
  PATCH_COMMAND "${CMAKE_COMMAND}" -E true
  CMAKE_ARGS
    ${darp_common_cmake_args}
    -DCMAKE_PREFIX_PATH=${darp_dependency_prefix}
    -DCMAKE_INSTALL_PREFIX=${darp_ros_cmake_install}
  BUILD_COMMAND "${CMAKE_COMMAND}" --build <BINARY_DIR> --config Release --parallel
  INSTALL_COMMAND "${CMAKE_COMMAND}" --install <BINARY_DIR> --config Release)

set(darp_source_dependency_prefix_path "${darp_ros_cmake_install}|${darp_dependency_prefix}")
ExternalProject_Add(
  boost_plugin_loader_upstream
  DEPENDS ros_industrial_cmake_boilerplate_upstream
  SOURCE_DIR "${darp_boost_plugin_loader_checkout}"
  BINARY_DIR "${darp_boost_plugin_loader_build}"
  DOWNLOAD_COMMAND "${CMAKE_COMMAND}" -E true
  UPDATE_COMMAND "${CMAKE_COMMAND}" -E true
  PATCH_COMMAND "${CMAKE_COMMAND}" -E true
  LIST_SEPARATOR "|"
  CMAKE_ARGS
    ${darp_common_cmake_args}
    -DCMAKE_PREFIX_PATH=${darp_source_dependency_prefix_path}
    -DCMAKE_INSTALL_PREFIX=${darp_boost_plugin_loader_install}
    -DBUILD_SHARED_LIBS=OFF
    -DBUILD_TESTING=OFF
  BUILD_COMMAND "${CMAKE_COMMAND}" --build <BINARY_DIR> --config Release --parallel
  INSTALL_COMMAND "${CMAKE_COMMAND}" --install <BINARY_DIR> --config Release)

ExternalProject_Add(
  opw_kinematics_upstream
  DEPENDS ros_industrial_cmake_boilerplate_upstream
  SOURCE_DIR "${darp_opw_kinematics_checkout}"
  BINARY_DIR "${darp_opw_kinematics_build}"
  DOWNLOAD_COMMAND "${CMAKE_COMMAND}" -E true
  UPDATE_COMMAND "${CMAKE_COMMAND}" -E true
  PATCH_COMMAND "${CMAKE_COMMAND}" -E true
  LIST_SEPARATOR "|"
  CMAKE_ARGS
    ${darp_common_cmake_args}
    -DCMAKE_PREFIX_PATH=${darp_source_dependency_prefix_path}
    -DCMAKE_INSTALL_PREFIX=${darp_opw_kinematics_install}
    -DOPW_ENABLE_TESTING=OFF
  # OPW is header-only. Its unconditional "node" target is a sample utility,
  # not an install prerequisite or part of the native package.
  BUILD_COMMAND "${CMAKE_COMMAND}" -E true
  INSTALL_COMMAND "${CMAKE_COMMAND}" --install <BINARY_DIR> --config Release)

set(darp_tesseract_prefix_path
  "${darp_boost_plugin_loader_install}|${darp_opw_kinematics_install}|${darp_ros_cmake_install}|${darp_dependency_prefix}")
ExternalProject_Add(
  tesseract_upstream
  DEPENDS boost_plugin_loader_upstream opw_kinematics_upstream
  SOURCE_DIR "${darp_tesseract_source}"
  BINARY_DIR "${darp_tesseract_build}"
  DOWNLOAD_COMMAND "${CMAKE_COMMAND}" -E true
  UPDATE_COMMAND "${CMAKE_COMMAND}" -E true
  PATCH_COMMAND "${CMAKE_COMMAND}" -E true
  LIST_SEPARATOR "|"
  CMAKE_ARGS
    ${darp_common_cmake_args}
    -DCMAKE_PREFIX_PATH=${darp_tesseract_prefix_path}
    -DCMAKE_INSTALL_PREFIX=${darp_tesseract_install}
    -DBUILD_SHARED_LIBS=OFF
    -DTESSERACT_ENABLE_TESTING=OFF
    -DTESSERACT_ENABLE_EXAMPLES=OFF
    -DTESSERACT_ENABLE_BENCHMARKING=OFF
    -DTESSERACT_ENABLE_CODE_COVERAGE=OFF
    -DTESSERACT_BUILD_GZ_VISUALIZATION=OFF
    -DTESSERACT_BUILD_TEST_SUITE=OFF
    -DTESSERACT_BUILD_VHACD=OFF
    -DTESSERACT_BUILD_FCL=ON
    -DTESSERACT_BUILD_IKFAST=ON
    -DTESSERACT_BUILD_KDL=ON
    -DTESSERACT_BUILD_OPW=ON
    -DTESSERACT_BUILD_UR=ON
    -DTESSERACT_URDF_ENABLE_POINT_CLOUDS=OFF
  BUILD_COMMAND "${CMAKE_COMMAND}" --build <BINARY_DIR> --config Release --parallel
  INSTALL_COMMAND "${CMAKE_COMMAND}" --install <BINARY_DIR> --config Release)

set(darp_wrapper_prefix_path
  "${darp_tesseract_install}|${darp_boost_plugin_loader_install}|${darp_opw_kinematics_install}|${darp_ros_cmake_install}|${darp_dependency_prefix}")
ExternalProject_Add(
  tesseract_wrapper
  DEPENDS tesseract_upstream
  SOURCE_DIR "${CMAKE_CURRENT_SOURCE_DIR}/native"
  BINARY_DIR "${darp_wrapper_build}"
  DOWNLOAD_COMMAND "${CMAKE_COMMAND}" -E true
  UPDATE_COMMAND "${CMAKE_COMMAND}" -E true
  PATCH_COMMAND "${CMAKE_COMMAND}" -E true
  LIST_SEPARATOR "|"
  CMAKE_ARGS
    ${darp_common_cmake_args}
    -DCMAKE_PREFIX_PATH=${darp_wrapper_prefix_path}
    -DCMAKE_INSTALL_PREFIX=${darp_wrapper_install}
    -DTESSERACT_CPP_PREFIX=${darp_tesseract_install}
    -DTESSERACT_SOURCE_DIR=${darp_tesseract_source}
  BUILD_COMMAND "${CMAKE_COMMAND}" --build <BINARY_DIR> --config Release --parallel
  INSTALL_COMMAND "${CMAKE_COMMAND}" --install <BINARY_DIR> --config Release)

file(WRITE "${CMAKE_BINARY_DIR}/wrapper-install-prefix.txt" "${darp_wrapper_install}")
