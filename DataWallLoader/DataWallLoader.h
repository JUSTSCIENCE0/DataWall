#pragma once

#ifndef DATA_WALL_CONTENT
#define  DATA_WALL_CONTENT
#define  DATA_WALL_CODE 0x4C575444 // DTWL
#define  CONTENT_DLL    0x4C4C44   // DLL
#define  CONTENT_IMAGE  0x474D49   // IMG
#define  CONTENT_TEXT   0x545854   // TXT
#define  CONTENT_SOUND  0x444E53   // SND
#define  CONTENT_BIN    0x4E4942   // BIN

#define  WRONG_DATA     0xFD000001
#define  WRONG_CONTENT  0xFD000002

typedef UINT32 ContentType;
#endif // !DATA_WALL_CONTENT

#ifdef DATAWALLLOADER_EXPORTS
#define DATAWALLLOADER_API __declspec(dllexport)
#else
#define DATAWALLLOADER_API __declspec(dllimport)
#endif

typedef void* HMEMORYMODULE;
#define DATA_WALL_CODE 0x4C575444 // DTWL
#define CONTENT_DLL    0x4C4C44   // DLL

namespace DataWallLoader
{
    extern "C" DATAWALLLOADER_API HRESULT EncryptData(BYTE* data, INT32 size, BYTE* key);
    extern "C" DATAWALLLOADER_API HRESULT DecryptData(BYTE* data, INT32 size, BYTE* key);
    extern "C" DATAWALLLOADER_API HMEMORYMODULE LoadEncryptedLibrary(const char* path, BYTE * key);
    extern "C" DATAWALLLOADER_API void FreeEncryptedLibrary(HMEMORYMODULE handle);
    extern "C" DATAWALLLOADER_API FARPROC LoadEncryptedFunction(HMEMORYMODULE handle, const char* name);
    extern "C" DATAWALLLOADER_API HRESULT InitDH();

    extern "C" DATAWALLLOADER_API HRESULT SendKey(const char* soft_name, BYTE* key);
    extern "C" DATAWALLLOADER_API BYTE*   LoadKey();
    extern "C" DATAWALLLOADER_API HRESULT ReadFromContainer(
        const char* container_name, 
        BYTE * key, 
        BYTE * &data, 
        INT32 & size, 
        ContentType & type);
}
