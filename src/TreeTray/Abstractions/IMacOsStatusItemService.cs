#region Interface: IMacOsStatusItemService

namespace TreeTray.Abstractions;

public interface IMacOsStatusItemService
{
	#region Methods: Public

	void Apply(
		TreeTrayConfiguration configuration,
		LauncherSnapshot snapshot,
		bool isLoading,
		Action<LauncherEntry> launchAction,
		Action openLauncherAction);

	void Remove();

	#endregion
}

#endregion
