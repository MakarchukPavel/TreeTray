#region Interface: IIconService

namespace TreeTray.Abstractions;

public interface IIconService
{
	#region Properties: Public

	WindowIcon ApplicationIcon { get; }

	Bitmap FolderIcon { get; }

	Bitmap LauncherIcon { get; }

	#endregion

	#region Methods: Public

	nint CreateWindowsTrayIconHandle();

	Bitmap GetEntryIcon(LauncherEntry entry);

	void PreloadEntryIcons(IEnumerable<LauncherEntry> entries);

	#endregion
}

#endregion
