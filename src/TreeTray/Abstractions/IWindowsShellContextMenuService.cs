#region Interface: IWindowsShellContextMenuService

namespace TreeTray.Abstractions;

public interface IWindowsShellContextMenuService
{
	#region Methods: Public

	bool ShowContextMenu(string path, PixelPoint screenPosition);

	#endregion
}

#endregion
