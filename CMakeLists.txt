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
add_subdirectory(TestDLL)

INCLUDE_EXTERNAL_MSPROJECT(DataWallClient  ${CMAKE_SOURCE_DIR}/DataWallClient/DataWallClient.csproj)

#${EXECUTABLE_OUTPUT_PATH}
#