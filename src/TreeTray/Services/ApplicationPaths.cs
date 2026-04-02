#region Class: ApplicationPaths

namespace TreeTray.Services;

public sealed class ApplicationPaths : IApplicationPaths
{
	#region Constants: Private

	private const string ApplicationName = "TreeTray";

	private const string ConfigurationFileName = "TreeTray.settings.xml";

	#endregion

	#region Fields: Private

	private readonly string _configurationFilePath;

	private readonly string _configurationDirectory;

	private readonly ApplicationStartupOptions _startupOptions;

	#endregion

	#region Constructors: Public

	public ApplicationPaths(ApplicationStartupOptions startupOptions)
	{
		_startupOptions = startupOptions;
		_configurationFilePath = ResolveConfigurationFilePath(startupOptions);
		_configurationDirectory = ResolveConfigurationDirectory(_configurationFilePath);
		Directory.CreateDirectory(_configurationDirectory);
	}

	#endregion

	#region Properties: Public

	public string ApplicationDirectory => AppContext.BaseDirectory;

	public string ConfigurationDirectory => _configurationDirectory;

	public string ConfigurationFilePath => _configurationFilePath;

	public bool HasCustomConfigurationFilePath => _startupOptions.HasCustomConfigurationFilePath;

	#endregion

	#region Methods: Private

	private IReadOnlyList<string> BuildStartupArguments(string? entryAssemblyLocation = null)
	{
		var arguments = new List<string>();
		if (!string.IsNullOrWhiteSpace(entryAssemblyLocation))
		{
			arguments.Add(entryAssemblyLocation);
		}

		if (HasCustomConfigurationFilePath)
		{
			arguments.Add("--config");
			arguments.Add(ConfigurationFilePath);
		}

		return arguments;
	}

	private static string GetDefaultConfigurationDirectory()
	{
		if (OperatingSystem.IsWindows())
		{
			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				ApplicationName);
		}

		if (OperatingSystem.IsMacOS())
		{
			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.Personal),
				"Library",
				"Application Support",
				ApplicationName);
		}

		return Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			ApplicationName);
	}

	private static string GetDefaultConfigurationFilePath()
	{
		return Path.Combine(GetDefaultConfigurationDirectory(), ConfigurationFileName);
	}

	private static string ResolveConfigurationDirectory(string configurationFilePath)
	{
		var configurationDirectory = Path.GetDirectoryName(configurationFilePath);
		if (string.IsNullOrWhiteSpace(configurationDirectory))
		{
			throw new InvalidOperationException("The configuration directory could not be resolved.");
		}

		return configurationDirectory;
	}

	private static string ResolveConfigurationFilePath(ApplicationStartupOptions startupOptions)
	{
		ArgumentNullException.ThrowIfNull(startupOptions);

		if (!startupOptions.HasCustomConfigurationFilePath)
		{
			return GetDefaultConfigurationFilePath();
		}

		var configurationFilePath = startupOptions.ConfigurationFilePathOverride
			?? throw new InvalidOperationException("The custom configuration file path is unavailable.");
		return Path.GetFullPath(configurationFilePath);
	}

	#endregion

	#region Methods: Public

	public LaunchCommand GetCurrentApplicationLaunchCommand()
	{
		var processPath = Environment.ProcessPath
			?? throw new InvalidOperationException("The current process path is unavailable.");
		var entryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;
		var processName = Path.GetFileNameWithoutExtension(processPath);

		if (string.Equals(processName, "dotnet", StringComparison.OrdinalIgnoreCase)
			&& !string.IsNullOrWhiteSpace(entryAssemblyLocation)
			&& string.Equals(Path.GetExtension(entryAssemblyLocation), ".dll", StringComparison.OrdinalIgnoreCase))
		{
			return new LaunchCommand(
				processPath,
				BuildStartupArguments(entryAssemblyLocation),
				Path.GetDirectoryName(entryAssemblyLocation),
				useShellExecute: false);
		}

		return new LaunchCommand(
			processPath,
			BuildStartupArguments(),
			Path.GetDirectoryName(processPath),
			useShellExecute: false);
	}

	public string GetDefaultLaunchersDirectory()
	{
		if (OperatingSystem.IsWindows())
		{
			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"Microsoft",
				"Windows",
				"Start Menu",
				"Programs");
		}

		if (OperatingSystem.IsMacOS())
		{
			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.Personal),
				"Applications");
		}

		return Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.Personal),
			".local",
			"share",
			"applications");
	}

	#endregion
}

#endregion
