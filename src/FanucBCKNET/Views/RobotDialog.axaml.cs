using Avalonia.Controls;
using Avalonia.Interactivity;
using FanucBCKNET.Models;

namespace FanucBCKNET.Views;

public partial class RobotDialog : Window
{
    public RobotDialog()
    {
        InitializeComponent();
        NombreBox.AttachedToVisualTree += (_, _) => NombreBox.Focus();
    }

    public RobotDialog(Robot existente) : this()
    {
        NombreBox.Text = existente.Nombre;
        IpBox.Text = existente.Ip;
        PuertoBox.Value = existente.Puerto;
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        var nombre = NombreBox.Text?.Trim() ?? "";
        var ip = IpBox.Text?.Trim() ?? "";

        if (nombre == "" || ip == "")
        {
            return;
        }

        var robot = new Robot
        {
            Nombre = nombre,
            Ip = ip,
            Puerto = (int)(PuertoBox.Value ?? 21)
        };

        Close(robot);
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(null);
}
