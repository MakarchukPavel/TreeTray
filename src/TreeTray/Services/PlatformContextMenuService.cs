#region Class: PlatformContextMenuService

namespace TreeTray.Services;

public sealed class PlatformContextMenuService : IPlatformContextMenuService
{
	#region Fields: Private

	private readonly IWindowsShellContextMenuService _windowsShellContextMenuService;

	private ContextMenu? _fallbackContextMenu;

	#endregion
	#region Constructors: Public

	public PlatformContextMenuService(IWindowsShellContextMenuService windowsShellContextMenuService)
	{
		_windowsShellContextMenuService = windowsShellContextMenuService;
	}

	#endregion
	#region Properties: Public

	public bool SupportsContextMenus => OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsLinux();

	#endregion
	#region Methods: Private

	private void ClearFallbackContextMenu(object? sender, RoutedEventArgs eventArgs)
	{
		if (sender is ContextMenu contextMenu)
		{
			contextMenu.Closed -= ClearFallbackContextMenu;
		}

		_fallbackContextMenu = null;
	}

	private void CloseFallbackContextMenu()
	{
		if (_fallbackContextMenu is null)
		{
			return;
		}

		var contextMenu = _fallbackContextMenu;
		_fallbackContextMenu = null;
		contextMenu.Close();
	}

	private static MenuItem CreateMenuItem(string header, Action action)
	{
		var menuItem = new MenuItem
		{
			Header = header
		};

		menuItem.Click += (_, _) =>
		{
			try
			{
				action();
			}
			catch
			{
				// Ignore platform shell failures to keep the launcher UI responsive.
			}
		};

		return menuItem;
	}

	private static ContextMenu CreateLinuxContextMenu(string path, Control placementTarget)
	{
		return new ContextMenu
		{
			ItemsSource = new object[]
			{
				CreateMenuItem("Open", () => RunPlatformCommand("xdg-open", path)),
				CreateMenuItem("Open containing folder", () => OpenContainingDirectory(path)),
				new Separator(),
				CreateMenuItem("Copy path", () => CopyPath(path, placementTarget))
			}
		};
	}

	private static ContextMenu CreateMacOsContextMenu(string path, Control placementTarget)
	{
		return new ContextMenu
		{
			ItemsSource = new object[]
			{
				CreateMenuItem("Open", () => RunPlatformCommand("open", path)),
				CreateMenuItem("Reveal in Finder", () => RunPlatformCommand("open", "-R", path)),
				CreateMenuItem("Get Info", () => ShowMacOsInfo(path)),
				CreateMenuItem("Move to Trash", () => MoveToMacOsTrash(path)),
				new Separator(),
				CreateMenuItem("Copy path", () => CopyPath(path, placementTarget))
			}
		};
	}

	private ContextMenu CreateNonWindowsContextMenu(string path, Control placementTarget)
	{
		if (OperatingSystem.IsMacOS())
		{
			return CreateMacOsContextMenu(path, placementTarget);
		}

		if (OperatingSystem.IsLinux())
		{
			return CreateLinuxContextMenu(path, placementTarget);
		}

		return new ContextMenu();
	}

	private static void CopyPath(string path, Control placementTarget)
	{
		var clipboard = TopLevel.GetTopLevel(placementTarget)?.Clipboard;
		if (clipboard is null)
		{
			return;
		}

		_ = clipboard.SetTextAsync(path);
	}

	private static string EscapeAppleScriptString(string value)
	{
		return value
			.Replace("\\", "\\\\", StringComparison.Ordinal)
			.Replace("\"", "\\\"", StringComparison.Ordinal);
	}

	private static void MoveToMacOsTrash(string path)
	{
		var escapedPath = EscapeAppleScriptString(path);
		RunPlatformCommand(
			"osascript",
			"-e",
			$"tell application \"Finder\" to delete (POSIX file \"{escapedPath}\" as alias)");
	}

	private static void OpenContainingDirectory(string path)
	{
		var directoryPath = Directory.Exists(path)
			? path
			: Path.GetDirectoryName(path);

		if (string.IsNullOrWhiteSpace(directoryPath))
		{
			return;
		}

		if (OperatingSystem.IsMacOS())
		{
			RunPlatformCommand("open", directoryPath);
			return;
		}

		if (OperatingSystem.IsLinux())
		{
			RunPlatformCommand("xdg-open", directoryPath);
		}
	}

	private static void RunPlatformCommand(string fileName, params string[] arguments)
	{
		var processStartInfo = new ProcessStartInfo
		{
			FileName = fileName,
			UseShellExecute = false
		};

		foreach (var argument in arguments)
		{
			processStartInfo.ArgumentList.Add(argument);
		}

		Process.Start(processStartInfo);
	}

	private static void ShowMacOsInfo(string path)
	{
		var escapedPath = EscapeAppleScriptString(path);
		RunPlatformCommand(
			"osascript",
			"-e",
			"tell application \"Finder\" to activate",
			"-e",
			$"tell application \"Finder\" to open information window of (POSIX file \"{escapedPath}\" as alias)");
	}

	private bool ShowFallbackContextMenu(string path, Control? placementTarget)
	{
		if (placementTarget is null)
		{
			return false;
		}

		CloseFallbackContextMenu();
		var contextMenu = CreateNonWindowsContextMenu(path, placementTarget);
		contextMenu.Placement = PlacementMode.Pointer;
		contextMenu.PlacementTarget = placementTarget;
		contextMenu.Closed += ClearFallbackContextMenu;
		_fallbackContextMenu = contextMenu;
		contextMenu.Open(placementTarget);
		return true;
	}

	#endregion
	#region Methods: Public

	public bool ShowContextMenu(string path, PixelPoint screenPosition, Control? placementTarget = null)
	{
		if (string.IsNullOrWhiteSpace(path)
			|| (!File.Exists(path) && !Directory.Exists(path)))
		{
			return false;
		}

		if (OperatingSystem.IsWindows())
		{
			return _windowsShellContextMenuService.ShowContextMenu(path, screenPosition);
		}

		return ShowFallbackContextMenu(path, placementTarget);
	}

	#endregion
}

#endregion
