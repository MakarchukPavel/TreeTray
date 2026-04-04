#region Class: ApplicationController

namespace TreeTray.Services;

public sealed class ApplicationController : IApplicationController
{
	#region Fields: Private

	private readonly IConfigurationService _configurationService;

	private readonly IApplicationPaths _applicationPaths;

	private readonly IIconService _iconService;

	private readonly ILauncherCatalogService _launcherCatalogService;

	private readonly ILauncherExecutionService _launcherExecutionService;

	private readonly IMacOsStatusItemService _macOsStatusItemService;

	private readonly IServiceProvider _serviceProvider;

	private readonly IStartupRegistrationService _startupRegistrationService;

	private readonly ITrayAppearanceService _trayAppearanceService;

	private readonly ITrayMenuBuilder _trayMenuBuilder;

	private readonly IUserNotificationService _userNotificationService;

	private readonly IWindowsTrayIconService _windowsTrayIconService;

	private IClassicDesktopStyleApplicationLifetime? _desktopLifetime;

	private bool _isLoading;

	private bool _isShuttingDown;

	private int _loadVersion;

	private Views.MainWindow? _mainWindow;

	private TrayIcon? _trayIcon;

	#endregion

	#region Constructors: Public

	public ApplicationController(
		IConfigurationService configurationService,
		IApplicationPaths applicationPaths,
		IIconService iconService,
		ILauncherCatalogService launcherCatalogService,
		ILauncherExecutionService launcherExecutionService,
		IMacOsStatusItemService macOsStatusItemService,
		IServiceProvider serviceProvider,
		IStartupRegistrationService startupRegistrationService,
		ITrayAppearanceService trayAppearanceService,
		ITrayMenuBuilder trayMenuBuilder,
		IUserNotificationService userNotificationService,
		IWindowsTrayIconService windowsTrayIconService)
	{
		_configurationService = configurationService;
		_applicationPaths = applicationPaths;
		_iconService = iconService;
		_launcherCatalogService = launcherCatalogService;
		_launcherExecutionService = launcherExecutionService;
		_macOsStatusItemService = macOsStatusItemService;
		_serviceProvider = serviceProvider;
		_startupRegistrationService = startupRegistrationService;
		_trayAppearanceService = trayAppearanceService;
		_trayMenuBuilder = trayMenuBuilder;
		_userNotificationService = userNotificationService;
		_windowsTrayIconService = windowsTrayIconService;
	}

	#endregion

	#region Properties: Private

	private bool ShouldOpenMainWindowOnStartup => !Configuration.EnableTrayIcon;

	#endregion

	#region Properties: Public

	public TreeTrayConfiguration Configuration { get; private set; } = new();

	public string ConfigurationFilePath => _applicationPaths.ConfigurationFilePath;

	public bool IsLoading => _isLoading;

	public bool IsShuttingDown => _isShuttingDown;

	public LauncherSnapshot Snapshot { get; private set; } = LauncherSnapshot.Empty;

	public bool ShouldHideMainWindowOnClose => Configuration.EnableTrayIcon;

	#endregion

	#region Events: Public

	public event EventHandler? StateChanged;

	#endregion

	#region Methods: Private

	private static TreeTrayConfiguration CloneConfiguration(TreeTrayConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(configuration);

		return new TreeTrayConfiguration
		{
			EnableTaskbarDockIcon = configuration.EnableTaskbarDockIcon,
			EnableTrayIcon = configuration.EnableTrayIcon,
			InvertTrayIconMouseButtons = configuration.InvertTrayIconMouseButtons,
			LaunchersDirectory = configuration.LaunchersDirectory,
			StartWithOperatingSystem = configuration.StartWithOperatingSystem,
			TrayIconBackgroundColor = configuration.TrayIconBackgroundColor,
			TrayIconForegroundColor = configuration.TrayIconForegroundColor,
			TrayIconGlyph = configuration.TrayIconGlyph,
			TrayToolTipText = configuration.TrayToolTipText
		};
	}

	private void AttachMainWindowToLifetime()
	{
		if (_mainWindow is null || _desktopLifetime is null)
		{
			return;
		}

		if (!ReferenceEquals(_desktopLifetime.MainWindow, _mainWindow))
		{
			_desktopLifetime.MainWindow = _mainWindow;
		}
	}

	private void EnsureMainWindow(bool attachToLifetime = false)
	{
		if (_mainWindow is not null)
		{
			_mainWindow.ShowInTaskbar = _mainWindow.IsVisible || !Configuration.EnableTrayIcon;
			if (attachToLifetime)
			{
				AttachMainWindowToLifetime();
			}

			return;
		}

		_mainWindow = _serviceProvider.GetRequiredService<Views.MainWindow>();
		_mainWindow.Icon = _iconService.ApplicationIcon;
		_mainWindow.ShowInTaskbar = !Configuration.EnableTrayIcon;
		_mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

		if (attachToLifetime)
		{
			AttachMainWindowToLifetime();
		}
	}

	private void BeginSnapshotLoad()
	{
		var loadVersion = Interlocked.Increment(ref _loadVersion);
		_isLoading = true;
		RefreshTrayPresentation();
		EnsureMainWindow();
		RaiseStateChanged();
		_ = LoadSnapshotAsync(loadVersion);
	}

	private void EnsureTrayIcon()
	{
		if (OperatingSystem.IsWindows())
		{
			_windowsTrayIconService.Apply(
				Configuration,
				Snapshot,
				_isLoading,
				Launch,
				ShowMainWindow);
			return;
		}

		if (OperatingSystem.IsMacOS())
		{
			_macOsStatusItemService.Apply(
				Configuration,
				Snapshot,
				_isLoading,
				Launch,
				ShowMainWindow);
			return;
		}

		if (_trayIcon is null)
		{
			_trayIcon = new TrayIcon();
			_trayIcon.Clicked += OnTrayIconClicked;
		}

		_trayIcon.Icon = _trayAppearanceService.GetTrayIcon(Configuration);
		_trayIcon.IsVisible = true;
		_trayIcon.ToolTipText = _trayAppearanceService.GetToolTipText(Configuration);
		_trayIcon.Menu = _trayMenuBuilder.Build(
			Snapshot,
			_isLoading,
			Launch);
	}

	private async Task LoadSnapshotAsync(int loadVersion)
	{
		LauncherSnapshot snapshot;

		try
		{
			snapshot = await Task.Run(() =>
			{
				var builtSnapshot = _launcherCatalogService.Build(Configuration);
				_iconService.PreloadEntryIcons(builtSnapshot.RootEntries);
				return builtSnapshot;
			});
		}
		catch
		{
			snapshot = Snapshot;
		}

		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			if (loadVersion != _loadVersion || _isShuttingDown)
			{
				return;
			}

			Snapshot = snapshot;
			_isLoading = false;
			RefreshTrayPresentation();
			RaiseStateChanged();
		});
	}

	private void LoadState()
	{
		Configuration = _configurationService.Load();
		_startupRegistrationService.Apply(Configuration);
	}

	private void ShowStartupMainWindow()
	{
		EnsureMainWindow(attachToLifetime: true);

		if (_mainWindow is null)
		{
			return;
		}

		_mainWindow.ShowInTaskbar = true;

		if (!_mainWindow.IsVisible)
		{
			_mainWindow.Show();
		}

		if (Configuration.EnableTrayIcon)
		{
			_mainWindow.WindowState = WindowState.Minimized;
			return;
		}

		_mainWindow.WindowState = WindowState.Normal;
		_mainWindow.Activate();
	}

	private void ScheduleMainWindowDisplay()
	{
		Dispatcher.UIThread.Post(
			() =>
			{
				if (_isShuttingDown || !ShouldOpenMainWindowOnStartup)
				{
					return;
				}

				ShowStartupMainWindow();
			},
			DispatcherPriority.Background);
	}

	private void OnTrayIconClicked(object? sender, EventArgs eventArgs)
	{
		ShowMainWindow();
	}

	private void RaiseStateChanged()
	{
		StateChanged?.Invoke(this, EventArgs.Empty);
	}

	private void RefreshTrayPresentation()
	{
		if (Configuration.EnableTrayIcon)
		{
			EnsureTrayIcon();
			return;
		}

		RemoveTrayIcon();
	}

	private void RemoveTrayIcon()
	{
		if (OperatingSystem.IsWindows())
		{
			_windowsTrayIconService.Remove();
			return;
		}

		if (OperatingSystem.IsMacOS())
		{
			_macOsStatusItemService.Remove();
			return;
		}

		if (_trayIcon is null)
		{
			return;
		}

		_trayIcon.Clicked -= OnTrayIconClicked;
		_trayIcon.Dispose();
		_trayIcon = null;
	}

	private void SyncShell(bool initialStartup)
	{
		if (ShouldOpenMainWindowOnStartup)
		{
			EnsureMainWindow(attachToLifetime: initialStartup);
		}
		else if (_mainWindow is not null)
		{
			_mainWindow.ShowInTaskbar = false;
		}

		if (Configuration.EnableTrayIcon)
		{
			EnsureTrayIcon();
		}
		else
		{
			RemoveTrayIcon();
		}

		_desktopLifetime!.ShutdownMode = Configuration.EnableTrayIcon
			? ShutdownMode.OnExplicitShutdown
			: ShutdownMode.OnMainWindowClose;

		if (initialStartup && ShouldOpenMainWindowOnStartup)
		{
			ScheduleMainWindowDisplay();
		}

		if (!initialStartup && ShouldOpenMainWindowOnStartup && _mainWindow is not null && !_mainWindow.IsVisible)
		{
			ShowMainWindow();
		}
	}

	#endregion

	#region Methods: Public

	public void Exit()
	{
		_isShuttingDown = true;
		RemoveTrayIcon();
		_desktopLifetime?.Shutdown();
	}

	public void Launch(LauncherEntry entry)
	{
		try
		{
			_launcherExecutionService.Launch(entry);
		}
		catch (LauncherExecutionException exception)
		{
			_userNotificationService.ShowError("Launch failed", exception.UserMessage);
		}
	}

	public void OpenConfigurationDirectory()
	{
		_launcherExecutionService.OpenDirectory(_applicationPaths.ConfigurationDirectory);
	}

	public void OpenLaunchersDirectory()
	{
		_launcherExecutionService.OpenDirectory(Configuration.LaunchersDirectory);
	}

	public void Reload()
	{
		LoadState();
		SyncShell(initialStartup: false);
		BeginSnapshotLoad();
	}

	public void ShowMainWindow()
	{
		EnsureMainWindow(attachToLifetime: true);

		if (_mainWindow is null)
		{
			return;
		}

		if (!_mainWindow.IsVisible)
		{
			_mainWindow.Show();
		}

		_mainWindow.ShowInTaskbar = true;
		_mainWindow.WindowState = WindowState.Normal;
		_mainWindow.Activate();
	}

	public void Start(Application application)
	{
		_desktopLifetime = application.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime
			?? throw new NotSupportedException("TreeTray requires the classic desktop lifetime.");

		LoadState();

		SyncShell(initialStartup: true);
		BeginSnapshotLoad();
	}

	public void UpdateConfiguration(TreeTrayConfiguration configuration)
	{
		Configuration = CloneConfiguration(configuration);
		_configurationService.Save(Configuration);
		_startupRegistrationService.Apply(Configuration);
		SyncShell(initialStartup: false);
		RaiseStateChanged();
	}

	#endregion
}

#endregion
