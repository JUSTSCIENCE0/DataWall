#define _CRT_SECURE_NO_WARNINGS

#include <iostream>
#include <ctime>
#include "../DataWallEngine/DataWallEngine.h"
#include "../DataWallLoader/DataWallLoader.h"

void print(BYTE* data, int size)
{
    for (int i = 0; i < size; i++)
        printf("%02X ", data[i]);
    printf("\n");
}

#define SIZE lib_size//56

typedef int (CALLBACK* TestFunc)(int, int);
TestFunc addNumbers = NULL;

int main()
{
    //HRESULT hr;
    UINT8 MBs, CPUs, GPUs;
    DataWallEngine::MotherboardInfo* mInfo;
    DataWallEngine::ProcessorInfo* pInfo;
    DataWallEngine::VideoAdapterInfo* vInfo;

    /*
    HMEMORYMODULE myLib = NULL;
    BYTE* key = new BYTE[16];
    for (int i = 0; i < 16; i++)
    {
        key[i] = (BYTE)i;
    }

    myLib = DataWallLoader::LoadEncryptedLibrary("D:\\DataWall\\library.pak", key);
    addNumbers = (TestFunc)DataWallLoader::LoadEncryptedFunction(myLib, "addNumbers");
    int res = addNumbers(2, 3);
    printf("%d\n", res);
    DataWallLoader::FreeEncryptedLibrary(myLib);
    */
    /*srand(time(0));
    unsigned int code = rand() % 65536;
    code <<= 16;
    code += rand() % 65536;
    printf("%ld\n", code);*/

    //while (true)
    //{
    //    char name[] = "";
    //    char password[] = "";
    //    scanf("%s", name);
    //    scanf("%s", password);

    //    BYTE hash[32] = "";
    //    printf("\n%s\n", name);
    //    printf("%s\n", password);
    //    HRESULT hr = DataWallEngine::CalculateHash((BYTE*)password, strlen(password), hash);
    //    //print(hash, 32);

    //    std::string path = "D:\\DataWall\\" + std::string(name);
    //    FILE* f = fopen(path.c_str(), "wb");
    //    char text[65] = "";
    //    char* pntr = text;
    //    for (int i = 0; i < 32; i++, pntr+=2)
    //    {
    //        snprintf(pntr, 3, "%02X", hash[i]);
    //    }

    //    printf("%s\n", text);
    //    fprintf(f, "%s", text);
    //    fflush(f);
    //    fclose(f);

    //}
    
    /*FILE* fdll = fopen("D:\\DataWall\\SampleDLL.dll", "rb");
    if (!fdll)
    {
        printf("Error when open file\n");
        return NULL;
    }

    fseek(fdll, 0, SEEK_END);
    size_t lib_size = ftell(fdll);
    if (!lib_size)
    {
        printf("Error when read size\n");
        fclose(fdll);
        return NULL;
    }

    BYTE* data = new BYTE[lib_size];
    fseek(fdll, 0, SEEK_SET);
    if (lib_size != fread(data, 1, lib_size, fdll))
    {
        printf("Error when fread\n");
        fclose(fdll);
        return NULL;
    }
    fclose(fdll);*/

    /*for (int i = 0; i < SIZE; i++)
    {
        data[i] = (BYTE)i;
    }*/

    //DataWallEngine::PackInContainer(data, (INT32)SIZE, CONTENT_DLL, key, "D:\\DataWall\\library.pak");

    /*int size;
    DataWallEngine::ContentType type;
    DataWallEngine::ReadFromContainer("D:\\DataWall\\container.pak", key, data, size, type);

    print(data, size);
    printf("\n");*/

    /*hr = DataWallEngine::EncryptData(data, SIZE, key);
    print(data, SIZE);
    printf("\n");

    hr = DataWallEngine::DecryptData(data, SIZE, key);
    print(data, SIZE);*/

    /*hr = DataWallEngine::CalculateHash(data, a, state);
    printf("0x%08x%08x%08x%08x\n", state[0], state[1], state[2], state[3]);*/

    HRESULT hr = DataWallEngine::InitializeEngine("D:\\logfile.txt", true, "192.168.56.1", 22876);
    if (FAILED(hr))
    {
        printf("Error!\n");
        system("pause");
        return hr;
    }

    //DataWallEngine::NetworkAuthentication("user", "user");
    //DataWallEngine::NetworkRegistration("user", "password");

    hr = DataWallEngine::GetSystemConfiguration(MBs, CPUs, GPUs);
    if (FAILED(hr))
    {
        printf("Error!\n");
        return hr;
    }

    mInfo = new DataWallEngine::MotherboardInfo[MBs];
    pInfo = new DataWallEngine::ProcessorInfo[CPUs];
    vInfo = new DataWallEngine::VideoAdapterInfo[GPUs];

    hr = DataWallEngine::GetMotherboardInfo(mInfo, MBs);
    if (FAILED(hr))
    {
        printf("Error!\n");
        return hr;
    }

    hr = DataWallEngine::GetProcessorInfo(pInfo, CPUs);
    if (FAILED(hr))
    {
        printf("Error!\n");
        return hr;
    }

    hr = DataWallEngine::GetVideoAdapterInfo(vInfo, GPUs);
    if (FAILED(hr))
    {
        printf("Error!\n");
        return hr;
    }

    system("pause");

    hr = DataWallEngine::UninitializeEngine();
    if (FAILED(hr))
    {
        printf("Error!\n");
        return hr;
    }

    printf("Success!\n");
}