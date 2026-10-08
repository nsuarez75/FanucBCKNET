using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FanucBCKNET.Models;
using FanucBCKNET.Services;

namespace FanucBCKNET.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ProjectStore _store;
    private readonly FtpBackupService _ftp;

    [ObservableProperty]
    private ObservableCollection<Project> _projects = new();

    [ObservableProperty]
    private Project? _selectedProject;

    [ObservableProperty]
    private string _destinationFolder = "";

    [ObservableProperty]
    private ObservableCollection<BackupItemViewModel> _backupItems = new();

    [ObservableProperty]
    private bool _isBackupRunning;

    [ObservableProperty]
    private double _overallProgress;

    public MainWindowViewModel() : this(new ProjectStore(), new FtpBackupService())
    {
    }

    public MainWindowViewModel(ProjectStore store, FtpBackupService ftp)
    {
        _store = store;
        _ftp = ftp;

        Projects = new ObservableCollection<Project>(_store.Load());
        SelectedProject = Projects.FirstOrDefault();
    }

    public bool HasSelectedProject => SelectedProject is not null;

    partial void OnSelectedProjectChanged(Project? value) => OnPropertyChanged(nameof(HasSelectedProject));

    private void Persist() => _store.Save(Projects);

    public void AddProject(string nombre)
    {
        var project = new Project { Nombre = nombre };
        Projects.Add(project);
        SelectedProject = project;
        Persist();
    }

    public void RenameSelectedProject(string nuevoNombre)
    {
        if (SelectedProject is null)
        {
            return;
        }

        SelectedProject.Nombre = nuevoNombre;
        Persist();
    }

    public void DeleteProject(Project project)
    {
        var index = Projects.IndexOf(project);
        Projects.Remove(project);

        if (SelectedProject == project)
        {
            SelectedProject = Projects.Count == 0
                ? null
                : Projects[Math.Min(index, Projects.Count - 1)];
        }

        Persist();
    }

    public void ExportProject(Project project, string filePath) => _store.ExportProject(project, filePath);

    public Project ImportProject(string filePath)
    {
        var project = _store.ImportProject(filePath);

        if (Projects.Any(p => p.Nombre == project.Nombre))
        {
            project.Nombre += " (importado)";
        }

        Projects.Add(project);
        SelectedProject = project;
        Persist();
        return project;
    }

    public void AddRobot(Robot robot)
    {
        SelectedProject?.Robots.Add(robot);
        Persist();
    }

    public void UpdateRobot(Robot original, Robot editado)
    {
        original.Nombre = editado.Nombre;
        original.Ip = editado.Ip;
        original.Puerto = editado.Puerto;
        Persist();
    }

    public void DeleteRobots(IEnumerable<Robot> robots)
    {
        if (SelectedProject is null)
        {
            return;
        }

        foreach (var robot in robots.ToList())
        {
            SelectedProject.Robots.Remove(robot);
        }

        Persist();
    }

    public async Task StartBackupAsync(string destino, IReadOnlyList<Robot> robots, CancellationToken token = default)
    {
        if (SelectedProject is null || robots.Count == 0 || string.IsNullOrWhiteSpace(destino))
        {
            return;
        }

        BackupItems = new ObservableCollection<BackupItemViewModel>(
            robots.Select(r => new BackupItemViewModel { RobotName = r.Nombre }));

        IsBackupRunning = true;
        OverallProgress = 0;

        var timestamp = DateTime.Now.ToString("ddMMyyyy_HHmm");
        var backupRoot = Path.Combine(destino, $"Backup_{SelectedProject.Nombre}_{timestamp}");
        Directory.CreateDirectory(backupRoot);
        var errorLogPath = Path.Combine(backupRoot, "error.log");

        var tareas = robots.Select(async robot =>
        {
            var item = BackupItems.First(i => i.RobotName == robot.Nombre);
            var carpetaRobot = Path.Combine(backupRoot, robot.Nombre);

            var progress = new Progress<FtpBackupStatus>(status =>
            {
                item.Progress = status.Percentage;
                item.Status = status.Message;
                RecalculateOverallProgress();
            });

            try
            {
                await _ftp.BackupRobotAsync(carpetaRobot, robot, progress, token);
                item.Progress = 100;
                item.Status = "Completado";
            }
            catch (Exception ex)
            {
                item.HasError = true;
                item.Status = $"Error: {ex.Message}";
                await File.AppendAllTextAsync(
                    errorLogPath,
                    $"{DateTime.Now:O} Error haciendo backup de {robot.Nombre}: {ex.Message}{Environment.NewLine}",
                    token);
            }
            finally
            {
                RecalculateOverallProgress();
            }
        });

        await Task.WhenAll(tareas);
        IsBackupRunning = false;
    }

    private void RecalculateOverallProgress()
    {
        OverallProgress = BackupItems.Count > 0 ? BackupItems.Average(i => i.Progress) : 0;
    }
}
