using Avalonia.Controls;
using Avalonia.Interactivity;

namespace FanucBCKNET.Views;

public partial class ProjectDialog : Window
{
    public ProjectDialog()
    {
        InitializeComponent();
        NombreBox.AttachedToVisualTree += (_, _) => NombreBox.Focus();
    }

    public ProjectDialog(string nombreInicial) : this()
    {
        NombreBox.Text = nombreInicial;
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        var nombre = NombreBox.Text?.Trim() ?? "";
        if (nombre == "")
        {
            return;
        }

        Close(nombre);
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(null);
}
