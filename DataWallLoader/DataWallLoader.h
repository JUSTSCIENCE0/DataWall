#pragma once

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
}
