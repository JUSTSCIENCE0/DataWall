cmake_minimum_required(VERSION 3.8)
project (DataWallService LANGUAGES CXX)

set(SOURCES DataWallService.cpp)

add_executable(DataWallService ${SOURCES})

include_directories("${CMAKE_SOURCE_DIR}\\DataWallEngine")
include_directories("${CMAKE_SOURCE_DIR}\\DataWallLoader")
target_link_libraries(DataWallService LINK_PUBLIC DataWallEngine)
target_link_libraries(DataWallService LINK_PUBLIC DataWallLoader)