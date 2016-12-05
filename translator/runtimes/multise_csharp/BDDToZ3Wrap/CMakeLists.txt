CMAKE_MINIMUM_REQUIRED(VERSION 3.0)

include(GenerateExportHeader)

project(BDD_Z3_Wrap)

file (GLOB SRC *.cxx)

if(WIN32)
    message(STATUS "Sylvan support is not supported on windows, skipped")
    option(USE_SYLVAN "Build with sylvan" OFF)
else()
    option(USE_SYLVAN "Build with sylvan" ON)
endif()


set(LIB_DIR ${CMAKE_CURRENT_SOURCE_DIR}/../../../libraries)

find_path(Z3_INCLUDE z3.h z3++.h HINTS $ENV{Z3_ROOT}/include)
find_library(Z3_LIBRARY
  NAMES z3
  HINTS $ENV{Z3_ROOT}/bin
)

set(CMAKE_SKIP_RPATH TRUE)

include_directories(${Z3_INCLUDE})

if(USE_SYLVAN)
    set(SYLVANSHARP_DIR ${LIB_DIR}/SylvanSharp)
    set(SYLVAN_DIR ${SYLVANSHARP_DIR}/native)
    set(SYLVAN_WRAPPER_DIR ${SYLVANSHARP_DIR}/wrapper)
    include_directories(${SYLVAN_DIR}/src)
    link_directories(${SYLVANSHARP_DIR}/build/wrapper/)
    set(SYLVANLIB sylvan_native)
    add_library(BDD_SYLVAN_Z3_Wrap MODULE ${SRC})
    generate_export_header(BDD_SYLVAN_Z3_Wrap)
    target_include_directories(BDD_SYLVAN_Z3_Wrap PUBLIC ${CMAKE_CURRENT_BINARY_DIR})
    target_link_libraries(BDD_SYLVAN_Z3_Wrap ${Z3_LIBRARY} sylvan_native)
    target_compile_definitions(BDD_SYLVAN_Z3_Wrap PRIVATE USE_SYLVAN)
    target_compile_features(BDD_SYLVAN_Z3_Wrap PRIVATE cxx_extern_templates)
endif()

set(BUDDYSHARP_DIR ${LIB_DIR}/BuDDySharp)
set(BUDDY_DIR ${BUDDYSHARP_DIR}/buddy/)
include_directories(${BUDDY_DIR}/src)
include_directories(${BUDDYSHARP_DIR}/build/buddy/src)
link_directories(${BUDDYSHARP_DIR}/build/buddy/src)
add_library(BDD_BUDDY_Z3_Wrap MODULE ${SRC})
generate_export_header(BDD_BUDDY_Z3_Wrap)
target_include_directories(BDD_BUDDY_Z3_Wrap PUBLIC ${CMAKE_CURRENT_BINARY_DIR})
target_link_libraries(BDD_BUDDY_Z3_Wrap ${Z3_LIBRARY} buddy_shared)
target_compile_features(BDD_BUDDY_Z3_Wrap PRIVATE cxx_extern_templates)
