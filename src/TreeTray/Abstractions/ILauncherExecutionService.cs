#region Interface: ILauncherExecutionService

namespace TreeTray.Abstractions;

public interface ILauncherExecutionService
{
	#region Methods: Public

	void Launch(LauncherEntry entry);

	void OpenDirectory(string directoryPath);

	#endregion
}

#endregion
