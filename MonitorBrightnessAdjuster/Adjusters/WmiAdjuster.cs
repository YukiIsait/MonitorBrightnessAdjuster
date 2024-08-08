using System.Management;

namespace MonitorBrightnessAdjuster.Adjusters {
    public sealed class WmiAdjuster: IAdjuster, IDisposable {
        private readonly ManagementClass monitorBrightnessClass;
        private readonly ManagementClass monitorBrightnessMethodsClass;

        public WmiAdjuster() {
            monitorBrightnessClass = new("WmiMonitorBrightness") {
                Scope = new(@"\\.\root\wmi")
            };
            monitorBrightnessMethodsClass = new("WmiMonitorBrightnessMethods") {
                Scope = new(@"\\.\root\wmi")
            };
        }

        public void Dispose() {
            monitorBrightnessClass.Dispose();
            monitorBrightnessMethodsClass.Dispose();
        }

        private IEnumerable<KeyValuePair<ManagementObject, ManagementObject>> GetActiveMonitorBrightnessInstances() {
            IEnumerable<ManagementObject> monitorBrightnessInstances = monitorBrightnessClass
                .GetInstances()
                .Cast<ManagementObject>();
            IEnumerable<ManagementObject> monitorBrightnessMethodsInstances = monitorBrightnessMethodsClass
                .GetInstances()
                .Cast<ManagementObject>();
            return monitorBrightnessInstances
                .Zip(monitorBrightnessMethodsInstances, (instance, methodInstance) =>
                    new KeyValuePair<ManagementObject, ManagementObject>(instance, methodInstance))
                .Where(current => current.Key.GetPropertyValue("Active") as bool? == true);
        }

        public int GetNumberOfMonitors() {
            return GetActiveMonitorBrightnessInstances().Count();
        }

        public int GetBrightnessPercentage(int monitorIndex) {
            IEnumerable<KeyValuePair<ManagementObject, ManagementObject>> monitorBrightnessInstances = GetActiveMonitorBrightnessInstances();
            if (monitorIndex >= monitorBrightnessInstances.Count() || monitorIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(monitorIndex));
            }
            ManagementObject currentInstance = monitorBrightnessInstances
                .Skip(monitorIndex)
                .First()
                .Key;
            // 获取所有的可用亮度级别和当前的亮度
            byte[]? brightnessLevel = currentInstance.GetPropertyValue("Level") as byte[];
            byte? currentBrightness = currentInstance.GetPropertyValue("CurrentBrightness") as byte?;
            if (brightnessLevel == null || brightnessLevel.Length <= 0) {
                throw new ArgumentOutOfRangeException(nameof(monitorIndex));
            }
            // 查找当前亮度级别在所有级别的的位置并计算百分比
            int index = Array.IndexOf(brightnessLevel, currentBrightness);
            if (index < 0) {
                throw new ArgumentOutOfRangeException(nameof(monitorIndex));
            }
            return (int) ((float) (index + 1) / brightnessLevel.Length * 100);
        }

        public void SetBrightnessPercentage(int monitorIndex, int brightness) {
            IEnumerable<KeyValuePair<ManagementObject, ManagementObject>> monitorBrightnessInstances = GetActiveMonitorBrightnessInstances();
            if (monitorIndex >= monitorBrightnessInstances.Count() || monitorIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(monitorIndex));
            }
            if (brightness < 0 || brightness > 100) {
                throw new ArgumentOutOfRangeException(nameof(brightness));
            }
            KeyValuePair<ManagementObject, ManagementObject> currentInstance = monitorBrightnessInstances
                .Skip(monitorIndex)
                .First();
            // 获取所有的可用亮度级别
            byte[]? brightnessLevel = currentInstance.Key.GetPropertyValue("Level") as byte[] ??
                throw new ArgumentOutOfRangeException(nameof(monitorIndex));
            // 查找距离给定亮度百分比最近的亮度级别
            byte newBrightness = brightnessLevel
                .Select((value, index) =>
                    new KeyValuePair<int, byte>(
                        Math.Abs((int) ((float) (index + 1) / brightnessLevel.Length * 100) - brightness),
                        value
                    )
                ) // 计算百分比及距离
                .OrderBy(pair => pair.Key) // 最短距离
                .First()
                .Value;
            currentInstance.Value.InvokeMethod("WmiSetBrightness", new object[] { 1, newBrightness });
        }
    }
}
