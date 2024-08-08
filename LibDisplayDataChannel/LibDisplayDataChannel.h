#ifdef LIBDISPLAYDATACHANNEL_EXPORTS
#define LIBDISPLAYDATACHANNEL_API __declspec(dllexport)
#else
#define LIBDISPLAYDATACHANNEL_API __declspec(dllimport)
#endif

#ifdef __cplusplus
#include <cstdint>
#include <cstdbool>
extern "C" {
#else
#include <stdint.h>
#include <stdbool.h>
#endif

LIBDISPLAYDATACHANNEL_API void* DdcInitialize();
LIBDISPLAYDATACHANNEL_API void DdcDestroy(void* handle);

LIBDISPLAYDATACHANNEL_API bool DdcGetAvailableCount(void* handle,
                                                     uint32_t* count);
LIBDISPLAYDATACHANNEL_API bool DdcGetBrightness(void* handle,
                                                uint32_t monitorIndex,
                                                uint32_t* currentBrightness,
                                                uint32_t* minimumBrightness,
                                                uint32_t* maximumBrightness);
LIBDISPLAYDATACHANNEL_API bool DdcSetBrightness(void* handle,
                                                uint32_t monitorIndex,
                                                uint32_t brightness);

#ifdef __cplusplus
}
#endif
