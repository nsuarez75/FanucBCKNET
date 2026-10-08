using CommunityToolkit.Mvvm.ComponentModel;

namespace FanucBCKNET.Models;

public partial class Robot : ObservableObject
{
    [ObservableProperty]
    private string _nombre = "";

    [ObservableProperty]
    private string _ip = "";

    [ObservableProperty]
    private int _puerto = 21;

    public Robot Clone() => new() { Nombre = Nombre, Ip = Ip, Puerto = Puerto };
}
