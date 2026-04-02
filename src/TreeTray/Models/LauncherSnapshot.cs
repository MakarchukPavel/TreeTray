#region Class: LauncherSnapshot

namespace TreeTray.Models;

public sealed class LauncherSnapshot
{
	#region Fields: Private

	private readonly IReadOnlyList<LauncherEntry> _rootEntries;

	#endregion

	#region Constructors: Public

	public LauncherSnapshot(
		IEnumerable<LauncherEntry>? rootEntries,
		DateTimeOffset generatedAt,
		int launcherCount)
	{
		_rootEntries = rootEntries?.ToArray() ?? Array.Empty<LauncherEntry>();
		GeneratedAt = generatedAt;
		LauncherCount = launcherCount;
	}

	#endregion

	#region Properties: Public

	public static LauncherSnapshot Empty { get; } = new(Array.Empty<LauncherEntry>(), DateTimeOffset.MinValue, 0);

	public DateTimeOffset GeneratedAt { get; }

	public int LauncherCount { get; }

	public IReadOnlyList<LauncherEntry> RootEntries => _rootEntries;

	#endregion
}

#endregion
