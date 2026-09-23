using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Recyclage.ViewModels;

public partial class MonthFilterOption : ObservableObject
{
    private static readonly string[] ArabicNames =
    [
        "يناير", "فبراير", "مارس", "أبريل", "ماي", "يونيو",
        "يوليوز", "غشت", "شتنبر", "أكتوبر", "نونبر", "دجنبر"
    ];

    public int Number { get; init; }

    public string Name { get; init; } = string.Empty;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isCurrentMonth;

    public static ObservableCollection<MonthFilterOption> Create(int selectedMonth)
    {
        var currentMonth = DateTime.Now.Month;
        var months = new ObservableCollection<MonthFilterOption>();

        for (var month = 1; month <= 12; month++)
        {
            months.Add(new MonthFilterOption
            {
                Number = month,
                Name = ArabicNames[month - 1],
                IsCurrentMonth = month == currentMonth,
                IsSelected = month == selectedMonth
            });
        }

        return months;
    }
}
