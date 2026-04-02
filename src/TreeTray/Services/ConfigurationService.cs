#region Class: ConfigurationService

namespace TreeTray.Services;

public sealed class ConfigurationService : IConfigurationService
{
	#region Fields: Private

	private readonly IApplicationPaths _applicationPaths;

	#endregion

	#region Constructors: Public

	public ConfigurationService(IApplicationPaths applicationPaths)
	{
		_applicationPaths = applicationPaths;
	}

	#endregion

	#region Methods: Private

	private static bool GetBooleanValue(XElement? parentElement, string elementName, bool defaultValue)
	{
		var rawValue = parentElement?.Element(elementName)?.Value;
		return bool.TryParse(rawValue, out var parsedValue) ? parsedValue : defaultValue;
	}

	private static string GetStringValue(XElement? parentElement, string elementName, string defaultValue)
	{
		var rawValue = parentElement?.Element(elementName)?.Value;
		return string.IsNullOrWhiteSpace(rawValue)
			? defaultValue
			: rawValue.Trim();
	}

	private TreeTrayConfiguration CreateDefaultConfiguration()
	{
		return new TreeTrayConfiguration
		{
			LaunchersDirectory = _applicationPaths.GetDefaultLaunchersDirectory(),
			EnableTrayIcon = true,
			EnableTaskbarDockIcon = false,
			InvertTrayIconMouseButtons = false,
			StartWithOperatingSystem = false,
			TrayIconBackgroundColor = string.Empty,
			TrayIconForegroundColor = string.Empty,
			TrayIconGlyph = string.Empty,
			TrayToolTipText = string.Empty
		};
	}

	private string NormalizeDirectoryPath(string rawPath)
	{
		var expandedPath = Environment.ExpandEnvironmentVariables(rawPath.Trim());

		if (Path.IsPathRooted(expandedPath))
		{
			return Path.GetFullPath(expandedPath);
		}

		return Path.GetFullPath(expandedPath, _applicationPaths.ConfigurationDirectory);
	}

	private TreeTrayConfiguration Parse(XDocument document, TreeTrayConfiguration defaultConfiguration)
	{
		var rootElement = document.Element(nameof(TreeTrayConfiguration));
		var rawLaunchersDirectory = rootElement?.Element(nameof(TreeTrayConfiguration.LaunchersDirectory))?.Value;
		var launchersDirectory = string.IsNullOrWhiteSpace(rawLaunchersDirectory)
			? defaultConfiguration.LaunchersDirectory
			: NormalizeDirectoryPath(rawLaunchersDirectory);
		var enableTrayIcon = GetBooleanValue(rootElement, nameof(TreeTrayConfiguration.EnableTrayIcon), defaultConfiguration.EnableTrayIcon);

		return new TreeTrayConfiguration
		{
			LaunchersDirectory = launchersDirectory,
			EnableTrayIcon = enableTrayIcon,
			EnableTaskbarDockIcon = !enableTrayIcon,
			InvertTrayIconMouseButtons = GetBooleanValue(rootElement, nameof(TreeTrayConfiguration.InvertTrayIconMouseButtons), defaultConfiguration.InvertTrayIconMouseButtons),
			StartWithOperatingSystem = GetBooleanValue(rootElement, nameof(TreeTrayConfiguration.StartWithOperatingSystem), defaultConfiguration.StartWithOperatingSystem),
			TrayIconBackgroundColor = GetStringValue(rootElement, nameof(TreeTrayConfiguration.TrayIconBackgroundColor), defaultConfiguration.TrayIconBackgroundColor),
			TrayIconForegroundColor = GetStringValue(rootElement, nameof(TreeTrayConfiguration.TrayIconForegroundColor), defaultConfiguration.TrayIconForegroundColor),
			TrayIconGlyph = GetStringValue(rootElement, nameof(TreeTrayConfiguration.TrayIconGlyph), defaultConfiguration.TrayIconGlyph),
			TrayToolTipText = GetStringValue(rootElement, nameof(TreeTrayConfiguration.TrayToolTipText), defaultConfiguration.TrayToolTipText)
		};
	}

	private void WriteConfigurationFile(TreeTrayConfiguration configuration)
	{
		Directory.CreateDirectory(_applicationPaths.ConfigurationDirectory);

		var settings = new XmlWriterSettings
		{
			Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
			Indent = true,
			NewLineChars = Environment.NewLine,
			OmitXmlDeclaration = false
		};

		using var writer = XmlWriter.Create(_applicationPaths.ConfigurationFilePath, settings);

		writer.WriteStartDocument();
		writer.WriteComment("TreeTray configuration file.");
		writer.WriteComment("Edit the values below and reload the application from the launcher window or the tray menu.");
		writer.WriteStartElement(nameof(TreeTrayConfiguration));

		WriteSetting(
			writer,
			nameof(TreeTrayConfiguration.LaunchersDirectory),
			configuration.LaunchersDirectory,
			"The absolute or relative path to the directory that contains launchers. Relative paths are resolved from the configuration file directory. Default: "
			+ configuration.LaunchersDirectory);

		WriteSetting(
			writer,
			nameof(TreeTrayConfiguration.EnableTrayIcon),
			configuration.EnableTrayIcon.ToString().ToLowerInvariant(),
			"When true, TreeTray shows a tray icon on Windows and Linux, or a menu bar item on macOS. When false, TreeTray keeps the launcher window in the Windows taskbar, the macOS Dock, or the Linux shell task switcher. Accepted values: true, false. Default: true.");

		WriteSetting(
			writer,
			nameof(TreeTrayConfiguration.StartWithOperatingSystem),
			configuration.StartWithOperatingSystem.ToString().ToLowerInvariant(),
			"When true, TreeTray registers itself to start automatically after sign-in. Accepted values: true, false. Default: false.");

		WriteSetting(
			writer,
			nameof(TreeTrayConfiguration.InvertTrayIconMouseButtons),
			configuration.InvertTrayIconMouseButtons.ToString().ToLowerInvariant(),
			"When true, TreeTray inverts the tray or menu bar click behavior on Windows and macOS: left click opens the main window and right click opens the launcher menu. When false, left click opens the launcher menu and right click opens the main window. Accepted values: true, false. Default: false.");

		WriteSetting(
			writer,
			nameof(TreeTrayConfiguration.TrayIconGlyph),
			configuration.TrayIconGlyph,
			"Optional tray icon glyph. TreeTray uses the first visible text element from this value and renders it into a generated tray icon. Leave empty to use the default application icon.");

		WriteSetting(
			writer,
			nameof(TreeTrayConfiguration.TrayIconForegroundColor),
			configuration.TrayIconForegroundColor,
			"Optional foreground color for the generated tray icon glyph. Use HTML-style colors such as #FFFFFF. Leave empty to use the default value.");

		WriteSetting(
			writer,
			nameof(TreeTrayConfiguration.TrayIconBackgroundColor),
			configuration.TrayIconBackgroundColor,
			"Optional background color for the generated tray icon glyph. Use HTML-style colors such as #2F6FED. Leave empty to use the default value.");

		WriteSetting(
			writer,
			nameof(TreeTrayConfiguration.TrayToolTipText),
			configuration.TrayToolTipText,
			"Optional tooltip shown when the pointer hovers over the tray icon or the menu bar item. TreeTray also uses this value in the launcher window title and in the internal window header. Leave empty to use the default application name.");

		writer.WriteEndElement();
		writer.WriteEndDocument();
	}

	private static void WriteSetting(XmlWriter writer, string elementName, string value, string documentation)
	{
		writer.WriteComment(documentation);
		writer.WriteElementString(elementName, value);
	}

	#endregion

	#region Methods: Public

	public TreeTrayConfiguration Load()
	{
		var defaultConfiguration = CreateDefaultConfiguration();

		if (!File.Exists(_applicationPaths.ConfigurationFilePath))
		{
			WriteConfigurationFile(defaultConfiguration);
			return defaultConfiguration;
		}

		try
		{
			var document = XDocument.Load(_applicationPaths.ConfigurationFilePath, LoadOptions.PreserveWhitespace);
			return Parse(document, defaultConfiguration);
		}
		catch
		{
			return defaultConfiguration;
		}
	}

	public void Save(TreeTrayConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(configuration);
		WriteConfigurationFile(configuration);
	}

	#endregion
}

#endregion
