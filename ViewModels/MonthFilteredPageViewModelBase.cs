using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Recyclage.ViewModels;

public abstract partial class MonthFilteredPageViewModelBase : PageViewModelBase
{
    [ObservableProperty]
    private int _selectedMonth = DateTime.Now.Month;

    public ObservableCollection<MonthFilterOption> Months { get; } =
        MonthFilterOption.Create(DateTime.Now.Month);

    protected int SelectedYear => DateTime.Now.Year;

    protected string SelectedMonthPrefix => $"{SelectedYear:D4}-{SelectedMonth:D2}";

    [RelayCommand]
    private void SelectMonth(MonthFilterOption month) => SelectedMonth = month.Number;

    partial void OnSelectedMonthChanged(int value)
    {
        foreach (var month in Months)
            month.IsSelected = month.Number == value;

        OnMonthFilterChanged();
    }

    protected virtual void OnMonthFilterChanged()
    {
    }
}
