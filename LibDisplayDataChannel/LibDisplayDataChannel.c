#include "Framework.h"
#include "LibDisplayDataChannel.h"

typedef enum MonitorEnumProcPurposeEnum {
    MonitorEnumProcCount,
    MonitorEnumProcGet,
    MonitorEnumProcSet
} MonitorEnumProcPurpose, * PMonitorEnumProcPurpose;

typedef struct MonitorEnumProcParamStruct {
    MonitorEnumProcPurpose purpose;
    DWORD monitorIndex;
    DWORD currentBrightness;
    DWORD minimumBrightness;
    DWORD maximumBrightness;
    DWORD physicalMonitorCount;
    BOOL result;
} MonitorEnumProcParam, * PMonitorEnumProcParam;

static BOOL CALLBACK MonitorEnumProc(HMONITOR monitor, HDC dc, LPRECT rect, LPARAM data) {
    MonitorEnumProcParam* param = (MonitorEnumProcParam*) data;
    BOOL success;

    // 统计每个监视器句柄对应的多个物理监视器数量
    DWORD monitorCount;
    if (!GetNumberOfPhysicalMonitorsFromHMONITOR(monitor, &monitorCount)) {
        return FALSE;
    }

    // 获取对应的物理监视器句柄
    HANDLE processHeap = GetProcessHeap();
    LPPHYSICAL_MONITOR physicalMonitors = HeapAlloc(processHeap, 0, monitorCount * sizeof(PHYSICAL_MONITOR));
    if (!physicalMonitors) {
        return FALSE;
    }
    success = GetPhysicalMonitorsFromHMONITOR(monitor, monitorCount, physicalMonitors);
    if (!success) {
        goto free;
    }

    // 计数: 统计支持调节亮度的监视器数量
    //for (DWORD i = 0; i < monitorCount; i++) {
    //    DWORD monitorCapabilities;
    //    DWORD supportedColorTemperatures;
    //    if (GetMonitorCapabilities(physicalMonitors[i].hPhysicalMonitor,
    //                               &monitorCapabilities,
    //                               &supportedColorTemperatures) &&
    //        monitorCapabilities & MC_CAPS_BRIGHTNESS) {
    //        param->physicalMonitorCount++;
    //    }
    //}

    // 计数: 统计支持调节亮度的监视器数量
    for (DWORD i = 0; i < monitorCount; i++) {
        DWORD currentBrightness;
        DWORD minimumBrightness;
        DWORD maximumBrightness;
        if (GetMonitorBrightness(physicalMonitors[i].hPhysicalMonitor,
                                 &minimumBrightness,
                                 &currentBrightness,
                                 &maximumBrightness)) {
            param->physicalMonitorCount++;
        }
    }

    // 计数: 不统计直接计数
    //param->physicalMonitorCount += monitorCount;

    // 目的是计数则到此结束
    if (param->purpose == MonitorEnumProcCount) {
        param->result = success; // 当前循环计数完成
        goto closeAndFree;
    }

    // 目的是获取或设置
    if (param->purpose != MonitorEnumProcGet && param->purpose != MonitorEnumProcSet) {
        goto closeAndFree;
    }
    if (param->physicalMonitorCount > param->monitorIndex) {
        // 根据铺平后的索引获取对应的物理监视器句柄
        HANDLE physicalMonitor = physicalMonitors[param->monitorIndex - param->physicalMonitorCount + monitorCount].hPhysicalMonitor;
        if (param->purpose == MonitorEnumProcGet) {
            success = GetMonitorBrightness(physicalMonitor,
                                           &param->minimumBrightness,
                                           &param->currentBrightness,
                                           &param->maximumBrightness);
        } else {
            success = SetMonitorBrightness(physicalMonitor,
                                           param->currentBrightness);
        }
        param->result = success; // 设置完成标志
        success = FALSE; // 设置后结束循环
    }

    // 关闭物理监视器句柄
closeAndFree:
    DestroyPhysicalMonitors(monitorCount, physicalMonitors);
    // 释放堆内存
free:
    HeapFree(processHeap, 0, physicalMonitors);
    return success;
}

bool GetNumberOfPhysicalMonitors(uint32_t* numberOfPhysicalMonitors) {
    MonitorEnumProcParam param = {
        .purpose = MonitorEnumProcCount
    };
    if (!EnumDisplayMonitors(NULL, NULL, MonitorEnumProc, (LPARAM) &param) && !param.result) {
        return false;
    }
    *numberOfPhysicalMonitors = param.physicalMonitorCount;
    return true;
}

bool GetPhysicalMonitorBrightness(uint32_t monitorIndex,
                                  uint32_t* currentBrightness,
                                  uint32_t* minimumBrightness,
                                  uint32_t* maximumBrightness) {
    MonitorEnumProcParam param = {
        .purpose = MonitorEnumProcGet,
        .monitorIndex = monitorIndex
    };
    if (!EnumDisplayMonitors(NULL, NULL, MonitorEnumProc, (LPARAM) &param) && !param.result) {
        return false;
    }
    if (param.currentBrightness == 0 &&
        param.minimumBrightness == 0 &&
        param.maximumBrightness == 0) {
        return false;
    }
    *currentBrightness = param.currentBrightness;
    *minimumBrightness = param.minimumBrightness;
    *maximumBrightness = param.maximumBrightness;
    return true;
}

bool SetPhysicalMonitorBrightness(uint32_t monitorIndex, uint32_t brightness) {
    MonitorEnumProcParam param = {
        .purpose = MonitorEnumProcSet,
        .monitorIndex = monitorIndex,
        .currentBrightness = brightness
    };
    if (!EnumDisplayMonitors(NULL, NULL, MonitorEnumProc, (LPARAM) &param) && !param.result) {
        return false;
    }
    return true;
}
