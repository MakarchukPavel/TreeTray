#region Class: TreeTrayConfiguration

namespace TreeTray.Models;

public sealed class TreeTrayConfiguration
{
	#region Properties: Public

	public bool EnableTaskbarDockIcon { get; set; }

	public bool EnableTrayIcon { get; set; }

	public bool InvertTrayIconMouseButtons { get; set; }

	public string LaunchersDirectory { get; set; } = string.Empty;

	public bool StartWithOperatingSystem { get; set; }

	public string TrayIconBackgroundColor { get; set; } = string.Empty;

	public string TrayIconForegroundColor { get; set; } = string.Empty;

	public string TrayIconGlyph { get; set; } = string.Empty;

	public string TrayToolTipText { get; set; } = string.Empty;

	#endregion
}

#endregion
