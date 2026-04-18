using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaApplication2.Views;

public partial class DirectionsView : UserControl
{
    public DirectionsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
