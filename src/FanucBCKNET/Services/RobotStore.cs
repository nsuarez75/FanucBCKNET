using System.Text.Json;
using FanucBCKNET.Models;

namespace FanucBCKNET.Services;

public class RobotStore
{
    private readonly string _path;

    public RobotStore(string? path = null)
    {
        _path = path ?? Path.Combine(AppContext.BaseDirectory, "robots.json");
    }

    public List<Robot> Load()
    {
        if (!File.Exists(_path))
        {
            return new List<Robot>();
        }

        try
        {
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<List<Robot>>(json) ?? new List<Robot>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error leyendo robots.json: {ex.Message}");
            return new List<Robot>();
        }
    }

    public void Save(List<Robot> robots)
    {
        var json = JsonSerializer.Serialize(robots, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_path, json);
    }

    public void Add(Robot robot)
    {
        var robots = Load();
        robots.Add(robot);
        Save(robots);
    }

    public void Remove(string nombre, string grupo)
    {
        var robots = Load();
        robots.RemoveAll(r => r.Nombre == nombre && r.Grupo == grupo);
        Save(robots);
    }
}
