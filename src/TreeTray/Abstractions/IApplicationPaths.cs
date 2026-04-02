#region Interface: IApplicationPaths

namespace TreeTray.Abstractions;

public interface IApplicationPaths
{
	#region Properties: Public

	string ApplicationDirectory { get; }

	string ConfigurationDirectory { get; }

	string ConfigurationFilePath { get; }

	bool HasCustomConfigurationFilePath { get; }

	#endregion

	#region Methods: Public

	LaunchCommand GetCurrentApplicationLaunchCommand();

	string GetDefaultLaunchersDirectory();

	#endregion
}

#endregion
