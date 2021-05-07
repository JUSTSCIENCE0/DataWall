cmake_minimum_required(VERSION 3.8)
project (TestDLL LANGUAGES CXX)

set(SOURCES TestDLL.cpp dllmain.cpp pch.cpp)
set(HEADERS TestDLL.h framework.h)

set ( CMAKE_WINDOWS_EXPORT_ALL_SYMBOLS ON )
add_library(TestDLL SHARED ${SOURCES} ${HEADERS})

include_directories("${CMAKE_SOURCE_DIR}\\MemoryModule")
target_precompile_headers(TestDLL PRIVATE "pch.h")

add_compile_definitions(TESTDLL_EXPORTS)