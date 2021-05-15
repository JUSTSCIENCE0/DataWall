#pragma once
#include <windows.h>
#include <string>

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

#ifdef DATAWALLENGINE_EXPORTS
#define DATAWALLENGINE_API __declspec(dllexport)
#else
#define DATAWALLENGINE_API __declspec(dllimport)
#endif

namespace DataWallEngine
{
    typedef struct {
        BSTR SerialNumber;
        BSTR Manufacturer;
        BSTR Product;
    } MotherboardInfo;

    typedef struct {
        BSTR Manufacturer;
        BSTR Name;
        BSTR ProcessorId;
        BSTR SerialNumber;
        UINT32 NumberOfCores;
    } ProcessorInfo;

    typedef struct {
        BSTR Caption;
        BSTR Name;
        BSTR VideoProcessor;
        BSTR DeviceID;
    } VideoAdapterInfo;

    typedef struct {
        UINT64 id;
        std::string name;
        UINT64 code;
    } LibraryUnit;

    extern "C" DATAWALLENGINE_API HRESULT InitializeEngine(const char* logfile, bool log_to_stdout, const char* server_addr, UINT16 port);
    extern "C" DATAWALLENGINE_API HRESULT UninitializeEngine();
    extern "C" DATAWALLENGINE_API HRESULT GetSystemConfiguration(UINT8 & MBs, UINT8 & CPUs, UINT8 & GPUs);
    extern "C" DATAWALLENGINE_API HRESULT GetMotherboardInfo(MotherboardInfo * &info, UINT8 MBs);
    extern "C" DATAWALLENGINE_API HRESULT GetProcessorInfo(ProcessorInfo * &info, UINT8 CPUs);
    extern "C" DATAWALLENGINE_API HRESULT GetVideoAdapterInfo(VideoAdapterInfo * &info, UINT8 GPUs);
    
    extern "C" DATAWALLENGINE_API HRESULT NetworkAuthentication(const char* nickname, const char* password);
    extern "C" DATAWALLENGINE_API HRESULT NetworkRegistration(const char* nickname, const char* password);
    extern "C" DATAWALLENGINE_API HRESULT SendDeviceConfiguration(const char* mb, const char* cpu, const char* gpu);
    extern "C" DATAWALLENGINE_API HRESULT RequestLibrary(LibraryUnit* &library, int &number);
    extern "C" DATAWALLENGINE_API HRESULT GenerateKey(const char* sys_info, UINT64 lib_code, BYTE* key);
    extern "C" DATAWALLENGINE_API HRESULT InstallSoftware(const char* id, const char* path, BYTE* key);

    extern "C" DATAWALLENGINE_API HRESULT EncryptData(BYTE* data, INT32 size, BYTE* key);
    extern "C" DATAWALLENGINE_API HRESULT DecryptData(BYTE * data, INT32 size, BYTE * key);
    extern "C" DATAWALLENGINE_API HRESULT CalculateHash(BYTE * data, UINT64 size, BYTE* state);

    extern "C" DATAWALLENGINE_API HRESULT PackInContainer(BYTE* data, INT32 size, ContentType type, BYTE* key, const char* container_name);
    extern "C" DATAWALLENGINE_API HRESULT ReadFromContainer(const char* container_name, BYTE* key, BYTE* &data, INT32 &size, ContentType &type);

    extern "C" DATAWALLENGINE_API HRESULT CalculateSoftHASH(const char* path, BYTE * state);
}