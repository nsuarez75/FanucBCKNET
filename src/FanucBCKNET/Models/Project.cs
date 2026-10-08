using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FanucBCKNET.Models;

public partial class Project : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString("N");

    [ObservableProperty]
    private string _nombre = "";

    public ObservableCollection<Robot> Robots { get; set; } = new();
}
