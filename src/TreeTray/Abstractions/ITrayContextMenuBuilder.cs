#region Interface: ITrayContextMenuBuilder

namespace TreeTray.Abstractions;

public interface ITrayContextMenuBuilder
{
	#region Methods: Public

	ContextMenu Build(
		LauncherSnapshot snapshot,
		Action<LauncherEntry> launchAction,
		Action<LauncherEntry, PixelPoint> showNativeContextMenuAction);

	#endregion
}

#endregion
