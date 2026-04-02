#region Class: LauncherEntry

namespace TreeTray.Models;

public sealed class LauncherEntry
{
	#region Fields: Private

	private readonly IReadOnlyList<LauncherEntry> _children;

	#endregion

	#region Constructors: Public

	public LauncherEntry(
		LauncherEntryType entryType,
		string displayName,
		string sourcePath,
		LaunchCommand? command = null,
		IEnumerable<LauncherEntry>? children = null)
	{
		ArgumentNullException.ThrowIfNull(displayName);
		ArgumentNullException.ThrowIfNull(sourcePath);

		EntryType = entryType;
		DisplayName = displayName;
		SourcePath = sourcePath;
		Command = command;
		_children = children?.ToArray() ?? Array.Empty<LauncherEntry>();
	}

	#endregion

	#region Properties: Public

	public bool CanLaunch => Command is not null;

	public IReadOnlyList<LauncherEntry> Children => _children;

	public LaunchCommand? Command { get; }

	public string DisplayName { get; }

	public LauncherEntryType EntryType { get; }

	public string SourcePath { get; }

	#endregion
}

#endregion
