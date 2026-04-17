using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace AvaloniaApplication2.Views.Dialogs;

public partial class ConfirmCloseWindow : Window
{
    public ConfirmCloseWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public void ConfirmButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    public void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
