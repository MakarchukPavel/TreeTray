#region Class: LauncherEntryLaunchBatchResolver

namespace TreeTray.Models;

public static class LauncherEntryLaunchBatchResolver
{
	#region Methods: Public

	public static IReadOnlyList<LauncherEntry> GetDirectLaunchableChildren(LauncherEntry entry)
	{
		ArgumentNullException.ThrowIfNull(entry);

		if (entry.Children.Count == 0)
		{
			return Array.Empty<LauncherEntry>();
		}

		return entry.Children
			.Where(child => child.CanLaunch)
			.ToArray();
	}

	#endregion
}

#endregion
