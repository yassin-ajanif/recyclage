using CommunityToolkit.Mvvm.ComponentModel;

namespace Recyclage.ViewModels;

public abstract partial class DatedEditableRowViewModelBase : EditableRowViewModelBase
{
    private string _snapshotDate = string.Empty;
    private DateTime _defaultDate = DateTime.Today;

    [ObservableProperty]
    private string _date = DateTime.Today.ToString("yyyy-MM-dd");

    public DateTime? SelectedDate
    {
        get => DateTime.TryParse(Date, out var dt) ? dt.Date : DateTime.Today;

        set
        {
            var text = value?.Date.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
            if (Date != text)
                Date = text;
        }
    }

    public DateTime DisplayDate => SelectedDate ?? DateTime.Today;

    partial void OnDateChanged(string value)
    {
        OnPropertyChanged(nameof(SelectedDate));
        OnPropertyChanged(nameof(DisplayDate));
    }

    protected void CaptureDateSnapshot() => _snapshotDate = Date;

    protected void RestoreDateSnapshot() => Date = _snapshotDate;

    public void SetDefaultDate(DateTime date) => _defaultDate = date.Date;

    public void EnsureDateWithinMonth(int year, int month)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        var current = SelectedDate ?? _defaultDate;

        if (current < start || current > end)
        {
            var today = DateTime.Today;
            SelectedDate = today.Year == year && today.Month == month ? today : start;
        }
    }

    protected void ClearDateField() => Date = _defaultDate.ToString("yyyy-MM-dd");
}
