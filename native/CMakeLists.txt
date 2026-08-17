cmake_minimum_required(VERSION 3.21)
project(darp_tesseract_native LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

add_library(
  tesseract_common_csharp
  SHARED
  "${CMAKE_CURRENT_SOURCE_DIR}/../bindings/generated/TesseractCommon_wrap.cxx"
  "${CMAKE_CURRENT_SOURCE_DIR}/tesseract/common/src/timer.cpp"
  "${CMAKE_CURRENT_SOURCE_DIR}/tesseract/common/src/stopwatch.cpp")

target_include_directories(
  tesseract_common_csharp
  PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}/tesseract/common/include")

if(MSVC)
  # Keep the package independent of the host's Visual C++ redistributable.
  set_property(
    TARGET tesseract_common_csharp
    PROPERTY MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
  target_compile_options(tesseract_common_csharp PRIVATE /W4 /permissive-)
else()
  target_compile_options(tesseract_common_csharp PRIVATE -Wall -Wextra -Wpedantic)
  if(NOT APPLE)
    # libstdc++ and libgcc are not guaranteed to exist on a consumer machine.
    target_link_options(tesseract_common_csharp PRIVATE -static-libgcc -static-libstdc++)
  endif()
endif()

set_target_properties(
  tesseract_common_csharp
  PROPERTIES
    OUTPUT_NAME "tesseract_common_csharp"
    CXX_VISIBILITY_PRESET hidden
    VISIBILITY_INLINES_HIDDEN YES)
