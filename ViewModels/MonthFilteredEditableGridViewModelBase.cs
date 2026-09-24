using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Recyclage.ViewModels;

public abstract partial class MonthFilteredEditableGridViewModelBase<TRow> : EditableGridViewModelBase<TRow>
    where TRow : EditableRowViewModelBase
{
    [ObservableProperty]
    private int _selectedMonth = DateTime.Now.Month;

    public ObservableCollection<MonthFilterOption> Months { get; } =
        MonthFilterOption.Create(DateTime.Now.Month);

    protected int SelectedYear => DateTime.Now.Year;

    protected string SelectedMonthPrefix => $"{SelectedYear:D4}-{SelectedMonth:D2}";

    public DateTime SelectedMonthStart => new(SelectedYear, SelectedMonth, 1);

    public DateTime SelectedMonthEnd => SelectedMonthStart.AddMonths(1).AddDays(-1);

    public DateTime SelectedMonthDisplayDate
    {
        get
        {
            var today = DateTime.Today;
            return today.Year == SelectedYear && today.Month == SelectedMonth
                ? today
                : SelectedMonthStart;
        }
    }

    public bool IsDateInSelectedMonth(string? dateText) =>
        DateTime.TryParse(dateText, out var date) &&
        date.Year == SelectedYear &&
        date.Month == SelectedMonth;

    [RelayCommand]
    private void SelectMonth(MonthFilterOption month) => SelectedMonth = month.Number;

    partial void OnSelectedMonthChanged(int value)
    {
        foreach (var month in Months)
            month.IsSelected = month.Number == value;

        OnPropertyChanged(nameof(SelectedMonthStart));
        OnPropertyChanged(nameof(SelectedMonthEnd));
        OnPropertyChanged(nameof(SelectedMonthDisplayDate));

        OnMonthFilterChanged();
    }

    protected virtual void OnMonthFilterChanged()
    {
    }

    protected void ApplyMonthDateScope(DatedEditableRowViewModelBase row)
    {
        row.SetDefaultDate(SelectedMonthDisplayDate);
        row.EnsureDateWithinMonth(SelectedYear, SelectedMonth);
    }
}
