#region Class: LauncherCatalogService

namespace TreeTray.Services;

public sealed class LauncherCatalogService : ILauncherCatalogService
{
	#region Fields: Private

	private readonly IPlatformLauncherResolver _platformLauncherResolver;

	#endregion

	#region Constructors: Public

	public LauncherCatalogService(IPlatformLauncherResolver platformLauncherResolver)
	{
		_platformLauncherResolver = platformLauncherResolver;
	}

	#endregion

	#region Methods: Private

	private IReadOnlyList<LauncherEntry> BuildDirectoryEntries(string directoryPath)
	{
		var entries = new List<LauncherEntry>();

		foreach (var path in Directory.EnumerateFileSystemEntries(directoryPath))
		{
			if (ShouldIgnore(path))
			{
				continue;
			}

			if (Directory.Exists(path))
			{
				if (_platformLauncherResolver.IsLauncherPath(path))
				{
					entries.Add(CreateLauncherEntry(path));
					continue;
				}

				var childEntries = BuildDirectoryEntries(path);
				if (childEntries.Count == 0)
				{
					continue;
				}

				entries.Add(new LauncherEntry(
					LauncherEntryType.Folder,
					Path.GetFileName(path),
					path,
					children: childEntries));

				continue;
			}

			if (_platformLauncherResolver.IsLauncherPath(path))
			{
				entries.Add(CreateLauncherEntry(path));
			}
		}

		return SortEntries(entries);
	}

	private int CountLaunchers(IEnumerable<LauncherEntry> entries)
	{
		var count = 0;

		foreach (var entry in entries)
		{
			if (entry.CanLaunch)
			{
				count++;
			}

			if (entry.Children.Count > 0)
			{
				count += CountLaunchers(entry.Children);
			}
		}

		return count;
	}

	private LauncherEntry CreateLauncherEntry(string path)
	{
		return new LauncherEntry(
			LauncherEntryType.Launcher,
			_platformLauncherResolver.GetDisplayName(path),
			path,
			_platformLauncherResolver.CreateLaunchCommand(path));
	}

	private static IReadOnlyList<LauncherEntry> SortEntries(IEnumerable<LauncherEntry> entries)
	{
		return entries
			.OrderBy(entry => entry.EntryType)
			.ThenBy(entry => entry.DisplayName, StringComparer.OrdinalIgnoreCase)
			.ToArray();
	}

	private static bool ShouldIgnore(string path)
	{
		var name = Path.GetFileName(path);

		if (string.IsNullOrWhiteSpace(name))
		{
			return true;
		}

		if (name.StartsWith(".", StringComparison.Ordinal))
		{
			return true;
		}

		if (string.Equals(name, "desktop.ini", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(name, "thumbs.db", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(name, ".ds_store", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		try
		{
			var attributes = File.GetAttributes(path);
			return attributes.HasFlag(FileAttributes.Hidden) || attributes.HasFlag(FileAttributes.System);
		}
		catch
		{
			return false;
		}
	}

	#endregion

	#region Methods: Public

	public LauncherSnapshot Build(TreeTrayConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(configuration);

		Directory.CreateDirectory(configuration.LaunchersDirectory);

		var rootEntries = BuildDirectoryEntries(configuration.LaunchersDirectory);
		return new LauncherSnapshot(rootEntries, DateTimeOffset.Now, CountLaunchers(rootEntries));
	}

	#endregion
}

#endregion
