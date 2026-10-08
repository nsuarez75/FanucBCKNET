using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using FanucBCKNET.Models;
using FanucBCKNET.ViewModels;

namespace FanucBCKNET.Views;

public partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    // ---------- Proyectos ----------

    private async void OnNewProject(object? sender, RoutedEventArgs e)
    {
        var nombre = await new ProjectDialog().ShowDialog<string?>(this);
        if (nombre is not null)
        {
            ViewModel.AddProject(nombre);
        }
    }

    private async void OnEditProject(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedProject is null)
        {
            await Info("Selecciona primero un proyecto.");
            return;
        }

        var nombre = await new ProjectDialog(ViewModel.SelectedProject.Nombre).ShowDialog<string?>(this);
        if (nombre is not null)
        {
            ViewModel.RenameSelectedProject(nombre);
        }
    }

    private async void OnDeleteProject(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedProject is null)
        {
            await Info("Selecciona primero un proyecto.");
            return;
        }

        var confirmado = await new ConfirmDialog(
            $"¿Borrar el proyecto \"{ViewModel.SelectedProject.Nombre}\" y sus {ViewModel.SelectedProject.Robots.Count} robot(s)?",
            "Borrar proyecto").ShowDialog<bool>(this);

        if (confirmado)
        {
            ViewModel.DeleteProject(ViewModel.SelectedProject);
        }
    }

    private async void OnExportProject(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedProject is null)
        {
            await Info("Selecciona primero un proyecto.");
            return;
        }

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Exportar proyecto",
            SuggestedFileName = $"{ViewModel.SelectedProject.Nombre}.json",
            DefaultExtension = "json",
            FileTypeChoices = new[] { new FilePickerFileType("Proyecto FanucBCKNET (*.json)") { Patterns = new[] { "*.json" } } }
        });

        if (file is null)
        {
            return;
        }

        try
        {
            ViewModel.ExportProject(ViewModel.SelectedProject, file.Path.LocalPath);
        }
        catch (Exception ex)
        {
            await Info($"No se pudo exportar el proyecto: {ex.Message}");
        }
    }

    private async void OnImportProject(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Importar proyecto",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("Proyecto FanucBCKNET (*.json)") { Patterns = new[] { "*.json" } } }
        });

        var file = files.FirstOrDefault();
        if (file is null)
        {
            return;
        }

        try
        {
            ViewModel.ImportProject(file.Path.LocalPath);
        }
        catch (Exception ex)
        {
            await Info($"No se pudo importar el proyecto: {ex.Message}");
        }
    }

    // ---------- Robots ----------

    private async void OnAddRobot(object? sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedProject is null)
        {
            await Info("Crea o selecciona primero un proyecto.");
            return;
        }

        var robot = await new RobotDialog().ShowDialog<Robot?>(this);
        if (robot is not null)
        {
            ViewModel.AddRobot(robot);
        }
    }

    private async void OnEditRobot(object? sender, RoutedEventArgs e)
    {
        if (RobotsList.SelectedItem is not Robot seleccionado)
        {
            await Info("Selecciona un robot para editar.");
            return;
        }

        var editado = await new RobotDialog(seleccionado).ShowDialog<Robot?>(this);
        if (editado is not null)
        {
            ViewModel.UpdateRobot(seleccionado, editado);
        }
    }

    private async void OnDeleteRobot(object? sender, RoutedEventArgs e)
    {
        var seleccionados = RobotsList.SelectedItems?.Cast<Robot>().ToList() ?? new List<Robot>();
        if (seleccionados.Count == 0)
        {
            await Info("Selecciona uno o varios robots para borrar.");
            return;
        }

        var confirmado = await new ConfirmDialog(
            $"¿Borrar {seleccionados.Count} robot(s) del proyecto?", "Borrar robots").ShowDialog<bool>(this);

        if (confirmado)
        {
            ViewModel.DeleteRobots(seleccionados);
        }
    }

    // ---------- Backup ----------

    private async void OnChooseDestination(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Seleccionar carpeta de destino",
            AllowMultiple = false
        });

        var folder = folders.FirstOrDefault();
        if (folder is not null)
        {
            ViewModel.DestinationFolder = folder.Path.LocalPath;
        }
    }

    private async void OnBackup(object? sender, RoutedEventArgs e)
    {
        var seleccionados = RobotsList.SelectedItems?.Cast<Robot>().ToList() ?? new List<Robot>();

        if (string.IsNullOrWhiteSpace(ViewModel.DestinationFolder))
        {
            await Info("Elige primero una carpeta destino.");
            return;
        }

        if (seleccionados.Count == 0)
        {
            await Info("Selecciona uno o varios robots para hacer backup.");
            return;
        }

        await ViewModel.StartBackupAsync(ViewModel.DestinationFolder, seleccionados);
    }

    private Task Info(string mensaje) => new MessageDialog(mensaje).ShowDialog(this);
}
