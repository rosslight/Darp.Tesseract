if(NOT DEFINED DARP_DEPENDENCY_SOURCE_PATH)
  message(FATAL_ERROR "DARP_DEPENDENCY_SOURCE_PATH is required by the superbuild.")
endif()

if(MSVC)
  add_compile_options(
    /experimental:deterministic
    "/pathmap:${DARP_DEPENDENCY_SOURCE_PATH}=/darp-dependencies")
else()
  add_compile_options(
    "-ffile-prefix-map=${DARP_DEPENDENCY_SOURCE_PATH}=/darp-dependencies")
endif()
