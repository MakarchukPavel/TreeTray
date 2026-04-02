#region Class: MacOsStatusItemService

namespace TreeTray.Services;

public sealed class MacOsStatusItemService : IMacOsStatusItemService, IDisposable
{
	#region Delegates: Private

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void ObjectiveCAction(IntPtr self, IntPtr command, IntPtr sender);

	#endregion
	#region Struct: NativeSize

	[StructLayout(LayoutKind.Sequential)]
	private struct NativeSize
	{
		#region Fields: Public

		public double Width;

		public double Height;

		#endregion
	}

	#endregion
	#region Class: NativeMethods

	private static class NativeMethods
	{
		#region Methods: Public

		[DllImport("/usr/lib/libobjc.A.dylib", CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool class_addMethod(
			IntPtr classHandle,
			IntPtr selector,
			IntPtr implementation,
			string typeEncoding);

		[DllImport("/usr/lib/libobjc.A.dylib", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr objc_allocateClassPair(
			IntPtr superClass,
			string name,
			IntPtr extraBytes);

		[DllImport("/usr/lib/libobjc.A.dylib", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr objc_getClass(string name);

		[DllImport("/usr/lib/libobjc.A.dylib", CallingConvention = CallingConvention.Cdecl)]
		public static extern void objc_registerClassPair(IntPtr classHandle);

		[DllImport("/usr/lib/libobjc.A.dylib", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr sel_registerName(string selectorName);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr IntPtr_objc_msgSend_Double(IntPtr receiver, IntPtr selector, double value);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr IntPtr_objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr value);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr IntPtr_objc_msgSend_IntPtr_IntPtr(IntPtr receiver, IntPtr selector, IntPtr firstValue, IntPtr secondValue);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr IntPtr_objc_msgSend_IntPtr_IntPtr_IntPtr(IntPtr receiver, IntPtr selector, IntPtr firstValue, IntPtr secondValue, IntPtr thirdValue);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr IntPtr_objc_msgSend_String(IntPtr receiver, IntPtr selector, [MarshalAs(UnmanagedType.LPUTF8Str)] string value);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr IntPtr_objc_msgSend_IntPtr_UIntPtr(IntPtr receiver, IntPtr selector, IntPtr firstValue, UIntPtr secondValue);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern long long_objc_msgSend(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr UIntPtr_objc_msgSend_UIntPtr(IntPtr receiver, IntPtr selector, UIntPtr value);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern void void_objc_msgSend(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern void void_objc_msgSend_Bool(IntPtr receiver, IntPtr selector, [MarshalAs(UnmanagedType.I1)] bool value);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern void void_objc_msgSend_Int64(IntPtr receiver, IntPtr selector, long value);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern void void_objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr value);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern void void_objc_msgSend_NativeSize(IntPtr receiver, IntPtr selector, NativeSize value);

		#endregion
	}

	#endregion
	#region Constants: Private

	private const double VariableStatusItemLength = -1d;

	private const double StatusItemImageSize = 18d;

	private const ulong LeftMouseUpMask = 1UL << 2;

	private const ulong RightMouseUpMask = 1UL << 4;

	private const long LeftMouseUpEventType = 2;

	private const long RightMouseUpEventType = 4;

	private const string ManagedTargetClassName = "TreeTrayMacOsStatusItemTarget";

	#endregion
	#region Fields: Private

	private static readonly ObjectiveCAction MenuItemSelectedAction = OnMenuItemSelected;

	private static readonly ObjectiveCAction StatusItemClickedAction = OnStatusItemClicked;

	private static WeakReference<MacOsStatusItemService>? _currentServiceReference;

	private static IntPtr _managedTargetClassHandle;

	private readonly IIconService _iconService;

	private readonly Dictionary<long, LauncherEntry> _menuItemEntries = new();

	private readonly ITrayAppearanceService _trayAppearanceService;

	private Action<LauncherEntry>? _launchAction;

	private IntPtr _menuHandle;

	private long _nextMenuItemTag = 1;

	private Action? _openLauncherAction;

	private TreeTrayConfiguration _pendingConfiguration = new();

	private IntPtr _statusButtonHandle;

	private IntPtr _statusItemHandle;

	private IntPtr _targetHandle;

	#endregion
	#region Constructors: Public

	public MacOsStatusItemService(ITrayAppearanceService trayAppearanceService, IIconService iconService)
	{
		_trayAppearanceService = trayAppearanceService;
		_iconService = iconService;
		_currentServiceReference = new WeakReference<MacOsStatusItemService>(this);
	}

	#endregion
	#region Methods: Private

	private void AddLauncherEntries(IntPtr menuHandle, IEnumerable<LauncherEntry> entries)
	{
		foreach (var entry in entries)
		{
			var itemHandle = CreateLauncherMenuItem(entry);
			AddMenuItem(menuHandle, itemHandle);
			ReleaseObject(itemHandle);
		}
	}

	private static void AddMenuItem(IntPtr menuHandle, IntPtr menuItemHandle)
	{
		NativeMethods.void_objc_msgSend_IntPtr(menuHandle, Selectors.AddItem, menuItemHandle);
	}

	private void ApplyButtonConfiguration()
	{
		if (_statusButtonHandle == IntPtr.Zero)
		{
			return;
		}

		var imageHandle = CreateImageFromPngBytes(_trayAppearanceService.GetTrayIconPngBytes(_pendingConfiguration));
		if (imageHandle != IntPtr.Zero)
		{
			NativeMethods.void_objc_msgSend_IntPtr(_statusButtonHandle, Selectors.SetImage, imageHandle);
			ReleaseObject(imageHandle);
		}

		var toolTipHandle = CreateNSString(_trayAppearanceService.GetToolTipText(_pendingConfiguration));
		if (toolTipHandle != IntPtr.Zero)
		{
			NativeMethods.void_objc_msgSend_IntPtr(_statusButtonHandle, Selectors.SetToolTip, toolTipHandle);
		}
	}

	private static IntPtr CreateDisabledMenuItem(string title)
	{
		var itemHandle = CreateMenuItem(title, IntPtr.Zero);
		NativeMethods.void_objc_msgSend_Bool(itemHandle, Selectors.SetEnabled, false);
		return itemHandle;
	}

	private static IntPtr CreateImageFromBitmap(Bitmap bitmap)
	{
		using var memoryStream = new MemoryStream();
		bitmap.Save(memoryStream);
		return CreateImageFromPngBytes(memoryStream.ToArray());
	}

	private static IntPtr CreateImageFromPngBytes(byte[] pngBytes)
	{
		if (pngBytes.Length == 0)
		{
			return IntPtr.Zero;
		}

		var bytesHandle = Marshal.AllocHGlobal(pngBytes.Length);
		try
		{
			Marshal.Copy(pngBytes, 0, bytesHandle, pngBytes.Length);
			var dataHandle = NativeMethods.IntPtr_objc_msgSend(NativeClasses.NSData, Selectors.Alloc);
			dataHandle = NativeMethods.IntPtr_objc_msgSend_IntPtr_UIntPtr(dataHandle, Selectors.InitWithBytesLength, bytesHandle, new UIntPtr((uint)pngBytes.Length));
			if (dataHandle == IntPtr.Zero)
			{
				return IntPtr.Zero;
			}

			var imageHandle = NativeMethods.IntPtr_objc_msgSend(NativeClasses.NSImage, Selectors.Alloc);
			imageHandle = NativeMethods.IntPtr_objc_msgSend_IntPtr(imageHandle, Selectors.InitWithData, dataHandle);
			ReleaseObject(dataHandle);
			if (imageHandle == IntPtr.Zero)
			{
				return IntPtr.Zero;
			}

			NativeMethods.void_objc_msgSend_NativeSize(imageHandle, Selectors.SetSize, new NativeSize
			{
				Width = StatusItemImageSize,
				Height = StatusItemImageSize
			});
			NativeMethods.void_objc_msgSend_Bool(imageHandle, Selectors.SetTemplate, false);
			return imageHandle;
		}
		finally
		{
			Marshal.FreeHGlobal(bytesHandle);
		}
	}

	private static IntPtr CreateMenu(string title)
	{
		var menuHandle = NativeMethods.IntPtr_objc_msgSend(NativeClasses.NSMenu, Selectors.Alloc);
		return NativeMethods.IntPtr_objc_msgSend_IntPtr(menuHandle, Selectors.InitWithTitle, CreateNSString(title));
	}

	private void CreateMenuEntries(IntPtr menuHandle, LauncherSnapshot snapshot, bool isLoading)
	{
		_menuItemEntries.Clear();
		_nextMenuItemTag = 1;

		if (isLoading)
		{
			var loadingItemHandle = CreateDisabledMenuItem("Loading launchers...");
			AddMenuItem(menuHandle, loadingItemHandle);
			ReleaseObject(loadingItemHandle);
			return;
		}

		if (snapshot.RootEntries.Count == 0)
		{
			var emptyItemHandle = CreateDisabledMenuItem("No launchers were found");
			AddMenuItem(menuHandle, emptyItemHandle);
			ReleaseObject(emptyItemHandle);
			return;
		}

		AddLauncherEntries(menuHandle, snapshot.RootEntries);
	}

	private void CreateNativeMenu(LauncherSnapshot snapshot, bool isLoading)
	{
		var menuHandle = CreateMenu(_trayAppearanceService.GetToolTipText(_pendingConfiguration));
		CreateMenuEntries(menuHandle, snapshot, isLoading);
		ReplaceMenu(menuHandle);
	}

	private void CreateOrUpdateStatusItem()
	{
		if (_statusItemHandle != IntPtr.Zero)
		{
			return;
		}

		EnsureManagedTargetClass();
		var statusBarHandle = NativeMethods.IntPtr_objc_msgSend(NativeClasses.NSStatusBar, Selectors.SystemStatusBar);
		_statusItemHandle = NativeMethods.IntPtr_objc_msgSend_Double(statusBarHandle, Selectors.StatusItemWithLength, VariableStatusItemLength);
		_statusButtonHandle = NativeMethods.IntPtr_objc_msgSend(_statusItemHandle, Selectors.Button);
		_targetHandle = NativeMethods.IntPtr_objc_msgSend(_managedTargetClassHandle, Selectors.Alloc);
		_targetHandle = NativeMethods.IntPtr_objc_msgSend(_targetHandle, Selectors.Init);

		NativeMethods.void_objc_msgSend_IntPtr(_statusButtonHandle, Selectors.SetTarget, _targetHandle);
		NativeMethods.void_objc_msgSend_IntPtr(_statusButtonHandle, Selectors.SetAction, Selectors.StatusItemClickedAction);
		NativeMethods.UIntPtr_objc_msgSend_UIntPtr(
			_statusButtonHandle,
			Selectors.SendActionOn,
			new UIntPtr(LeftMouseUpMask | RightMouseUpMask));
	}

	private IntPtr CreateLauncherMenuItem(LauncherEntry entry)
	{
		var menuItemHandle = CreateMenuItem(entry.DisplayName, IntPtr.Zero);
		ApplyMenuItemIcon(menuItemHandle, entry);

		if (entry.Children.Count > 0)
		{
			var subMenuHandle = CreateMenu(entry.DisplayName);
			AddLauncherEntries(subMenuHandle, entry.Children);
			NativeMethods.void_objc_msgSend_IntPtr(menuItemHandle, Selectors.SetSubmenu, subMenuHandle);
			ReleaseObject(subMenuHandle);
			return menuItemHandle;
		}

		var tag = _nextMenuItemTag++;
		_menuItemEntries[tag] = entry;
		NativeMethods.void_objc_msgSend_IntPtr(menuItemHandle, Selectors.SetTarget, _targetHandle);
		NativeMethods.void_objc_msgSend_IntPtr(menuItemHandle, Selectors.SetAction, Selectors.MenuItemSelectedAction);
		NativeMethods.void_objc_msgSend_Int64(menuItemHandle, Selectors.SetTag, tag);
		return menuItemHandle;
	}

	private static IntPtr CreateMenuItem(string title, IntPtr actionSelector)
	{
		var menuItemHandle = NativeMethods.IntPtr_objc_msgSend(NativeClasses.NSMenuItem, Selectors.Alloc);
		return NativeMethods.IntPtr_objc_msgSend_IntPtr_IntPtr_IntPtr(
			menuItemHandle,
			Selectors.InitWithTitleActionKeyEquivalent,
			CreateNSString(title),
			actionSelector,
			CreateNSString(string.Empty));
	}

	private static IntPtr CreateNSString(string value)
	{
		return NativeMethods.IntPtr_objc_msgSend_String(NativeClasses.NSString, Selectors.StringWithUtf8String, value ?? string.Empty);
	}

	private void ApplyMenuItemIcon(IntPtr menuItemHandle, LauncherEntry entry)
	{
		try
		{
			var imageHandle = CreateImageFromBitmap(_iconService.GetEntryIcon(entry));
			if (imageHandle == IntPtr.Zero)
			{
				return;
			}

			NativeMethods.void_objc_msgSend_IntPtr(menuItemHandle, Selectors.SetImage, imageHandle);
			ReleaseObject(imageHandle);
		}
		catch
		{
		}
	}

	private static void EnsureManagedTargetClass()
	{
		if (_managedTargetClassHandle != IntPtr.Zero)
		{
			return;
		}

		_managedTargetClassHandle = NativeMethods.objc_getClass(ManagedTargetClassName);
		if (_managedTargetClassHandle != IntPtr.Zero)
		{
			return;
		}

		var baseClassHandle = NativeMethods.objc_getClass("NSObject");
		_managedTargetClassHandle = NativeMethods.objc_allocateClassPair(baseClassHandle, ManagedTargetClassName, IntPtr.Zero);
		NativeMethods.class_addMethod(
			_managedTargetClassHandle,
			Selectors.StatusItemClickedAction,
			Marshal.GetFunctionPointerForDelegate(StatusItemClickedAction),
			"v@:@");
		NativeMethods.class_addMethod(
			_managedTargetClassHandle,
			Selectors.MenuItemSelectedAction,
			Marshal.GetFunctionPointerForDelegate(MenuItemSelectedAction),
			"v@:@");
		NativeMethods.objc_registerClassPair(_managedTargetClassHandle);
	}

	private static long GetCurrentEventType()
	{
		var applicationHandle = NativeMethods.IntPtr_objc_msgSend(NativeClasses.NSApplication, Selectors.SharedApplication);
		var eventHandle = NativeMethods.IntPtr_objc_msgSend(applicationHandle, Selectors.CurrentEvent);
		return eventHandle == IntPtr.Zero
			? 0
			: NativeMethods.long_objc_msgSend(eventHandle, Selectors.Type);
	}

	private static bool TryGetCurrentService(out MacOsStatusItemService? service)
	{
		service = null;
		return _currentServiceReference is not null
			&& _currentServiceReference.TryGetTarget(out service)
			&& service is not null;
	}

	private static void OnMenuItemSelected(IntPtr self, IntPtr command, IntPtr sender)
	{
		if (!TryGetCurrentService(out var service) || service is null)
		{
			return;
		}

		service.HandleMenuItemSelected(sender);
	}

	private static void OnStatusItemClicked(IntPtr self, IntPtr command, IntPtr sender)
	{
		if (!TryGetCurrentService(out var service) || service is null)
		{
			return;
		}

		service.HandleStatusItemClicked();
	}

	private void HandleMenuItemSelected(IntPtr sender)
	{
		if (sender == IntPtr.Zero)
		{
			return;
		}

		var tag = NativeMethods.long_objc_msgSend(sender, Selectors.Tag);
		if (!_menuItemEntries.TryGetValue(tag, out var entry) || _launchAction is null)
		{
			return;
		}

		Dispatcher.UIThread.Post(() => _launchAction(entry), DispatcherPriority.Background);
	}

	private void HandleStatusItemClicked()
	{
		var eventType = GetCurrentEventType();
		var openMainWindowOnPrimaryClick = _pendingConfiguration.InvertTrayIconMouseButtons;
		var isPrimaryClick = eventType == LeftMouseUpEventType;
		var isSecondaryClick = eventType == RightMouseUpEventType;

		if (!isPrimaryClick && !isSecondaryClick)
		{
			return;
		}

		if ((openMainWindowOnPrimaryClick && isPrimaryClick)
			|| (!openMainWindowOnPrimaryClick && isSecondaryClick))
		{
			if (_openLauncherAction is null)
			{
				return;
			}

			Dispatcher.UIThread.Post(_openLauncherAction, DispatcherPriority.Background);
			return;
		}

		if (_statusItemHandle == IntPtr.Zero || _menuHandle == IntPtr.Zero)
		{
			return;
		}

		NativeMethods.void_objc_msgSend_IntPtr(_statusItemHandle, Selectors.PopUpStatusItemMenu, _menuHandle);
	}

	private static void ReleaseObject(IntPtr handle)
	{
		if (handle != IntPtr.Zero)
		{
			NativeMethods.void_objc_msgSend(handle, Selectors.Release);
		}
	}

	private void ReplaceMenu(IntPtr newMenuHandle)
	{
		if (_menuHandle != IntPtr.Zero)
		{
			ReleaseObject(_menuHandle);
		}

		_menuHandle = newMenuHandle;
	}

	private void ResetNativeHandles()
	{
		_menuItemEntries.Clear();
		_nextMenuItemTag = 1;
		_statusItemHandle = IntPtr.Zero;
		_statusButtonHandle = IntPtr.Zero;
		_targetHandle = IntPtr.Zero;
		_menuHandle = IntPtr.Zero;
	}

	#endregion
	#region Methods: Public

	public void Apply(
		TreeTrayConfiguration configuration,
		LauncherSnapshot snapshot,
		bool isLoading,
		Action<LauncherEntry> launchAction,
		Action openLauncherAction)
	{
		if (!OperatingSystem.IsMacOS())
		{
			return;
		}

		_pendingConfiguration = new TreeTrayConfiguration
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
		_launchAction = launchAction;
		_openLauncherAction = openLauncherAction;

		CreateOrUpdateStatusItem();
		ApplyButtonConfiguration();
		CreateNativeMenu(snapshot, isLoading);
	}

	public void Dispose()
	{
		Remove();
	}

	public void Remove()
	{
		if (!OperatingSystem.IsMacOS())
		{
			return;
		}

		if (_statusItemHandle != IntPtr.Zero)
		{
			var statusBarHandle = NativeMethods.IntPtr_objc_msgSend(NativeClasses.NSStatusBar, Selectors.SystemStatusBar);
			NativeMethods.void_objc_msgSend_IntPtr(statusBarHandle, Selectors.RemoveStatusItem, _statusItemHandle);
		}

		if (_menuHandle != IntPtr.Zero)
		{
			ReleaseObject(_menuHandle);
		}

		if (_targetHandle != IntPtr.Zero)
		{
			ReleaseObject(_targetHandle);
		}

		ResetNativeHandles();
	}

	#endregion
	#region Class: NativeClasses

	private static class NativeClasses
	{
		#region Fields: Public

		public static readonly IntPtr NSApplication = NativeMethods.objc_getClass("NSApplication");

		public static readonly IntPtr NSData = NativeMethods.objc_getClass("NSData");

		public static readonly IntPtr NSImage = NativeMethods.objc_getClass("NSImage");

		public static readonly IntPtr NSMenu = NativeMethods.objc_getClass("NSMenu");

		public static readonly IntPtr NSMenuItem = NativeMethods.objc_getClass("NSMenuItem");

		public static readonly IntPtr NSStatusBar = NativeMethods.objc_getClass("NSStatusBar");

		public static readonly IntPtr NSString = NativeMethods.objc_getClass("NSString");

		#endregion
	}

	#endregion
	#region Class: Selectors

	private static class Selectors
	{
		#region Fields: Public

		public static readonly IntPtr AddItem = NativeMethods.sel_registerName("addItem:");

		public static readonly IntPtr Alloc = NativeMethods.sel_registerName("alloc");

		public static readonly IntPtr Button = NativeMethods.sel_registerName("button");

		public static readonly IntPtr CurrentEvent = NativeMethods.sel_registerName("currentEvent");

		public static readonly IntPtr Init = NativeMethods.sel_registerName("init");

		public static readonly IntPtr InitWithBytesLength = NativeMethods.sel_registerName("initWithBytes:length:");

		public static readonly IntPtr InitWithData = NativeMethods.sel_registerName("initWithData:");

		public static readonly IntPtr InitWithTitle = NativeMethods.sel_registerName("initWithTitle:");

		public static readonly IntPtr InitWithTitleActionKeyEquivalent = NativeMethods.sel_registerName("initWithTitle:action:keyEquivalent:");

		public static readonly IntPtr MenuItemSelectedAction = NativeMethods.sel_registerName("treeTrayMenuItemSelected:");

		public static readonly IntPtr PopUpStatusItemMenu = NativeMethods.sel_registerName("popUpStatusItemMenu:");

		public static readonly IntPtr Release = NativeMethods.sel_registerName("release");

		public static readonly IntPtr RemoveStatusItem = NativeMethods.sel_registerName("removeStatusItem:");

		public static readonly IntPtr SendActionOn = NativeMethods.sel_registerName("sendActionOn:");

		public static readonly IntPtr SetAction = NativeMethods.sel_registerName("setAction:");

		public static readonly IntPtr SetEnabled = NativeMethods.sel_registerName("setEnabled:");

		public static readonly IntPtr SetImage = NativeMethods.sel_registerName("setImage:");

		public static readonly IntPtr SetSize = NativeMethods.sel_registerName("setSize:");

		public static readonly IntPtr SetSubmenu = NativeMethods.sel_registerName("setSubmenu:");

		public static readonly IntPtr SetTag = NativeMethods.sel_registerName("setTag:");

		public static readonly IntPtr SetTarget = NativeMethods.sel_registerName("setTarget:");

		public static readonly IntPtr SetTemplate = NativeMethods.sel_registerName("setTemplate:");

		public static readonly IntPtr SetToolTip = NativeMethods.sel_registerName("setToolTip:");

		public static readonly IntPtr SharedApplication = NativeMethods.sel_registerName("sharedApplication");

		public static readonly IntPtr StatusItemClickedAction = NativeMethods.sel_registerName("treeTrayStatusItemClicked:");

		public static readonly IntPtr StatusItemWithLength = NativeMethods.sel_registerName("statusItemWithLength:");

		public static readonly IntPtr StringWithUtf8String = NativeMethods.sel_registerName("stringWithUTF8String:");

		public static readonly IntPtr SystemStatusBar = NativeMethods.sel_registerName("systemStatusBar");

		public static readonly IntPtr Tag = NativeMethods.sel_registerName("tag");

		public static readonly IntPtr Type = NativeMethods.sel_registerName("type");

		#endregion
	}

	#endregion
}

#endregion
