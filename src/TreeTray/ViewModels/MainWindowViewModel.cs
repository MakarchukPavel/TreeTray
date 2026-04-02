#region Class: MainWindowViewModel

namespace TreeTray.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
	#region Fields: Private

	private readonly IApplicationController _applicationController;

	private readonly IIconService _iconService;

	private string _configurationFilePath = string.Empty;

	private bool _isSynchronizingState;

	private bool _isTaskbarDockIconEnabled;

	private bool _isTrayIconEnabled;

	private bool _isLoading;

	private bool _invertTrayIconMouseButtons;

	private string _launchersDirectory = string.Empty;

	private string _headerTitle = "TreeTray";

	private LauncherItemViewModel? _selectedItem;

	private string _selectedItemKind = "Launcher";

	private string _selectedItemName = "Nothing selected";

	private string _selectedItemPath = "Choose a folder or launcher from the tree to inspect it here.";

	private bool _startWithOperatingSystem;

	private string _statusText = "Loading launchers...";

	private string _trayIconBackgroundColor = string.Empty;

	private Avalonia.Media.Color _trayIconBackgroundColorValue = Avalonia.Media.Color.Parse("#2F6FED");

	private string _trayIconForegroundColor = string.Empty;

	private Avalonia.Media.Color _trayIconForegroundColorValue = Avalonia.Media.Color.Parse("#FFFFFF");

	private string _trayIconGlyph = string.Empty;

	private string _trayToolTipText = string.Empty;

	private string _windowTitle = "TreeTray";

	#endregion

	#region Constructors: Public

	public MainWindowViewModel(IApplicationController applicationController, IIconService iconService)
	{
		_applicationController = applicationController;
		_iconService = iconService;
		RootItems = new ObservableCollection<LauncherItemViewModel>();

		ReloadCommand = new RelayCommand(_applicationController.Reload, () => !IsLoading);
		LaunchSelectedCommand = new RelayCommand(LaunchSelected, () => !IsLoading && SelectedItem?.CanLaunch == true);
		OpenConfigurationFolderCommand = new RelayCommand(_applicationController.OpenConfigurationDirectory);
		OpenLaunchersFolderCommand = new RelayCommand(_applicationController.OpenLaunchersDirectory);
		ExitCommand = new RelayCommand(_applicationController.Exit);

		_applicationController.StateChanged += OnApplicationStateChanged;
		RefreshState();
	}

	#endregion

	#region Properties: Public

	public string ConfigurationFilePath
	{
		get => _configurationFilePath;
		private set => SetProperty(ref _configurationFilePath, value);
	}

	public RelayCommand ExitCommand { get; }

	public string HeaderTitle
	{
		get => _headerTitle;
		private set => SetProperty(ref _headerTitle, value);
	}

	public bool HasEntries => RootItems.Count > 0;

	public bool HasNoEntries => !HasEntries;

	public bool IsTaskbarDockIconEnabled
	{
		get => _isTaskbarDockIconEnabled;
		set
		{
			if (!SetProperty(ref _isTaskbarDockIconEnabled, value))
			{
				return;
			}

			PersistRuntimeSettings();
		}
	}

	public bool IsLoading
	{
		get => _isLoading;
		private set => SetProperty(ref _isLoading, value);
	}

	public bool InvertTrayIconMouseButtons
	{
		get => _invertTrayIconMouseButtons;
		set
		{
			if (!SetProperty(ref _invertTrayIconMouseButtons, value))
			{
				return;
			}

			PersistRuntimeSettings();
		}
	}

	public bool SupportsTrayIconMouseButtonInversion => OperatingSystem.IsWindows() || OperatingSystem.IsMacOS();

	public bool IsTrayIconEnabled
	{
		get => _isTrayIconEnabled;
		set
		{
			if (!SetProperty(ref _isTrayIconEnabled, value))
			{
				return;
			}

			PersistRuntimeSettings();
		}
	}

	public RelayCommand LaunchSelectedCommand { get; }

	public string LaunchersDirectory
	{
		get => _launchersDirectory;
		private set => SetProperty(ref _launchersDirectory, value);
	}

	public RelayCommand OpenConfigurationFolderCommand { get; }

	public RelayCommand OpenLaunchersFolderCommand { get; }

	public RelayCommand ReloadCommand { get; }

	public ObservableCollection<LauncherItemViewModel> RootItems { get; }

	public LauncherItemViewModel? SelectedItem
	{
		get => _selectedItem;
		set
		{
			if (!SetProperty(ref _selectedItem, value))
			{
				return;
			}

			SelectedItemName = value?.DisplayName ?? "Nothing selected";
			SelectedItemPath = value?.SourcePath ?? "Choose a folder or launcher from the tree to inspect it here.";
			SelectedItemKind = value?.EntryKindText ?? "Launcher";
			LaunchSelectedCommand.NotifyCanExecuteChanged();
		}
	}

	public string SelectedItemKind
	{
		get => _selectedItemKind;
		private set => SetProperty(ref _selectedItemKind, value);
	}

	public string SelectedItemName
	{
		get => _selectedItemName;
		private set => SetProperty(ref _selectedItemName, value);
	}

	public string SelectedItemPath
	{
		get => _selectedItemPath;
		private set => SetProperty(ref _selectedItemPath, value);
	}

	public bool StartWithOperatingSystem
	{
		get => _startWithOperatingSystem;
		set
		{
			if (!SetProperty(ref _startWithOperatingSystem, value))
			{
				return;
			}

			PersistRuntimeSettings();
		}
	}

	public string StatusText
	{
		get => _statusText;
		private set => SetProperty(ref _statusText, value);
	}

	public string TrayIconBackgroundColor
	{
		get => _trayIconBackgroundColor;
		set => SetTrayIconBackgroundColor(value, persistChanges: true);
	}

	public string TrayIconForegroundColor
	{
		get => _trayIconForegroundColor;
		set => SetTrayIconForegroundColor(value, persistChanges: true);
	}

	public string TrayIconGlyph
	{
		get => _trayIconGlyph;
		set
		{
			if (!SetProperty(ref _trayIconGlyph, value))
			{
				return;
			}

			PersistRuntimeSettings();
		}
	}

	public Avalonia.Media.Color TrayIconBackgroundColorValue
	{
		get => _trayIconBackgroundColorValue;
		set
		{
			if (!SetProperty(ref _trayIconBackgroundColorValue, value))
			{
				return;
			}

			SetTrayIconBackgroundColor(FormatColor(value), persistChanges: true);
		}
	}

	public Avalonia.Media.Color TrayIconForegroundColorValue
	{
		get => _trayIconForegroundColorValue;
		set
		{
			if (!SetProperty(ref _trayIconForegroundColorValue, value))
			{
				return;
			}

			SetTrayIconForegroundColor(FormatColor(value), persistChanges: true);
		}
	}

	public string TrayToolTipText
	{
		get => _trayToolTipText;
		set
		{
			if (!SetProperty(ref _trayToolTipText, value))
			{
				return;
			}

			UpdateTitles();
			PersistRuntimeSettings();
		}
	}

	public string WindowTitle
	{
		get => _windowTitle;
		private set => SetProperty(ref _windowTitle, value);
	}

	#endregion

	#region Methods: Private

	private void LaunchSelected()
	{
		if (SelectedItem?.CanLaunch == true)
		{
			_applicationController.Launch(SelectedItem.Model);
		}
	}

	private static string BuildWindowTitle(string displayTitle)
	{
		return displayTitle;
	}

	private static string NormalizeTitle(string? trayToolTipText)
	{
		return string.IsNullOrWhiteSpace(trayToolTipText)
			? "TreeTray"
			: trayToolTipText.Trim();
	}

	private static string FormatColor(Avalonia.Media.Color color)
	{
		return string.Format(
			CultureInfo.InvariantCulture,
			"#{0:X2}{1:X2}{2:X2}",
			color.R,
			color.G,
			color.B);
	}

	private static bool TryParseColor(string? rawColor, out Avalonia.Media.Color color)
	{
		if (Avalonia.Media.Color.TryParse(rawColor, out color))
		{
			return true;
		}

		if (Avalonia.Media.Color.TryParse("#000000", out color))
		{
			return false;
		}

		color = default;
		return false;
	}

	private void OnApplicationStateChanged(object? sender, EventArgs eventArgs)
	{
		Dispatcher.UIThread.Post(RefreshState);
	}

	private void PersistRuntimeSettings()
	{
		if (_isSynchronizingState)
		{
			return;
		}

		_applicationController.UpdateConfiguration(new TreeTrayConfiguration
		{
			EnableTaskbarDockIcon = !IsTrayIconEnabled,
			EnableTrayIcon = IsTrayIconEnabled,
			InvertTrayIconMouseButtons = InvertTrayIconMouseButtons,
			LaunchersDirectory = _applicationController.Configuration.LaunchersDirectory,
			StartWithOperatingSystem = StartWithOperatingSystem,
			TrayIconBackgroundColor = TrayIconBackgroundColor,
			TrayIconForegroundColor = TrayIconForegroundColor,
			TrayIconGlyph = TrayIconGlyph,
			TrayToolTipText = TrayToolTipText
		});
	}

	private void UpdateTitles()
	{
		var displayTitle = NormalizeTitle(TrayToolTipText);
		HeaderTitle = displayTitle;
		WindowTitle = BuildWindowTitle(displayTitle);
	}

	private void RefreshState()
	{
		_isSynchronizingState = true;

		try
		{
			ConfigurationFilePath = _applicationController.ConfigurationFilePath;
			IsLoading = _applicationController.IsLoading;
			LaunchersDirectory = _applicationController.Configuration.LaunchersDirectory;
			IsTrayIconEnabled = _applicationController.Configuration.EnableTrayIcon;
			IsTaskbarDockIconEnabled = _applicationController.Configuration.EnableTaskbarDockIcon;
			InvertTrayIconMouseButtons = _applicationController.Configuration.InvertTrayIconMouseButtons;
			StartWithOperatingSystem = _applicationController.Configuration.StartWithOperatingSystem;
			TrayIconGlyph = _applicationController.Configuration.TrayIconGlyph;
			TrayIconForegroundColor = _applicationController.Configuration.TrayIconForegroundColor;
			TrayIconBackgroundColor = _applicationController.Configuration.TrayIconBackgroundColor;
			TrayToolTipText = _applicationController.Configuration.TrayToolTipText;
			UpdateTitles();

			RootItems.Clear();
			foreach (var entry in _applicationController.Snapshot.RootEntries)
			{
				RootItems.Add(new LauncherItemViewModel(entry, _iconService));
			}

			StatusText = IsLoading
				? "Loading launchers..."
				: _applicationController.Snapshot.LauncherCount == 0
				? "No launchers were discovered in the configured directory."
				: string.Format(
					CultureInfo.CurrentCulture,
					"{0} launchers indexed at {1:t}.",
					_applicationController.Snapshot.LauncherCount,
					_applicationController.Snapshot.GeneratedAt.LocalDateTime);

			SelectedItem = RootItems.FirstOrDefault();
			ReloadCommand.NotifyCanExecuteChanged();
			LaunchSelectedCommand.NotifyCanExecuteChanged();
			OnPropertyChanged(nameof(HasEntries));
			OnPropertyChanged(nameof(HasNoEntries));
		}
		finally
		{
			_isSynchronizingState = false;
		}
	}

	private void SetTrayIconBackgroundColor(string? value, bool persistChanges)
	{
		var normalizedValue = value?.Trim() ?? string.Empty;
		if (!SetProperty(ref _trayIconBackgroundColor, normalizedValue, nameof(TrayIconBackgroundColor)))
		{
			return;
		}

		if (TryParseColor(normalizedValue, out var parsedColor))
		{
			SetProperty(ref _trayIconBackgroundColorValue, parsedColor, nameof(TrayIconBackgroundColorValue));
		}

		if (persistChanges)
		{
			PersistRuntimeSettings();
		}
	}

	private void SetTrayIconForegroundColor(string? value, bool persistChanges)
	{
		var normalizedValue = value?.Trim() ?? string.Empty;
		if (!SetProperty(ref _trayIconForegroundColor, normalizedValue, nameof(TrayIconForegroundColor)))
		{
			return;
		}

		if (TryParseColor(normalizedValue, out var parsedColor))
		{
			SetProperty(ref _trayIconForegroundColorValue, parsedColor, nameof(TrayIconForegroundColorValue));
		}

		if (persistChanges)
		{
			PersistRuntimeSettings();
		}
	}

	#endregion
}

#endregion
