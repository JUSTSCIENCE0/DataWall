#pragma once

#ifdef TESTDLL_EXPORTS
#define TESTDLL_EXPORTS_API __declspec(dllexport)
#else
#define TESTDLL_EXPORTS_API __declspec(dllimport)
#endif