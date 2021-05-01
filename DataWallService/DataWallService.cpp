#define _CRT_SECURE_NO_WARNINGS
#define SERVICE_NAME  (LPSTR)("DataWallService")   

#define DEBUG

#include <iostream>
#include <Windows.h>
#include <thread>
#include <tchar.h>

#ifdef DEBUG
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
void print_log(const char* _Format, ...) {}
#endif

SERVICE_STATUS        g_ServiceStatus = { 0 };
SERVICE_STATUS_HANDLE g_StatusHandle = NULL;
HANDLE                hThread = INVALID_HANDLE_VALUE;
HANDLE                g_ServiceStopEvent = INVALID_HANDLE_VALUE;
HANDLE                hNamedPipe = INVALID_HANDLE_VALUE;

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
        print_log("Data Wall Service: ServiceMain: SetServiceStatus returned error");
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
            print_log("Data Wall Service: ServiceMain: SetServiceStatus returned error");
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
        print_log("Data Wall Service: ServiceMain: SetServiceStatus returned error");
    }

    // Start a thread that will perform the main task of the service
    hThread = CreateThread(NULL, 0, ServiceWorkerThread, NULL, 0, NULL);

    if (hThread)
        WaitForSingleObject(hThread, INFINITE);
    else
        return;

    CloseHandle(g_ServiceStopEvent);
    //Service unstoppable
    //goto START;

    return;
}

VOID WINAPI ServiceCtrlHandler(DWORD CtrlCode)
{
    switch (CtrlCode)
    {
    case SERVICE_CONTROL_STOP:

        if (g_ServiceStatus.dwCurrentState != SERVICE_RUNNING)
            break;

        SetEvent(g_ServiceStopEvent);

        if (hThread)
            WaitForSingleObject(hThread, INFINITE);
        /*
         * Perform tasks necessary to stop the service here
         */

        g_ServiceStatus.dwControlsAccepted = 0;
        g_ServiceStatus.dwCurrentState = SERVICE_STOPPED;
        g_ServiceStatus.dwWin32ExitCode = 0;
        g_ServiceStatus.dwCheckPoint = 4;

        if (SetServiceStatus(g_StatusHandle, &g_ServiceStatus) == FALSE)
        {
            print_log("Data Wall Service: ServiceCtrlHandler: SetServiceStatus returned error");
        }

        // This will signal the worker thread to start shutting down
        print_log("SERVICE_CONTROL_STOP");

        break;

    default:
        break;
    }
}

DWORD WINAPI ServiceWorkerThread(LPVOID lpParam)
{
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
        print_log("Data Wall Service: Start work: Failed to create named pipe");
        return ERROR_PATH_NOT_FOUND;
    }

    print_log("Data Wall Service: Start work: Created named pipe");

    //  Periodically check if the service has been requested to stop
    while (WaitForSingleObject(g_ServiceStopEvent, 0) != WAIT_OBJECT_0)
    {
        std::this_thread::sleep_for(std::chrono::seconds(1));
    }

    print_log("ServiceWorkerThread stopped");
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
        print_log("StartServiceCtrlDispatcher(ServiceTable) == FALSE");
        return GetLastError();
    }

    print_log("End");

    return 0;
}