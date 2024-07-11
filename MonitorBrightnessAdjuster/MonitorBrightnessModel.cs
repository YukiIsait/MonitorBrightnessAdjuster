using CommunityToolkit.Mvvm.ComponentModel;

using MonitorBrightnessAdjuster.Adjusters;

namespace MonitorBrightnessAdjuster {
    public enum MonitorBrightnessChannel {
        WMI,
        DDC
    }

    public partial class MonitorBrightnessModel: ObservableObject {
        [ObservableProperty]
        private MonitorBrightnessChannel channel;

        [ObservableProperty]
        private int index;

        [ObservableProperty]
        private int brightness;

        partial void OnBrightnessChanging(int brightness) {
            switch (Channel) {
                case MonitorBrightnessChannel.WMI:
                    AdjusterSingletons.WmiAdjusterInstance.SetBrightnessPercentage(Index, brightness);
                    break;
                case MonitorBrightnessChannel.DDC:
                    AdjusterSingletons.DdcAdjusterInstance.SetBrightnessPercentage(Index, brightness);
                    break;
                default:
                    throw new ArgumentException(nameof(Channel));
            }
        }
    }
}
