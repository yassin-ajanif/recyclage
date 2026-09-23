using CommunityToolkit.Mvvm.ComponentModel;

namespace Recyclage.ViewModels;

public partial class SupplierRowViewModel : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _ice = string.Empty;

    public bool IsEmpty => string.IsNullOrWhiteSpace(Name);
    public bool CanDelete => Id > 0;

    public void MarkAsSaved(int id)
    {
        Id = id;
        OnPropertyChanged(nameof(CanDelete));
    }
}
