using System.Threading.Tasks;

namespace Codexus.Game.Launcher.Managers;

public class TaskManager
{
	private static TaskManager? _instance;
	
	private TaskFactory? _factory;

	public static TaskManager Instance => _instance ?? (_instance = new TaskManager());

	private TaskManager()
	{
	}

	public TaskFactory GetFactory()
	{
		return _factory ?? (_factory = new TaskFactory());
	}
}
