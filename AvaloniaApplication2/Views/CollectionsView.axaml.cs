using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaApplication2.Views;

public partial class CollectionsView : UserControl
{
    public CollectionsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
