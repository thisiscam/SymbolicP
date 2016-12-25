CMAKE_MINIMUM_REQUIRED(VERSION 3.0)

include(ExternalProject)

project(MeddlyWrap)

set(MEDDLY_PROJ_PREFIX meddly-project)
set(MEDDLY_PROJ_DIR ${CMAKE_CURRENT_SOURCE_DIR}/${MEDDLY_PROJ_PREFIX})

ExternalProject_Add(meddly_project
	PREFIX ${MEDDLY_PROJ_PREFIX}
	SVN_REPOSITORY svn://svn.code.sf.net/p/meddly/code/trunk
	UPDATE_COMMAND ""
	CONFIGURE_COMMAND 
				./autogen.sh 
			  COMMAND 
			  	./configure --prefix=${MEDDLY_PROJ_DIR} CXXFLAGS=-g\ -O0\ 
	BUILD_COMMAND make
	BUILD_IN_SOURCE 1
	INSTALL_COMMAND make install
)

set(meddly_INCLUDE_DIR ${MEDDLY_PROJ_DIR}/include)
set(meddly_LIBRARY_DIR ${MEDDLY_PROJ_DIR}/lib)
link_directories(${meddly_LIBRARY_DIR})

add_subdirectory(wrapper)
