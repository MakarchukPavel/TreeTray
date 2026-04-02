#region Interface: IPlatformLauncherResolver

namespace TreeTray.Abstractions;

public interface IPlatformLauncherResolver
{
	#region Methods: Public

	LaunchCommand CreateLaunchCommand(string path);

	string GetDisplayName(string path);

	bool IsLauncherPath(string path);

	#endregion
}

#endregion
