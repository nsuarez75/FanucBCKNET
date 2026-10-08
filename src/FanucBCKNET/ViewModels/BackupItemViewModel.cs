using CommunityToolkit.Mvvm.ComponentModel;

namespace FanucBCKNET.ViewModels;

public partial class BackupItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _robotName = "";

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private string _status = "En espera...";

    [ObservableProperty]
    private bool _hasError;
}
