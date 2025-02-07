using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MonitorBrightnessAdjuster.Adjusters {
    public sealed class DdcAdjuster: IAdjuster, IDisposable {
        private UIntPtr handle;

        public DdcAdjuster() {
            handle = DdcInitialize();
            if (handle == UIntPtr.Zero) {
                throw new Win32Exception();
            }
        }

        ~DdcAdjuster() {
            Destroy();
        }

        public void Dispose() {
            Destroy();
            GC.SuppressFinalize(this);
        }

        private void Destroy() {
            if (handle != UIntPtr.Zero) {
                DdcDestroy(handle);
                handle = UIntPtr.Zero;
            }
        }

        public int GetNumberOfMonitors() {
            if (!DdcGetAvailableCount(handle, out uint count)) {
                throw new Win32Exception();
            }
            return (int) count;
        }

        public int GetBrightnessPercentage(int monitorIndex) {
            if (monitorIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(monitorIndex));
            }
            if (!DdcGetBrightness(handle, (uint) monitorIndex, out uint currentBrightness, out uint minimumBrightness, out uint maximumBrightness)) {
                throw new Win32Exception();
            }
            // 计算当前亮度在亮度区间的百分比
            return (int) ((float) currentBrightness / (maximumBrightness - minimumBrightness) * 100);
        }

        public void SetBrightnessPercentage(int monitorIndex, int brightness) {
            if (monitorIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(monitorIndex));
            }
            if (brightness < 0 || brightness > 100) {
                throw new ArgumentOutOfRangeException(nameof(brightness));
            }
            if (!DdcGetBrightness(handle, (uint) monitorIndex, out _, out uint minimumBrightness, out uint maximumBrightness)) {
                throw new Win32Exception();
            }
            // 将亮度百分比在亮度区间做映射
            uint newBrightness = (uint) ((float) (maximumBrightness - minimumBrightness) * brightness / 100 + minimumBrightness);
            if (!DdcSetBrightness(handle, (uint) monitorIndex, newBrightness)) {
                throw new Win32Exception();
            }
        }

        [DllImport("LibDisplayDataChannel.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern UIntPtr DdcInitialize();

        [DllImport("LibDisplayDataChannel.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern void DdcDestroy(UIntPtr handle);

        [DllImport("LibDisplayDataChannel.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern bool DdcGetAvailableCount(UIntPtr handle, out uint count);

        [DllImport("LibDisplayDataChannel.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern bool DdcGetBrightness(UIntPtr handle, uint monitorIndex, out uint currentBrightness, out uint minimumBrightness, out uint maximumBrightness);

        [DllImport("LibDisplayDataChannel.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern bool DdcSetBrightness(UIntPtr handle, uint monitorIndex, uint brightness);
    }
}
