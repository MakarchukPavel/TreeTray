#region Class: TrayContextMenuBuilder

using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace TreeTray.Services;

public sealed class TrayContextMenuBuilder : ITrayContextMenuBuilder
{
	#region Struct: NativePoint

	[StructLayout(LayoutKind.Sequential)]
	private struct NativePoint
	{
		#region Fields: Public

		public int X;

		public int Y;

		#endregion
	}

	#endregion

	#region Constants: Private

	private const string User32LibraryName = "user32.dll";

	private const double SubmenuOverlap = 8;

	#endregion

	#region Fields: Private

	private readonly IIconService _iconService;

	#endregion

	#region Constructors: Public

	public TrayContextMenuBuilder(IIconService iconService)
	{
		_iconService = iconService;
	}

	#endregion

	#region Methods: Private

	[DllImport(User32LibraryName, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetCursorPos(out NativePoint point);

	private void AddLauncherItems(
		IList<object> items,
		IEnumerable<LauncherEntry> entries,
		Action<LauncherEntry> launchAction,
		Action<LauncherEntry, PixelPoint> showNativeContextMenuAction)
	{
		foreach (var entry in entries)
		{
			items.Add(CreateLauncherItem(entry, launchAction, showNativeContextMenuAction));
		}
	}

	private MenuItem CreateLauncherItem(
		LauncherEntry entry,
		Action<LauncherEntry> launchAction,
		Action<LauncherEntry, PixelPoint> showNativeContextMenuAction)
	{
		var item = new MenuItem
		{
			Header = entry.DisplayName,
			Icon = CreateMenuIcon(entry)
		};
		item.AddHandler(
			InputElement.PointerPressedEvent,
			(_, eventArgs) => OnLauncherItemPointerPressed(item, entry, showNativeContextMenuAction, eventArgs),
			RoutingStrategies.Bubble);

		if (entry.Children.Count > 0)
		{
			item.SubmenuOpened += (_, _) => AdjustSubmenuPopupOffset(item);

			var children = new List<object>();
			AddLauncherItems(children, entry.Children, launchAction, showNativeContextMenuAction);
			item.ItemsSource = children;
			return item;
		}

		item.Click += (_, _) => launchAction(entry);
		return item;
	}

	private Control CreateMenuIcon(LauncherEntry entry)
	{
		return new Image
		{
			Height = 16,
			Width = 16,
			Source = _iconService.GetEntryIcon(entry),
			Stretch = Avalonia.Media.Stretch.Uniform
		};
	}

	private static bool IsDirectMenuItemSource(MenuItem item, PointerPressedEventArgs eventArgs)
	{
		if (eventArgs.Source is not Avalonia.Visual visual)
		{
			return false;
		}

		var sourceMenuItem = visual as MenuItem ?? visual.FindAncestorOfType<MenuItem>();
		return ReferenceEquals(sourceMenuItem, item);
	}

	private static void AdjustSubmenuPopupOffset(MenuItem item)
	{
		Dispatcher.UIThread.Post(
			() =>
			{
				var popup = item
					.GetVisualDescendants()
					.OfType<Popup>()
					.FirstOrDefault(candidate => candidate.IsOpen && candidate.Child is not null);
				if (popup?.Child is not Visual popupChild)
				{
					return;
				}

				var popupScreenPoint = popupChild.PointToScreen(new Point());
				var itemScreenPoint = item.PointToScreen(new Point());
				popup.HorizontalOffset = popupScreenPoint.X >= itemScreenPoint.X
					? -SubmenuOverlap
					: SubmenuOverlap;
			},
			DispatcherPriority.Input);
	}

	private static void OnLauncherItemPointerPressed(
		MenuItem item,
		LauncherEntry entry,
		Action<LauncherEntry, PixelPoint> showNativeContextMenuAction,
		PointerPressedEventArgs eventArgs)
	{
		var currentPoint = eventArgs.GetCurrentPoint(null);
		if (!currentPoint.Properties.IsRightButtonPressed
			&& currentPoint.Properties.PointerUpdateKind != PointerUpdateKind.RightButtonPressed)
		{
			return;
		}

		if (!IsDirectMenuItemSource(item, eventArgs))
		{
			return;
		}

		if (!OperatingSystem.IsWindows() || !GetCursorPos(out var point))
		{
			return;
		}

		eventArgs.Handled = true;
		showNativeContextMenuAction(entry, new PixelPoint(point.X, point.Y));
	}

	#endregion

	#region Methods: Public

	public ContextMenu Build(
		LauncherSnapshot snapshot,
		Action<LauncherEntry> launchAction,
		Action<LauncherEntry, PixelPoint> showNativeContextMenuAction)
	{
		var items = new List<object>();

		if (snapshot.RootEntries.Count == 0)
		{
			items.Add(new MenuItem
			{
				Header = "No launchers were found",
				IsEnabled = false
			});
		}
		else
		{
			AddLauncherItems(items, snapshot.RootEntries, launchAction, showNativeContextMenuAction);
		}

		return new ContextMenu
		{
			ItemsSource = items
		};
	}

	#endregion
}

#endregion
