using System.Threading.Tasks;

namespace Codexus.Game.Launcher.Managers;

public class TaskManager {
    private static TaskManager _instance;

    private TaskFactory _factory;

    private TaskManager()
    {
    }

    public static TaskManager Instance => _instance ??= new TaskManager();

    public TaskFactory GetFactory()
    {
        return _factory ??= new TaskFactory();
    }
}