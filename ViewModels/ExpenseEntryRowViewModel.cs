using CommunityToolkit.Mvvm.ComponentModel;

namespace Recyclage.ViewModels;

public partial class ExpenseEntryRowViewModel : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private string _date = DateTime.Today.ToString("yyyy-MM-dd");

    [ObservableProperty]
    private string _expenseType = string.Empty;

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private string _description = string.Empty;

    public bool IsEmpty => string.IsNullOrWhiteSpace(ExpenseType) || Amount <= 0;
    public bool CanDelete => Id > 0;

    public void MarkAsSaved(int id)
    {
        Id = id;
        OnPropertyChanged(nameof(CanDelete));
    }
}
