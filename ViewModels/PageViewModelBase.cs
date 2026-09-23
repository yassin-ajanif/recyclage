using CommunityToolkit.Mvvm.ComponentModel;

namespace Recyclage.ViewModels;

public abstract class PageViewModelBase : ObservableObject
{
    public abstract string Title { get; }
}
