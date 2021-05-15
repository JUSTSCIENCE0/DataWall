#pragma once

#define WIN32_LEAN_AND_MEAN             // Исключите редко используемые компоненты из заголовков Windows
#define _CRT_SECURE_NO_WARNINGS

// Файлы заголовков Windows
#include <windows.h>
#include <wmmintrin.h> 
#include <stdio.h>
#include <stdint.h>
#include <string>
#include <ctime>

#include <wmmintrin.h> 

#include <openssl/applink.c>
#include <openssl/ssl.h>
#include <openssl/err.h>
#include <openssl/dh.h>
#include <openssl/engine.h>