CMAKE_MINIMUM_REQUIRED(VERSION 3.0)

project(BDD_Z3_Wrap)

file (GLOB SRC *.cxx)

OPTION(USE_SYLVAN "Build with sylvan" OFF)

set(LIB_DIR ${CMAKE_CURRENT_SOURCE_DIR}/../../../libraries)

if(USE_SYLVAN)
	set(SYLVANSHARP_DIR ${LIB_DIR}/SylvanSharp)
	set(SYLVAN_DIR ${SYLVANSHARP_DIR}/native)
	set(SYLVAN_WRAPPER_DIR ${SYLVANSHARP_DIR}/wrapper)
	include_directories(${SYLVAN_DIR}/src)
	link_directories(${SYLVANSHARP_DIR}/build/wrapper/)
	set(BDDLIB sylvan_native)
else()
	set(BUDDYSHARP_DIR ${LIB_DIR}/BuDDySharp)
	set(BUDDY_DIR ${BUDDYSHARP_DIR}/buddy/)
	include_directories(${BUDDY_DIR}/src)
	include_directories(${BUDDYSHARP_DIR}/build/buddy/src)
	link_directories(${BUDDYSHARP_DIR}/build/buddy/src)
	set(BDDLIB buddy_shared)
endif()

add_library(BDD_Z3_Wrap MODULE ${SRC})

find_library(Z3_LIBRARY
  NAMES z3
  PATHS ${Z3_PKGCONF_LIBRARY_DIRS}
)

target_link_libraries(BDD_Z3_Wrap ${Z3_LIBRARY} ${BDDLIB})

if(USE_SYLVAN)
	target_compile_definitions(BDD_Z3_Wrap PRIVATE USE_SYLVAN)
endif()