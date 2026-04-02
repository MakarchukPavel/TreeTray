#region Class: StartupRegistrationService

namespace TreeTray.Services;

public sealed class StartupRegistrationService : IStartupRegistrationService
{
	#region Constants: Private

	private const string LinuxAutostartFileName = "TreeTray.desktop";

	private const string MacLaunchAgentFileName = "com.treetray.launcher.plist";

	private const string WindowsStartupFileName = "TreeTray.cmd";

	#endregion

	#region Fields: Private

	private readonly IApplicationPaths _applicationPaths;

	#endregion

	#region Constructors: Public

	public StartupRegistrationService(IApplicationPaths applicationPaths)
	{
		_applicationPaths = applicationPaths;
	}

	#endregion

	#region Methods: Private

	private string BuildAutostartFileName(string defaultFileName)
	{
		if (!_applicationPaths.HasCustomConfigurationFilePath)
		{
			return defaultFileName;
		}

		var fileExtension = Path.GetExtension(defaultFileName);
		var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(defaultFileName);
		return $"{fileNameWithoutExtension}.{BuildStartupInstanceSuffix()}{fileExtension}";
	}

	private string BuildLaunchAgentLabel()
	{
		const string defaultLabel = "com.treetray.launcher";
		return !_applicationPaths.HasCustomConfigurationFilePath
			? defaultLabel
			: $"{defaultLabel}.{BuildStartupInstanceSuffix()}";
	}

	private string BuildStartupInstanceSuffix()
	{
		var hashBytes = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(_applicationPaths.ConfigurationFilePath));
		return Convert.ToHexString(hashBytes)[..12].ToLowerInvariant();
	}

	private void ApplyLinux(bool isEnabled, LaunchCommand command)
	{
		var directoryPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"autostart");
		var filePath = Path.Combine(directoryPath, BuildAutostartFileName(LinuxAutostartFileName));

		if (!isEnabled)
		{
			DeleteFile(filePath);
			return;
		}

		Directory.CreateDirectory(directoryPath);

		var execLine = string.Join(' ', BuildProgramArguments(command).Select(EscapeDesktopArgument));
		var content = $"""
[Desktop Entry]
Type=Application
Version=1.0
Name=TreeTray
Comment=Start TreeTray automatically after sign-in.
Exec={execLine}
Terminal=false
X-GNOME-Autostart-enabled=true
""";

		File.WriteAllText(filePath, content, new UTF8Encoding(false));
	}

	private void ApplyMac(bool isEnabled, LaunchCommand command)
	{
		var directoryPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.Personal),
			"Library",
			"LaunchAgents");
		var filePath = Path.Combine(directoryPath, BuildAutostartFileName(MacLaunchAgentFileName));

		if (!isEnabled)
		{
			DeleteFile(filePath);
			return;
		}

		Directory.CreateDirectory(directoryPath);

		var argumentsXml = string.Join(
			Environment.NewLine,
			BuildProgramArguments(command).Select(argument => $"    <string>{SecurityElement.Escape(argument)}</string>"));
		var workingDirectory = command.WorkingDirectory ?? _applicationPaths.ApplicationDirectory;
		var content = $"""
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>Label</key>
  <string>{BuildLaunchAgentLabel()}</string>
  <key>ProgramArguments</key>
  <array>
{argumentsXml}
  </array>
  <key>RunAtLoad</key>
  <true/>
  <key>KeepAlive</key>
  <false/>
  <key>WorkingDirectory</key>
  <string>{SecurityElement.Escape(workingDirectory)}</string>
</dict>
</plist>
""";

		File.WriteAllText(filePath, content, new UTF8Encoding(false));
	}

	private void ApplyWindows(bool isEnabled, LaunchCommand command)
	{
		var directoryPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"Microsoft",
			"Windows",
			"Start Menu",
			"Programs",
			"Startup");
		var filePath = Path.Combine(directoryPath, BuildAutostartFileName(WindowsStartupFileName));

		if (!isEnabled)
		{
			DeleteFile(filePath);
			return;
		}

		Directory.CreateDirectory(directoryPath);

		var commandLine = string.Join(' ', BuildProgramArguments(command).Select(EscapeBatchArgument));
		var content = $"""
@echo off
start "" {commandLine}
""";

		File.WriteAllText(filePath, content, new UTF8Encoding(false));
	}

	private static IReadOnlyList<string> BuildProgramArguments(LaunchCommand command)
	{
		return new[] { command.FileName }.Concat(command.Arguments).ToArray();
	}

	private static void DeleteFile(string filePath)
	{
		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}
	}

	private static string EscapeBatchArgument(string argument)
	{
		return $"\"{argument.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
	}

	private static string EscapeDesktopArgument(string argument)
	{
		if (argument.Any(char.IsWhiteSpace))
		{
			return $"\"{argument.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
		}

		return argument;
	}

	#endregion

	#region Methods: Public

	public void Apply(TreeTrayConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(configuration);

		var launchCommand = _applicationPaths.GetCurrentApplicationLaunchCommand();

		if (OperatingSystem.IsWindows())
		{
			ApplyWindows(configuration.StartWithOperatingSystem, launchCommand);
			return;
		}

		if (OperatingSystem.IsMacOS())
		{
			ApplyMac(configuration.StartWithOperatingSystem, launchCommand);
			return;
		}

		ApplyLinux(configuration.StartWithOperatingSystem, launchCommand);
	}

	#endregion
}

#endregion
