#region Class: TrayPopupWindow

namespace TreeTray.Views;

public sealed class TrayPopupWindow : Window
{
	#region Fields: Private

	private readonly Border _anchor;

	private readonly ContextMenu _contextMenu;

	#endregion

	#region Constructors: Public

	public TrayPopupWindow(ContextMenu contextMenu, PixelPoint screenPosition)
	{
		_contextMenu = contextMenu ?? throw new ArgumentNullException(nameof(contextMenu));
		_anchor = new Border
		{
			Width = 1,
			Height = 1,
			Background = Avalonia.Media.Brushes.Transparent
		};

		CanResize = false;
		Content = _anchor;
		Height = 1;
		Opacity = 0;
		Position = screenPosition;
		ShowActivated = true;
		ShowInTaskbar = false;
		SystemDecorations = SystemDecorations.None;
		Topmost = true;
		Width = 1;

		Opened += OnOpened;
	}

	#endregion

	#region Methods: Private

	private void OnContextMenuClosed(object? sender, RoutedEventArgs eventArgs)
	{
		_contextMenu.Closed -= OnContextMenuClosed;
		Close();
	}

	private void OnOpened(object? sender, EventArgs eventArgs)
	{
		Opened -= OnOpened;
		_contextMenu.Closed += OnContextMenuClosed;
		_contextMenu.Placement = PlacementMode.BottomEdgeAlignedLeft;
		_contextMenu.Open(_anchor);
	}

	#endregion
}

#endregion
