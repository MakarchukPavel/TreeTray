#region Class: TrayPopupMenuService

namespace TreeTray.Services;

public sealed class TrayPopupMenuService : ITrayPopupMenuService
{
	#region Fields: Private

	private readonly IPlatformContextMenuService _platformContextMenuService;

	private readonly ITrayContextMenuBuilder _trayContextMenuBuilder;

	private Window? _popupWindow;

	#endregion

	#region Constructors: Public

	public TrayPopupMenuService(
		IPlatformContextMenuService platformContextMenuService,
		ITrayContextMenuBuilder trayContextMenuBuilder)
	{
		_platformContextMenuService = platformContextMenuService;
		_trayContextMenuBuilder = trayContextMenuBuilder;
	}

	#endregion

	#region Properties: Public

	public bool IsOpen => _popupWindow is not null;

	#endregion

	#region Methods: Private

	private void ResetPopupWindow(object? sender, EventArgs eventArgs)
	{
		if (sender is Window popupWindow)
		{
			popupWindow.Closed -= ResetPopupWindow;
		}

		_popupWindow = null;
	}

	private void ShowContextMenu(LauncherEntry entry, PixelPoint screenPosition)
	{
		Dispatcher.UIThread.Post(
			() =>
			{
				try
				{
					_platformContextMenuService.ShowContextMenu(entry.SourcePath, screenPosition);
				}
				finally
				{
					Hide();
				}
			},
			DispatcherPriority.Background);
	}

	#endregion

	#region Methods: Public

	public void Hide()
	{
		if (_popupWindow is null)
		{
			return;
		}

		var popupWindow = _popupWindow;
		_popupWindow = null;
		popupWindow.Close();
	}

	public void Show(
		LauncherSnapshot snapshot,
		Action<LauncherEntry> launchAction,
		PixelPoint screenPosition)
	{
		if (_popupWindow is Views.TrayLoadingWindow)
		{
			Hide();
		}
		else if (_popupWindow is not null)
		{
			Hide();
			return;
		}

		var contextMenu = _trayContextMenuBuilder.Build(
			snapshot,
			entry =>
			{
				launchAction(entry);
				Hide();
			},
			ShowContextMenu);

		var popupWindow = new Views.TrayPopupWindow(contextMenu, screenPosition);
		popupWindow.Closed += ResetPopupWindow;
		_popupWindow = popupWindow;
		popupWindow.Show();
	}

	public void ShowLoading(PixelPoint screenPosition)
	{
		if (_popupWindow is Views.TrayLoadingWindow)
		{
			return;
		}

		if (_popupWindow is not null)
		{
			Hide();
		}

		var popupWindow = new Views.TrayLoadingWindow(screenPosition);
		popupWindow.Closed += ResetPopupWindow;
		_popupWindow = popupWindow;
		popupWindow.Show();
	}

	#endregion
}

#endregion
