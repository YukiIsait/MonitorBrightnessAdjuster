namespace MonitorBrightnessAdjuster.Adjusters {
    public class AdjusterSingletons {
        private static readonly IAdjuster wmiAdjuster = new WmiAdjuster();
        private static readonly IAdjuster ddcAdjuster = new DdcAdjuster();

        public static IAdjuster WmiAdjusterInstance {
            get => wmiAdjuster;
        }

        public static IAdjuster DdcAdjusterInstance {
            get => ddcAdjuster;
        }
    }
}
