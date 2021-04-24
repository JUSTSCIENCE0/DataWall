cmake_minimum_required(VERSION 3.8)
project (DataWallEngine LANGUAGES CXX)

set(SOURCES DataWallEngine.cpp dllmain.cpp pch.cpp)
set(HEADERS DataWallEngine.h framework.h)

set ( CMAKE_WINDOWS_EXPORT_ALL_SYMBOLS ON )
add_library(DataWallEngine SHARED ${SOURCES} ${HEADERS})

include_directories("C:\\Program Files\\OpenSSL-Win64\\include")
target_precompile_headers(DataWallEngine PRIVATE "pch.h")
target_link_libraries(DataWallEngine "C:\\Program Files\\OpenSSL-Win64\\lib\\VC\\*.lib")

add_compile_definitions(DATAWALLENGINE_EXPORTS)