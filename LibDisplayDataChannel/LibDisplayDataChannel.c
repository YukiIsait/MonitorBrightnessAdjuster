#include "Framework.h"
#include "LibDisplayDataChannel.h"

typedef struct MonitorEnumProcParamStruct {
    BOOL countOnly;
    DWORD monitorCount;
    DWORD availableMonitorCount;
    LPHANDLE availablePhysicalMonitorHandles;
    LPPHYSICAL_MONITOR physicalMonitors;
} MonitorEnumProcParam, *PMonitorEnumProcParam;

static BOOL CALLBACK MonitorEnumProc(HMONITOR monitor, HDC dc, LPRECT rect, LPARAM data) {
    MonitorEnumProcParam* param = (MonitorEnumProcParam*) data;
    // 当前监视器数量
    DWORD monitorCount = 0;
    // 统计每个监视器句柄对应的多个物理监视器数量
    if (!GetNumberOfPhysicalMonitorsFromHMONITOR(monitor, &monitorCount)) {
        return FALSE;
    }
    param->monitorCount += monitorCount;
    // 仅计数则直接返回
    if (param->countOnly) {
        return TRUE;
    }
    // 当前监视器数组位置
    LPPHYSICAL_MONITOR currentPhysicalMonitors = &param->physicalMonitors[param->monitorCount - monitorCount];
    // 获取对应的物理监视器句柄
    if (!GetPhysicalMonitorsFromHMONITOR(monitor, monitorCount, currentPhysicalMonitors)) {
        return FALSE;
    }
    // 统计支持亮度调节的监视器
    for (DWORD i = 0; i < monitorCount; i++) {
        DWORD currentBrightness;
        DWORD minimumBrightness;
        DWORD maximumBrightness;
        if (GetMonitorBrightness(currentPhysicalMonitors[i].hPhysicalMonitor,
                                 &minimumBrightness,
                                 &currentBrightness,
                                 &maximumBrightness)) {
            // 记录支持亮度调节的监视器句柄
            param->availablePhysicalMonitorHandles[param->availableMonitorCount++] = currentPhysicalMonitors[i].hPhysicalMonitor;
        }
    }
    return TRUE;
}

void* DdcInitialize() {
    HANDLE processHeap = GetProcessHeap();
    // 初始化变量
    PMonitorEnumProcParam param = (PMonitorEnumProcParam) HeapAlloc(processHeap, HEAP_ZERO_MEMORY, sizeof(MonitorEnumProcParam));
    if (!param) {
        DdcDestroy(param);
        return NULL;
    }
    // 首次遍历获取监视器数量
    param->countOnly = TRUE;
    if (!EnumDisplayMonitors(NULL, NULL, MonitorEnumProc, (LPARAM) param)) {
        DdcDestroy(param);
        return NULL;
    }
    // 初始化监视器数组和可用标记数组
    param->physicalMonitors = (LPPHYSICAL_MONITOR) HeapAlloc(processHeap, 0, param->monitorCount * sizeof(PHYSICAL_MONITOR));
    param->availablePhysicalMonitorHandles = (LPHANDLE) HeapAlloc(processHeap, 0, param->monitorCount * sizeof(HANDLE));
    param->countOnly = FALSE;
    param->monitorCount = 0;
    param->availableMonitorCount = 0;
    if (!param->physicalMonitors || !param->availablePhysicalMonitorHandles) {
        DdcDestroy(param);
        return NULL;
    }
    // 获取所有监视器以及是否可用的状态
    if (!EnumDisplayMonitors(NULL, NULL, MonitorEnumProc, (LPARAM) param)) {
        DdcDestroy(param);
        return NULL;
    }
    return param;
}

void DdcDestroy(void* handle) {
    if (!handle) {
        return;
    }
    HANDLE processHeap = GetProcessHeap();
    PMonitorEnumProcParam param = (PMonitorEnumProcParam) handle;
    DestroyPhysicalMonitors(param->monitorCount, param->physicalMonitors);
    HeapFree(processHeap, 0, param->physicalMonitors);
    HeapFree(processHeap, 0, param->availablePhysicalMonitorHandles);
    HeapFree(processHeap, 0, param);
}

bool DdcGetAvailableCount(void* handle, uint32_t* count) {
    if (!handle || !count) {
        return false;
    }
    PMonitorEnumProcParam param = (PMonitorEnumProcParam) handle;
    *count = param->availableMonitorCount;
    return true;
}

bool DdcGetBrightness(void* handle,
                      uint32_t monitorIndex,
                      uint32_t* currentBrightness,
                      uint32_t* minimumBrightness,
                      uint32_t* maximumBrightness) {
    if (!handle || !currentBrightness || !minimumBrightness || !maximumBrightness) {
        return false;
    }
    PMonitorEnumProcParam param = (PMonitorEnumProcParam) handle;
    if (monitorIndex >= param->availableMonitorCount) {
        return false;
    }
    HANDLE physicalMonitorHandle = param->availablePhysicalMonitorHandles[monitorIndex];
    return GetMonitorBrightness(physicalMonitorHandle, minimumBrightness, currentBrightness, maximumBrightness);
}

bool DdcSetBrightness(void* handle, uint32_t monitorIndex, uint32_t brightness) {
    if (!handle) {
        return false;
    }
    PMonitorEnumProcParam param = (PMonitorEnumProcParam) handle;
    if (monitorIndex >= param->availableMonitorCount) {
        return false;
    }
    HANDLE physicalMonitorHandle = param->availablePhysicalMonitorHandles[monitorIndex];
    return SetMonitorBrightness(physicalMonitorHandle, brightness);
}
