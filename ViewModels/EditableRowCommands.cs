namespace Recyclage.ViewModels;

internal static class EditableRowCommands
{
    public static void CancelOtherEdits<T>(IEnumerable<T> rows, T current)
        where T : EditableRowViewModelBase
    {
        foreach (var row in rows)
        {
            if (row.IsEditing && !ReferenceEquals(row, current))
                row.CancelEdit();
        }
    }
}
