#region Class: WindowsTrayIconService

namespace TreeTray.Services;

public sealed class WindowsTrayIconService : IWindowsTrayIconService
{
	#region Delegates: Private

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	private delegate IntPtr WindowProcedure(IntPtr windowHandle, uint message, IntPtr wParam, IntPtr lParam);

	#endregion

	#region Struct: PrivateMessage

	[StructLayout(LayoutKind.Sequential)]
	private struct PrivateMessage
	{
		#region Fields: Public

		public IntPtr WindowHandle;

		public uint Message;

		public IntPtr WParam;

		public IntPtr LParam;

		public uint Time;

		public PrivatePoint Point;

		public uint PrivateData;

		#endregion
	}

	#endregion

	#region Struct: PrivateNotifyIconData

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct PrivateNotifyIconData
	{
		#region Fields: Public

		public uint Size;

		public IntPtr WindowHandle;

		public uint IconId;

		public uint Flags;

		public uint CallbackMessage;

		public IntPtr IconHandle;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string ToolTipText;

		public uint State;

		public uint StateMask;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string BalloonText;

		public uint VersionOrTimeout;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string BalloonTitle;

		public uint BalloonFlags;

		public Guid GuidItem;

		public IntPtr BalloonIconHandle;

		#endregion
	}

	#endregion

	#region Struct: PrivatePoint

	[StructLayout(LayoutKind.Sequential)]
	private struct PrivatePoint
	{
		#region Fields: Public

		public int X;

		public int Y;

		#endregion
	}

	#endregion

	#region Struct: PrivateWindowClass

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct PrivateWindowClass
	{
		#region Fields: Public

		public uint Size;

		public uint Style;

		public IntPtr WindowProcedurePointer;

		public int ClassExtraBytes;

		public int WindowExtraBytes;

		public IntPtr InstanceHandle;

		public IntPtr IconHandle;

		public IntPtr CursorHandle;

		public IntPtr BackgroundBrushHandle;

		public string? MenuName;

		public string ClassName;

		public IntPtr SmallIconHandle;

		#endregion
	}

	#endregion

	#region Constants: Private

	private const string WindowClassName = "TreeTray.WindowsTrayIconHost";

	private const uint WmApp = 0x8000;

	private const uint CallbackMessageId = WmApp + 1;

	private const uint NifIcon = 0x00000002;

	private const uint NifMessage = 0x00000001;

	private const uint NifTip = 0x00000004;

	private const uint NifShowTip = 0x00000080;

	private const uint NimAdd = 0x00000000;

	private const uint NimDelete = 0x00000002;

	private const uint NimModify = 0x00000001;

	private const uint NimSetVersion = 0x00000004;

	private const uint NotifyIconVersion4 = 4;

	private const uint WorkerUpdateMessage = WmApp + 2;

	private const uint WorkerDisposeMessage = WmApp + 3;

	private const int NinKeySelect = 0x0401;

	private const int NinSelect = 0x0400;

	private const int WmContextMenu = 0x007B;

	private const int WmDestroy = 0x0002;

	private const int WmLButtonUp = 0x0202;

	private const int WmRButtonUp = 0x0205;

	private const long DuplicateTrayActionThresholdMilliseconds = 250;

	#endregion

	#region Fields: Private

	private readonly object _syncRoot = new();

	private TreeTrayConfiguration _configuration = new();

	private readonly ITrayAppearanceService _trayAppearanceService;

	private readonly ITrayPopupMenuService _trayPopupMenuService;

	private readonly WindowProcedure _windowProcedure;

	private bool _hasNotifyIcon;

	private bool _isLoading;

	private Action<LauncherEntry>? _launchAction;

	private Thread? _messageLoopThread;

	private Action? _openLauncherAction;

	private PixelPoint? _pendingMenuScreenPosition;

	private string? _lastTrayActionKey;

	private long _lastTrayActionTimestamp;

	private LauncherSnapshot _snapshot = LauncherSnapshot.Empty;

	private uint _taskbarCreatedMessage;

	private ManualResetEventSlim? _windowReadyEvent;

	private IntPtr _windowHandle;

	#endregion

	#region Constructors: Public

	public WindowsTrayIconService(ITrayAppearanceService trayAppearanceService, ITrayPopupMenuService trayPopupMenuService)
	{
		_trayAppearanceService = trayAppearanceService;
		_trayPopupMenuService = trayPopupMenuService;
		_windowProcedure = OnWindowProcedure;
	}

	#endregion

	#region Methods: Private

	private void AddOrUpdateNotifyIcon(bool forceAdd)
	{
		if (_windowHandle == IntPtr.Zero)
		{
			return;
		}

		var iconHandle = _trayAppearanceService.CreateWindowsTrayIconHandle(_configuration);

		try
		{
			var notifyIconData = new PrivateNotifyIconData
			{
				Size = (uint)Marshal.SizeOf<PrivateNotifyIconData>(),
				WindowHandle = _windowHandle,
				IconId = 1,
				Flags = NifMessage | NifIcon | NifTip | NifShowTip,
				CallbackMessage = CallbackMessageId,
				IconHandle = iconHandle,
				ToolTipText = GetWindowsToolTipText()
			};

			var operation = _hasNotifyIcon && !forceAdd ? NimModify : NimAdd;
			_hasNotifyIcon = ShellNotifyIcon(operation, ref notifyIconData);

			if (_hasNotifyIcon && operation == NimAdd)
			{
				notifyIconData.VersionOrTimeout = NotifyIconVersion4;
				ShellNotifyIcon(NimSetVersion, ref notifyIconData);
			}
		}
		finally
		{
			if (iconHandle != IntPtr.Zero)
			{
				DestroyIcon(iconHandle);
			}
		}
	}

	private IntPtr CreateWorkerWindow()
	{
		_taskbarCreatedMessage = RegisterWindowMessage("TaskbarCreated");

		var instanceHandle = GetModuleHandle(null);
		var windowClass = new PrivateWindowClass
		{
			Size = (uint)Marshal.SizeOf<PrivateWindowClass>(),
			WindowProcedurePointer = Marshal.GetFunctionPointerForDelegate(_windowProcedure),
			InstanceHandle = instanceHandle,
			ClassName = WindowClassName
		};

		RegisterClassEx(ref windowClass);

		return CreateWindowEx(
			0,
			WindowClassName,
			WindowClassName,
			0,
			0,
			0,
			0,
			0,
			IntPtr.Zero,
			IntPtr.Zero,
			instanceHandle,
			IntPtr.Zero);
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private void EnsureMessageLoopThread()
	{
		lock (_syncRoot)
		{
			if (_messageLoopThread is { IsAlive: true } && _windowHandle != IntPtr.Zero)
			{
				return;
			}

			_windowReadyEvent = new ManualResetEventSlim(false);
			_messageLoopThread = new Thread(MessageLoopThreadMain)
			{
				Name = "TreeTray Win32 Tray Thread",
				IsBackground = true
			};
			_messageLoopThread.SetApartmentState(ApartmentState.STA);
			_messageLoopThread.Start();
		}

		if (_windowReadyEvent is null || !_windowReadyEvent.Wait(TimeSpan.FromSeconds(5)))
		{
			throw new TimeoutException("The Win32 tray thread did not initialize in time.");
		}

		if (_windowHandle == IntPtr.Zero)
		{
			throw new InvalidOperationException("The Win32 tray host window was not created.");
		}
	}

	private static int GetHighWord(IntPtr value)
	{
		return (short)(((long)value >> 16) & 0xFFFF);
	}

	private static int GetLowWord(IntPtr value)
	{
		return (short)((long)value & 0xFFFF);
	}

	private void HandleCallbackMessage(IntPtr wParam, IntPtr lParam)
	{
		var eventCode = GetLowWord(lParam);
		var anchorPoint = new PixelPoint(GetLowWord(wParam), GetHighWord(wParam));

		switch (eventCode)
		{
			case WmLButtonUp:
			case NinSelect:
			case NinKeySelect:
				HandlePrimaryTrayClick(anchorPoint);
				break;
			case WmContextMenu:
			case WmRButtonUp:
				HandleSecondaryTrayClick(anchorPoint);
				break;
		}
	}

	private void HandlePrimaryTrayClick(PixelPoint anchorPoint)
	{
		if (_configuration.InvertTrayIconMouseButtons)
		{
			PostTrayAction(
				"OpenLauncherWindow",
				() => OpenLauncherWindow(anchorPoint));
			return;
		}

		PostTrayAction(
			"ShowTrayPopupMenu",
			() => ShowTrayPopupMenu(anchorPoint));
	}

	private void HandleSecondaryTrayClick(PixelPoint anchorPoint)
	{
		if (_configuration.InvertTrayIconMouseButtons)
		{
			PostTrayAction(
				"ShowTrayPopupMenu",
				() => ShowTrayPopupMenu(anchorPoint));
			return;
		}

		PostTrayAction(
			"OpenLauncherWindow",
			() => OpenLauncherWindow(anchorPoint));
	}

	private void MessageLoopThreadMain()
	{
		try
		{
			_windowHandle = CreateWorkerWindow();
			_windowReadyEvent?.Set();

			if (_windowHandle == IntPtr.Zero)
			{
				return;
			}

			AddOrUpdateNotifyIcon(forceAdd: true);

			while (GetMessage(out var message, IntPtr.Zero, 0, 0) > 0)
			{
				TranslateMessage(ref message);
				DispatchMessage(ref message);
			}
		}
		finally
		{
			RemoveNotifyIcon();
			_windowHandle = IntPtr.Zero;
			_windowReadyEvent?.Set();
		}
	}

	private void PostTrayAction(string actionKey, Action action)
	{
		if (IsDuplicateTrayAction(actionKey))
		{
			return;
		}

		Dispatcher.UIThread.Post(action);
	}

	private bool IsDuplicateTrayAction(string actionKey)
	{
		var currentTimestamp = Environment.TickCount64;

		if (string.Equals(_lastTrayActionKey, actionKey, StringComparison.Ordinal) &&
			currentTimestamp - _lastTrayActionTimestamp <= DuplicateTrayActionThresholdMilliseconds)
		{
			return true;
		}

		_lastTrayActionKey = actionKey;
		_lastTrayActionTimestamp = currentTimestamp;
		return false;
	}

	private void OpenLauncherWindow(PixelPoint? anchorPoint = null)
	{
		_pendingMenuScreenPosition = null;

		if (_isLoading)
		{
			_trayPopupMenuService.ShowLoading(ResolveScreenPosition(anchorPoint));
			Dispatcher.UIThread.Post(
				() =>
				{
					_trayPopupMenuService.Hide();
					_openLauncherAction?.Invoke();
				},
				Avalonia.Threading.DispatcherPriority.Background);
			return;
		}

		_trayPopupMenuService.Hide();
		_openLauncherAction?.Invoke();
	}

	private void OpenPendingMenuIfRequested()
	{
		if (_pendingMenuScreenPosition is not PixelPoint screenPosition)
		{
			return;
		}

		_pendingMenuScreenPosition = null;
		Dispatcher.UIThread.Post(
			() => ShowTrayPopupMenu(screenPosition),
			Avalonia.Threading.DispatcherPriority.Background);
	}

	private IntPtr OnWindowProcedure(IntPtr windowHandle, uint message, IntPtr wParam, IntPtr lParam)
	{
		if (message == _taskbarCreatedMessage)
		{
			_hasNotifyIcon = false;
			AddOrUpdateNotifyIcon(forceAdd: true);
			return IntPtr.Zero;
		}

		switch (message)
		{
			case CallbackMessageId:
				HandleCallbackMessage(wParam, lParam);
				return IntPtr.Zero;
			case WorkerUpdateMessage:
				AddOrUpdateNotifyIcon(forceAdd: !_hasNotifyIcon);
				return IntPtr.Zero;
			case WorkerDisposeMessage:
				DestroyWindow(windowHandle);
				return IntPtr.Zero;
			case WmDestroy:
				PostQuitMessage(0);
				return IntPtr.Zero;
			default:
				return DefWindowProc(windowHandle, message, wParam, lParam);
		}
	}

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern IntPtr DispatchMessage(ref PrivateMessage message);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern IntPtr DefWindowProc(IntPtr windowHandle, uint message, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool DestroyIcon(IntPtr iconHandle);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool DestroyWindow(IntPtr windowHandle);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetCursorPos(out PrivatePoint point);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern int GetMessage(out PrivateMessage message, IntPtr windowHandle, uint messageFilterMin, uint messageFilterMax);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string? moduleName);

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

	private string GetWindowsToolTipText()
	{
		var toolTipText = _trayAppearanceService.GetToolTipText(_configuration);
		return toolTipText.Length <= 127
			? toolTipText
			: toolTipText[..127];
	}

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool PostMessage(IntPtr windowHandle, uint message, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern void PostQuitMessage(int exitCode);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern IntPtr CreateWindowEx(
		uint extendedStyle,
		string className,
		string windowName,
		uint style,
		int x,
		int y,
		int width,
		int height,
		IntPtr parentHandle,
		IntPtr menuHandle,
		IntPtr instanceHandle,
		IntPtr parameter);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern ushort RegisterClassEx(ref PrivateWindowClass windowClass);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern uint RegisterWindowMessage(string message);

	private void RemoveNotifyIcon()
	{
		if (!_hasNotifyIcon || _windowHandle == IntPtr.Zero)
		{
			return;
		}

		var notifyIconData = new PrivateNotifyIconData
		{
			Size = (uint)Marshal.SizeOf<PrivateNotifyIconData>(),
			WindowHandle = _windowHandle,
			IconId = 1
		};

		ShellNotifyIcon(NimDelete, ref notifyIconData);
		_hasNotifyIcon = false;
	}

	[DllImport("shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "Shell_NotifyIconW", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool ShellNotifyIcon(uint message, ref PrivateNotifyIconData data);

	private PixelPoint ResolveScreenPosition(PixelPoint? anchorPoint)
	{
		var screenPosition = anchorPoint;

		if (screenPosition is null || (screenPosition.Value.X == 0 && screenPosition.Value.Y == 0))
		{
			if (!GetCursorPos(out var cursorPosition))
			{
				cursorPosition = new PrivatePoint
				{
					X = 0,
					Y = 0
				};
			}

			screenPosition = new PixelPoint(cursorPosition.X, cursorPosition.Y);
		}

		return screenPosition.Value;
	}

	private void ShowTrayPopupMenu(PixelPoint? anchorPoint = null)
	{
		var screenPosition = ResolveScreenPosition(anchorPoint);

		if (_isLoading)
		{
			_pendingMenuScreenPosition = screenPosition;

			if (!_trayPopupMenuService.IsOpen)
			{
				_trayPopupMenuService.ShowLoading(screenPosition);
			}

			return;
		}

		_trayPopupMenuService.Show(
			_snapshot,
			_launchAction ?? (_ => { }),
			screenPosition);
	}

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool TranslateMessage(ref PrivateMessage message);

	#endregion

	#region Methods: Public

	public void Apply(
		TreeTrayConfiguration configuration,
		LauncherSnapshot snapshot,
		bool isLoading,
		Action<LauncherEntry> launchAction,
		Action openLauncherAction)
	{
		if (!OperatingSystem.IsWindows())
		{
			return;
		}

		var shouldOpenPendingMenu = _isLoading && !isLoading && _pendingMenuScreenPosition is not null;
		if (!shouldOpenPendingMenu)
		{
			_trayPopupMenuService.Hide();
		}

		_snapshot = snapshot;
		_isLoading = isLoading;
		_configuration = CloneConfiguration(configuration);
		_launchAction = launchAction;
		_openLauncherAction = openLauncherAction;

		EnsureMessageLoopThread();
		PostMessage(_windowHandle, WorkerUpdateMessage, IntPtr.Zero, IntPtr.Zero);

		if (shouldOpenPendingMenu)
		{
			OpenPendingMenuIfRequested();
		}
	}

	public void Remove()
	{
		if (!OperatingSystem.IsWindows())
		{
			return;
		}

		_trayPopupMenuService.Hide();

		Thread? messageLoopThread;
		IntPtr windowHandle;

		lock (_syncRoot)
		{
			messageLoopThread = _messageLoopThread;
			windowHandle = _windowHandle;
			_messageLoopThread = null;
			_windowReadyEvent = null;
		}

		if (windowHandle != IntPtr.Zero)
		{
			PostMessage(windowHandle, WorkerDisposeMessage, IntPtr.Zero, IntPtr.Zero);
		}

		if (messageLoopThread is { IsAlive: true })
		{
			messageLoopThread.Join(TimeSpan.FromSeconds(2));
		}
	}

	#endregion
}

#endregion
