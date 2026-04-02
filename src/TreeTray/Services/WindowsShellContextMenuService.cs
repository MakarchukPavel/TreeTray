#region Class: WindowsShellContextMenuService

namespace TreeTray.Services;

public sealed class WindowsShellContextMenuService : IWindowsShellContextMenuService, IDisposable
{
	#region Enum: ContextMenuFlags

	[Flags]
	private enum ContextMenuFlags : uint
	{
		Normal = 0x00000000
	}

	#endregion

	#region Enum: CommandInvokeMaskFlags

	[Flags]
	private enum CommandInvokeMaskFlags : uint
	{
		Unicode = 0x00004000,
		PtInvoke = 0x20000000
	}

	#endregion

	#region Enum: TrackPopupMenuFlags

	[Flags]
	private enum TrackPopupMenuFlags : uint
	{
		RightButton = 0x0002,
		ReturnCommand = 0x0100
	}

	#endregion

	#region Delegates: Private

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	private delegate IntPtr WindowProcedure(IntPtr windowHandle, uint message, IntPtr wParam, IntPtr lParam);

	#endregion

	#region Interface: Private

	[ComImport]
	[Guid("000214E6-0000-0000-C000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IShellFolder
	{
		#region Methods: Public

		[PreserveSig]
		int ParseDisplayName(
			IntPtr windowHandle,
			IntPtr bindContext,
			[MarshalAs(UnmanagedType.LPWStr)] string displayName,
			ref uint eatenCharacters,
			out IntPtr itemIdentifierList,
			ref uint attributes);

		[PreserveSig]
		int EnumObjects(IntPtr windowHandle, int flags, out IntPtr enumIdentifierList);

		[PreserveSig]
		int BindToObject(IntPtr itemIdentifierList, IntPtr bindContext, ref Guid interfaceId, out IntPtr shellFolder);

		[PreserveSig]
		int BindToStorage(IntPtr itemIdentifierList, IntPtr bindContext, ref Guid interfaceId, out IntPtr objectPointer);

		[PreserveSig]
		int CompareIDs(int leftParam, IntPtr firstItemIdentifierList, IntPtr secondItemIdentifierList);

		[PreserveSig]
		int CreateViewObject(IntPtr windowHandle, ref Guid interfaceId, out IntPtr objectPointer);

		[PreserveSig]
		int GetAttributesOf(uint itemCount, IntPtr itemIdentifierListArray, ref uint attributes);

		[PreserveSig]
		int GetUIObjectOf(
			IntPtr windowHandle,
			uint itemCount,
			[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] IntPtr[] itemIdentifierListArray,
			ref Guid interfaceId,
			IntPtr reserved,
			out IntPtr objectPointer);

		[PreserveSig]
		int GetDisplayNameOf(IntPtr itemIdentifierList, uint flags, IntPtr name);

		[PreserveSig]
		int SetNameOf(IntPtr windowHandle, IntPtr itemIdentifierList, [MarshalAs(UnmanagedType.LPWStr)] string name, uint flags, out IntPtr newItemIdentifierList);

		#endregion
	}

	[ComImport]
	[Guid("000214E4-0000-0000-C000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IContextMenu
	{
		#region Methods: Public

		[PreserveSig]
		int QueryContextMenu(
			IntPtr menuHandle,
			uint indexMenu,
			uint commandIdFirst,
			uint commandIdLast,
			ContextMenuFlags flags);

		void InvokeCommand(ref CommandInvokeInfoEx commandInfo);

		void GetCommandString(
			UIntPtr commandId,
			uint flags,
			IntPtr reserved,
			[MarshalAs(UnmanagedType.LPStr)] StringBuilder commandString,
			uint commandStringLength);

		#endregion
	}

	[ComImport]
	[Guid("000214F4-0000-0000-C000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IContextMenu2
	{
		#region Methods: Public

		[PreserveSig]
		int QueryContextMenu(
			IntPtr menuHandle,
			uint indexMenu,
			uint commandIdFirst,
			uint commandIdLast,
			ContextMenuFlags flags);

		void InvokeCommand(ref CommandInvokeInfoEx commandInfo);

		void GetCommandString(
			UIntPtr commandId,
			uint flags,
			IntPtr reserved,
			[MarshalAs(UnmanagedType.LPStr)] StringBuilder commandString,
			uint commandStringLength);

		[PreserveSig]
		int HandleMenuMsg(uint message, IntPtr wParam, IntPtr lParam);

		#endregion
	}

	[ComImport]
	[Guid("BCFCE0A0-EC17-11D0-8D10-00A0C90F2719")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IContextMenu3
	{
		#region Methods: Public

		[PreserveSig]
		int QueryContextMenu(
			IntPtr menuHandle,
			uint indexMenu,
			uint commandIdFirst,
			uint commandIdLast,
			ContextMenuFlags flags);

		void InvokeCommand(ref CommandInvokeInfoEx commandInfo);

		void GetCommandString(
			UIntPtr commandId,
			uint flags,
			IntPtr reserved,
			[MarshalAs(UnmanagedType.LPStr)] StringBuilder commandString,
			uint commandStringLength);

		[PreserveSig]
		int HandleMenuMsg(uint message, IntPtr wParam, IntPtr lParam);

		[PreserveSig]
		int HandleMenuMsg2(uint message, IntPtr wParam, IntPtr lParam, out IntPtr result);

		#endregion
	}

	#endregion

	#region Struct: CommandInvokeInfoEx

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct CommandInvokeInfoEx
	{
		#region Fields: Public

		public uint Size;

		public CommandInvokeMaskFlags Mask;

		public IntPtr WindowHandle;

		public IntPtr Verb;

		public IntPtr Parameters;

		public IntPtr Directory;

		public int ShowCommand;

		public uint HotKey;

		public IntPtr IconHandle;

		public IntPtr Title;

		public IntPtr VerbUnicode;

		public IntPtr ParametersUnicode;

		public IntPtr DirectoryUnicode;

		public IntPtr TitleUnicode;

		public NativePoint InvokePoint;

		#endregion
	}

	#endregion

	#region Struct: NativePoint

	[StructLayout(LayoutKind.Sequential)]
	private struct NativePoint
	{
		#region Fields: Public

		public int X;

		public int Y;

		#endregion
	}

	#endregion

	#region Struct: WindowClass

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct WindowClass
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

	private const string User32LibraryName = "user32.dll";

	private const string Shell32LibraryName = "shell32.dll";

	private const string Ole32LibraryName = "ole32.dll";

	private const string WindowClassName = "TreeTray.WindowsShellContextMenuHost";

	private const int ClassAlreadyExistsErrorCode = 1410;

	private const int ShowNormal = 1;

	private const uint FirstCommandId = 1;

	private const uint LastCommandId = 0x7FFF;

	private const uint WmNull = 0x0000;

	private const uint WmInitMenuPopup = 0x0117;

	private const uint WmMeasureItem = 0x002C;

	private const uint WmDrawItem = 0x002B;

	private const uint WmMenuChar = 0x0120;

	private const uint WmUninitMenuPopup = 0x0125;

	#endregion

	#region Fields: Private

	private readonly WindowProcedure _windowProcedure;

	private IContextMenu2? _activeContextMenu2;

	private IContextMenu3? _activeContextMenu3;

	private IntPtr _windowHandle;

	#endregion

	#region Constructors: Public

	public WindowsShellContextMenuService()
	{
		_windowProcedure = OnWindowProcedure;
	}

	#endregion

	#region Methods: Private

	[DllImport(User32LibraryName, CharSet = CharSet.Unicode, SetLastError = true)]
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

	[DllImport(User32LibraryName, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool DestroyMenu(IntPtr menuHandle);

	[DllImport(User32LibraryName, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool DestroyWindow(IntPtr windowHandle);

	[DllImport(User32LibraryName, CharSet = CharSet.Unicode)]
	private static extern IntPtr DefWindowProc(IntPtr windowHandle, uint message, IntPtr wParam, IntPtr lParam);

	[DllImport(User32LibraryName, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetCursorPos(out NativePoint point);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string? moduleName);

	[DllImport(User32LibraryName, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool PostMessage(IntPtr windowHandle, uint message, IntPtr wParam, IntPtr lParam);

	[DllImport(User32LibraryName, CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern ushort RegisterClassEx(ref WindowClass windowClass);

	[DllImport(User32LibraryName, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetForegroundWindow(IntPtr windowHandle);

	[DllImport(Shell32LibraryName, CharSet = CharSet.Unicode)]
	private static extern int SHBindToParent(
		IntPtr itemIdentifierList,
		ref Guid interfaceId,
		[MarshalAs(UnmanagedType.Interface)] out IShellFolder shellFolder,
		out IntPtr childItemIdentifierList);

	[DllImport(Shell32LibraryName, CharSet = CharSet.Unicode)]
	private static extern int SHParseDisplayName(
		[MarshalAs(UnmanagedType.LPWStr)] string name,
		IntPtr bindingContext,
		out IntPtr itemIdentifierList,
		uint attributes,
		out uint attributesOut);

	[DllImport(Ole32LibraryName)]
	private static extern void CoTaskMemFree(IntPtr pointer);

	[DllImport(User32LibraryName, SetLastError = true)]
	private static extern IntPtr CreatePopupMenu();

	[DllImport(User32LibraryName, SetLastError = true)]
	private static extern uint TrackPopupMenuEx(
		IntPtr menuHandle,
		TrackPopupMenuFlags flags,
		int x,
		int y,
		IntPtr windowHandle,
		IntPtr reserved);

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private static void ReleaseComObject(object? value)
	{
		try
		{
			if (value is not null && Marshal.IsComObject(value))
			{
				Marshal.FinalReleaseComObject(value);
			}
		}
		catch
		{
		}
	}

	private static void ThrowIfFailed(int result)
	{
		if (result < 0)
		{
			Marshal.ThrowExceptionForHR(result);
		}
	}

	private void ClearActiveContextMenuHandlers()
	{
		_activeContextMenu3 = null;
		_activeContextMenu2 = null;
	}

	private void EnsureWindowHandle()
	{
		if (_windowHandle != IntPtr.Zero)
		{
			return;
		}

		var instanceHandle = GetModuleHandle(null);
		var windowClass = new WindowClass
		{
			Size = (uint)Marshal.SizeOf<WindowClass>(),
			ClassName = WindowClassName,
			InstanceHandle = instanceHandle,
			WindowProcedurePointer = Marshal.GetFunctionPointerForDelegate(_windowProcedure)
		};

		var classResult = RegisterClassEx(ref windowClass);
		if (classResult == 0)
		{
			var errorCode = Marshal.GetLastWin32Error();
			if (errorCode != ClassAlreadyExistsErrorCode)
			{
				throw new Win32Exception(errorCode);
			}
		}

		_windowHandle = CreateWindowEx(
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

		if (_windowHandle == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
	}

	private static bool IsMenuMessageForIContextMenu2(uint message)
	{
		return message == WmInitMenuPopup
			|| message == WmMeasureItem
			|| message == WmDrawItem;
	}

	private static bool IsMenuMessageForIContextMenu3(uint message)
	{
		return message == WmInitMenuPopup
			|| message == WmMeasureItem
			|| message == WmDrawItem
			|| message == WmMenuChar
			|| message == WmUninitMenuPopup;
	}

	private void InvokeCommand(IContextMenu contextMenu, uint selectedCommandId, PixelPoint screenPosition)
	{
		var commandOffset = unchecked((nint)(selectedCommandId - FirstCommandId));
		var commandInfo = new CommandInvokeInfoEx
		{
			Size = (uint)Marshal.SizeOf<CommandInvokeInfoEx>(),
			Mask = CommandInvokeMaskFlags.Unicode | CommandInvokeMaskFlags.PtInvoke,
			WindowHandle = _windowHandle,
			Verb = commandOffset,
			ShowCommand = ShowNormal,
			VerbUnicode = commandOffset,
			InvokePoint = new NativePoint
			{
				X = screenPosition.X,
				Y = screenPosition.Y
			}
		};

		contextMenu.InvokeCommand(ref commandInfo);
	}

	private IntPtr OnWindowProcedure(IntPtr windowHandle, uint message, IntPtr wParam, IntPtr lParam)
	{
		if (_activeContextMenu3 is not null && IsMenuMessageForIContextMenu3(message))
		{
			if (_activeContextMenu3.HandleMenuMsg2(message, wParam, lParam, out var result) >= 0)
			{
				return result;
			}
		}
		else if (_activeContextMenu2 is not null && IsMenuMessageForIContextMenu2(message))
		{
			if (_activeContextMenu2.HandleMenuMsg(message, wParam, lParam) >= 0)
			{
				return IntPtr.Zero;
			}
		}

		return DefWindowProc(windowHandle, message, wParam, lParam);
	}

	private PixelPoint ResolveScreenPosition(PixelPoint screenPosition)
	{
		if (screenPosition.X != 0 || screenPosition.Y != 0)
		{
			return screenPosition;
		}

		if (GetCursorPos(out var point))
		{
			return new PixelPoint(point.X, point.Y);
		}

		return screenPosition;
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private bool ShowContextMenuCore(string path, PixelPoint screenPosition)
	{
		if (!File.Exists(path) && !Directory.Exists(path))
		{
			return false;
		}

		EnsureWindowHandle();

		IntPtr absoluteItemIdentifierList = IntPtr.Zero;
		IntPtr popupMenuHandle = IntPtr.Zero;
		IntPtr contextMenuPointer = IntPtr.Zero;
		IShellFolder? parentFolder = null;
		object? contextMenuObject = null;

		try
		{
			ThrowIfFailed(SHParseDisplayName(path, IntPtr.Zero, out absoluteItemIdentifierList, 0, out _));

			var shellFolderInterfaceId = typeof(IShellFolder).GUID;
			ThrowIfFailed(SHBindToParent(
				absoluteItemIdentifierList,
				ref shellFolderInterfaceId,
				out parentFolder,
				out var childItemIdentifierList));

			var contextMenuInterfaceId = typeof(IContextMenu).GUID;
			ThrowIfFailed(parentFolder.GetUIObjectOf(
				_windowHandle,
				1,
				new[] { childItemIdentifierList },
				ref contextMenuInterfaceId,
				IntPtr.Zero,
				out contextMenuPointer));

			contextMenuObject = Marshal.GetObjectForIUnknown(contextMenuPointer);
			Marshal.Release(contextMenuPointer);
			contextMenuPointer = IntPtr.Zero;

			if (contextMenuObject is not IContextMenu contextMenu)
			{
				return false;
			}

			popupMenuHandle = CreatePopupMenu();
			if (popupMenuHandle == IntPtr.Zero)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}

			ThrowIfFailed(contextMenu.QueryContextMenu(
				popupMenuHandle,
				0,
				FirstCommandId,
				LastCommandId,
				ContextMenuFlags.Normal));

			_activeContextMenu2 = contextMenuObject as IContextMenu2;
			_activeContextMenu3 = contextMenuObject as IContextMenu3;

			var resolvedScreenPosition = ResolveScreenPosition(screenPosition);
			SetForegroundWindow(_windowHandle);
			var selectedCommandId = TrackPopupMenuEx(
				popupMenuHandle,
				TrackPopupMenuFlags.ReturnCommand | TrackPopupMenuFlags.RightButton,
				resolvedScreenPosition.X,
				resolvedScreenPosition.Y,
				_windowHandle,
				IntPtr.Zero);
			PostMessage(_windowHandle, WmNull, IntPtr.Zero, IntPtr.Zero);

			if (selectedCommandId != 0)
			{
				InvokeCommand(contextMenu, selectedCommandId, resolvedScreenPosition);
			}

			return true;
		}
		finally
		{
			ClearActiveContextMenuHandlers();

			if (popupMenuHandle != IntPtr.Zero)
			{
				DestroyMenu(popupMenuHandle);
			}

			if (contextMenuPointer != IntPtr.Zero)
			{
				Marshal.Release(contextMenuPointer);
			}

			ReleaseComObject(contextMenuObject);
			ReleaseComObject(parentFolder);

			if (absoluteItemIdentifierList != IntPtr.Zero)
			{
				CoTaskMemFree(absoluteItemIdentifierList);
			}
		}
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private bool ShowContextMenuWindows(string path, PixelPoint screenPosition)
	{
		if (Dispatcher.UIThread.CheckAccess())
		{
			return ShowContextMenuCore(path, screenPosition);
		}

		return Dispatcher.UIThread.Invoke(() => ShowContextMenuCore(path, screenPosition));
	}

	#endregion

	#region Methods: Public

	public void Dispose()
	{
		ClearActiveContextMenuHandlers();

		if (_windowHandle == IntPtr.Zero)
		{
			return;
		}

		DestroyWindow(_windowHandle);
		_windowHandle = IntPtr.Zero;
	}

	public bool ShowContextMenu(string path, PixelPoint screenPosition)
	{
		ArgumentNullException.ThrowIfNull(path);

		if (!OperatingSystem.IsWindows())
		{
			return false;
		}

		return ShowContextMenuWindows(path, screenPosition);
	}

	#endregion
}

#endregion
