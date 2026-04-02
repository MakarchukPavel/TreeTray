#region Interface: ITrayPopupMenuService

namespace TreeTray.Abstractions;

public interface ITrayPopupMenuService
{
	#region Properties: Public

	bool IsOpen { get; }

	#endregion

	#region Methods: Public

	void Hide();

	void Show(
		LauncherSnapshot snapshot,
		Action<LauncherEntry> launchAction,
		PixelPoint screenPosition);

	void ShowLoading(PixelPoint screenPosition);

	#endregion
}

#endregion
