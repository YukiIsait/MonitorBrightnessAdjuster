#include <stdio.h>
#include "../LibDisplayDataChannel/LibDisplayDataChannel.h"

int main() {
    void* handle = DdcInitialize();

    uint32_t count;
    if (!DdcGetAvailableCount(handle, &count)) {
        puts("ERROR");
        return -1;
    }

    uint32_t currentBrightness = 0;
    uint32_t minimumBrightness = 0;
    uint32_t maximumBrightness = 0;
    printf_s("COUNT: %d\n\nGET\n", count);
    for (uint32_t i = 0; i < (uint32_t) count; i++) {
        if (DdcGetBrightness(handle, i, &currentBrightness, &minimumBrightness, &maximumBrightness)) {
            printf_s("NUM: %u, CUR: %u, MIN: %u, MAX: %u\n", i, currentBrightness, minimumBrightness, maximumBrightness);
        } else {
            printf_s("NUM: %u, N/A\n", i);
        }
    }

    printf_s("\nSET\nNUM: ");
    uint32_t num;
    scanf_s("%u", &num);
    uint32_t percent;
    printf_s("PER: ");
    scanf_s("%u", &percent);
    if (DdcSetBrightness(handle, num, (uint32_t) ((float) (maximumBrightness - minimumBrightness) * percent / 100 + minimumBrightness))) {
        puts("\nOK");
    }

    DdcDestroy(handle);
    return 0;
}
