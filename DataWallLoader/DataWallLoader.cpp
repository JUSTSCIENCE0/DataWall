#include "pch.h"
#include "DataWallLoader.h"
#include "MemoryModule.h"

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
    char buffer[1024];
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

BOOL  SendString(HANDLE pipe, char* str)
{
    if (pipe == INVALID_HANDLE_VALUE)
        return false;
    if (!str) return false;

    BYTE EOS[1] = { 0 };

    ULONG writen;
    if (!WriteFile(pipe, str, strlen(str), &writen, NULL))
    {
        print_log("writen != strlen(str)");
        return false;
    }
    if (writen != strlen(str))
    {
        print_log("writen != strlen(str)");
        return false;
    }
    WriteFile(pipe, EOS, 1, &writen, NULL);
    if (writen != 1)
    { 
        print_log("writen != 1");
        return false;
    }

    return true;
}
char* RecvString(HANDLE pipe)
{
    char* str = new char[1024];
    char* pntr = str - 1;
    ULONG readed = 0;

    memset(str, 0, 1024);

    do
    {
        pntr++;
        if (!ReadFile(pipe, pntr, 1, &readed, NULL))
        {
            delete[] str;
            return NULL;
        }
        /*if (readed != 2)
        {
            MessageBox(NULL,
                std::to_string(readed).c_str(),
                "readed != 2",
                NULL);
            delete[] str;
            return NULL;
        }*/
    } while (*pntr != 0);
    return str;
}

DH*     private_key = NULL;
BIGNUM* public_key = NULL;
BYTE*   master_key = NULL;
int     master_size = 0;
BOOL generateDH(
    const char* p_char,
    const char* q_char,
    const char* g_char)
{
    int codes;

    if (NULL == (private_key = DH_new()))
        return false;

    if (p_char == NULL ||
        g_char == NULL)
    {
        if (1 != DH_generate_parameters_ex
        (private_key, 512, DH_GENERATOR_2, NULL))
            return false;
    }
    else
    {
        BIGNUM* p = NULL;
        BIGNUM* q = NULL;
        BIGNUM* g = NULL;

        BN_hex2bn(&p, p_char);
        if (q_char)
            BN_hex2bn(&q, q_char);
        BN_hex2bn(&g, g_char);

        if (!p || !g || !q)
            return false;

        if (1 != DH_set0_pqg(private_key, p, q, g))
            return false;
    }
    

    if (1 != DH_check(private_key, &codes))
        return false;
    if (codes != 0)
    {
        abort();
        return false;
    }
    if (1 != DH_generate_key(private_key))
        return false;

    return true;
}
BOOL sendDHKey(HANDLE pipe, BOOL key_only)
{
    print_log("start send dh key");
    BIGNUM* pub = (BIGNUM*)DH_get0_pub_key(private_key);
    if (!pub) return false;
    char* pk_char = BN_bn2hex(pub);

    print_log("public key = %s", pk_char);

    if (!SendString(pipe, pk_char))
    {
        return false;
    }

    print_log("delete[] pk_char;");
    print_log("success");
    if (key_only)
        return true;

    print_log("start send dh p, g, q");
    BIGNUM* p = (BIGNUM*)DH_get0_p(private_key);
    BIGNUM* g = (BIGNUM*)DH_get0_g(private_key);
    BIGNUM* q = (BIGNUM*)DH_get0_q(private_key);

    print_log("readed dh p, g, q");
    if (!p || !g) return false;
    print_log("p and g");
    char* p_char = BN_bn2hex(p);
    char* g_char = BN_bn2hex(g);
    char* q_char = "NULL";
    if (q) q_char = BN_bn2hex(q);

    print_log("p = %s", p_char);
    print_log("g = %s", g_char);
    print_log("q = %s", q_char);

    if (!SendString(pipe, p_char))
    {
        return false;
    }
    if (!SendString(pipe, g_char))
    {
        return false;
    }
    if (!SendString(pipe, q_char))
    {
        return false;
    }
    
    return true;
}
BOOL recvDHKey(HANDLE pipe, BOOL key_only)
{
    print_log("start recv_public key");
    char* pub = RecvString(pipe);
    if (!pub) return false;

    print_log("recv_public key %s", pub);
    BN_hex2bn(&public_key, pub);
    if (!public_key)
    {
        delete[] pub;
        return false;
    }

    delete[] pub;
    if (key_only) return true;

    char* p_char = RecvString(pipe);
    if (!p_char) return false;

    char* g_char = RecvString(pipe);
    if (!g_char)
    {
        delete[] p_char;
        return false;
    }

    char* q_char = RecvString(pipe);
    if (!q_char)
    {
        delete[] p_char;
        delete[] g_char;
        return false;
    }

    BIGNUM *p = NULL,
           *g = NULL,
           *q = NULL;
    BN_hex2bn(&p, p_char);
    BN_hex2bn(&g, g_char);
    if (!strcmp(q_char, "NULL"))
        BN_hex2bn(&q, q_char);

    delete[] p_char;
    delete[] g_char;
    delete[] q_char;
    if (!p || !g) return false;

    if (private_key)
        DH_free(private_key);
    private_key = NULL;
    if (NULL == (private_key = DH_new())) return false;
    if (1 != DH_set0_pqg(private_key, p, q, g)) return false;
    if (1 != DH_generate_key(private_key)) return false;

    return true;
}
BOOL calcSecret()
{
    if (!private_key) return false;

    master_size = DH_size(private_key);
    if (!master_size)
    {
        print_log("Couldn't get master size");
        return false;
    }
    print_log("Master size %d", master_size);
    master_key = new BYTE[master_size];

    if (0 > (master_size = DH_compute_key(master_key, public_key, private_key)))
    {
        print_log("Couldn't compute master key");
        return false;
    }

    for (int i = 0; i < master_size; i++)
    {
        print_log("%d", master_key[i]);
    }

    return true;
}
void XORwithMK(BYTE* data, int len)
{
    if (!master_key) return;

    for (int i = 0; i < len; i++)
    {
        data[i] = data[i] ^ master_key[i];
    }
}

namespace DataWallLoader
{
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

    HMEMORYMODULE LoadEncryptedLibrary(const char* path, BYTE* key)
    {
        size_t lib_size = 0, reader_size = 0;
        BYTE* lib_data = NULL;
        FILE* lib_file = NULL;

        lib_file = fopen(path, "rb");
        if (NULL == lib_file)
        {
            return NULL;
        }

        BYTE* header = new BYTE[16];
        if (16 != fread(header, 1, 16, lib_file))
        {
            delete[] header;
            return NULL;
        }

        HRESULT hr = DecryptData(header, 16, key);
        if (!SUCCEEDED(hr))
        {
            delete[] header;
            return NULL;
        }

        UINT32* NumberPNTR = (UINT32*)header;
        UINT32 DataWallCode = *NumberPNTR;

        if (DataWallCode != DATA_WALL_CODE)
        {
            delete[] header;
            return NULL;
        }
        NumberPNTR++;

        UINT32 TypeOfContent = *NumberPNTR;
        if (TypeOfContent != CONTENT_DLL)
        {
            delete[] header;
            return NULL;
        }
        NumberPNTR++;

        int encrSize = (*NumberPNTR) - 16;
        NumberPNTR++;
        lib_size = (size_t)(*NumberPNTR);

        lib_data = new BYTE[encrSize];
        if (encrSize != fread(lib_data, 1, encrSize, lib_file))
        {
            delete[] header;
            delete[] lib_data;
            return NULL;
        }
        hr = DecryptData(lib_data, encrSize, key);
        if (!SUCCEEDED(hr))
        {
            delete[] header;
            delete[] lib_data;
            return NULL;
        }

        delete[] header;
        return MemoryLoadLibrary(lib_data, lib_size);
    }

    void FreeEncryptedLibrary(HMEMORYMODULE handle)
    {
        MemoryFreeLibrary(handle);
    }

    FARPROC LoadEncryptedFunction(HMEMORYMODULE handle, const char* name)
    {
        return MemoryGetProcAddress(handle, name);
    }

    HRESULT SendKey(const char* soft_name, BYTE* key)
    {
        char pipename[1500];
        snprintf(pipename, 1500, "\\\\.\\pipe\\%s", soft_name);

        HANDLE hSoftPipe = CreateNamedPipe(pipename,
            PIPE_ACCESS_DUPLEX,
            PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT,
            1,
            1024 * 16,
            1024 * 16,
            NMPWAIT_USE_DEFAULT_WAIT,
            NULL);
        if (hSoftPipe == INVALID_HANDLE_VALUE) return E_FAIL;

        if (!ConnectNamedPipe(hSoftPipe, NULL))
        {
            CloseHandle(hSoftPipe);
            return E_FAIL;
        }

        if (!sendDHKey(hSoftPipe, false))
        {
            CloseHandle(hSoftPipe);
            return E_FAIL;
        }

        if (!recvDHKey(hSoftPipe, true))
        {
            CloseHandle(hSoftPipe);
            return E_FAIL;
        }

        if (!calcSecret())
        {
            CloseHandle(hSoftPipe);
            return E_FAIL;
        }

        XORwithMK(key, 16);

        ULONG writen;
        if (!WriteFile(hSoftPipe, key, 16, &writen, NULL))
        {
            CloseHandle(hSoftPipe);
            return E_FAIL;
        }

        char buff[2];
        ULONG readen;
        if (!ReadFile(hSoftPipe, buff, 2, &readen, NULL))
        {
            CloseHandle(hSoftPipe);
            return E_FAIL;
        }

        CloseHandle(hSoftPipe);
        return S_OK;
    }

    HRESULT InitDH()
    {
        if (!generateDH(NULL, NULL, NULL))
        {
            return E_FAIL;
        }

        return S_OK;
    }

    BYTE* LoadKey(const char* soft_name)
    {
        char pipename[1500];
        snprintf(pipename, 1500, "\\\\.\\pipe\\%s", soft_name);

        BYTE* key = new BYTE[16];
        Sleep(1000);
        HANDLE DataWallHandle = CreateFile(
            pipename,
            GENERIC_READ | GENERIC_WRITE,
            FILE_SHARE_WRITE,
            NULL,
            OPEN_EXISTING,
            0,
            NULL);

        if (!recvDHKey(DataWallHandle, false))
        {
            CloseHandle(DataWallHandle);
            return NULL;
        }

        if (!sendDHKey(DataWallHandle, true))
        {
            CloseHandle(DataWallHandle);
            return NULL;
        }

        if (!calcSecret())
        {
            CloseHandle(DataWallHandle);
            return NULL;
        }

        ULONG readed = 0;
        if (!ReadFile(DataWallHandle, key, 16, &readed, NULL))
            return NULL;

        if (readed != 16)
            return NULL;

        XORwithMK(key, 16);

        BYTE answ[2] = { 200, 0 };
        if (!WriteFile(DataWallHandle, answ, 1, NULL, NULL))
            return NULL;

        CloseHandle(DataWallHandle);

        return key;
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
        case CONTENT_BIN:
            type = CONTENT_BIN;
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