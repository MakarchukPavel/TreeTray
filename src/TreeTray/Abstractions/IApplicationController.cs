#region Interface: IApplicationController

namespace TreeTray.Abstractions;

public interface IApplicationController
{
	#region Properties: Public

	TreeTrayConfiguration Configuration { get; }

	string ConfigurationFilePath { get; }

	bool IsLoading { get; }

	bool IsShuttingDown { get; }

	LauncherSnapshot Snapshot { get; }

	bool ShouldHideMainWindowOnClose { get; }

	#endregion

	#region Events: Public

	event EventHandler? StateChanged;

	#endregion

	#region Methods: Public

	void Exit();

	void Launch(LauncherEntry entry);

	void OpenConfigurationDirectory();

	void OpenLaunchersDirectory();

	void Reload();

	void ShowMainWindow();

	void Start(Application application);

	void UpdateConfiguration(TreeTrayConfiguration configuration);

	#endregion
}

#endregion
