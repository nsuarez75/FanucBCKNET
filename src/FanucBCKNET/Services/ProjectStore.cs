using System.Text.Json;
using FanucBCKNET.Models;

namespace FanucBCKNET.Services;

public class ProjectStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _path;

    public ProjectStore(string? path = null)
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FanucBCKNET");
        Directory.CreateDirectory(dir);
        _path = path ?? Path.Combine(dir, "projects.json");
    }

    public List<Project> Load()
    {
        if (!File.Exists(_path))
        {
            return new List<Project>();
        }

        try
        {
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<List<Project>>(json) ?? new List<Project>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error leyendo projects.json: {ex.Message}");
            return new List<Project>();
        }
    }

    public void Save(IEnumerable<Project> projects)
    {
        var json = JsonSerializer.Serialize(projects.ToList(), JsonOptions);
        File.WriteAllText(_path, json);
    }

    public void ExportProject(Project project, string filePath)
    {
        var json = JsonSerializer.Serialize(project, JsonOptions);
        File.WriteAllText(filePath, json);
    }

    public Project ImportProject(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var project = JsonSerializer.Deserialize<Project>(json)
            ?? throw new InvalidDataException("El archivo no contiene un proyecto válido.");

        project.Id = Guid.NewGuid().ToString("N");
        return project;
    }
}
