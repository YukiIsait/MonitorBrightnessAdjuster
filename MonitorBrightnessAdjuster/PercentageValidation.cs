using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace MonitorBrightnessAdjuster;

public class PercentageValidation : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (int.TryParse(value as string, out var num) && num is >= 0 and <= 100)
        {
            return ValidationResult.ValidResult;
        }
        return new ValidationResult(false, "Invalid percentage value");
    }
}