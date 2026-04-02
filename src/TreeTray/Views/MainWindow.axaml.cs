#region Class: MainWindow

using Avalonia.VisualTree;

namespace TreeTray.Views;

public partial class MainWindow : Window
{
	#region Fields: Private

	private readonly IApplicationController? _applicationController;

	private readonly IPlatformContextMenuService? _platformContextMenuService;

	private TreeView? _launchersTreeView;

	#endregion

	#region Constructors: Public

	public MainWindow()
	{
		InitializeComponent();
		InitializeLaunchersTreeView();
	}

	public MainWindow(
		MainWindowViewModel viewModel,
		IApplicationController applicationController,
		IPlatformContextMenuService platformContextMenuService)
		: this()
	{
		_applicationController = applicationController;
		_platformContextMenuService = platformContextMenuService;
		DataContext = viewModel;
	}

	#endregion

	#region Methods: Private

	private void ApplyLaunchersExpansionState(bool isExpanded)
	{
		if (_launchersTreeView is null)
		{
			return;
		}

		var rootItemsCount = _launchersTreeView.ItemCount;
		for (var index = 0; index < rootItemsCount; index++)
		{
			if (_launchersTreeView.ContainerFromIndex(index) is not TreeViewItem treeViewItem)
			{
				continue;
			}

			treeViewItem.IsExpanded = isExpanded;
			if (isExpanded)
			{
				_launchersTreeView.ExpandSubTree(treeViewItem);
				continue;
			}

			_launchersTreeView.CollapseSubTree(treeViewItem);
		}
	}

	private MainWindowViewModel? GetViewModel()
	{
		return DataContext as MainWindowViewModel;
	}

	private void InitializeLaunchersTreeView()
	{
		_launchersTreeView = this.FindControl<TreeView>("LaunchersTreeView");
		if (_launchersTreeView is null)
		{
			return;
		}

		_launchersTreeView.AddHandler(
			InputElement.PointerPressedEvent,
			OnLaunchersTreeViewPointerPressed,
			RoutingStrategies.Tunnel,
			handledEventsToo: true);
	}

	private static LauncherItemViewModel? ResolveLauncherItemViewModel(object? source)
	{
		if (source is not Visual visual)
		{
			return null;
		}

		var currentVisual = visual;
		while (currentVisual is not null)
		{
			if (currentVisual is StyledElement styledElement
				&& styledElement.DataContext is LauncherItemViewModel launcherItemViewModel)
			{
				return launcherItemViewModel;
			}

			currentVisual = currentVisual.GetVisualParent();
		}

		return null;
	}

	private Control? ResolveContextMenuPlacementTarget(object? source)
	{
		if (source is not Visual visual)
		{
			return _launchersTreeView;
		}

		var currentVisual = visual;
		while (currentVisual is not null)
		{
			if (currentVisual is Control control)
			{
				return control;
			}

			currentVisual = currentVisual.GetVisualParent();
		}

		return _launchersTreeView;
	}

	private LauncherItemViewModel? ResolveLauncherItemViewModel(PointerPressedEventArgs eventArgs)
	{
		var launcherItemViewModel = ResolveLauncherItemViewModel(eventArgs.Source);
		if (launcherItemViewModel is not null)
		{
			return launcherItemViewModel;
		}

		return GetViewModel()?.SelectedItem;
	}

	#endregion

	#region Methods: Public

	public void OnCollapseAllLaunchersClick(object? sender, RoutedEventArgs eventArgs)
	{
		ApplyLaunchersExpansionState(isExpanded: false);
	}

	public void OnExpandAllLaunchersClick(object? sender, RoutedEventArgs eventArgs)
	{
		ApplyLaunchersExpansionState(isExpanded: true);
	}

	public void OnLaunchersTreeViewDoubleTapped(object? sender, TappedEventArgs eventArgs)
	{
		GetViewModel()?.LaunchSelectedCommand.Execute(null);
	}

	public void OnLaunchersTreeViewKeyDown(object? sender, KeyEventArgs eventArgs)
	{
		if (eventArgs.Key != Key.Enter)
		{
			return;
		}

		GetViewModel()?.LaunchSelectedCommand.Execute(null);
		eventArgs.Handled = true;
	}

	public void OnLaunchersTreeViewPointerPressed(object? sender, PointerPressedEventArgs eventArgs)
	{
		if (eventArgs.Handled && sender is not TreeView)
		{
			return;
		}

		var currentPoint = eventArgs.GetCurrentPoint(this);
		if (!currentPoint.Properties.IsRightButtonPressed
			&& currentPoint.Properties.PointerUpdateKind != PointerUpdateKind.RightButtonPressed)
		{
			return;
		}

		if (_platformContextMenuService?.SupportsContextMenus != true)
		{
			return;
		}

		var launcherItemViewModel = ResolveLauncherItemViewModel(eventArgs);
		if (launcherItemViewModel is null)
		{
			return;
		}

		GetViewModel()!.SelectedItem = launcherItemViewModel;
		var screenPoint = this.PointToScreen(eventArgs.GetPosition(this));
		var placementTarget = ResolveContextMenuPlacementTarget(eventArgs.Source);
		if (_platformContextMenuService.ShowContextMenu(launcherItemViewModel.SourcePath, screenPoint, placementTarget))
		{
			eventArgs.Handled = true;
		}
	}

	public void OnWindowClosing(object? sender, WindowClosingEventArgs eventArgs)
	{
		if (_applicationController?.ShouldHideMainWindowOnClose != true
			|| _applicationController.IsShuttingDown)
		{
			return;
		}

		eventArgs.Cancel = true;
		ShowInTaskbar = false;
		Hide();
	}

	#endregion
}

#endregion
