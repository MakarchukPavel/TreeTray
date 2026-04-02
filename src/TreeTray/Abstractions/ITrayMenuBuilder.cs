#region Interface: ITrayMenuBuilder

namespace TreeTray.Abstractions;

public interface ITrayMenuBuilder
{
	#region Methods: Public

	NativeMenu Build(
		LauncherSnapshot snapshot,
		bool isLoading,
		Action<LauncherEntry> launchAction);

	#endregion
}

#endregion
