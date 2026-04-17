using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaApplication2.Services;
using AvaloniaApplication2.ViewModels;

namespace AvaloniaApplication2;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var catalogDataService = new CatalogDataService();
            var mainWindowViewModel = new MainWindowViewModel(
                catalogDataService,
                pageSize: 3,
                defaultSort: "alphabetAsc",
                applicationTitle: "Матье");

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
