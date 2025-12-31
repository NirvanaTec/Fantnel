using System;

namespace Codexus.Game.Launcher.Utils.Progress;

public class SyncCallback<T>(Action<T> handler) : IProgress<T>
{
	public void Report(T value)
	{
		handler(value);
	}
}
