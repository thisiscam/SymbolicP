CMAKE_MINIMUM_REQUIRED(VERSION 3.0)

project(BDD_Z3_Wrap)

file (GLOB SRC *.cxx)

set(LIB_DIR ${CMAKE_CURRENT_SOURCE_DIR}/../../../libraries)

set(BUDDYSHARP_DIR ${LIB_DIR}/BuDDySharp)
set(BUDDY_DIR ${BUDDYSHARP_DIR}/buddy/)

include_directories(${BUDDY_DIR}/src)

include_directories(${BUDDYSHARP_DIR}/build/buddy/src)
link_directories(${BUDDYSHARP_DIR}/build/buddy/src)

add_library(BDD_Z3_Wrap MODULE ${SRC})

find_library(Z3_LIBRARY
  NAMES z3
  PATHS ${Z3_PKGCONF_LIBRARY_DIRS}
)

target_link_libraries(BDD_Z3_Wrap ${Z3_LIBRARY} buddy_shared)
