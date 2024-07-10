using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MonitorBrightnessAdjuster.Adjusters;

using System.Collections.ObjectModel;

namespace MonitorBrightnessAdjuster {
    public partial class MainWindowViewModel: ObservableObject {
        [ObservableProperty]
        private bool enabledWmiChannel = true;

        [ObservableProperty]
        private bool enabledDdcChannel = true;

        public ObservableCollection<MonitorBrightnessModel> MonitorBrightnessModels { get; set; }

        public MainWindowViewModel() {
            MonitorBrightnessModels = new ObservableCollection<MonitorBrightnessModel>();
            Refresh();
        }

        [RelayCommand]
        private void Refresh() {
            MonitorBrightnessModels.Clear();
            if (EnabledWmiChannel) {
                for (int i = 0; i < AdjusterSingletons.WmiAdjusterInstance.GetNumberOfMonitors(); i++) {
                    MonitorBrightnessModels.Add(new MonitorBrightnessModel() {
                        Channel = MonitorBrightnessChannel.WMI,
                        Index = i,
                        Brightness = AdjusterSingletons.WmiAdjusterInstance.GetBrightnessPercentage(i)
                    });
                }
            }
            if (EnabledDdcChannel) {
                for (int i = 0; i < AdjusterSingletons.DdcAdjusterInstance.GetNumberOfMonitors(); i++) {
                    MonitorBrightnessModels.Add(new MonitorBrightnessModel() {
                        Channel = MonitorBrightnessChannel.DDC,
                        Index = i,
                        Brightness = AdjusterSingletons.DdcAdjusterInstance.GetBrightnessPercentage(i)
                    });
                }
            }
        }

        partial void OnEnabledWmiChannelChanged(bool _) {
            Refresh();
        }

        partial void OnEnabledDdcChannelChanged(bool _) {
            Refresh();
        }
    }
}
