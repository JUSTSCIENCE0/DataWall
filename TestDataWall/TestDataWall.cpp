#define _CRT_SECURE_NO_WARNINGS

#include <windows.h>
#include <stdlib.h>
#include <string.h>
#include <tchar.h>
#include <stdio.h>
#include <string>

#include "DataWallLoader.h"

void print(BYTE* data, int size)
{
    for (int i = 0; i < size; i++)
        printf("%02X ", data[i]);
    printf("\n");
}

typedef void (CALLBACK* LOADLOGODATA)(BYTE*, TCHAR*);
typedef int (CALLBACK* INITWIND)(HINSTANCE, HINSTANCE, LPSTR, int);

int CALLBACK WinMain(
    _In_ HINSTANCE hInstance,
    _In_opt_ HINSTANCE hPrevInstance,
    _In_ LPSTR     lpCmdLine,
    _In_ int       nCmdShow)
{
    /*BYTE key[16] = { 
        12, 159, 215, 177, 
        120, 46, 30, 5,
        88, 13, 240, 142,
        32, 69, 124, 216
    };*/
    BYTE* key = DataWallLoader::LoadKey();

    MessageBox(NULL,
        _T(lpCmdLine),
        _T("Info"),
        NULL);

    HMEMORYMODULE hDLL;
    INITWIND  InitializeWindow;
    LOADLOGODATA LoadLogoData;
    HRESULT hr;

    char path[1024];
    snprintf(path, 1024, "%s\\TestDLL.pak", lpCmdLine);
    hDLL = DataWallLoader::LoadEncryptedLibrary(path, key);

    if (!hDLL)
    {
        MessageBox(NULL,
            _T("Error when load dll"),
            _T("Error"),
            NULL);
        return -1;
    }

    LoadLogoData = (LOADLOGODATA)DataWallLoader::LoadEncryptedFunction(hDLL, "LoadLogoData");
    if (!LoadLogoData)
    {
        MessageBox(NULL,
            _T("Error when load function"),
            _T("Error"),
            NULL);
        DataWallLoader::FreeEncryptedLibrary(hDLL);
        return -1;
    }

    BYTE* bLogo = NULL;
    int bLogoSize = 0;
    ContentType logoType = 0;

    snprintf(path, 1024, "%s\\logo.pak", lpCmdLine);
    hr = DataWallLoader::ReadFromContainer(
        path,
        key,
        bLogo,
        bLogoSize,
        logoType);
    if (FAILED(hr) || logoType != CONTENT_IMAGE)
    {
        MessageBox(NULL,
            _T("Error when load logo"),
            _T("Error"),
            NULL);
        DataWallLoader::FreeEncryptedLibrary(hDLL);
        return -1;
    }

    BYTE* bText = NULL;
    int bTextSize = 0;
    ContentType textType = 0;

    snprintf(path, 1024, "%s\\text.pak", lpCmdLine);
    hr = DataWallLoader::ReadFromContainer(
        path,
        key,
        bText,
        bTextSize,
        textType);
    if (FAILED(hr) || textType != CONTENT_TEXT)
    {
        MessageBox(NULL,
            _T("Error when load text"),
            _T("Error"),
            NULL);
        delete[] bLogo;
        DataWallLoader::FreeEncryptedLibrary(hDLL);
        return -1;
    }

    LoadLogoData(bLogo, (TCHAR*)bText);

    InitializeWindow = (INITWIND)DataWallLoader::LoadEncryptedFunction(hDLL, "InitializeWindow");
    if (!InitializeWindow)
    {
        MessageBox(NULL,
            _T("Error when load function"),
            _T("Error"),
            NULL);
        delete[] bLogo;
        delete[] bText;
        DataWallLoader::FreeEncryptedLibrary(hDLL);
        return -1;
    }

    // call the function
    int result = InitializeWindow(hInstance, hPrevInstance,
        lpCmdLine, nCmdShow);

    if (result)
    {
        MessageBox(NULL,
            _T("Error when exec function"),
            _T("Error"),
            NULL);
        delete[] bLogo;
        delete[] bText;
        DataWallLoader::FreeEncryptedLibrary(hDLL);
        return -1;
    }

    DataWallLoader::FreeEncryptedLibrary(hDLL);
    delete[] bLogo;
    delete[] bText;
    printf("Success!\n");
    return 0;// (int)msg.wParam;
}