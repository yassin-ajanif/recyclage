using CommunityToolkit.Mvvm.ComponentModel;

namespace Recyclage.ViewModels;

public partial class ExpenseEntryRowViewModel : EditableRowViewModelBase
{
    private string _snapshotDate = string.Empty;
    private string _snapshotExpenseType = string.Empty;
    private decimal _snapshotAmount;
    private string _snapshotDescription = string.Empty;

    [ObservableProperty]
    private string _date = DateTime.Today.ToString("yyyy-MM-dd");

    [ObservableProperty]
    private string _expenseType = string.Empty;

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private string _description = string.Empty;

    public bool IsEmpty => string.IsNullOrWhiteSpace(ExpenseType) || Amount <= 0;

    protected override void CaptureSnapshot()
    {
        _snapshotDate = Date;
        _snapshotExpenseType = ExpenseType;
        _snapshotAmount = Amount;
        _snapshotDescription = Description;
    }

    protected override void RestoreSnapshot()
    {
        Date = _snapshotDate;
        ExpenseType = _snapshotExpenseType;
        Amount = _snapshotAmount;
        Description = _snapshotDescription;
    }

    protected override void ClearFields()
    {
        Date = DateTime.Today.ToString("yyyy-MM-dd");
        ExpenseType = string.Empty;
        Amount = 0;
        Description = string.Empty;
    }
}
