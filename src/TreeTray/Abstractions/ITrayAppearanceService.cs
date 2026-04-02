#region Interface: ITrayAppearanceService

namespace TreeTray.Abstractions;

public interface ITrayAppearanceService
{
	#region Methods: Public

	nint CreateWindowsTrayIconHandle(TreeTrayConfiguration configuration);

	byte[] GetTrayIconPngBytes(TreeTrayConfiguration configuration);

	WindowIcon GetTrayIcon(TreeTrayConfiguration configuration);

	string GetToolTipText(TreeTrayConfiguration configuration);

	#endregion
}

#endregion
