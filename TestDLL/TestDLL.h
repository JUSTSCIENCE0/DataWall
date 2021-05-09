#pragma once

#ifdef TESTDLL_EXPORTS
#define TESTDLL_EXPORTS_API __declspec(dllexport)
#else
#define TESTDLL_EXPORTS_API __declspec(dllimport)
#endif

extern "C" TESTDLL_EXPORTS_API
void LoadLogoData(BYTE* logo, TCHAR* text);

extern "C" TESTDLL_EXPORTS_API 
int InitializeWindow(
    HINSTANCE hInstance,
    HINSTANCE hPrevInstance,
    LPSTR     lpCmdLine,
    int       nCmdShow);