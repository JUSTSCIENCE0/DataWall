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
}