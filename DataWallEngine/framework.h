#pragma once

#define WIN32_LEAN_AND_MEAN
#define _CRT_SECURE_NO_WARNINGS
#define _WINSOCK_DEPRECATED_NO_WARNINGS
// Файлы заголовков Windows
#include <windows.h>
#include <ctime>
#include <iostream>
#include <list>
#include <comdef.h>
#include <Wbemidl.h>
#include <WinSock2.h>

#include <openssl/ssl.h>
#include <openssl/err.h>
#include <openssl/sha.h>

#include <wmmintrin.h> 
#include <immintrin.h>

#pragma comment(lib, "wbemuuid.lib")
#pragma comment(lib, "Ws2_32.lib")

#pragma warning ( disable : 6386 )

