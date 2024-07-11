using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace MonitorBrightnessAdjuster {
    public class TextBoxAttachedProperties {
        public static bool GetIsPercentage(DependencyObject obj) {
            return (bool) obj.GetValue(MatchingPatternProperty);
        }

        public static void SetIsPercentage(DependencyObject obj, bool value) {
            obj.SetValue(MatchingPatternProperty, value);
        }

        public static readonly DependencyProperty MatchingPatternProperty =
            DependencyProperty.RegisterAttached("IsPercentage", typeof(bool), typeof(TextBox), new PropertyMetadata(false,
                (s, e) => {
                    if (s is TextBox textBox) {
                        textBox.SetValue(InputMethod.IsInputMethodEnabledProperty, !(bool) e.NewValue);
                        textBox.PreviewTextInput -= TextInput;
                        if ((bool) e.NewValue) {
                            textBox.PreviewTextInput += TextInput;
                        }
                    }
                }));

        private static void TextInput(object sender, TextCompositionEventArgs e) {
            if (sender is TextBox textBox) {
                e.Handled = !(int.TryParse(textBox.Text + e.Text, out int number) && number >= 0 && number <= 100);
            }
        }
    }
}
