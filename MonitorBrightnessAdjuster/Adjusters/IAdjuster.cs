namespace MonitorBrightnessAdjuster.Adjusters {
    public interface IAdjuster {
        public int GetNumberOfMonitors();
        public int GetBrightnessPercentage(int monitorIndex);
        public void SetBrightnessPercentage(int monitorIndex, int brightness);
    }
}
