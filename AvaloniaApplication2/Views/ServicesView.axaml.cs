using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaApplication2.Views;

public partial class ServicesView : UserControl
{
    public ServicesView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
