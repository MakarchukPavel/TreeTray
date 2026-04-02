#region Class: LauncherItemViewModel

namespace TreeTray.ViewModels;

public sealed class LauncherItemViewModel : ViewModelBase
{
	#region Fields: Private

	private readonly IReadOnlyList<LauncherItemViewModel> _children;

	private readonly LauncherEntry _entry;

	#endregion

	#region Constructors: Public

	public LauncherItemViewModel(LauncherEntry entry, IIconService iconService)
	{
		_entry = entry ?? throw new ArgumentNullException(nameof(entry));
		ArgumentNullException.ThrowIfNull(iconService);

		_children = entry.Children
			.Select(childEntry => new LauncherItemViewModel(childEntry, iconService))
			.ToArray();
		Icon = iconService.GetEntryIcon(entry);
	}

	#endregion

	#region Properties: Public

	public bool CanLaunch => _entry.CanLaunch;

	public IReadOnlyList<LauncherItemViewModel> Children => _children;

	public string DisplayName => _entry.DisplayName;

	public string EntryKindText => CanLaunch ? "Launcher" : "Folder";

	public Bitmap Icon { get; }

	public LauncherEntry Model => _entry;

	public string SourcePath => _entry.SourcePath;

	#endregion
}

#endregion
