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
                        textBox.AllowDrop = !(bool) e.NewValue;
                        textBox.CommandBindings.Clear();
                        textBox.PreviewTextInput -= TextInput;
                        if ((bool) e.NewValue) {
                            textBox.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, null, CommandCanExecute));
                            textBox.PreviewTextInput += TextInput;
                        }
                    }
                }));

        private static void CommandCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = false;
            e.Handled = true;
        }

        private static void TextInput(object sender, TextCompositionEventArgs e) {
            if (sender is TextBox textBox) {
                e.Handled = !(int.TryParse(textBox.Text.Substring(0, textBox.SelectionStart) +
                                           e.Text +
                                           textBox.Text.Substring(textBox.SelectionStart + textBox.SelectionLength),
                                           out int number) && number >= 0 && number <= 100);
            }
        }
    }
}
