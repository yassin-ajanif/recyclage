using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Recyclage.Converters;

public class NegativeAmountBrushConverter : IValueConverter
{
    private static readonly IBrush NegativeBrush = new SolidColorBrush(Color.Parse("#DC2626"));
    private static readonly IBrush NormalBrush = new SolidColorBrush(Color.Parse("#1F2937"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (IsNegative(value))
            return NegativeBrush;

        return NormalBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static bool IsNegative(object? value)
    {
        return value switch
        {
            decimal d => d < 0,
            double d => d < 0,
            float f => f < 0,
            int i => i < 0,
            long l => l < 0,
            string text => IsNegativeText(text),
            _ => false
        };
    }

    private static bool IsNegativeText(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith('-'))
            return true;

        var digits = new string(trimmed
            .TakeWhile(c => char.IsDigit(c) || c is '.' or ',' or '-')
            .ToArray());

        if (string.IsNullOrWhiteSpace(digits))
            return false;

        return decimal.TryParse(
            digits.Replace(',', '.'),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var amount) && amount < 0;
    }
}
