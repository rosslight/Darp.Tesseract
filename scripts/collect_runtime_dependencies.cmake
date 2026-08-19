cmake_minimum_required(VERSION 3.21)
if(POLICY CMP0207)
  cmake_policy(SET CMP0207 NEW)
endif()

if(NOT DEFINED WRAPPER_LIBRARY OR NOT EXISTS "${WRAPPER_LIBRARY}")
  message(FATAL_ERROR "WRAPPER_LIBRARY must point to the built native wrapper.")
endif()
if(NOT DEFINED OUTPUT_DIRECTORY)
  message(FATAL_ERROR "OUTPUT_DIRECTORY must be provided.")
endif()
if("$ENV{CONDA_PREFIX}" STREQUAL "")
  message(FATAL_ERROR "CONDA_PREFIX is unavailable. Run this script through the build Pixi environment.")
endif()

if(WIN32)
  if(NOT DEFINED RUNTIME_DEPENDENCY_TOOL OR NOT EXISTS "${RUNTIME_DEPENDENCY_TOOL}")
    message(FATAL_ERROR "RUNTIME_DEPENDENCY_TOOL must point to dumpbin.exe on Windows.")
  endif()
  set(CMAKE_GET_RUNTIME_DEPENDENCIES_PLATFORM "windows+pe")
  set(CMAKE_GET_RUNTIME_DEPENDENCIES_TOOL "dumpbin")
  set(CMAKE_GET_RUNTIME_DEPENDENCIES_COMMAND "${RUNTIME_DEPENDENCY_TOOL}")
endif()

if(WIN32)
  set(runtime_search_directory "$ENV{CONDA_PREFIX}/Library/bin")
else()
  set(runtime_search_directory "$ENV{CONDA_PREFIX}/lib")
endif()

# Tesseract and the selected collision/kinematics factories are linked into
# the wrapper. The wrapper is the sole runtime root; remaining files are the
# small third-party closure that is intentionally kept dynamic.
set(runtime_roots "${WRAPPER_LIBRARY}")
set(system_dependency_names
  "api-ms-.*"
  "ext-ms-.*"
  "^(AzureAttestManager|AzureAttestNormal|HvsiFileTrust|PdmUtilities|wpaxholder)\\.dll$")
if(UNIX AND NOT APPLE)
  # A standard Linux distribution supplies the platform C/C++ runtime and
  # zlib. Package the robotics dependency graph, not a competing toolchain.
  list(APPEND system_dependency_names
    "^ld-linux.*\\.so.*$"
    "^lib(c|m|pthread|dl|rt)\\.so.*$"
    "^lib(stdc\\+\\+|gcc_s|gomp|z)\\.so.*$")
endif()
file(
  GET_RUNTIME_DEPENDENCIES
  LIBRARIES ${runtime_roots}
  RESOLVED_DEPENDENCIES_VAR resolved_dependencies
  UNRESOLVED_DEPENDENCIES_VAR unresolved_dependencies
  CONFLICTING_DEPENDENCIES_PREFIX dependency_conflicts
  DIRECTORIES "${runtime_search_directory}"
  PRE_EXCLUDE_REGEXES ${system_dependency_names}
  POST_EXCLUDE_REGEXES
    ".*[Ww][Ii][Nn][Dd][Oo][Ww][Ss][/\\\\][Ss][Yy][Ss][Tt][Ee][Mm]32[/\\\\].*"
    "^/lib/.*"
    "^/lib64/.*"
    "^/usr/lib/.*"
    "^/System/Library/.*")

# Prefer the Pixi-provided app-local runtime when a matching library is also
# installed globally (the common Windows case for the Visual C++ runtime).
foreach(conflicting_name IN LISTS dependency_conflicts_FILENAMES)
  set(app_local_dependency "${runtime_search_directory}/${conflicting_name}")
  if(NOT EXISTS "${app_local_dependency}")
    message(FATAL_ERROR "Conflicting dependency '${conflicting_name}' has no app-local build copy.")
  endif()
  list(APPEND resolved_dependencies "${app_local_dependency}")
endforeach()

if(WIN32)
  list(
    FILTER unresolved_dependencies EXCLUDE
    REGEX ".*(AzureAttestManager|AzureAttestNormal|HvsiFileTrust|PdmUtilities|wpaxholder)\\.dll$")
endif()

if(unresolved_dependencies)
  list(JOIN unresolved_dependencies ", " unresolved_list)
  message(FATAL_ERROR "Native runtime dependencies could not be resolved: ${unresolved_list}")
endif()

file(REMOVE_RECURSE "${OUTPUT_DIRECTORY}")
file(MAKE_DIRECTORY "${OUTPUT_DIRECTORY}")
set(runtime_files ${runtime_roots} ${resolved_dependencies})
list(REMOVE_DUPLICATES runtime_files)
foreach(runtime_file IN LISTS runtime_files)
  file(COPY "${runtime_file}" DESTINATION "${OUTPUT_DIRECTORY}" FOLLOW_SYMLINK_CHAIN)
endforeach()

list(LENGTH runtime_files runtime_file_count)
message(STATUS "Collected ${runtime_file_count} self-contained native runtime files in '${OUTPUT_DIRECTORY}'.")
