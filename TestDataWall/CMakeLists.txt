cmake_minimum_required(VERSION 3.8)
project (TestDataWall LANGUAGES CXX)

set(SOURCES TestDataWall.cpp)
cmake_policy(SET CMP0079 NEW)
add_executable(TestDataWall ${SOURCES})

include_directories("${CMAKE_SOURCE_DIR}\\DataWallEngine")
include_directories("${CMAKE_SOURCE_DIR}\\DataWallLoader")
target_link_libraries(TestDataWall LINK_PUBLIC DataWallEngine)
target_link_libraries(TestDataWall LINK_PUBLIC DataWallLoader)