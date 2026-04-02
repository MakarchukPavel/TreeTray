#region Interface: IWindowsTrayIconService

namespace TreeTray.Abstractions;

public interface IWindowsTrayIconService
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
