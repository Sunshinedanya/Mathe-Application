using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaApplication2.ViewModels;
using AvaloniaApplication2.Views.Dialogs;

namespace AvaloniaApplication2;

public partial class MainWindow : Window
{
    private bool isCloseConfirmed;

    public MainWindow()
    {
        InitializeComponent();

        Opened += OnOpened;
        Closing += OnClosing;
    }

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        if (ViewModel.InitializeCommand.CanExecute(null))
        {
            await ViewModel.InitializeCommand.ExecuteAsync(null);
        }
    }

    private async void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (isCloseConfirmed)
        {
            return;
        }

        e.Cancel = true;

        var isConfirmed = await ShowCloseConfirmationAsync();
        if (!isConfirmed)
        {
            return;
        }

        isCloseConfirmed = true;
        Close();
    }

    private async Task<bool> ShowCloseConfirmationAsync()
    {
        var dialog = new ConfirmCloseWindow();
        return await dialog.ShowDialog<bool>(this);
    }

    public void TitleBar_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    public void MinimizeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    public void CloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
