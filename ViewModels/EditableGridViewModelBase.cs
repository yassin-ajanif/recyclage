using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace Recyclage.ViewModels;

public abstract partial class EditableGridViewModelBase<TRow> : PageViewModelBase
    where TRow : EditableRowViewModelBase
{
    protected abstract ObservableCollection<TRow> EditableRows { get; }

    [RelayCommand]
    public void BeginEditRow(TRow row)
    {
        EditableRowCommands.CancelOtherEdits(EditableRows, row);
        row.BeginEdit();
    }

    [RelayCommand]
    public async Task CommitRow(TRow row) => await SaveRowAsync(row);

    [RelayCommand]
    public void CancelEditRow(TRow row) => row.CancelEdit();

    public abstract Task<bool> SaveRowAsync(TRow row);
}
