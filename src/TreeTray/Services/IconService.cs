#region Class: IconService

namespace TreeTray.Services;

public sealed class IconService : IIconService
{
	#region Fields: Private

	private readonly ConcurrentDictionary<string, Bitmap?> _appxIconCache = new(StringComparer.OrdinalIgnoreCase);

	private readonly ConcurrentDictionary<string, Bitmap?> _macOsWorkspaceIconCache = new(StringComparer.OrdinalIgnoreCase);

	private readonly ConcurrentDictionary<string, Bitmap> _entryIconCache = new(StringComparer.OrdinalIgnoreCase);

	private readonly WindowIcon _applicationIcon;

	private readonly Bitmap _folderIcon;

	private readonly Bitmap _launcherIcon;

	#endregion

	#region Constructors: Public

	public IconService()
	{
		_applicationIcon = LoadWindowIcon("AppIcon.png");
		_folderIcon = LoadBitmap("FolderIcon.png");
		_launcherIcon = LoadBitmap("LauncherIcon.png");
	}

	#endregion

	#region Properties: Public

	public WindowIcon ApplicationIcon => _applicationIcon;

	public Bitmap FolderIcon => _folderIcon;

	public Bitmap LauncherIcon => _launcherIcon;

	#endregion

	#region Methods: Private

	#region Struct: MacOsNativeSize

	[StructLayout(LayoutKind.Sequential)]
	private struct MacOsNativeSize
	{
		#region Fields: Public

		public double Width;

		public double Height;

		#endregion
	}

	#endregion

	#region Class: MacOsNativeMethods

	private static class MacOsNativeMethods
	{
		#region Methods: Public

		[DllImport("/usr/lib/libobjc.A.dylib", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr objc_getClass(string name);

		[DllImport("/usr/lib/libobjc.A.dylib", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr sel_registerName(string selectorName);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr IntPtr_objc_msgSend_String(
			IntPtr receiver,
			IntPtr selector,
			[MarshalAs(UnmanagedType.LPUTF8Str)] string value);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr IntPtr_objc_msgSend_IntPtr(
			IntPtr receiver,
			IntPtr selector,
			IntPtr value);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr IntPtr_objc_msgSend_Int64_IntPtr(
			IntPtr receiver,
			IntPtr selector,
			long firstValue,
			IntPtr secondValue);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr UIntPtr_objc_msgSend(
			IntPtr receiver,
			IntPtr selector);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern void void_objc_msgSend_NativeSize(
			IntPtr receiver,
			IntPtr selector,
			MacOsNativeSize value);

		[DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend", CallingConvention = CallingConvention.Cdecl)]
		public static extern void void_objc_msgSend(
			IntPtr receiver,
			IntPtr selector);

		#endregion
	}

	#endregion

	#region Class: MacOsNativeClasses

	private static class MacOsNativeClasses
	{
		#region Fields: Public

		public static readonly IntPtr NSAutoreleasePool = MacOsNativeMethods.objc_getClass("NSAutoreleasePool");

		public static readonly IntPtr NSBitmapImageRep = MacOsNativeMethods.objc_getClass("NSBitmapImageRep");

		public static readonly IntPtr NSWorkspace = MacOsNativeMethods.objc_getClass("NSWorkspace");

		#endregion
	}

	#endregion

	#region Class: MacOsSelectors

	private static class MacOsSelectors
	{
		#region Fields: Public

		public static readonly IntPtr Alloc = MacOsNativeMethods.sel_registerName("alloc");

		public static readonly IntPtr Bytes = MacOsNativeMethods.sel_registerName("bytes");

		public static readonly IntPtr IconForFile = MacOsNativeMethods.sel_registerName("iconForFile:");

		public static readonly IntPtr ImageRepWithData = MacOsNativeMethods.sel_registerName("imageRepWithData:");

		public static readonly IntPtr Init = MacOsNativeMethods.sel_registerName("init");

		public static readonly IntPtr Length = MacOsNativeMethods.sel_registerName("length");

		public static readonly IntPtr Release = MacOsNativeMethods.sel_registerName("release");

		public static readonly IntPtr RepresentationUsingTypeProperties = MacOsNativeMethods.sel_registerName("representationUsingType:properties:");

		public static readonly IntPtr SetSize = MacOsNativeMethods.sel_registerName("setSize:");

		public static readonly IntPtr SharedWorkspace = MacOsNativeMethods.sel_registerName("sharedWorkspace");

		public static readonly IntPtr TIFFRepresentation = MacOsNativeMethods.sel_registerName("TIFFRepresentation");

		#endregion
	}

	#endregion

	#region Constants: Private

	private const long MacOsPngImageFileType = 4;

	private const double MacOsWorkspaceIconSize = 32d;

	#endregion

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private sealed class WindowsShortcutShellAccessor : IDisposable
	{
		#region Fields: Private

		private readonly dynamic _shellApplication;

		#endregion

		#region Constructors: Public

		public WindowsShortcutShellAccessor()
		{
			var shellApplicationType = Type.GetTypeFromProgID("Shell.Application")
				?? throw new NotSupportedException("Shell.Application is not available.");
			_shellApplication = Activator.CreateInstance(shellApplicationType)
				?? throw new InvalidOperationException("Shell.Application could not be created.");
		}

		#endregion

		#region Methods: Public

		public string? GetTargetParsingPath(string shortcutPath)
		{
			var directoryPath = Path.GetDirectoryName(shortcutPath);
			var fileName = Path.GetFileName(shortcutPath);

			if (string.IsNullOrWhiteSpace(directoryPath) || string.IsNullOrWhiteSpace(fileName))
			{
				return null;
			}

			try
			{
				dynamic folder = _shellApplication.NameSpace(directoryPath);
				if (folder is null)
				{
					return null;
				}

				dynamic item = folder.ParseName(fileName);
				if (item is null)
				{
					return null;
				}

				return item.ExtendedProperty("System.Link.TargetParsingPath") as string;
			}
			catch
			{
				return null;
			}
		}

		public void Dispose()
		{
			try
			{
				if (Marshal.IsComObject(_shellApplication))
				{
					Marshal.FinalReleaseComObject(_shellApplication);
				}
			}
			catch
			{
			}
		}

		#endregion
	}

	private static Bitmap CloneBitmap(Bitmap sourceBitmap)
	{
		using var memoryStream = new MemoryStream();
		sourceBitmap.Save(memoryStream);
		memoryStream.Position = 0;
		return new Bitmap(memoryStream);
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private static Bitmap ConvertDrawingBitmap(System.Drawing.Bitmap drawingBitmap)
	{
		using var memoryStream = new MemoryStream();
		drawingBitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
		memoryStream.Position = 0;
		return new Bitmap(memoryStream);
	}

	private static Bitmap LoadBitmap(string fileName)
	{
		using var stream = AssetLoader.Open(new Uri($"avares://TreeTray/Assets/{fileName}"));
		return new Bitmap(stream);
	}

	private static WindowIcon LoadWindowIcon(string fileName)
	{
		using var stream = AssetLoader.Open(new Uri($"avares://TreeTray/Assets/{fileName}"));
		return new WindowIcon(stream);
	}

	private Bitmap GetOrCreateMacOsEntryIcon(string sourcePath, Bitmap fallbackBitmap)
	{
		return _entryIconCache.GetOrAdd(
			sourcePath,
			path => TryLoadMacOsWorkspaceIcon(path)
				?? TryLoadMacOsEntryIcon(path)
				?? CloneBitmap(fallbackBitmap));
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private Bitmap GetOrCreateWindowsEntryIcon(string sourcePath)
	{
		return _entryIconCache.GetOrAdd(
			sourcePath,
			path => TryLoadWindowsInternetShortcutIcon(path)
				?? TryLoadWindowsAppxShortcutIcon(path)
				?? TryLoadWindowsShellIcon(path)
				?? TryExtractAssociatedWindowsIcon(path)
				?? CloneBitmap(LauncherIcon));
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private static IEnumerable<string> GetCandidateWindowsAppxPackageDirectories(string packageFamilyName)
	{
		var separatorIndex = packageFamilyName.LastIndexOf('_');
		if (separatorIndex <= 0 || separatorIndex >= packageFamilyName.Length - 1)
		{
			return Array.Empty<string>();
		}

		var packageName = packageFamilyName[..separatorIndex];
		var publisherId = packageFamilyName[(separatorIndex + 1)..];
		var windowsAppsDirectoryPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
			"WindowsApps");

		try
		{
			return Directory.EnumerateDirectories(
					windowsAppsDirectoryPath,
					$"{packageName}_*__{publisherId}",
					SearchOption.TopDirectoryOnly)
				.OrderBy(GetWindowsAppxPackageDirectoryPriority)
				.ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
				.ToArray();
		}
		catch
		{
			return Array.Empty<string>();
		}
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private static int GetWindowsAppxPackageDirectoryPriority(string packageDirectoryPath)
	{
		var packageDirectoryName = Path.GetFileName(packageDirectoryPath);

		if (packageDirectoryName.Contains("_x64__", StringComparison.OrdinalIgnoreCase)
			|| packageDirectoryName.Contains("_x86__", StringComparison.OrdinalIgnoreCase)
			|| packageDirectoryName.Contains("_arm64__", StringComparison.OrdinalIgnoreCase))
		{
			return 0;
		}

		if (packageDirectoryName.Contains("_neutral_~_", StringComparison.OrdinalIgnoreCase))
		{
			return 1;
		}

		return 2;
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private static string? GetWindowsAppxLogoRelativePath(string packageDirectoryPath, string applicationId)
	{
		var manifestPath = Path.Combine(packageDirectoryPath, "AppxManifest.xml");
		if (!File.Exists(manifestPath))
		{
			return null;
		}

		try
		{
			var document = XDocument.Load(manifestPath, LoadOptions.None);
			var applicationElement = document
				.Descendants()
				.FirstOrDefault(element =>
					string.Equals(element.Name.LocalName, "Application", StringComparison.Ordinal) &&
					string.Equals((string?)element.Attribute("Id"), applicationId, StringComparison.OrdinalIgnoreCase));

			var visualElementsElement = applicationElement?
				.Elements()
				.FirstOrDefault(element => string.Equals(element.Name.LocalName, "VisualElements", StringComparison.Ordinal));

			return (string?)visualElementsElement?.Attribute("Square44x44Logo")
				?? (string?)visualElementsElement?.Attribute("Square150x150Logo");
		}
		catch
		{
			return null;
		}
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private Bitmap? TryLoadWindowsAppxShortcutIcon(string shortcutPath)
	{
		if (!string.Equals(Path.GetExtension(shortcutPath), ".lnk", StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}

		return _appxIconCache.GetOrAdd(
			shortcutPath,
			path =>
			{
				var targetParsingPath = TryGetWindowsShortcutTargetParsingPath(path);
				if (string.IsNullOrWhiteSpace(targetParsingPath))
				{
					return null;
				}

				var separatorIndex = targetParsingPath.IndexOf('!');
				if (separatorIndex <= 0 || separatorIndex >= targetParsingPath.Length - 1)
				{
					return null;
				}

				var packageFamilyName = targetParsingPath[..separatorIndex];
				var applicationId = targetParsingPath[(separatorIndex + 1)..];

				foreach (var packageDirectoryPath in GetCandidateWindowsAppxPackageDirectories(packageFamilyName))
				{
					var logoRelativePath = GetWindowsAppxLogoRelativePath(packageDirectoryPath, applicationId);
					if (string.IsNullOrWhiteSpace(logoRelativePath))
					{
						continue;
					}

					var logoFilePath = ResolveWindowsAppxLogoFilePath(packageDirectoryPath, logoRelativePath);
					if (logoFilePath is null)
					{
						continue;
					}

					try
					{
						using var stream = File.OpenRead(logoFilePath);
						return new Bitmap(stream);
					}
					catch
					{
					}
				}

				return null;
			});
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private static string? ResolveWindowsAppxLogoFilePath(string packageDirectoryPath, string logoRelativePath)
	{
		var normalizedRelativePath = logoRelativePath
			.Replace('\\', Path.DirectorySeparatorChar)
			.Replace('/', Path.DirectorySeparatorChar);
		var exactLogoFilePath = Path.Combine(packageDirectoryPath, normalizedRelativePath);

		if (File.Exists(exactLogoFilePath))
		{
			return exactLogoFilePath;
		}

		var logoDirectoryPath = Path.GetDirectoryName(exactLogoFilePath);
		if (string.IsNullOrWhiteSpace(logoDirectoryPath) || !Directory.Exists(logoDirectoryPath))
		{
			return null;
		}

		var logoFileName = Path.GetFileNameWithoutExtension(exactLogoFilePath);
		var logoExtension = Path.GetExtension(exactLogoFilePath);

		try
		{
			return Directory.EnumerateFiles(logoDirectoryPath, $"{logoFileName}*{logoExtension}", SearchOption.TopDirectoryOnly)
				.OrderBy(filePath => filePath.Contains("targetsize-32", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
				.ThenBy(filePath => filePath.Contains("scale-100", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
				.ThenBy(filePath => filePath, StringComparer.OrdinalIgnoreCase)
				.FirstOrDefault();
		}
		catch
		{
			return null;
		}
	}

	private void PreloadEntryIconsCore(IEnumerable<LauncherEntry> entries)
	{
		foreach (var entry in entries)
		{
			if (entry.EntryType == LauncherEntryType.Launcher)
			{
				try
				{
					GetEntryIcon(entry);
				}
				catch
				{
				}
			}

			if (entry.Children.Count > 0)
			{
				PreloadEntryIconsCore(entry.Children);
			}
		}
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private static (string? IconPath, int IconIndex) GetDefaultWindowsInternetShortcutIconLocation()
	{
		const string defaultIconLocation = "%SystemRoot%\\system32\\url.dll,5";
		var rawIconLocation = Microsoft.Win32.Registry.GetValue(
			@"HKEY_CLASSES_ROOT\InternetShortcut\DefaultIcon",
			string.Empty,
			defaultIconLocation) as string ?? defaultIconLocation;

		return ParseWindowsIconLocation(rawIconLocation);
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private static bool IsWindowsRemotePath(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return false;
		}

		return path.StartsWith(@"\\", StringComparison.Ordinal)
			|| path.StartsWith("//", StringComparison.Ordinal);
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private static (string? IconPath, int IconIndex) ParseWindowsIconLocation(string? rawIconLocation)
	{
		if (string.IsNullOrWhiteSpace(rawIconLocation))
		{
			return (null, 0);
		}

		var trimmedIconLocation = Environment.ExpandEnvironmentVariables(rawIconLocation.Trim().Trim('"'));
		var separatorIndex = trimmedIconLocation.LastIndexOf(',');
		if (separatorIndex < 0)
		{
			return (trimmedIconLocation, 0);
		}

		var iconPath = trimmedIconLocation[..separatorIndex].Trim().Trim('"');
		var iconIndexText = trimmedIconLocation[(separatorIndex + 1)..].Trim();
		return int.TryParse(iconIndexText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var iconIndex)
			? (iconPath, iconIndex)
			: (trimmedIconLocation, 0);
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private static (string? IconPath, int IconIndex) TryGetWindowsInternetShortcutCustomIconLocation(string shortcutPath)
	{
		if (!string.Equals(Path.GetExtension(shortcutPath), ".url", StringComparison.OrdinalIgnoreCase)
			|| !File.Exists(shortcutPath))
		{
			return (null, 0);
		}

		try
		{
			string? iconFilePath = null;
			var iconIndex = 0;

			foreach (var line in File.ReadLines(shortcutPath))
			{
				if (line.StartsWith("IconFile=", StringComparison.OrdinalIgnoreCase))
				{
					iconFilePath = line["IconFile=".Length..].Trim().Trim('"');
					continue;
				}

				if (line.StartsWith("IconIndex=", StringComparison.OrdinalIgnoreCase))
				{
					var iconIndexText = line["IconIndex=".Length..].Trim();
					if (int.TryParse(iconIndexText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedIconIndex))
					{
						iconIndex = parsedIconIndex;
					}
				}
			}

			if (string.IsNullOrWhiteSpace(iconFilePath))
			{
				return (null, 0);
			}

			var resolvedIconFilePath = Environment.ExpandEnvironmentVariables(iconFilePath);
			if (!Path.IsPathRooted(resolvedIconFilePath))
			{
				var shortcutDirectoryPath = Path.GetDirectoryName(shortcutPath);
				if (string.IsNullOrWhiteSpace(shortcutDirectoryPath))
				{
					return (null, 0);
				}

				resolvedIconFilePath = Path.GetFullPath(Path.Combine(shortcutDirectoryPath, resolvedIconFilePath));
			}

			return IsWindowsRemotePath(resolvedIconFilePath)
				? (null, 0)
				: (resolvedIconFilePath, iconIndex);
		}
		catch
		{
			return (null, 0);
		}
	}

	private static string? GetMacOsPlistStringValue(XElement? dictionaryElement, string key)
	{
		if (dictionaryElement is null)
		{
			return null;
		}

		var elements = dictionaryElement.Elements().ToArray();
		for (var index = 0; index < elements.Length - 1; index++)
		{
			if (!string.Equals(elements[index].Name.LocalName, "key", StringComparison.Ordinal)
				|| !string.Equals(elements[index].Value, key, StringComparison.Ordinal))
			{
				continue;
			}

			var valueElement = elements[index + 1];
			return string.Equals(valueElement.Name.LocalName, "string", StringComparison.Ordinal)
				? valueElement.Value
				: null;
		}

		return null;
	}

	private static XElement? GetMacOsPlistValueElement(XElement? dictionaryElement, string key)
	{
		if (dictionaryElement is null)
		{
			return null;
		}

		var elements = dictionaryElement.Elements().ToArray();
		for (var index = 0; index < elements.Length - 1; index++)
		{
			if (string.Equals(elements[index].Name.LocalName, "key", StringComparison.Ordinal)
				&& string.Equals(elements[index].Value, key, StringComparison.Ordinal))
			{
				return elements[index + 1];
			}
		}

		return null;
	}

	private static string? GetMacOsSymbolicLinkTarget(string path)
	{
		try
		{
			if (Directory.Exists(path))
			{
				var directoryInfo = new DirectoryInfo(path);
				if (string.IsNullOrWhiteSpace(directoryInfo.LinkTarget))
				{
					return null;
				}

				return Path.GetFullPath(Path.Combine(directoryInfo.Parent?.FullName ?? string.Empty, directoryInfo.LinkTarget));
			}

			if (!File.Exists(path))
			{
				return null;
			}

			var fileInfo = new FileInfo(path);
			if (string.IsNullOrWhiteSpace(fileInfo.LinkTarget))
			{
				return null;
			}

			return Path.GetFullPath(Path.Combine(fileInfo.DirectoryName ?? string.Empty, fileInfo.LinkTarget));
		}
		catch
		{
			return null;
		}
	}

	private static byte[]? GetMacOsDataBytes(IntPtr dataHandle)
	{
		if (dataHandle == IntPtr.Zero)
		{
			return null;
		}

		var bytesPointer = MacOsNativeMethods.IntPtr_objc_msgSend(dataHandle, MacOsSelectors.Bytes);
		var length = MacOsNativeMethods.UIntPtr_objc_msgSend(dataHandle, MacOsSelectors.Length);
		if (bytesPointer == IntPtr.Zero || length == UIntPtr.Zero)
		{
			return null;
		}

		var byteCount = checked((int)length.ToUInt64());
		var bytes = new byte[byteCount];
		Marshal.Copy(bytesPointer, bytes, 0, byteCount);
		return bytes;
	}

	private static IntPtr CreateMacOsAutoreleasePool()
	{
		var poolHandle = MacOsNativeMethods.IntPtr_objc_msgSend(MacOsNativeClasses.NSAutoreleasePool, MacOsSelectors.Alloc);
		return MacOsNativeMethods.IntPtr_objc_msgSend(poolHandle, MacOsSelectors.Init);
	}

	private static string? ReadMacOsProcessOutput(string fileName, params string[] arguments)
	{
		try
		{
			var processStartInfo = new ProcessStartInfo
			{
				FileName = fileName,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				UseShellExecute = false
			};

			foreach (var argument in arguments)
			{
				processStartInfo.ArgumentList.Add(argument);
			}

			using var process = Process.Start(processStartInfo);
			if (process is null)
			{
				return null;
			}

			var standardOutput = process.StandardOutput.ReadToEnd();
			process.WaitForExit(5000);
			return process.ExitCode == 0
				? standardOutput
				: null;
		}
		catch
		{
			return null;
		}
	}

	private static void ReleaseMacOsObject(IntPtr handle)
	{
		if (handle != IntPtr.Zero)
		{
			MacOsNativeMethods.void_objc_msgSend(handle, MacOsSelectors.Release);
		}
	}

	private static IReadOnlyList<string> GetMacOsBundleIconNames(string appBundlePath)
	{
		var infoPlistPath = Path.Combine(appBundlePath, "Contents", "Info.plist");
		if (!File.Exists(infoPlistPath))
		{
			return Array.Empty<string>();
		}

		var document = TryLoadMacOsPlistDocument(infoPlistPath);
		var rootDictionaryElement = document?
			.Root?
			.Elements()
			.FirstOrDefault(element => string.Equals(element.Name.LocalName, "dict", StringComparison.Ordinal));

		if (rootDictionaryElement is null)
		{
			return Array.Empty<string>();
		}

		var iconNames = new List<string>();
		AddMacOsIconName(iconNames, GetMacOsPlistStringValue(rootDictionaryElement, "CFBundleIconFile"));
		AddMacOsIconName(iconNames, GetMacOsPlistStringValue(rootDictionaryElement, "CFBundleIconName"));

		var bundleIconsElement = GetMacOsPlistValueElement(rootDictionaryElement, "CFBundleIcons");
		var primaryIconElement = GetMacOsPlistValueElement(bundleIconsElement, "CFBundlePrimaryIcon");
		var iconFilesElement = GetMacOsPlistValueElement(primaryIconElement, "CFBundleIconFiles");
		if (iconFilesElement is not null
			&& string.Equals(iconFilesElement.Name.LocalName, "array", StringComparison.Ordinal))
		{
			foreach (var iconFileElement in iconFilesElement.Elements())
			{
				if (string.Equals(iconFileElement.Name.LocalName, "string", StringComparison.Ordinal))
				{
					AddMacOsIconName(iconNames, iconFileElement.Value);
				}
			}
		}

		return iconNames;
	}

	private static void AddMacOsIconName(ICollection<string> iconNames, string? iconName)
	{
		if (string.IsNullOrWhiteSpace(iconName))
		{
			return;
		}

		var normalizedIconName = iconName.Trim();
		if (iconNames.Contains(normalizedIconName, StringComparer.OrdinalIgnoreCase))
		{
			return;
		}

		iconNames.Add(normalizedIconName);
	}

	private static string? ResolveMacOsBundleIconFilePath(string appBundlePath)
	{
		var resourcesDirectoryPath = Path.Combine(appBundlePath, "Contents", "Resources");
		if (!Directory.Exists(resourcesDirectoryPath))
		{
			return null;
		}

		foreach (var iconName in GetMacOsBundleIconNames(appBundlePath))
		{
			foreach (var candidatePath in EnumerateMacOsBundleIconCandidates(resourcesDirectoryPath, iconName))
			{
				if (File.Exists(candidatePath))
				{
					return candidatePath;
				}
			}
		}

		try
		{
			return Directory.EnumerateFiles(resourcesDirectoryPath, "*.icns", SearchOption.TopDirectoryOnly)
				.OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
				.FirstOrDefault();
		}
		catch
		{
			return null;
		}
	}

	private static IEnumerable<string> EnumerateMacOsBundleIconCandidates(string resourcesDirectoryPath, string iconName)
	{
		var candidatePaths = new List<string>
		{
			Path.Combine(resourcesDirectoryPath, iconName)
		};

		if (string.IsNullOrWhiteSpace(Path.GetExtension(iconName)))
		{
			candidatePaths.Add(Path.Combine(resourcesDirectoryPath, iconName + ".icns"));
			candidatePaths.Add(Path.Combine(resourcesDirectoryPath, iconName + ".png"));
		}

		try
		{
			candidatePaths.AddRange(Directory.EnumerateFiles(resourcesDirectoryPath, iconName + "*", SearchOption.TopDirectoryOnly)
				.OrderBy(path => path.Contains(".icns", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
				.ThenBy(path => path, StringComparer.OrdinalIgnoreCase));
		}
		catch
		{
		}

		return candidatePaths;
	}

	private static XDocument? TryLoadMacOsPlistDocument(string plistPath)
	{
		try
		{
			return XDocument.Load(plistPath, LoadOptions.None);
		}
		catch
		{
		}

		var xmlContent = ReadMacOsProcessOutput("plutil", "-convert", "xml1", "-o", "-", plistPath);
		if (string.IsNullOrWhiteSpace(xmlContent))
		{
			return null;
		}

		try
		{
			return XDocument.Parse(xmlContent, LoadOptions.None);
		}
		catch
		{
			return null;
		}
	}

	private Bitmap? TryLoadMacOsAppBundleIcon(string path)
	{
		if (!Directory.Exists(path)
			|| !string.Equals(Path.GetExtension(path), ".app", StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}

		var iconFilePath = ResolveMacOsBundleIconFilePath(path);
		if (string.IsNullOrWhiteSpace(iconFilePath))
		{
			return TryLoadMacOsQuickLookIcon(path);
		}

		return TryLoadMacOsBitmapFromFile(iconFilePath)
			?? TryLoadMacOsQuickLookIcon(path);
	}

	private Bitmap? TryLoadMacOsBitmapFromFile(string filePath)
	{
		if (!File.Exists(filePath))
		{
			return null;
		}

		if (string.Equals(Path.GetExtension(filePath), ".icns", StringComparison.OrdinalIgnoreCase))
		{
			return TryLoadMacOsIcnsBitmap(filePath);
		}

		try
		{
			using var stream = File.OpenRead(filePath);
			return new Bitmap(stream);
		}
		catch
		{
			return null;
		}
	}

	private Bitmap? TryLoadMacOsEntryIcon(string sourcePath)
	{
		if (!OperatingSystem.IsMacOS())
		{
			return null;
		}

		var resolvedPath = GetMacOsSymbolicLinkTarget(sourcePath) ?? sourcePath;
		return TryLoadMacOsAppBundleIcon(resolvedPath)
			?? TryLoadMacOsQuickLookIcon(resolvedPath);
	}

	private Bitmap? TryLoadMacOsWorkspaceIcon(string sourcePath)
	{
		if (!OperatingSystem.IsMacOS())
		{
			return null;
		}

		return _macOsWorkspaceIconCache.GetOrAdd(
			sourcePath,
			path =>
			{
				if (!File.Exists(path) && !Directory.Exists(path))
				{
					return null;
				}

				var poolHandle = CreateMacOsAutoreleasePool();
				try
				{
					var workspaceHandle = MacOsNativeMethods.IntPtr_objc_msgSend(MacOsNativeClasses.NSWorkspace, MacOsSelectors.SharedWorkspace);
					if (workspaceHandle == IntPtr.Zero)
					{
						return null;
					}

					var imageHandle = MacOsNativeMethods.IntPtr_objc_msgSend_String(workspaceHandle, MacOsSelectors.IconForFile, path);
					if (imageHandle == IntPtr.Zero)
					{
						return null;
					}

					MacOsNativeMethods.void_objc_msgSend_NativeSize(
						imageHandle,
						MacOsSelectors.SetSize,
						new MacOsNativeSize
						{
							Width = MacOsWorkspaceIconSize,
							Height = MacOsWorkspaceIconSize
						});

					var tiffDataHandle = MacOsNativeMethods.IntPtr_objc_msgSend(imageHandle, MacOsSelectors.TIFFRepresentation);
					if (tiffDataHandle == IntPtr.Zero)
					{
						return null;
					}

					var bitmapImageRepHandle = MacOsNativeMethods.IntPtr_objc_msgSend_IntPtr(
						MacOsNativeClasses.NSBitmapImageRep,
						MacOsSelectors.ImageRepWithData,
						tiffDataHandle);
					if (bitmapImageRepHandle == IntPtr.Zero)
					{
						return null;
					}

					var pngDataHandle = MacOsNativeMethods.IntPtr_objc_msgSend_Int64_IntPtr(
						bitmapImageRepHandle,
						MacOsSelectors.RepresentationUsingTypeProperties,
						MacOsPngImageFileType,
						IntPtr.Zero);
					var pngBytes = GetMacOsDataBytes(pngDataHandle);
					if (pngBytes is null || pngBytes.Length == 0)
					{
						return null;
					}

					using var memoryStream = new MemoryStream(pngBytes, writable: false);
					return new Bitmap(memoryStream);
				}
				catch
				{
					return null;
				}
				finally
				{
					ReleaseMacOsObject(poolHandle);
				}
			});
	}

	private Bitmap? TryLoadMacOsIcnsBitmap(string iconFilePath)
	{
		var temporaryDirectoryPath = Path.Combine(Path.GetTempPath(), "TreeTray", "MacIcons", Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
		Directory.CreateDirectory(temporaryDirectoryPath);
		var outputFilePath = Path.Combine(temporaryDirectoryPath, "icon.png");

		try
		{
			var processStartInfo = new ProcessStartInfo
			{
				FileName = "sips",
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				UseShellExecute = false
			};
			processStartInfo.ArgumentList.Add("-s");
			processStartInfo.ArgumentList.Add("format");
			processStartInfo.ArgumentList.Add("png");
			processStartInfo.ArgumentList.Add(iconFilePath);
			processStartInfo.ArgumentList.Add("--out");
			processStartInfo.ArgumentList.Add(outputFilePath);

			using var process = Process.Start(processStartInfo);
			if (process is null)
			{
				return null;
			}

			process.WaitForExit(5000);
			if (process.ExitCode != 0 || !File.Exists(outputFilePath))
			{
				return null;
			}

			using var stream = File.OpenRead(outputFilePath);
			return new Bitmap(stream);
		}
		catch
		{
			return null;
		}
		finally
		{
			try
			{
				Directory.Delete(temporaryDirectoryPath, recursive: true);
			}
			catch
			{
			}
		}
	}

	private Bitmap? TryLoadMacOsQuickLookIcon(string path)
	{
		if (!OperatingSystem.IsMacOS()
			|| (!File.Exists(path) && !Directory.Exists(path)))
		{
			return null;
		}

		var temporaryDirectoryPath = Path.Combine(Path.GetTempPath(), "TreeTray", "QuickLook", Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
		Directory.CreateDirectory(temporaryDirectoryPath);

		try
		{
			var processStartInfo = new ProcessStartInfo
			{
				FileName = "qlmanage",
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				UseShellExecute = false
			};
			processStartInfo.ArgumentList.Add("-t");
			processStartInfo.ArgumentList.Add("-s");
			processStartInfo.ArgumentList.Add("64");
			processStartInfo.ArgumentList.Add("-o");
			processStartInfo.ArgumentList.Add(temporaryDirectoryPath);
			processStartInfo.ArgumentList.Add(path);

			using var process = Process.Start(processStartInfo);
			if (process is null)
			{
				return null;
			}

			process.WaitForExit(5000);
			if (process.ExitCode != 0)
			{
				return null;
			}

			var thumbnailPath = Directory.EnumerateFiles(temporaryDirectoryPath, "*.png", SearchOption.AllDirectories)
				.OrderByDescending(File.GetLastWriteTimeUtc)
				.FirstOrDefault();

			if (string.IsNullOrWhiteSpace(thumbnailPath))
			{
				return null;
			}

			using var stream = File.OpenRead(thumbnailPath);
			return new Bitmap(stream);
		}
		catch
		{
			return null;
		}
		finally
		{
			try
			{
				Directory.Delete(temporaryDirectoryPath, recursive: true);
			}
			catch
			{
			}
		}
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private static Bitmap? TryExtractAssociatedWindowsIcon(string path)
	{
		if (!OperatingSystem.IsWindows() || string.IsNullOrWhiteSpace(path) || !File.Exists(path))
		{
			return null;
		}

		try
		{
			using var icon = System.Drawing.Icon.ExtractAssociatedIcon(path);
			if (icon is null)
			{
				return null;
			}

			using var drawingBitmap = icon.ToBitmap();
			return ConvertDrawingBitmap(drawingBitmap);
		}
		catch
		{
			return null;
		}
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private static Bitmap? TryLoadWindowsInternetShortcutIcon(string shortcutPath)
	{
		if (!string.Equals(Path.GetExtension(shortcutPath), ".url", StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}

		var (iconPath, iconIndex) = TryGetWindowsInternetShortcutCustomIconLocation(shortcutPath);
		if (string.IsNullOrWhiteSpace(iconPath))
		{
			(iconPath, iconIndex) = GetDefaultWindowsInternetShortcutIconLocation();
		}

		return TryLoadWindowsIconResource(iconPath, iconIndex)
			?? TryLoadWindowsShellIcon(shortcutPath)
			?? TryExtractAssociatedWindowsIcon(shortcutPath);
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private static string? TryGetWindowsShortcutTargetParsingPath(string shortcutPath)
	{
		try
		{
			using var shellAccessor = new WindowsShortcutShellAccessor();
			return shellAccessor.GetTargetParsingPath(shortcutPath);
		}
		catch
		{
			return null;
		}
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private static Bitmap? TryLoadWindowsShellIcon(string path)
	{
		if (!OperatingSystem.IsWindows() || string.IsNullOrWhiteSpace(path))
		{
			return null;
		}

		var flags = WindowsShellFileInfoFlags.Icon | WindowsShellFileInfoFlags.SmallIcon;
		var shellResult = SHGetFileInfo(
			path,
			0,
			out var shellFileInfo,
			(uint)Marshal.SizeOf<WindowsShellFileInfo>(),
			(uint)flags);

		if (shellResult == IntPtr.Zero || shellFileInfo.IconHandle == IntPtr.Zero)
		{
			return null;
		}

		try
		{
			using var icon = System.Drawing.Icon.FromHandle(shellFileInfo.IconHandle);
			using var drawingBitmap = icon.ToBitmap();
			return ConvertDrawingBitmap(drawingBitmap);
		}
		catch
		{
			return null;
		}
		finally
		{
			DestroyIcon(shellFileInfo.IconHandle);
		}
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private static Bitmap? TryLoadWindowsIconResource(string? iconPath, int iconIndex)
	{
		if (string.IsNullOrWhiteSpace(iconPath) || !File.Exists(iconPath))
		{
			return null;
		}

		var smallIcons = new IntPtr[1];
		try
		{
			var extractedIconsCount = ExtractIconEx(iconPath, iconIndex, null, smallIcons, 1);
			if (extractedIconsCount == 0 || smallIcons[0] == IntPtr.Zero)
			{
				return null;
			}

			using var icon = System.Drawing.Icon.FromHandle(smallIcons[0]);
			using var drawingBitmap = icon.ToBitmap();
			return ConvertDrawingBitmap(drawingBitmap);
		}
		catch
		{
			return null;
		}
		finally
		{
			if (smallIcons[0] != IntPtr.Zero)
			{
				DestroyIcon(smallIcons[0]);
			}
		}
	}

	[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
	private static extern uint ExtractIconEx(
		string fileName,
		int iconIndex,
		IntPtr[]? largeIcons,
		IntPtr[]? smallIcons,
		uint iconsCount);

	[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
	private static extern IntPtr SHGetFileInfo(
		string path,
		uint fileAttributes,
		out WindowsShellFileInfo shellFileInfo,
		uint shellFileInfoSize,
		uint flags);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool DestroyIcon(IntPtr iconHandle);

	#endregion

	#region Struct: WindowsShellFileInfo

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct WindowsShellFileInfo
	{
		#region Fields: Public

		public IntPtr IconHandle;

		public int IconIndex;

		public uint Attributes;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string DisplayName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
		public string TypeName;

		#endregion
	}

	#endregion

	#region Enum: WindowsShellFileInfoFlags

	[Flags]
	private enum WindowsShellFileInfoFlags : uint
	{
		Icon = 0x000000100,
		SmallIcon = 0x000000001
	}

	#endregion

	#region Methods: Public

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	public nint CreateWindowsTrayIconHandle()
	{
		if (!OperatingSystem.IsWindows())
		{
			return IntPtr.Zero;
		}

		using var stream = AssetLoader.Open(new Uri("avares://TreeTray/Assets/AppIcon.png"));
		using var drawingBitmap = new System.Drawing.Bitmap(stream);
		return drawingBitmap.GetHicon();
	}

	public Bitmap GetEntryIcon(LauncherEntry entry)
	{
		ArgumentNullException.ThrowIfNull(entry);

		try
		{
			if (OperatingSystem.IsMacOS())
			{
				return GetOrCreateMacOsEntryIcon(
					entry.SourcePath,
					entry.EntryType == LauncherEntryType.Folder
						? FolderIcon
						: LauncherIcon);
			}

			if (entry.EntryType == LauncherEntryType.Folder)
			{
				return FolderIcon;
			}

			if (!OperatingSystem.IsWindows())
			{
				return LauncherIcon;
			}

			return GetOrCreateWindowsEntryIcon(entry.SourcePath);
		}
		catch
		{
			return entry.EntryType == LauncherEntryType.Folder
				? FolderIcon
				: LauncherIcon;
		}
	}

	public void PreloadEntryIcons(IEnumerable<LauncherEntry> entries)
	{
		ArgumentNullException.ThrowIfNull(entries);
		PreloadEntryIconsCore(entries);
	}

	#endregion
}

#endregion
