using Avalonia.Controls;
using Avalonia.Interactivity;

namespace FanucBCKNET.Views;

public partial class ConfirmDialog : Window
{
    public ConfirmDialog()
    {
        InitializeComponent();
    }

    public ConfirmDialog(string mensaje, string tituloVentana = "Confirmar", string textoBoton = "Borrar") : this()
    {
        Title = tituloVentana;
        MessageText.Text = mensaje;
        YesButton.Content = textoBoton;
    }

    private void OnYes(object? sender, RoutedEventArgs e) => Close(true);

    private void OnNo(object? sender, RoutedEventArgs e) => Close(false);
}
