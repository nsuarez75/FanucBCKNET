using Avalonia.Controls;
using Avalonia.Interactivity;

namespace FanucBCKNET.Views;

public partial class MessageDialog : Window
{
    public MessageDialog()
    {
        InitializeComponent();
    }

    public MessageDialog(string mensaje, string tituloVentana = "Aviso") : this()
    {
        Title = tituloVentana;
        MessageText.Text = mensaje;
    }

    private void OnOk(object? sender, RoutedEventArgs e) => Close();
}
