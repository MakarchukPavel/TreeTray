#region Interface: IPlatformContextMenuService

namespace TreeTray.Abstractions;

public interface IPlatformContextMenuService
{
	#region Properties: Public

	bool SupportsContextMenus { get; }

	#endregion
	#region Methods: Public

	bool ShowContextMenu(string path, PixelPoint screenPosition, Control? placementTarget = null);

	#endregion
}

#endregion
