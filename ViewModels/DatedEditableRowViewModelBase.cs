using CommunityToolkit.Mvvm.ComponentModel;

namespace Recyclage.ViewModels;

public abstract partial class DatedEditableRowViewModelBase : EditableRowViewModelBase
{
    private string _snapshotDate = string.Empty;

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

    protected void ClearDateField() => Date = DateTime.Today.ToString("yyyy-MM-dd");
}
