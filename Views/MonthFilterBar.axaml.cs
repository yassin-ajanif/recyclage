using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace Recyclage.Views;

public partial class MonthFilterBar : UserControl
{
    public static readonly StyledProperty<ICommand?> SelectMonthCommandProperty =
        AvaloniaProperty.Register<MonthFilterBar, ICommand?>(nameof(SelectMonthCommand));

    public ICommand? SelectMonthCommand
    {
        get => GetValue(SelectMonthCommandProperty);
        set => SetValue(SelectMonthCommandProperty, value);
    }

    public MonthFilterBar()
    {
        InitializeComponent();
    }
}
