using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MonitorBrightnessAdjuster.Adjusters {
    public class DdcAdjuster: IAdjuster {
        [DllImport("LibDisplayDataChannel.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern bool GetNumberOfPhysicalMonitors(out uint numberOfPhysicalMonitors);

        [DllImport("LibDisplayDataChannel.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern bool GetPhysicalMonitorBrightness(uint monitorIndex,
                                                               out uint currentBrightness,
                                                               out uint minimumBrightness,
                                                               out uint maximumBrightness);

        [DllImport("LibDisplayDataChannel.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern bool SetPhysicalMonitorBrightness(uint monitorIndex, uint brightness);

        public int GetNumberOfMonitors() {
            if (!GetNumberOfPhysicalMonitors(out uint numberOfPhysicalMonitors)) {
                throw new Win32Exception();
            }
            return (int) numberOfPhysicalMonitors;
        }

        public int GetBrightnessPercentage(int monitorIndex) {
            if (monitorIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(monitorIndex));
            }
            if (!GetPhysicalMonitorBrightness((uint) monitorIndex, out uint currentBrightness, out uint minimumBrightness, out uint maximumBrightness)) {
                throw new Win32Exception();
            }
            // 计算当前亮度在亮度区间的百分比
            return (int) ((float) currentBrightness / (maximumBrightness - minimumBrightness) * 100);
        }

        public void SetBrightnessPercentage(int monitorIndex, int brightness) {
            if (monitorIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(monitorIndex));
            }
            if (brightness < 0) {
                throw new ArgumentOutOfRangeException(nameof(brightness));
            }
            if (!GetPhysicalMonitorBrightness((uint) monitorIndex, out _, out uint minimumBrightness, out uint maximumBrightness)) {
                throw new Win32Exception();
            }
            // 将亮度百分比在亮度区间做映射
            uint newBrightness = (uint) ((float) (maximumBrightness - minimumBrightness) * brightness / 100 + minimumBrightness);
            if (!SetPhysicalMonitorBrightness((uint) monitorIndex, newBrightness)) {
                throw new Win32Exception();
            }
        }
    }
}
