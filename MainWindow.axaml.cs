using Avalonia.Controls;
using Recyclage.ViewModels;

namespace Recyclage;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new AppShellViewModel();
    }
}
