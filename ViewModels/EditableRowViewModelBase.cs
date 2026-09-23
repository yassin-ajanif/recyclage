using CommunityToolkit.Mvvm.ComponentModel;

namespace Recyclage.ViewModels;

public abstract partial class EditableRowViewModelBase : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private bool _isEditing;

    public bool IsDisplayMode => !IsEditing;
    public bool ShowEditDelete => Id > 0 && !IsEditing;
    public bool ShowSaveCancel => IsEditing;
    public bool IsNewRow => Id == 0;
    public bool CanDelete => Id > 0;

    partial void OnIsEditingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsDisplayMode));
        OnPropertyChanged(nameof(ShowEditDelete));
        OnPropertyChanged(nameof(ShowSaveCancel));
    }

    public void MarkAsSaved(int id)
    {
        Id = id;
        IsEditing = false;
        NotifyActionProperties();
    }

    public void BeginEdit()
    {
        CaptureSnapshot();
        IsEditing = true;
    }

    public void CancelEdit()
    {
        if (IsNewRow)
            ClearFields();
        else
            RestoreSnapshot();

        IsEditing = false;
    }

    public void EndEdit()
    {
        IsEditing = false;
        NotifyActionProperties();
    }

    public void StartAsNewRow() => IsEditing = true;

    protected void NotifyActionProperties()
    {
        OnPropertyChanged(nameof(ShowEditDelete));
        OnPropertyChanged(nameof(ShowSaveCancel));
        OnPropertyChanged(nameof(IsNewRow));
        OnPropertyChanged(nameof(CanDelete));
    }

    protected abstract void CaptureSnapshot();
    protected abstract void RestoreSnapshot();
    protected abstract void ClearFields();
}
