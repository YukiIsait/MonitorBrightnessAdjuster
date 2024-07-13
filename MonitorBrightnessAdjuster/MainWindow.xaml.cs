using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MonitorBrightnessAdjuster {
    public partial class MainWindow: Window {
        public MainWindow() {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private void Slider_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (sender is Slider slider) {
                slider.Value += e.Delta / 120;
            }
        }
    }
}
