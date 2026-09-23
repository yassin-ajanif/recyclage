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
        EnsureTrailingRowEditable();
    }

    [RelayCommand]
    public async Task CommitRow(TRow row)
    {
        if (await SaveRowAsync(row))
            EnsureTrailingRowEditable();
    }

    [RelayCommand]
    public void CancelEditRow(TRow row)
    {
        row.CancelEdit();
        EnsureTrailingRowEditable();
    }

    protected void EnsureTrailingRowEditable()
    {
        if (EditableRows.Count == 0)
            return;

        var last = EditableRows[^1];
        if (last.IsNewRow && !last.IsEditing)
            last.StartAsNewRow();
    }

    public abstract Task<bool> SaveRowAsync(TRow row);
}
