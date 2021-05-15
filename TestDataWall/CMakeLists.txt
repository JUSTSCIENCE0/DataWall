cmake_minimum_required(VERSION 3.8)
project (TestSoft LANGUAGES CXX)

set(SOURCES TestDataWall.cpp)
cmake_policy(SET CMP0079 NEW)
add_executable(TestSoft WIN32 ${SOURCES})

include_directories("${CMAKE_SOURCE_DIR}\\DataWallLoader")
target_link_libraries(TestSoft LINK_PUBLIC DataWallLoader)