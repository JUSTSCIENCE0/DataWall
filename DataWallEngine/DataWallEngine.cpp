#include "pch.h"
#include "DataWallEngine.h"

#define DO_ENC_BLOCK(m,k) \
    do{\
        m = _mm_xor_si128       (m, k[ 0]); \
        m = _mm_aesenc_si128    (m, k[ 1]); \
        m = _mm_aesenc_si128    (m, k[ 2]); \
        m = _mm_aesenc_si128    (m, k[ 3]); \
        m = _mm_aesenc_si128    (m, k[ 4]); \
        m = _mm_aesenc_si128    (m, k[ 5]); \
        m = _mm_aesenc_si128    (m, k[ 6]); \
        m = _mm_aesenc_si128    (m, k[ 7]); \
        m = _mm_aesenc_si128    (m, k[ 8]); \
        m = _mm_aesenc_si128    (m, k[ 9]); \
        m = _mm_aesenclast_si128(m, k[10]);\
    }while(0)
#define DO_DEC_BLOCK(m,k) \
    do{\
        m = _mm_xor_si128       (m, k[10+0]); \
        m = _mm_aesdec_si128    (m, k[10+1]); \
        m = _mm_aesdec_si128    (m, k[10+2]); \
        m = _mm_aesdec_si128    (m, k[10+3]); \
        m = _mm_aesdec_si128    (m, k[10+4]); \
        m = _mm_aesdec_si128    (m, k[10+5]); \
        m = _mm_aesdec_si128    (m, k[10+6]); \
        m = _mm_aesdec_si128    (m, k[10+7]); \
        m = _mm_aesdec_si128    (m, k[10+8]); \
        m = _mm_aesdec_si128    (m, k[10+9]); \
        m = _mm_aesdeclast_si128(m, k[0]);\
    }while(0)
#define AES_128_key_exp(k, rcon) aes_128_key_expansion(k, _mm_aeskeygenassist_si128(k, rcon))
__m128i aes_128_key_expansion(__m128i key, __m128i keygened) 
{
    keygened = _mm_shuffle_epi32(keygened, _MM_SHUFFLE(3, 3, 3, 3));
    key = _mm_xor_si128(key, _mm_slli_si128(key, 4));
    key = _mm_xor_si128(key, _mm_slli_si128(key, 4));
    key = _mm_xor_si128(key, _mm_slli_si128(key, 4));
    return _mm_xor_si128(key, keygened);
}
void aes128_load_key_enc_only(uint8_t* enc_key, __m128i* key_schedule) 
{
    key_schedule[0] = _mm_loadu_si128((const __m128i*) enc_key);
    key_schedule[1] = AES_128_key_exp(key_schedule[0], 0x01);
    key_schedule[2] = AES_128_key_exp(key_schedule[1], 0x02);
    key_schedule[3] = AES_128_key_exp(key_schedule[2], 0x04);
    key_schedule[4] = AES_128_key_exp(key_schedule[3], 0x08);
    key_schedule[5] = AES_128_key_exp(key_schedule[4], 0x10);
    key_schedule[6] = AES_128_key_exp(key_schedule[5], 0x20);
    key_schedule[7] = AES_128_key_exp(key_schedule[6], 0x40);
    key_schedule[8] = AES_128_key_exp(key_schedule[7], 0x80);
    key_schedule[9] = AES_128_key_exp(key_schedule[8], 0x1B);
    key_schedule[10] = AES_128_key_exp(key_schedule[9], 0x36);
}
void aes128_load_key(uint8_t* enc_key, __m128i* key_schedule) 
{
    aes128_load_key_enc_only(enc_key, key_schedule);

    // generate decryption keys in reverse order.
    // k[10] is shared by last encryption and first decryption rounds
    // k[0] is shared by first encryption round and last decryption round (and is the original user key)
    // For some implementation reasons, decryption key schedule is NOT the encryption key schedule in reverse order
    key_schedule[11] = _mm_aesimc_si128(key_schedule[9]);
    key_schedule[12] = _mm_aesimc_si128(key_schedule[8]);
    key_schedule[13] = _mm_aesimc_si128(key_schedule[7]);
    key_schedule[14] = _mm_aesimc_si128(key_schedule[6]);
    key_schedule[15] = _mm_aesimc_si128(key_schedule[5]);
    key_schedule[16] = _mm_aesimc_si128(key_schedule[4]);
    key_schedule[17] = _mm_aesimc_si128(key_schedule[3]);
    key_schedule[18] = _mm_aesimc_si128(key_schedule[2]);
    key_schedule[19] = _mm_aesimc_si128(key_schedule[1]);
}
void aes128_enc(__m128i* key_schedule, uint8_t* plainText, uint8_t* cipherText) 
{
    __m128i m = _mm_loadu_si128((__m128i*) plainText);

    DO_ENC_BLOCK(m, key_schedule);

    _mm_storeu_si128((__m128i*) cipherText, m);
}
void aes128_dec(__m128i* key_schedule, uint8_t* cipherText, uint8_t* plainText) 
{
    __m128i m = _mm_loadu_si128((__m128i*) cipherText);

    DO_DEC_BLOCK(m, key_schedule);

    _mm_storeu_si128((__m128i*) plainText, m);
}

FILE* f_logs = NULL;
void print_log(const char* _Format, ...)
{
    if (!f_logs)
        return;

    std::time_t t = std::time(0);
    std::tm* now = std::localtime(&t);

    va_list args;
    va_start(args, _Format);
    char* buffer = new char[1024];
    memset(buffer, ' ', 20);
    sprintf(buffer, "%02d.%02d.%04d %02d:%02d:%02d  ",
        now->tm_mday,
        now->tm_mon + 1,
        now->tm_year + 1900,
        now->tm_hour,
        now->tm_min,
        now->tm_sec
        );
    vsprintf(buffer + 20, _Format, args);
    size_t size = std::strlen(buffer);
    if (size < 1023)
    {
        buffer[size] = '\n';
        size++;
        buffer[size] = 0;
    }
    va_end(args);

    fprintf(f_logs, buffer);
    fflush(f_logs);
}
char* convert_w(BSTR data)
{
    size_t len = wcslen(data);
    char* res = new char[len + 1];
    wcstombs(res, data, len);
    res[len] = 0;
    return res;
}

bool InitializeSockets()
{
    WSADATA WsaData;
    return WSAStartup(MAKEWORD(2, 2), &WsaData) == NO_ERROR;
}

void ShutdownSockets()
{
    WSACleanup();
}

namespace DataWallEngine
{
    IWbemLocator* pLoc = NULL;
    IWbemServices* pSvc = NULL;
    IEnumWbemClassObject* pEnumerator = NULL;
    IWbemClassObject* pclsObj = NULL;
    ULONG uReturn = 0;
    VARIANT vtProp;
    bool Initialized = false;

    SOCKET hsock = 0;
    SSL* ssl;

    int SendPacket(BYTE* buf, size_t size)
    {
        BYTE* send_buf = new BYTE[size + 4];
        memcpy(send_buf, &size, 4);
        memcpy(send_buf + 4, buf, size);
        size_t res = SSL_write(ssl, send_buf, (int)(size + 4));
        if (res < 0)
        {
            int err = SSL_get_error(ssl, (int)res);
            switch (err)
            {
                case SSL_ERROR_WANT_WRITE:
                    return 0;
                case SSL_ERROR_WANT_READ:
                    return 0;
                case SSL_ERROR_ZERO_RETURN:
                case SSL_ERROR_SYSCALL:
                case SSL_ERROR_SSL:
                default:
                    return -1;
            }
        }
        return (int)(res - 4);
    }

    HRESULT InitializeEngine(const char* logfile, const char* server_addr, UINT16 port)
    {
        HRESULT hr;

        if (logfile)
        {
            f_logs = fopen(logfile, "wb");
        }
        print_log("DataWallEngine::InitializeEngine - Logging initialized");

        print_log("DataWallEngine::InitializeEngine - Initialize COM");
        hr = CoInitializeEx(0, COINIT_MULTITHREADED);
        if (FAILED(hr))
        {
            print_log("Error, code = 0x%x", hr);
            return hr;
        }
        print_log("Successfull");

        print_log("DataWallEngine::InitializeEngine - Set general COM security levels");
        hr = CoInitializeSecurity(NULL, -1, NULL, NULL, RPC_C_AUTHN_LEVEL_DEFAULT,
            RPC_C_IMP_LEVEL_IMPERSONATE, NULL, EOAC_NONE, NULL);
        if (FAILED(hr))
        {
            print_log("Error, code = 0x%x", hr);
            CoUninitialize();
            return hr;
        }
        print_log("Successfull");

        print_log("DataWallEngine::InitializeEngine - Obtain the initial locator to WMI");
        hr = CoCreateInstance(CLSID_WbemLocator, 0, CLSCTX_INPROC_SERVER,
            IID_IWbemLocator, (LPVOID*)&pLoc);
        if (FAILED(hr))
        {
            print_log("Error, code = 0x%x", hr);
            CoUninitialize();
            return hr;
        }
        print_log("Successfull");
        
        print_log("DataWallEngine::InitializeEngine - Connect to WMI through the IWbemLocator::ConnectServer method");
        hr = pLoc->ConnectServer(_bstr_t(L"ROOT\\CIMV2"), NULL, NULL, 0, NULL, 0, 0, &pSvc);
        if (FAILED(hr))
        {
            print_log("Error, code = 0x%x", hr);
            pLoc->Release();
            CoUninitialize();
            return 1;
        }
        print_log("Successfull");

        print_log("DataWallEngine::InitializeEngine - Set security levels on the proxy");
        hr = CoSetProxyBlanket(pSvc, RPC_C_AUTHN_WINNT, RPC_C_AUTHZ_NONE, NULL,                        // Server principal name 
            RPC_C_AUTHN_LEVEL_CALL, RPC_C_IMP_LEVEL_IMPERSONATE, NULL, EOAC_NONE);
        if (FAILED(hr))
        {
            print_log("Error, code = 0x%x", hr);
            pSvc->Release();
            pLoc->Release();
            CoUninitialize();
            return hr;
        }
        print_log("Successfull");

        if (!server_addr)
        {
            Initialized = true;
            return S_OK;
        }

        print_log("Initialize WinSockets");
        if (!InitializeSockets())
        {
            print_log("Error, code = 0x%x", WSAGetLastError());
            return E_FAIL;
        }
        print_log("Successfull");

        print_log("Initialize socket");
        hsock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
        if (hsock < 0)
        {
            print_log("Error, code = 0x%x", WSAGetLastError());
            return E_FAIL;
        }

        struct sockaddr_in sa;
        memset(&sa, 0, sizeof(sa));
        sa.sin_family = AF_INET;
        sa.sin_addr.s_addr = inet_addr(server_addr);
        sa.sin_port = htons(port);
        int socklen = sizeof(sa);
        print_log("Successfull");

        print_log("Connect with server");
        if (connect(hsock, (struct sockaddr*)&sa, socklen))
        {
            print_log("Error, code = 0x%x", WSAGetLastError());
            return E_FAIL;
        }
        print_log("Successfull");
        
        print_log("Init SSL");
        SSL_library_init();
        SSLeay_add_ssl_algorithms();
        SSL_load_error_strings();
        const SSL_METHOD* meth = TLSv1_2_client_method();
        SSL_CTX* ctx = SSL_CTX_new(meth);
        ssl = SSL_new(ctx);
        if (!ssl) 
        {
            print_log("Error, code = 0x%x", ERR_get_error());
            return E_FAIL;
        }
        print_log("Successfull");

        print_log("Connect over SSL");
        SSL_set_fd(ssl, (int)hsock);
        if (SSL_connect(ssl) <= 0)
        {
            print_log("Error, code = 0x%x", ERR_get_error());
            return E_FAIL;
        }
        print_log("Successfull");

        Initialized = true;
        return S_OK;
    }

    HRESULT UninitializeEngine()
    {
        if (!Initialized)
            return E_FAIL;

        print_log("DataWallEngine::UninitializeEngine");
        pSvc->Release();
        pLoc->Release();
        pEnumerator->Release();
        CoUninitialize();
        print_log("Successfull");

        print_log("Clear SSL");
        if (!SSL_clear(ssl))
        {
            print_log("Error, code = 0x%x", ERR_get_error());
            return E_FAIL;
        }
        print_log("Successfull");

        print_log("Close socket");
        if (closesocket(hsock))
        {
            print_log("Error, code = 0x%x", WSAGetLastError());
            return E_FAIL;
        }
        print_log("Successfull");

        Initialized = false;
        return S_OK;
    }

    HRESULT GetSystemConfiguration(UINT8& MBs, UINT8& CPUs, UINT8& GPUs)
    {
        if (!Initialized)
            return E_FAIL;
        MBs = 0; CPUs = 0; GPUs = 0;
        HRESULT hr;

        print_log("DataWallEngine::GetSystemConfiguration - Query for motherboards quantity");
        hr = pSvc->ExecQuery(bstr_t("WQL"), bstr_t("SELECT * FROM Win32_BaseBoard"),
            WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY, NULL, &pEnumerator);
        if (FAILED(hr))
        {
            print_log("Error, code = 0x%x", hr);
            return hr;
        }
        print_log("Successfull");

        print_log("DataWallEngine::GetSystemConfiguration - Motherboards count");
        while (pEnumerator)
        {
            hr = pEnumerator->Next(WBEM_INFINITE, 1,
                &pclsObj, &uReturn);
            if (FAILED(hr))
            {
                print_log("Error, code = 0x%x", hr);
                return hr;
            }
            if (0 == uReturn)
            {
                break;
            }

            MBs++;
        }
        print_log("Motherboards quantity = %d", MBs);
        print_log("Successfull");

        print_log("DataWallEngine::GetSystemConfiguration - Query for CPUs quantity");
        hr = pSvc->ExecQuery(bstr_t("WQL"), bstr_t("SELECT * FROM Win32_Processor"),
            WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY, NULL, &pEnumerator);
        if (FAILED(hr))
        {
            print_log("Error, code = 0x%x", hr);
            return hr;
        }
        print_log("Successfull");

        print_log("DataWallEngine::GetSystemConfiguration - CPUs count");
        while (pEnumerator)
        {
            hr = pEnumerator->Next(WBEM_INFINITE, 1,
                &pclsObj, &uReturn);
            if (FAILED(hr))
            {
                print_log("Error, code = 0x%x", hr);
                return hr;
            }
            if (0 == uReturn)
            {
                break;
            }

            CPUs++;
        }
        print_log("CPUs quantity = %d", CPUs);
        print_log("Successfull");

        print_log("DataWallEngine::GetSystemConfiguration - Query for GPUs quantity");
        hr = pSvc->ExecQuery(bstr_t("WQL"), bstr_t("SELECT * FROM Win32_VideoController"),
            WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY, NULL, &pEnumerator);
        if (FAILED(hr))
        {
            print_log("Error, code = 0x%x", hr);
            return hr;
        }
        print_log("Successfull");

        print_log("DataWallEngine::GetSystemConfiguration - GPUs count");
        while (pEnumerator)
        {
            hr = pEnumerator->Next(WBEM_INFINITE, 1,
                &pclsObj, &uReturn);
            if (FAILED(hr))
            {
                print_log("Error, code = 0x%x", hr);
                return hr;
            }
            if (0 == uReturn)
            {
                break;
            }

            GPUs++;
        }
        print_log("GPUs quantity = %d", GPUs);
        print_log("Successfull");

        return S_OK;
    }

    HRESULT GetMotherboardInfo(MotherboardInfo* &info, UINT8 MBs)
    {
        if (!Initialized)
            return E_FAIL;

        HRESULT hr;
        print_log("DataWallEngine::GetMotherboardInfo - Query for data");
        hr = pSvc->ExecQuery(bstr_t("WQL"), bstr_t("SELECT * FROM Win32_BaseBoard"),
            WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY, NULL, &pEnumerator);
        if (FAILED(hr))
        {
            print_log("Error, code = 0x%x", hr);
            return hr;
        }
        print_log("Successfull");

        int n = 0;
        print_log("DataWallEngine::GetMotherboardInfo - Read INFO");
        while (pEnumerator && (n < MBs))
        {
            hr = pEnumerator->Next(WBEM_INFINITE, 1,
                &pclsObj, &uReturn);
            if (FAILED(hr))
            {
                print_log("Error, code = 0x%x", hr);
                return hr;
            }
            if (0 == uReturn)
            {
                break;
            }

            print_log("Motherboard INFO:");

            pclsObj->Get(L"SerialNumber", 0, &vtProp, 0, 0);
            print_log("SerialNumber: %s", convert_w(vtProp.bstrVal));
            info[n].SerialNumber = vtProp.bstrVal;
            VariantClear(&vtProp);

            pclsObj->Get(L"Manufacturer", 0, &vtProp, 0, 0);
            print_log("Manufacturer: %s", convert_w(vtProp.bstrVal));
            info[n].Manufacturer = vtProp.bstrVal;
            VariantClear(&vtProp);

            pclsObj->Get(L"Product", 0, &vtProp, 0, 0);
            print_log("Product: %s", convert_w(vtProp.bstrVal));
            info[n].Product = vtProp.bstrVal;
            VariantClear(&vtProp);

            pclsObj->Release();
            n++;
        }
        print_log("Successfull");

        return S_OK;
    }

    HRESULT GetProcessorInfo(ProcessorInfo* &info, UINT8 CPUs)
    {
        if (!Initialized)
            return E_FAIL;

        HRESULT hr;
        print_log("DataWallEngine::GetProcessorInfo - Query for data");
        hr = pSvc->ExecQuery(bstr_t("WQL"), bstr_t("SELECT * FROM Win32_Processor"),
            WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY, NULL, &pEnumerator);
        if (FAILED(hr))
        {
            print_log("Error, code = 0x%x", hr);
            return hr;
        }
        print_log("Successfull");

        int n = 0;
        print_log("DataWallEngine::GetProcessorInfo - Read INFO");
        while (pEnumerator && (n < CPUs))
        {
            hr = pEnumerator->Next(WBEM_INFINITE, 1,
                &pclsObj, &uReturn);
            if (FAILED(hr))
            {
                print_log("Error, code = 0x%x", hr);
                return hr;
            }
            if (0 == uReturn)
            {
                break;
            }

            print_log("Processor INFO:");

            hr = pclsObj->Get(L"Manufacturer", 0, &vtProp, 0, 0);
            print_log("Manufacturer: %s", convert_w(vtProp.bstrVal));
            info[n].Manufacturer = vtProp.bstrVal;
            VariantClear(&vtProp);

            hr = pclsObj->Get(L"Name", 0, &vtProp, 0, 0);
            print_log("Name: %s", convert_w(vtProp.bstrVal));
            info[n].Name = vtProp.bstrVal;
            VariantClear(&vtProp);

            hr = pclsObj->Get(L"NumberOfCores", 0, &vtProp, 0, 0);
            print_log("NumberOfCores: %d", vtProp.lVal);
            info[n].NumberOfCores = vtProp.lVal;
            VariantClear(&vtProp);

            hr = pclsObj->Get(L"ProcessorId", 0, &vtProp, 0, 0);
            print_log("ProcessorId: %s", convert_w(vtProp.bstrVal));
            info[n].ProcessorId = vtProp.bstrVal;
            VariantClear(&vtProp);

            hr = pclsObj->Get(L"SerialNumber", 0, &vtProp, 0, 0);
            print_log("SerialNumber: %s", convert_w(vtProp.bstrVal));
            info[n].SerialNumber = vtProp.bstrVal;
            VariantClear(&vtProp);

            pclsObj->Release();
            n++;
        }
        print_log("Successfull");

        return S_OK;
    }

    HRESULT GetVideoAdapterInfo(VideoAdapterInfo* &info, UINT8 GPUs)
    {
        if (!Initialized)
            return E_FAIL;

        HRESULT hr;
        print_log("DataWallEngine::GetVideoAdapterInfo - Query for data");
        hr = pSvc->ExecQuery(bstr_t("WQL"), bstr_t("SELECT * FROM Win32_VideoController"),
            WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY, NULL, &pEnumerator);
        if (FAILED(hr))
        {
            print_log("Error, code = 0x%x", hr);
            return hr;
        }
        print_log("Successfull");

        int n = 0;
        print_log("DataWallEngine::GetProcessorInfo - Read INFO");
        while (pEnumerator && (n < GPUs))
        {
            HRESULT hr = pEnumerator->Next(WBEM_INFINITE, 1,
                &pclsObj, &uReturn);
            if (FAILED(hr))
            {
                print_log("Error, code = 0x%x", hr);
                return hr;
            }
            if (0 == uReturn)
            {
                break;
            }

            print_log("Video Adapter INFO:");

            hr = pclsObj->Get(L"Caption", 0, &vtProp, 0, 0);
            print_log("Caption: %s", convert_w(vtProp.bstrVal));
            info[n].Caption = vtProp.bstrVal;
            VariantClear(&vtProp);

            hr = pclsObj->Get(L"Name", 0, &vtProp, 0, 0);
            print_log("Name: %s", convert_w(vtProp.bstrVal));
            info[n].Name = vtProp.bstrVal;
            VariantClear(&vtProp);

            hr = pclsObj->Get(L"VideoProcessor", 0, &vtProp, 0, 0);
            print_log("VideoProcessor: %s", convert_w(vtProp.bstrVal));
            info[n].VideoProcessor = vtProp.bstrVal;
            VariantClear(&vtProp);

            n++;
            pclsObj->Release();
        }
        print_log("Successfull");

        return S_OK;
    }

    HRESULT CalculateHash(BYTE* data, UINT64 size, UINT32* state)
    {
        if (!data)
            return E_INVALIDARG;

        if (!state)
            return E_INVALIDARG;

        __m128i STATE0, STATE1;
        __m128i MSG, TMP;
        __m128i MSG0, MSG1, MSG2, MSG3;
        __m128i ABEF_SAVE, CDGH_SAVE;
        const __m128i MASK = _mm_set_epi64x(0x0c0d0e0f08090a0bULL, 0x0405060700010203ULL);

        /* Load initial values */
        TMP = _mm_loadu_si128((const __m128i*)&state[0]);
        STATE1 = _mm_loadu_si128((const __m128i*)&state[4]);

        TMP = _mm_shuffle_epi32(TMP, 0xB1);          /* CDAB */
        STATE1 = _mm_shuffle_epi32(STATE1, 0x1B);    /* EFGH */
        STATE0 = _mm_alignr_epi8(TMP, STATE1, 8);    /* ABEF */
        STATE1 = _mm_blend_epi16(STATE1, TMP, 0xF0); /* CDGH */

        while (size >= 64)
        {
            /* Save current state */
            ABEF_SAVE = STATE0;
            CDGH_SAVE = STATE1;

            /* Rounds 0-3 */
            MSG = _mm_loadu_si128((const __m128i*) (data + 0));
            MSG0 = _mm_shuffle_epi8(MSG, MASK);
            MSG = _mm_add_epi32(MSG0, _mm_set_epi64x(0xE9B5DBA5B5C0FBCFULL, 0x71374491428A2F98ULL));
            STATE1 = _mm_sha256rnds2_epu32(STATE1, STATE0, MSG);
            MSG = _mm_shuffle_epi32(MSG, 0x0E);
            STATE0 = _mm_sha256rnds2_epu32(STATE0, STATE1, MSG);

            /* Rounds 4-7 */
            MSG1 = _mm_loadu_si128((const __m128i*) (data + 16));
            MSG1 = _mm_shuffle_epi8(MSG1, MASK);
            MSG = _mm_add_epi32(MSG1, _mm_set_epi64x(0xAB1C5ED5923F82A4ULL, 0x59F111F13956C25BULL));
            STATE1 = _mm_sha256rnds2_epu32(STATE1, STATE0, MSG);
            MSG = _mm_shuffle_epi32(MSG, 0x0E);
            STATE0 = _mm_sha256rnds2_epu32(STATE0, STATE1, MSG);
            MSG0 = _mm_sha256msg1_epu32(MSG0, MSG1);

            /* Rounds 8-11 */
            MSG2 = _mm_loadu_si128((const __m128i*) (data + 32));
            MSG2 = _mm_shuffle_epi8(MSG2, MASK);
            MSG = _mm_add_epi32(MSG2, _mm_set_epi64x(0x550C7DC3243185BEULL, 0x12835B01D807AA98ULL));
            STATE1 = _mm_sha256rnds2_epu32(STATE1, STATE0, MSG);
            MSG = _mm_shuffle_epi32(MSG, 0x0E);
            STATE0 = _mm_sha256rnds2_epu32(STATE0, STATE1, MSG);
            MSG1 = _mm_sha256msg1_epu32(MSG1, MSG2);

            /* Rounds 12-15 */
            MSG3 = _mm_loadu_si128((const __m128i*) (data + 48));
            MSG3 = _mm_shuffle_epi8(MSG3, MASK);
            MSG = _mm_add_epi32(MSG3, _mm_set_epi64x(0xC19BF1749BDC06A7ULL, 0x80DEB1FE72BE5D74ULL));
            STATE1 = _mm_sha256rnds2_epu32(STATE1, STATE0, MSG);
            TMP = _mm_alignr_epi8(MSG3, MSG2, 4);
            MSG0 = _mm_add_epi32(MSG0, TMP);
            MSG0 = _mm_sha256msg2_epu32(MSG0, MSG3);
            MSG = _mm_shuffle_epi32(MSG, 0x0E);
            STATE0 = _mm_sha256rnds2_epu32(STATE0, STATE1, MSG);
            MSG2 = _mm_sha256msg1_epu32(MSG2, MSG3);

            /* Rounds 16-19 */
            MSG = _mm_add_epi32(MSG0, _mm_set_epi64x(0x240CA1CC0FC19DC6ULL, 0xEFBE4786E49B69C1ULL));
            STATE1 = _mm_sha256rnds2_epu32(STATE1, STATE0, MSG);
            TMP = _mm_alignr_epi8(MSG0, MSG3, 4);
            MSG1 = _mm_add_epi32(MSG1, TMP);
            MSG1 = _mm_sha256msg2_epu32(MSG1, MSG0);
            MSG = _mm_shuffle_epi32(MSG, 0x0E);
            STATE0 = _mm_sha256rnds2_epu32(STATE0, STATE1, MSG);
            MSG3 = _mm_sha256msg1_epu32(MSG3, MSG0);

            /* Rounds 20-23 */
            MSG = _mm_add_epi32(MSG1, _mm_set_epi64x(0x76F988DA5CB0A9DCULL, 0x4A7484AA2DE92C6FULL));
            STATE1 = _mm_sha256rnds2_epu32(STATE1, STATE0, MSG);
            TMP = _mm_alignr_epi8(MSG1, MSG0, 4);
            MSG2 = _mm_add_epi32(MSG2, TMP);
            MSG2 = _mm_sha256msg2_epu32(MSG2, MSG1);
            MSG = _mm_shuffle_epi32(MSG, 0x0E);
            STATE0 = _mm_sha256rnds2_epu32(STATE0, STATE1, MSG);
            MSG0 = _mm_sha256msg1_epu32(MSG0, MSG1);

            /* Rounds 24-27 */
            MSG = _mm_add_epi32(MSG2, _mm_set_epi64x(0xBF597FC7B00327C8ULL, 0xA831C66D983E5152ULL));
            STATE1 = _mm_sha256rnds2_epu32(STATE1, STATE0, MSG);
            TMP = _mm_alignr_epi8(MSG2, MSG1, 4);
            MSG3 = _mm_add_epi32(MSG3, TMP);
            MSG3 = _mm_sha256msg2_epu32(MSG3, MSG2);
            MSG = _mm_shuffle_epi32(MSG, 0x0E);
            STATE0 = _mm_sha256rnds2_epu32(STATE0, STATE1, MSG);
            MSG1 = _mm_sha256msg1_epu32(MSG1, MSG2);

            /* Rounds 28-31 */
            MSG = _mm_add_epi32(MSG3, _mm_set_epi64x(0x1429296706CA6351ULL, 0xD5A79147C6E00BF3ULL));
            STATE1 = _mm_sha256rnds2_epu32(STATE1, STATE0, MSG);
            TMP = _mm_alignr_epi8(MSG3, MSG2, 4);
            MSG0 = _mm_add_epi32(MSG0, TMP);
            MSG0 = _mm_sha256msg2_epu32(MSG0, MSG3);
            MSG = _mm_shuffle_epi32(MSG, 0x0E);
            STATE0 = _mm_sha256rnds2_epu32(STATE0, STATE1, MSG);
            MSG2 = _mm_sha256msg1_epu32(MSG2, MSG3);

            /* Rounds 32-35 */
            MSG = _mm_add_epi32(MSG0, _mm_set_epi64x(0x53380D134D2C6DFCULL, 0x2E1B213827B70A85ULL));
            STATE1 = _mm_sha256rnds2_epu32(STATE1, STATE0, MSG);
            TMP = _mm_alignr_epi8(MSG0, MSG3, 4);
            MSG1 = _mm_add_epi32(MSG1, TMP);
            MSG1 = _mm_sha256msg2_epu32(MSG1, MSG0);
            MSG = _mm_shuffle_epi32(MSG, 0x0E);
            STATE0 = _mm_sha256rnds2_epu32(STATE0, STATE1, MSG);
            MSG3 = _mm_sha256msg1_epu32(MSG3, MSG0);

            /* Rounds 36-39 */
            MSG = _mm_add_epi32(MSG1, _mm_set_epi64x(0x92722C8581C2C92EULL, 0x766A0ABB650A7354ULL));
            STATE1 = _mm_sha256rnds2_epu32(STATE1, STATE0, MSG);
            TMP = _mm_alignr_epi8(MSG1, MSG0, 4);
            MSG2 = _mm_add_epi32(MSG2, TMP);
            MSG2 = _mm_sha256msg2_epu32(MSG2, MSG1);
            MSG = _mm_shuffle_epi32(MSG, 0x0E);
            STATE0 = _mm_sha256rnds2_epu32(STATE0, STATE1, MSG);
            MSG0 = _mm_sha256msg1_epu32(MSG0, MSG1);

            /* Rounds 40-43 */
            MSG = _mm_add_epi32(MSG2, _mm_set_epi64x(0xC76C51A3C24B8B70ULL, 0xA81A664BA2BFE8A1ULL));
            STATE1 = _mm_sha256rnds2_epu32(STATE1, STATE0, MSG);
            TMP = _mm_alignr_epi8(MSG2, MSG1, 4);
            MSG3 = _mm_add_epi32(MSG3, TMP);
            MSG3 = _mm_sha256msg2_epu32(MSG3, MSG2);
            MSG = _mm_shuffle_epi32(MSG, 0x0E);
            STATE0 = _mm_sha256rnds2_epu32(STATE0, STATE1, MSG);
            MSG1 = _mm_sha256msg1_epu32(MSG1, MSG2);

            /* Rounds 44-47 */
            MSG = _mm_add_epi32(MSG3, _mm_set_epi64x(0x106AA070F40E3585ULL, 0xD6990624D192E819ULL));
            STATE1 = _mm_sha256rnds2_epu32(STATE1, STATE0, MSG);
            TMP = _mm_alignr_epi8(MSG3, MSG2, 4);
            MSG0 = _mm_add_epi32(MSG0, TMP);
            MSG0 = _mm_sha256msg2_epu32(MSG0, MSG3);
            MSG = _mm_shuffle_epi32(MSG, 0x0E);
            STATE0 = _mm_sha256rnds2_epu32(STATE0, STATE1, MSG);
            MSG2 = _mm_sha256msg1_epu32(MSG2, MSG3);

            /* Rounds 48-51 */
            MSG = _mm_add_epi32(MSG0, _mm_set_epi64x(0x34B0BCB52748774CULL, 0x1E376C0819A4C116ULL));
            STATE1 = _mm_sha256rnds2_epu32(STATE1, STATE0, MSG);
            TMP = _mm_alignr_epi8(MSG0, MSG3, 4);
            MSG1 = _mm_add_epi32(MSG1, TMP);
            MSG1 = _mm_sha256msg2_epu32(MSG1, MSG0);
            MSG = _mm_shuffle_epi32(MSG, 0x0E);
            STATE0 = _mm_sha256rnds2_epu32(STATE0, STATE1, MSG);
            MSG3 = _mm_sha256msg1_epu32(MSG3, MSG0);

            /* Rounds 52-55 */
            MSG = _mm_add_epi32(MSG1, _mm_set_epi64x(0x682E6FF35B9CCA4FULL, 0x4ED8AA4A391C0CB3ULL));
            STATE1 = _mm_sha256rnds2_epu32(STATE1, STATE0, MSG);
            TMP = _mm_alignr_epi8(MSG1, MSG0, 4);
            MSG2 = _mm_add_epi32(MSG2, TMP);
            MSG2 = _mm_sha256msg2_epu32(MSG2, MSG1);
            MSG = _mm_shuffle_epi32(MSG, 0x0E);
            STATE0 = _mm_sha256rnds2_epu32(STATE0, STATE1, MSG);

            /* Rounds 56-59 */
            MSG = _mm_add_epi32(MSG2, _mm_set_epi64x(0x8CC7020884C87814ULL, 0x78A5636F748F82EEULL));
            STATE1 = _mm_sha256rnds2_epu32(STATE1, STATE0, MSG);
            TMP = _mm_alignr_epi8(MSG2, MSG1, 4);
            MSG3 = _mm_add_epi32(MSG3, TMP);
            MSG3 = _mm_sha256msg2_epu32(MSG3, MSG2);
            MSG = _mm_shuffle_epi32(MSG, 0x0E);
            STATE0 = _mm_sha256rnds2_epu32(STATE0, STATE1, MSG);

            /* Rounds 60-63 */
            MSG = _mm_add_epi32(MSG3, _mm_set_epi64x(0xC67178F2BEF9A3F7ULL, 0xA4506CEB90BEFFFAULL));
            STATE1 = _mm_sha256rnds2_epu32(STATE1, STATE0, MSG);
            MSG = _mm_shuffle_epi32(MSG, 0x0E);
            STATE0 = _mm_sha256rnds2_epu32(STATE0, STATE1, MSG);

            /* Combine state  */
            STATE0 = _mm_add_epi32(STATE0, ABEF_SAVE);
            STATE1 = _mm_add_epi32(STATE1, CDGH_SAVE);

            data += 64;
            size -= 64;
        }

        TMP = _mm_shuffle_epi32(STATE0, 0x1B);       /* FEBA */
        STATE1 = _mm_shuffle_epi32(STATE1, 0xB1);    /* DCHG */
        STATE0 = _mm_blend_epi16(TMP, STATE1, 0xF0); /* DCBA */
        STATE1 = _mm_alignr_epi8(STATE1, TMP, 8);    /* ABEF */

        /* Save state */
        _mm_storeu_si128((__m128i*)&state[0], STATE0);
        _mm_storeu_si128((__m128i*)&state[4], STATE1);

        return S_OK;
    }

    HRESULT EncryptData(BYTE* data, INT32 size, BYTE* key)
    {
        if (!data || size <= 0 || !key)
            return E_INVALIDARG;

        if (size % 16 != 0)
            return E_INVALIDARG;

        __m128i key_schedule[20];
        aes128_load_key(key, key_schedule);

        while (size >= 16)
        {
            aes128_enc(key_schedule, data, data);
            size -= 16;
            data += 16;
        }

        return S_OK;
    }

    HRESULT DecryptData(BYTE* data, INT32 size, BYTE* key)
    {
        if (!data || size <= 0 || !key)
            return E_INVALIDARG;

        if (size % 16 != 0)
            return E_INVALIDARG;

        __m128i key_schedule[20];
        aes128_load_key(key, key_schedule);

        while (size >= 16)
        {
            aes128_dec(key_schedule, data, data);
            size -= 16;
            data += 16;
        }

        return S_OK;
    }

    HRESULT NetworkRegistration(const char* nickname, const char* password)
    {
        if (!Initialized)
            return E_FAIL;

        print_log("Start network registration");
        size_t size = std::strlen(nickname) + std::strlen(password) + 3;
        BYTE* buffer = new BYTE[size];
        buffer[0] = 240; // registration code
        memcpy(buffer + 1, nickname, std::strlen(nickname));
        buffer[std::strlen(nickname) + 1] = 0;
        memcpy(buffer + std::strlen(nickname) + 2, password, std::strlen(password));
        buffer[size - 1] = 0;

        if (SendPacket(buffer, size) != size)
        {
            print_log("Failed to send registration info");
            delete[] buffer;
            return E_FAIL;
        }


        delete[] buffer;
        return S_OK;
    }

    HRESULT PackInContainer(BYTE* data, INT32 size, ContentType type, BYTE* key, const char* container_name)
    {
        int sz16;
        if (size % 16 != 0)
            sz16 = (size / 16 + 2) * 16;
        else
            sz16 = size + 16;

        BYTE* buffer = new BYTE[sz16];
        UINT32 special_code = DATA_WALL_CODE;

        memset(buffer, 0, sz16);

        memcpy(buffer,      &special_code, 4    );
        memcpy(buffer + 4,  &type,         4    );
        memcpy(buffer + 8,  &sz16,         4    );
        memcpy(buffer + 12, &size,         4    );
        memcpy(buffer + 16, data,          size );

        HRESULT hr = EncryptData(buffer, sz16, key);
        if (!SUCCEEDED(hr))
        {
            delete[] buffer;
            return hr;
        }

        FILE* f = fopen(container_name, "wb");
        if (!f)
        {
            delete[] buffer;
            return E_FAIL;
        }
        fwrite(buffer, 1, sz16, f);
        fflush(f);
        fclose(f);

        delete[] buffer;
        return S_OK;
    }

    HRESULT ReadFromContainer(const char* container_name, BYTE* key, BYTE*& data, INT32& size, ContentType& type)
    {
        FILE* f = fopen(container_name, "rb");
        if (!f)
        {
            return E_FAIL;
        }

        BYTE* header = new BYTE[16];
        if (16 != fread(header, 1, 16, f))
        {
            delete[] header;
            return E_FAIL;
        }

        HRESULT hr = DecryptData(header, 16, key);
        if (!SUCCEEDED(hr))
        {
            delete[] header;
            return hr;
        }

        UINT32* NumberPNTR = (UINT32*)header;
        UINT32 DataWallCode = *NumberPNTR;
        
        if (DataWallCode != DATA_WALL_CODE)
        {
            delete[] header;
            return WRONG_DATA;
        }
        NumberPNTR++;

        UINT32 TypeOfContent = *NumberPNTR;
        switch (TypeOfContent)
        {
        case CONTENT_DLL:
            type = CONTENT_DLL;
            break;
        case CONTENT_IMAGE:
            type = CONTENT_IMAGE;
            break;
        case CONTENT_SOUND:
            type = CONTENT_SOUND;
            break;
        case CONTENT_TEXT:
            type = CONTENT_TEXT;
            break;
        default:
            delete[] header;
            delete NumberPNTR;
            return WRONG_CONTENT;
        }
        NumberPNTR++;

        int encrSize = (*NumberPNTR) - 16;
        NumberPNTR++;
        size = *NumberPNTR;

        BYTE* enc_data = new BYTE[encrSize];
        if (encrSize != fread(enc_data, 1, encrSize, f))
        {
            delete[] header;
            delete[] enc_data;
            return E_FAIL;
        }
        hr = DecryptData(enc_data, encrSize, key);
        if (!SUCCEEDED(hr))
        {
            delete[] header;
            delete[] enc_data;
            return hr;
        }

        data = enc_data;

        delete[] header;
        return S_OK;
    }
}