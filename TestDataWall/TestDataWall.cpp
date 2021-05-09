#define _CRT_SECURE_NO_WARNINGS

#include <windows.h>
#include <stdlib.h>
#include <string.h>
#include <tchar.h>

#include <stdio.h>

// Global variables

//HINSTANCE hInst;
//static TCHAR szWindowClass[] = _T("DesktopApp");
//static TCHAR szTitle[] = _T("TestSoft");

void print(BYTE* data, int size)
{
    for (int i = 0; i < size; i++)
        printf("%02X ", data[i]);
    printf("\n");
}

/*LRESULT CALLBACK WndProc(
    _In_ HWND   hWnd,
    _In_ UINT   message,
    _In_ WPARAM wParam,
    _In_ LPARAM lParam
)
{
    PAINTSTRUCT ps;
    HDC hdc;
    TCHAR greeting[] = _T("Hello, Windows desktop!");

    switch (message)
    {
    case WM_PAINT:
        hdc = BeginPaint(hWnd, &ps);

        // Here your application is laid out.
        // For this introduction, we just print out "Hello, Windows desktop!"
        // in the top left corner.
        TextOut(hdc,
            5, 5,
            greeting, _tcslen(greeting));
        // End application-specific layout section.

        EndPaint(hWnd, &ps);
        break;
    case WM_DESTROY:
        PostQuitMessage(0);
        break;
    default:
        return DefWindowProc(hWnd, message, wParam, lParam);
        break;
    }

    return 0;
}*/

typedef void (CALLBACK* LOADLOGODATA)(BYTE*, TCHAR*);
typedef int (CALLBACK* INITWIND)(HINSTANCE, HINSTANCE, LPSTR, int);

int CALLBACK WinMain(
    _In_ HINSTANCE hInstance,
    _In_opt_ HINSTANCE hPrevInstance,
    _In_ LPSTR     lpCmdLine,
    _In_ int       nCmdShow)
{
    /*HMEMORYMODULE myLib = NULL;
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
    DataWallEngine::PackInContainer(data, (INT32)SIZE, CONTENT_DLL, key, "D:\\DataWall\\library.pak");
    int size;
    DataWallEngine::ContentType type;
    DataWallEngine::ReadFromContainer("D:\\DataWall\\container.pak", key, data, size, type);
    print(data, size);
    printf("\n");*/
    /*
    WNDCLASSEX wcex;

    wcex.cbSize = sizeof(WNDCLASSEX);
    wcex.style = CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc = WndProc;
    wcex.cbClsExtra = 0;
    wcex.cbWndExtra = 0;
    wcex.hInstance = hInstance;
    wcex.hIcon = LoadIcon(hInstance, IDI_APPLICATION);
    wcex.hCursor = LoadCursor(NULL, IDC_ARROW);
    wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
    wcex.lpszMenuName = NULL;
    wcex.lpszClassName = szWindowClass;
    wcex.hIconSm = LoadIcon(wcex.hInstance, IDI_APPLICATION);

    if (!RegisterClassEx(&wcex))
    {
        MessageBox(NULL,
            _T("Call to RegisterClassEx failed!"),
            _T("Windows Desktop Guided Tour"),
            NULL);

        return 1;
    }

    HWND hWnd = CreateWindow(
        szWindowClass,
        szTitle,
        WS_OVERLAPPEDWINDOW,
        500, 200,
        500, 500,
        NULL,
        NULL,
        hInstance,
        NULL
    );
    if (!hWnd)
    {
        MessageBox(NULL,
            _T("Call to CreateWindow failed!"),
            _T("Windows Desktop Guided Tour"),
            NULL);

        return 1;
    }

    ShowWindow(hWnd,
        nCmdShow);
    UpdateWindow(hWnd);

    // Main message loop:
    MSG msg;
    while (GetMessage(&msg, NULL, 0, 0))
    {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }
    */

    HINSTANCE hDLL;
    INITWIND  InitializeWindow;
    LOADLOGODATA LoadLogoData;
    HRESULT hr;

    hDLL = LoadLibrary("TestDLL.dll");

    if (!hDLL)
    {
        MessageBox(NULL,
            _T("Error when load dll"),
            _T("Error"),
            NULL);
        return -1;
    }

    LoadLogoData = (LOADLOGODATA)GetProcAddress(hDLL, "LoadLogoData");
    if (!LoadLogoData)
    {
        MessageBox(NULL,
            _T("Error when load function"),
            _T("Error"),
            NULL);
        FreeLibrary(hDLL);
        return -1;
    }

    FILE* fLogo = fopen("D:\\logo.bmp", "rb");
    if (!fLogo)
    {
        MessageBox(NULL,
            _T("Error when load logo"),
            _T("Error"),
            NULL);
        FreeLibrary(hDLL);
        return -1;
    }

    fseek(fLogo, 0, SEEK_END);
    size_t lSize = ftell(fLogo);
    rewind(fLogo);

    BYTE* bLogo = new BYTE[lSize];
    fread(bLogo, 1, lSize, fLogo);
    fclose(fLogo);

    FILE* fText = fopen("D:\\text.txt", "rb");
    if (!fText)
    {
        MessageBox(NULL,
            _T("Error when load text"),
            _T("Error"),
            NULL);
        delete[] bLogo;
        FreeLibrary(hDLL);
        return -1;
    }

    fseek(fText, 0, SEEK_END);
    lSize = (ftell(fText) / sizeof(TCHAR));
    rewind(fText);

    TCHAR* bText = new TCHAR[lSize];
    fread(bText, sizeof(TCHAR), lSize, fText);
    fclose(fText);

    LoadLogoData(bLogo, bText);

    InitializeWindow = (INITWIND)GetProcAddress(hDLL, "InitializeWindow");
    if (!InitializeWindow)
    {
        MessageBox(NULL,
            _T("Error when load function"),
            _T("Error"),
            NULL);
        delete[] bLogo;
        FreeLibrary(hDLL);
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
        FreeLibrary(hDLL);
        return -1;
    }

    FreeLibrary(hDLL);
    delete[] bLogo;
    printf("Success!\n");
    return 0;// (int)msg.wParam;
}