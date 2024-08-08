using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MonitorBrightnessAdjuster.Adjusters;

using System.Collections.ObjectModel;
using System.Windows;

namespace MonitorBrightnessAdjuster {
    public partial class MainWindowViewModel: ObservableObject {
        [ObservableProperty]
        private bool enabledWmiChannel = true;

        [ObservableProperty]
        private bool enabledDdcChannel = true;

        [ObservableProperty]
        private bool showNoSupportedMonitorsFound = false;

        public ObservableCollection<MonitorBrightnessModel> MonitorBrightnessModels { get; set; }

        public MainWindowViewModel() {
            MonitorBrightnessModels = new ObservableCollection<MonitorBrightnessModel>();
            Refresh();
        }

        [RelayCommand]
        private void Refresh() {
            MonitorBrightnessModels.Clear();
            if (EnabledWmiChannel) {
                try {
                    for (int i = 0; i < AdjusterSingletons.WmiAdjusterInstance.GetNumberOfMonitors(); i++) {
                        MonitorBrightnessModels.Add(new MonitorBrightnessModel() {
                            Channel = MonitorBrightnessChannel.WMI,
                            Index = i,
                            Brightness = AdjusterSingletons.WmiAdjusterInstance.GetBrightnessPercentage(i)
                        });
                    }
                } catch { }
            }
            if (EnabledDdcChannel) {
                try {
                    for (int i = 0; i < AdjusterSingletons.DdcAdjusterInstance.GetNumberOfMonitors(); i++) {
                        MonitorBrightnessModels.Add(new MonitorBrightnessModel() {
                            Channel = MonitorBrightnessChannel.DDC,
                            Index = i,
                            Brightness = AdjusterSingletons.DdcAdjusterInstance.GetBrightnessPercentage(i)
                        });
                    }
                } catch { }
            }
            ShowNoSupportedMonitorsFound = MonitorBrightnessModels.Count == 0;
        }

        [RelayCommand]
        private void About() {
            MessageBox.Show(AboutUtil.GetAboutInformation(), "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        partial void OnEnabledWmiChannelChanged(bool _) {
            Refresh();
        }

        partial void OnEnabledDdcChannelChanged(bool _) {
            Refresh();
        }
    }
}
