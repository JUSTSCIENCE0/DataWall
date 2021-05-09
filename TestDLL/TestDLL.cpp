#include "pch.h"
#include "TestDLL.h"

HINSTANCE hInst;
static TCHAR szWindowClass[] = _T("DesktopApp");
static TCHAR szTitle[] = _T("TestSoft");

HBITMAP hBitmap = NULL;
TCHAR* greeting = _T("Hello, Windows desktop!");

LRESULT CALLBACK WndProc(
    _In_ HWND   hWnd,
    _In_ UINT   message,
    _In_ WPARAM wParam,
    _In_ LPARAM lParam
)
{
    PAINTSTRUCT ps;
    HDC hdc;

    RECT Rect;
    LONG Width;
    LONG Height;

    switch (message)
    {
    case WM_CREATE:
        break;
    case WM_PAINT:
        BITMAP          bitmap;
        HDC             hdcMem;
        HGDIOBJ         oldBitmap;

        GetWindowRect(hWnd, &Rect);
        Width = Rect.right - Rect.left - 30;
        Height = Rect.bottom - Rect.top - 50;

        hdc = BeginPaint(hWnd, &ps);

        hdcMem = CreateCompatibleDC(hdc);
        oldBitmap = SelectObject(hdcMem, hBitmap);

        GetObject(hBitmap, sizeof(bitmap), &bitmap);
        StretchBlt(hdc, 0, 0, Width, Height, 
            hdcMem, 0, 0, bitmap.bmWidth, bitmap.bmHeight, SRCCOPY);

        SelectObject(hdcMem, oldBitmap);
        DeleteDC(hdcMem);

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
        DeleteObject(hBitmap);
        PostQuitMessage(0);
        break;
    default:
        return DefWindowProc(hWnd, message, wParam, lParam);
        break;
    }

    return 0;
}

void LoadLogoData(BYTE* logo, TCHAR* text )
{
    BITMAPFILEHEADER* bmfh;
    bmfh = (BITMAPFILEHEADER*)logo;

    BITMAPINFOHEADER* bmih;
    bmih = (BITMAPINFOHEADER*)(logo + sizeof(BITMAPFILEHEADER));
    BITMAPINFO* bmi;
    bmi = (BITMAPINFO*)bmih;

    void* bits;
    bits = (void*)(logo + bmfh->bfOffBits);

    HDC hdc = ::GetDC(NULL);

    hBitmap = CreateDIBitmap(hdc, bmih, CBM_INIT, bits, bmi, DIB_RGB_COLORS);

    ::ReleaseDC(NULL, hdc);

    greeting = _T(text);
}

int InitializeWindow(
    HINSTANCE hInstance,
    HINSTANCE hPrevInstance,
    LPSTR     lpCmdLine,
    int       nCmdShow)
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
        432, 404,
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

    printf("Success!\n");
    return (int)msg.wParam;
}