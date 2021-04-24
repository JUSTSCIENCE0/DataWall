cmake_minimum_required(VERSION 3.8)
project (DataWallLoader LANGUAGES CXX)

set(SOURCES DataWallLoader.cpp dllmain.cpp pch.cpp)
set(HEADERS DataWallLoader.h framework.h)

set ( CMAKE_WINDOWS_EXPORT_ALL_SYMBOLS ON )
add_library(DataWallLoader SHARED ${SOURCES} ${HEADERS})

include_directories("${CMAKE_SOURCE_DIR}\\MemoryModule")
target_precompile_headers(DataWallLoader PRIVATE "pch.h")
target_link_libraries(DataWallLoader LINK_PUBLIC MemoryModule)

add_compile_definitions(DATAWALLLOADER_EXPORTS)