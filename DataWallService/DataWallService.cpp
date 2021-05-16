#define _CRT_SECURE_NO_WARNINGS
#define SERVICE_NAME  (LPSTR)("DataWallService")   
#define SET_FAILED bWasFail = true
#define BREAK_FAILED { bWasFail = true; break; }

#define DEBUG

#include <iostream>
#include <Windows.h>
#include <thread>
#include <tchar.h>

#include "../DataWallEngine/DataWallEngine.h"
#include "../DataWallLoader/DataWallLoader.h"

#ifdef DEBUG
#define ENGINE_LOG "D:\\engine_log.txt"
FILE* f_logs = fopen("D:\\serv_log.txt", "wb");
void print_log(const char* _Format, ...)
{
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
#else
#define ENGINE_LOG NULL
void print_log(const char* _Format, ...) {}
#endif

BOOL                  bWasFail = false;

SERVICE_STATUS        g_ServiceStatus = { 0 };
SERVICE_STATUS_HANDLE g_StatusHandle = NULL;
HANDLE                hThread = INVALID_HANDLE_VALUE;
HANDLE                g_ServiceStopEvent = INVALID_HANDLE_VALUE;
HANDLE                hNamedPipe = INVALID_HANDLE_VALUE;

UINT8 MBs, CPUs, GPUs;
DataWallEngine::MotherboardInfo* mInfo;
DataWallEngine::ProcessorInfo* pInfo;
DataWallEngine::VideoAdapterInfo* vInfo;

DataWallEngine::LibraryUnit* library = NULL;
int library_size = 0;

char Motherboard[1024] = "";
char CPU[1024] = "";
char GPU[1024] = "";

bool ReadString(char* output) 
{
    if (hNamedPipe == INVALID_HANDLE_VALUE)
        return false;

    ULONG read = 0;
    int index = 0;
    do {
        if (!ReadFile(hNamedPipe, output + index++, 1, &read, NULL))
        {
            print_log("Error when read from named pipe");
            return false;
        }
    } while (read > 0 && *(output + index - 1) != 0);

    return true;
}
bool WriteString(char* output)
{
    if (hNamedPipe == INVALID_HANDLE_VALUE)
        return false;

    if (!WriteFile(hNamedPipe, output, (DWORD)strlen(output), nullptr, NULL))
    {
        print_log("Error when write to named pipe");
        return false;
    }

    BYTE nl = 0;
    if (!WriteFile(hNamedPipe, &nl, 1, nullptr, NULL))
    {
        print_log("Error when write to named pipe");
        return false;
    }

    return true;
}
char* convert_w(BSTR data)
{
    if (!data)
        return "";

    size_t len = wcslen(data);
    char* res = new char[len + 1];
    wcstombs(res, data, len);
    res[len] = 0;
    return res;
}

VOID WINAPI ServiceMain(DWORD argc, LPTSTR* argv);
VOID WINAPI ServiceCtrlHandler(DWORD);
DWORD WINAPI ServiceWorkerThread(LPVOID lpParam);

VOID WINAPI ServiceMain(DWORD argc, LPTSTR* argv)
{
START:
    DWORD Status = E_FAIL;

    // Register our service control handler with the SCM
    g_StatusHandle = RegisterServiceCtrlHandler(SERVICE_NAME, ServiceCtrlHandler);

    if (g_StatusHandle == NULL)
    {
        return;
    }

    // Tell the service controller we are starting
    ZeroMemory(&g_ServiceStatus, sizeof(g_ServiceStatus));
    g_ServiceStatus.dwServiceType = SERVICE_WIN32_OWN_PROCESS;
    g_ServiceStatus.dwControlsAccepted = SERVICE_ACCEPT_STOP | SERVICE_ACCEPT_SHUTDOWN;
    g_ServiceStatus.dwCurrentState = SERVICE_START_PENDING;
    g_ServiceStatus.dwWin32ExitCode = 0;
    g_ServiceStatus.dwServiceSpecificExitCode = 0;
    g_ServiceStatus.dwCheckPoint = 0;
    g_ServiceStatus.dwWaitHint = 0;

    if (SetServiceStatus(g_StatusHandle, &g_ServiceStatus) == FALSE)
    {
        print_log("ServiceMain: SetServiceStatus returned error");
    }

    /*
     * Perform tasks necessary to start the service here
     */

     // Create a service stop event to wait on later
    g_ServiceStopEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
    if (g_ServiceStopEvent == NULL)
    {
        // Error creating event
        // Tell service controller we are stopped and exit
        g_ServiceStatus.dwControlsAccepted = 0;
        g_ServiceStatus.dwCurrentState = SERVICE_STOPPED;
        g_ServiceStatus.dwWin32ExitCode = GetLastError();
        g_ServiceStatus.dwCheckPoint = 1;

        if (SetServiceStatus(g_StatusHandle, &g_ServiceStatus) == FALSE)
        {
            print_log("ServiceMain: SetServiceStatus returned error");
        }
        return;
    }

    // Tell the service controller we are started
    g_ServiceStatus.dwControlsAccepted = SERVICE_ACCEPT_STOP;
    g_ServiceStatus.dwCurrentState = SERVICE_RUNNING;
    g_ServiceStatus.dwWin32ExitCode = 0;
    g_ServiceStatus.dwCheckPoint = 0;

    if (SetServiceStatus(g_StatusHandle, &g_ServiceStatus) == FALSE)
    {
        print_log("ServiceMain: SetServiceStatus returned error");
    }

    // Start a thread that will perform the main task of the service
    hThread = CreateThread(NULL, 0, ServiceWorkerThread, NULL, 0, NULL);

    if (hThread)
        WaitForSingleObject(hThread, INFINITE);
    else
        return;

    CloseHandle(g_ServiceStopEvent);

    //Service unstoppable
    if (!bWasFail)
    {
        print_log("Service unstoppable");
        goto START;
    }
    else
    {
        print_log("Was FAIL, server stopped");
    }

    g_ServiceStatus.dwControlsAccepted = 0;
    g_ServiceStatus.dwCurrentState = SERVICE_STOPPED;
    g_ServiceStatus.dwWin32ExitCode = 0;
    g_ServiceStatus.dwCheckPoint = 4;

    if (SetServiceStatus(g_StatusHandle, &g_ServiceStatus) == FALSE)
    {
        print_log("ServiceMain: SetServiceStatus returned error");
    }

    return;
}

VOID WINAPI ServiceCtrlHandler(DWORD CtrlCode)
{
    switch (CtrlCode)
    {
    case SERVICE_CONTROL_STOP:

        /*if (g_ServiceStatus.dwCurrentState != SERVICE_RUNNING)
            break;

        SetEvent(g_ServiceStopEvent);

        g_ServiceStatus.dwControlsAccepted = 0;
        g_ServiceStatus.dwCurrentState = SERVICE_STOPPED;
        g_ServiceStatus.dwWin32ExitCode = 0;
        g_ServiceStatus.dwCheckPoint = 4;

        if (SetServiceStatus(g_StatusHandle, &g_ServiceStatus) == FALSE)
        {
            print_log("ServiceCtrlHandler: SetServiceStatus returned error");
        }

        DataWallEngine::UninitializeEngine();
        print_log("SERVICE_CONTROL_STOP");*/

        break;

    default:
        break;
    }
}

DWORD WINAPI ServiceWorkerThread(LPVOID lpParam)
{
    HRESULT hr;
    hNamedPipe = CreateNamedPipe("\\\\.\\pipe\\datawallpipe",
        PIPE_ACCESS_DUPLEX,
        PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT,
        1,
        1024 * 16,
        1024 * 16,
        NMPWAIT_USE_DEFAULT_WAIT,
        NULL);

    if (hNamedPipe == INVALID_HANDLE_VALUE)
    {
        print_log("Start work: Failed to create named pipe");
        SET_FAILED;
        return ERROR_PATH_NOT_FOUND;
    }
    print_log("Start work: Created named pipe");

    hr = DataWallLoader::InitDH();
    if (FAILED(hr))
    {
        print_log("Error in initialize DH");
        SET_FAILED;
        return hr;
    }

    hr = DataWallEngine::InitializeEngine(ENGINE_LOG, false, "192.168.56.1", 22876);
    if (FAILED(hr))
    {
        print_log("Error in initialize engine");
        CloseHandle(hNamedPipe);
        SET_FAILED;
        return hr;
    }
    print_log("Engine initialized");

    hr = DataWallEngine::GetSystemConfiguration(MBs, CPUs, GPUs);
    if (FAILED(hr))
    {
        print_log("Error when GetSystemConfiguration");
        CloseHandle(hNamedPipe);
        DataWallEngine::UninitializeEngine();
        SET_FAILED;
        return hr;
    }
    print_log(
        "In system detected Motherboard - %d, CPU - %d, GPU - %d",
        MBs,
        CPUs,
        GPUs);

    mInfo = new DataWallEngine::MotherboardInfo[MBs];
    pInfo = new DataWallEngine::ProcessorInfo[CPUs];
    vInfo = new DataWallEngine::VideoAdapterInfo[GPUs];

    hr = DataWallEngine::GetMotherboardInfo(mInfo, MBs);
    if (FAILED(hr))
    {
        print_log("Failed to get motherboard info");
        CloseHandle(hNamedPipe);
        DataWallEngine::UninitializeEngine();
        delete[] mInfo;
        delete[] pInfo;
        delete[] vInfo;
        SET_FAILED;
        return hr;
    }
    size_t offset = 0;
    for (int i = 0; i < MBs; i++)
    {
        sprintf(Motherboard + offset, "%s %s %s ", 
            convert_w(mInfo[i].Manufacturer), 
            convert_w(mInfo[i].Product),
            convert_w(mInfo[i].SerialNumber));
        offset = strlen(Motherboard);
    }
    print_log("Motherboard: %s", Motherboard);

    hr = DataWallEngine::GetProcessorInfo(pInfo, MBs);
    if (FAILED(hr))
    {
        print_log("Failed to get CPU info");
        CloseHandle(hNamedPipe);
        DataWallEngine::UninitializeEngine();
        delete[] mInfo;
        delete[] pInfo;
        delete[] vInfo;
        SET_FAILED;
        return hr;
    }
    offset = 0;
    for (int i = 0; i < CPUs; i++)
    {
        sprintf(CPU + offset, "%s %s %ld %s %s ",// 
            convert_w(pInfo[i].Manufacturer),
            convert_w(pInfo[i].Name),
            pInfo[i].NumberOfCores,
            convert_w(pInfo[i].ProcessorId),
            convert_w(pInfo[i].SerialNumber)
        );
        offset = strlen(CPU);
    }
    print_log("CPU: %s", CPU);

    hr = DataWallEngine::GetVideoAdapterInfo(vInfo, MBs);
    if (FAILED(hr))
    {
        print_log("Failed to get GPU info");
        CloseHandle(hNamedPipe);
        DataWallEngine::UninitializeEngine();
        delete[] mInfo;
        delete[] pInfo;
        delete[] vInfo;
        SET_FAILED;
        return hr;
    }
    offset = 0;
    for (int i = 0; i < GPUs; i++)
    {
        sprintf(GPU + offset, "%s %s %s %s ",
            convert_w(vInfo[i].Caption),
            convert_w(vInfo[i].DeviceID),
            convert_w(vInfo[i].Name),
            convert_w(vInfo[i].VideoProcessor)
            );
        offset = strlen(GPU);
    }
    print_log("GPU: %s", GPU);

    if (!ConnectNamedPipe(hNamedPipe, NULL))
    {
        print_log("Failed to connect client to named pipe");
        CloseHandle(hNamedPipe);
        DataWallEngine::UninitializeEngine();
        delete[] mInfo;
        delete[] pInfo;
        delete[] vInfo;
        SET_FAILED;
        return ERROR_INVALID_ACCESS;
    }
    print_log("Client connected to named pipe");

    //  Periodically check if the service has been requested to stop
    while (WaitForSingleObject(g_ServiceStopEvent, 0) != WAIT_OBJECT_0)
    {
        char str[1024];
        if (!ReadString(str)) break;
        print_log("string from pipe: %s", str);

        if ((BYTE)str[0] == 230)
        {
            char login[100], passwd[100];
            if (!ReadString(login)) BREAK_FAILED
            if (!ReadString(passwd)) BREAK_FAILED
            print_log("Auth data - %s %s", login, passwd);

            hr = DataWallEngine::NetworkAuthentication(login, passwd);
            BYTE res_code[2] = "";
            if (SUCCEEDED(hr))
            {
                hr = DataWallEngine::SendDeviceConfiguration(Motherboard, CPU, GPU);
                if (FAILED(hr)) BREAK_FAILED

                res_code[0] = 200;
            }
            else
            {
                res_code[0] = 255;
            }
            
            if (!WriteString((char*)res_code)) BREAK_FAILED

            continue;
        }
        if ((BYTE)str[0] == 100)
        {
            print_log("Request library");
            hr = DataWallEngine::RequestLibrary(library, library_size);
            if (FAILED(hr)) BREAK_FAILED

            for (int i = 0; i < library_size; i++)
                print_log("Library unit: %d - %s, code %ld",
                    library[i].id,
                    library[i].name.c_str(),
                    library[i].code);

            BYTE ok_answ[2] = { 200, 0 };
            std::string str_libsize = std::to_string(library_size);
            if (!WriteString((char*)ok_answ)) BREAK_FAILED
            if (!WriteString((char*)str_libsize.c_str())) BREAK_FAILED

            for (int i = 0; i < library_size; i++)
            {
                std::string unit_code = std::to_string(library[i].id);
                if (!WriteString((char*)unit_code.c_str())) BREAK_FAILED
                if (!WriteString((char*)library[i].name.c_str())) BREAK_FAILED
            }

            continue;
        }
        if ((BYTE)str[0] == 110)
        {
            print_log("User want to download soft:");
            char id_software[10], install_path[1024];
            if (!ReadString(id_software)) BREAK_FAILED
            if (!ReadString(install_path)) BREAK_FAILED

            UINT64 lib_code = 0;
            UINT64 id = atoll(id_software);
            for (int i = 0; i < library_size; i++)
            {
                if (library[i].id == id)
                {
                    lib_code = library[i].code;
                    break;
                }
            }
            if (!lib_code) BREAK_FAILED

            BYTE current_key[16];
            char sys_info[3072] = "";
            snprintf(sys_info, 3072, "%s %s %s", CPU, Motherboard, GPU);
            hr = DataWallEngine::GenerateKey(sys_info, lib_code, current_key);
            if (FAILED(hr)) BREAK_FAILED

            print_log("%s at path %s", id_software, install_path);
            hr = DataWallEngine::InstallSoftware(id_software, install_path, current_key);
            if (FAILED(hr)) BREAK_FAILED
            BYTE ok_answ[2] = { 200, 0 };
            WriteString((char*)ok_answ);
        }

        if ((BYTE)str[0] == 130)
        {
            print_log("User want to check soft hash:");
            char id_software[10], install_path[1024];
            if (!ReadString(id_software)) BREAK_FAILED
            if (!ReadString(install_path)) BREAK_FAILED
            print_log("code %s at path %s", id_software, install_path);

            BYTE answ[2] = { 200, 0 };
            hr = DataWallEngine::CheckSoftHASH(id_software, install_path);
            if (FAILED(hr)) BREAK_FAILED
            if (hr == S_FALSE)
            {
                print_log("Check not passed");
                answ[0] = 255;
            }
            print_log("Check passed");
            
            if (!WriteString((char*)answ)) BREAK_FAILED
        }

        if ((BYTE)str[0] == 150)
        {
            print_log("User want to start soft:");
            char id_software[10];
            if (!ReadString(id_software)) BREAK_FAILED

            print_log("code %s", id_software);
            UINT64 lib_code = 0;
            std::string lib_name = "";
            UINT64 id = atoll(id_software);
            for (int i = 0; i < library_size; i++)
            {
                if (library[i].id == id)
                {
                    lib_code = library[i].code;
                    lib_name = library[i].name;
                    break;
                }
            }
            if (!lib_code) BREAK_FAILED

            BYTE current_key[16];
            char sys_info[3072] = "";
            snprintf(sys_info, 3072, "%s %s %s", CPU, Motherboard, GPU);
            hr = DataWallEngine::GenerateKey(sys_info, lib_code, current_key);
            if (FAILED(hr)) BREAK_FAILED

            hr = DataWallLoader::SendKey(lib_name.c_str(), current_key);
            if (FAILED(hr))
            {
                print_log("Error when send key");
                BREAK_FAILED
            }

            BYTE ok_answ[2] = { 200, 0 };
            WriteString((char*)ok_answ);
        }

        std::this_thread::sleep_for(std::chrono::milliseconds(1));
    }

    CloseHandle(hNamedPipe);
    DataWallEngine::UninitializeEngine();
    delete[] mInfo;
    delete[] pInfo;
    delete[] vInfo;
    print_log("ServiceWorkerThread stopped");
    if (bWasFail)
    {
        BYTE err_answ[2] = { 255, 0 };
        WriteString((char*)err_answ);
    }
    return ERROR_SUCCESS;
}

int main()
{
    print_log("Start");

    SERVICE_TABLE_ENTRY ServiceTable[] =
    {
        {SERVICE_NAME, (LPSERVICE_MAIN_FUNCTION)ServiceMain},
        {NULL, NULL}
    };

    if (StartServiceCtrlDispatcher(ServiceTable) == FALSE)
    {
        print_log("Error when StartServiceCtrlDispatcher");
        return GetLastError();
    }

    print_log("End");

    return 0;
}