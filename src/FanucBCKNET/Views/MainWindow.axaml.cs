using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FanucBCKNET.Models;
using FanucBCKNET.Services;

namespace FanucBCKNET.Views;

public partial class MainWindow : Window
{
    private readonly RobotStore _store = new();
    private readonly FtpBackupService _ftpService = new();

    private string _targetFolder = "";
    private int _completed;

    public MainWindow()
    {
        InitializeComponent();
        RefreshGroups();
        RefreshList();
    }

    private void OnAddRobot(object? sender, RoutedEventArgs e)
    {
        var nombre = NombreBox.Text?.Trim() ?? "";
        var grupo = GrupoBox.Text?.Trim() ?? "";
        var ip = IpBox.Text?.Trim() ?? "";

        if (nombre == "" || grupo == "" || ip == "")
        {
            return;
        }

        _store.Add(new Robot { Nombre = nombre, Grupo = grupo, Ip = ip });

        NombreBox.Text = "";
        GrupoBox.Text = "";
        IpBox.Text = "";

        RefreshGroups();
        RefreshList();
    }

    private void OnDeleteRobot(object? sender, RoutedEventArgs e)
    {
        var grupo = GroupsCombo.SelectedItem as string ?? "";
        var seleccionados = RobotsList.SelectedItems?.Cast<string>().ToList() ?? new List<string>();

        foreach (var nombre in seleccionados)
        {
            _store.Remove(nombre, grupo);
        }

        RefreshGroups();
        RefreshList();
    }

    private void OnGroupChanged(object? sender, SelectionChangedEventArgs e)
    {
        RefreshList();
    }

    private async void OnChoosePath(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Seleccionar carpeta de destino",
            AllowMultiple = false
        });

        var folder = folders.FirstOrDefault();
        if (folder is null)
        {
            return;
        }

        _targetFolder = folder.Path.LocalPath;
        PathBox.Text = _targetFolder;
    }

    private async void OnBackup(object? sender, RoutedEventArgs e)
    {
        if (_targetFolder == "")
        {
            return;
        }

        var grupo = GroupsCombo.SelectedItem as string ?? "";
        var seleccionados = RobotsList.SelectedItems?.Cast<string>().ToHashSet() ?? new HashSet<string>();
        if (seleccionados.Count == 0)
        {
            return;
        }

        var robots = _store.Load()
            .Where(r => r.Grupo == grupo && seleccionados.Contains(r.Nombre))
            .ToList();

        if (robots.Count == 0)
        {
            return;
        }

        var timestamp = DateTime.Now.ToString("ddMMyyyy_HHmm");
        var backupRoot = Path.Combine(_targetFolder, $"Backup_{grupo}_{timestamp}");
        Directory.CreateDirectory(backupRoot);

        _completed = 0;

        SetBusy(true);
        BackupProgress.Minimum = 0;
        BackupProgress.Maximum = robots.Count;
        BackupProgress.Value = 0;
        BackupProgress.IsVisible = true;

        var errorLogPath = Path.Combine(backupRoot, "error.log");

        var tareas = robots.Select(robot => BackupOneAsync(backupRoot, robot, errorLogPath)).ToList();
        await Task.WhenAll(tareas);

        BackupProgress.IsVisible = false;
        SetBusy(false);
    }

    private async Task BackupOneAsync(string backupRoot, Robot robot, string errorLogPath)
    {
        var carpetaRobot = Path.Combine(backupRoot, robot.Nombre);
        try
        {
            await _ftpService.BackupRobotAsync(carpetaRobot, robot);
        }
        catch (Exception ex)
        {
            var linea = $"{DateTime.Now:O} Error haciendo backup de {robot.Nombre}: {ex.Message}{Environment.NewLine}";
            await Dispatcher.UIThread.InvokeAsync(() => File.AppendAllText(errorLogPath, linea));
        }
        finally
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _completed++;
                BackupProgress.Value = _completed;
            });
        }
    }

    private void SetBusy(bool busy)
    {
        AddButton.IsVisible = !busy;
        DeleteButton.IsVisible = !busy;
        BackupButton.IsVisible = !busy;
        PathButton.IsVisible = !busy;
        GroupsCombo.IsEnabled = !busy;
    }

    private void RefreshGroups()
    {
        var grupoActual = GroupsCombo.SelectedItem as string;

        var grupos = _store.Load()
            .Select(r => r.Grupo)
            .Distinct()
            .OrderBy(g => g)
            .ToList();

        GroupsCombo.ItemsSource = grupos;

        if (grupoActual is not null && grupos.Contains(grupoActual))
        {
            GroupsCombo.SelectedItem = grupoActual;
        }
        else if (grupos.Count > 0)
        {
            GroupsCombo.SelectedIndex = 0;
        }
    }

    private void RefreshList()
    {
        var grupo = GroupsCombo.SelectedItem as string;
        if (string.IsNullOrEmpty(grupo))
        {
            RobotsList.ItemsSource = Array.Empty<string>();
            return;
        }

        var nombres = _store.Load()
            .Where(r => r.Grupo == grupo)
            .Select(r => r.Nombre)
            .Distinct()
            .ToList();

        RobotsList.ItemsSource = nombres;
    }
}
