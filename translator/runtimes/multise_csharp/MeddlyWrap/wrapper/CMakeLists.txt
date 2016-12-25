file (GLOB SRC *.cxx)

add_library(Meddly_Wrap MODULE ${SRC})

target_compile_features(Meddly_Wrap PRIVATE cxx_override)

target_include_directories(Meddly_Wrap PRIVATE ${meddly_INCLUDE_DIR})
target_link_libraries(Meddly_Wrap meddly)
add_dependencies(Meddly_Wrap meddly_project)