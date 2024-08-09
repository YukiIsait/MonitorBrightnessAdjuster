#include "Framework.h"
#include "LibDisplayDataChannel.h"

typedef struct DdcDataStruct {
    DWORD monitorCount;
    DWORD availableMonitorCount;
    LPHANDLE availablePhysicalMonitorHandles;
    LPPHYSICAL_MONITOR physicalMonitors;
} DdcData, *PDdcData;

typedef struct MonitorEnumProcParamStruct {
    BOOL countOnly;
    PDdcData ddcData;
} MonitorEnumProcParam, *PMonitorEnumProcParam;

static BOOL CALLBACK MonitorEnumProc(HMONITOR monitor, HDC dc, LPRECT rect, LPARAM data) {
    PMonitorEnumProcParam param = (PMonitorEnumProcParam) data;
    PDdcData ddcData = param->ddcData;
    // 当前监视器数量
    DWORD monitorCount = 0;
    // 统计每个监视器句柄对应的多个物理监视器数量
    if (!GetNumberOfPhysicalMonitorsFromHMONITOR(monitor, &monitorCount)) {
        return FALSE;
    }
    ddcData->monitorCount += monitorCount;
    // 仅计数则直接返回
    if (param->countOnly) {
        return TRUE;
    }
    // 当前监视器数组位置
    LPPHYSICAL_MONITOR currentPhysicalMonitors = &ddcData->physicalMonitors[ddcData->monitorCount - monitorCount];
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
            ddcData->availablePhysicalMonitorHandles[ddcData->availableMonitorCount++] = currentPhysicalMonitors[i].hPhysicalMonitor;
        }
    }
    return TRUE;
}

void* DdcInitialize() {
    HANDLE processHeap = GetProcessHeap();
    // 初始化变量
    PDdcData ddcData = (PDdcData) HeapAlloc(processHeap, HEAP_ZERO_MEMORY, sizeof(DdcData));
    if (!ddcData) {
        DdcDestroy(ddcData);
        return NULL;
    }
    // 首次遍历获取监视器数量
    MonitorEnumProcParam param = {
        .countOnly = TRUE,
        .ddcData = ddcData
    };
    if (!EnumDisplayMonitors(NULL, NULL, MonitorEnumProc, (LPARAM) &param)) {
        DdcDestroy(ddcData);
        return NULL;
    }
    // 初始化监视器数组和可用标记数组
    ddcData->physicalMonitors = (LPPHYSICAL_MONITOR) HeapAlloc(processHeap, 0, ddcData->monitorCount * sizeof(PHYSICAL_MONITOR));
    ddcData->availablePhysicalMonitorHandles = (LPHANDLE) HeapAlloc(processHeap, 0, ddcData->monitorCount * sizeof(HANDLE));
    ddcData->monitorCount = 0;
    ddcData->availableMonitorCount = 0;
    if (!ddcData->physicalMonitors || !ddcData->availablePhysicalMonitorHandles) {
        DdcDestroy(ddcData);
        return NULL;
    }
    // 获取所有监视器以及是否可用的状态
    param.countOnly = FALSE;
    if (!EnumDisplayMonitors(NULL, NULL, MonitorEnumProc, (LPARAM) &param)) {
        DdcDestroy(ddcData);
        return NULL;
    }
    return ddcData;
}

void DdcDestroy(void* handle) {
    if (!handle) {
        return;
    }
    HANDLE processHeap = GetProcessHeap();
    PDdcData ddcData = (PDdcData) handle;
    DestroyPhysicalMonitors(ddcData->monitorCount, ddcData->physicalMonitors);
    HeapFree(processHeap, 0, ddcData->physicalMonitors);
    HeapFree(processHeap, 0, ddcData->availablePhysicalMonitorHandles);
    HeapFree(processHeap, 0, ddcData);
}

bool DdcGetAvailableCount(void* handle, uint32_t* count) {
    if (!handle || !count) {
        return false;
    }
    PDdcData ddcData = (PDdcData) handle;
    *count = ddcData->availableMonitorCount;
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
    PDdcData ddcData = (PDdcData) handle;
    if (monitorIndex >= ddcData->availableMonitorCount) {
        return false;
    }
    HANDLE physicalMonitorHandle = ddcData->availablePhysicalMonitorHandles[monitorIndex];
    return GetMonitorBrightness(physicalMonitorHandle, minimumBrightness, currentBrightness, maximumBrightness);
}

bool DdcSetBrightness(void* handle, uint32_t monitorIndex, uint32_t brightness) {
    if (!handle) {
        return false;
    }
    PDdcData ddcData = (PDdcData) handle;
    if (monitorIndex >= ddcData->availableMonitorCount) {
        return false;
    }
    HANDLE physicalMonitorHandle = ddcData->availablePhysicalMonitorHandles[monitorIndex];
    return SetMonitorBrightness(physicalMonitorHandle, brightness);
}
