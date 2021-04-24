cmake_minimum_required(VERSION 3.8)
project("DataWall" LANGUAGES CXX CSharp)

set( LIBRARY_OUTPUT_PATH ${CMAKE_BINARY_DIR}/bin )
set( EXECUTABLE_OUTPUT_PATH ${CMAKE_BINARY_DIR}/bin )

add_subdirectory(MemoryModule)
add_subdirectory(DataWallLoader)
add_subdirectory(DataWallEngine)
add_subdirectory(DataWallService)
add_subdirectory(DataWallServer)
add_subdirectory(TestDataWall)

#${EXECUTABLE_OUTPUT_PATH}
#