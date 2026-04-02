#region Class: TrayMenuBuilder

namespace TreeTray.Services;

public sealed class TrayMenuBuilder : ITrayMenuBuilder
{
	#region Fields: Private

	private readonly IIconService _iconService;

	#endregion

	#region Constructors: Public

	public TrayMenuBuilder(IIconService iconService)
	{
		_iconService = iconService;
	}

	#endregion

	#region Methods: Private

	private void AddLauncherEntries(NativeMenu menu, IEnumerable<LauncherEntry> entries, Action<LauncherEntry> launchAction)
	{
		foreach (var entry in entries)
		{
			if (entry.Children.Count > 0)
			{
				var subMenu = new NativeMenu();
				AddLauncherEntries(subMenu, entry.Children, launchAction);

				menu.Add(new NativeMenuItem(entry.DisplayName)
				{
					Icon = _iconService.GetEntryIcon(entry),
					Menu = subMenu
				});

				continue;
			}

			var menuItem = new NativeMenuItem(entry.DisplayName)
			{
				Icon = _iconService.GetEntryIcon(entry)
			};
			menuItem.Click += (_, _) => launchAction(entry);
			menu.Add(menuItem);
		}
	}

	public NativeMenu Build(
		LauncherSnapshot snapshot,
		bool isLoading,
		Action<LauncherEntry> launchAction)
	{
		var menu = new NativeMenu();

		if (isLoading)
		{
			menu.Add(new NativeMenuItem("Loading launchers...")
			{
				IsEnabled = false
			});
		}
		else if (snapshot.RootEntries.Count == 0)
		{
			menu.Add(new NativeMenuItem("No launchers were found")
			{
				IsEnabled = false
			});
		}
		else
		{
			AddLauncherEntries(menu, snapshot.RootEntries, launchAction);
		}

		return menu;
	}

	#endregion
}

#endregion
