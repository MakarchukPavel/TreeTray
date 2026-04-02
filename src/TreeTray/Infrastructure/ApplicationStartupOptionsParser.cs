#region Class: ApplicationStartupOptionsParser

namespace TreeTray.Infrastructure;

public static class ApplicationStartupOptionsParser
{
	#region Constants: Private

	private const string ConfigLongOptionName = "--config";

	private const string ConfigShortOptionName = "-c";

	private const string WindowsConfigOptionName = "/config";

	#endregion

	#region Methods: Private

	private static string NormalizeConfigurationFilePath(string rawPath)
	{
		var expandedPath = Environment.ExpandEnvironmentVariables(rawPath.Trim().Trim('"'));

		if (Path.IsPathRooted(expandedPath))
		{
			return Path.GetFullPath(expandedPath);
		}

		return Path.GetFullPath(expandedPath, Environment.CurrentDirectory);
	}

	private static bool TryParseConfigurationOption(string argument, out string? value, out bool consumesNextArgument)
	{
		ArgumentNullException.ThrowIfNull(argument);

		if (string.Equals(argument, ConfigLongOptionName, StringComparison.OrdinalIgnoreCase)
			|| string.Equals(argument, ConfigShortOptionName, StringComparison.OrdinalIgnoreCase)
			|| string.Equals(argument, WindowsConfigOptionName, StringComparison.OrdinalIgnoreCase))
		{
			value = null;
			consumesNextArgument = true;
			return true;
		}

		foreach (var prefix in new[] { $"{ConfigLongOptionName}=", $"{ConfigLongOptionName}:", $"{WindowsConfigOptionName}=", $"{WindowsConfigOptionName}:" })
		{
			if (argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				value = argument[prefix.Length..];
				consumesNextArgument = false;
				return true;
			}
		}

		value = null;
		consumesNextArgument = false;
		return false;
	}

	#endregion

	#region Methods: Public

	public static ApplicationStartupOptions Parse(string[] args)
	{
		ArgumentNullException.ThrowIfNull(args);

		for (var index = 0; index < args.Length; index++)
		{
			var argument = args[index];
			if (string.IsNullOrWhiteSpace(argument))
			{
				continue;
			}

			if (TryParseConfigurationOption(argument, out var inlineValue, out var consumesNextArgument))
			{
				if (!string.IsNullOrWhiteSpace(inlineValue))
				{
					return new ApplicationStartupOptions(NormalizeConfigurationFilePath(inlineValue));
				}

				if (consumesNextArgument
					&& index + 1 < args.Length
					&& !string.IsNullOrWhiteSpace(args[index + 1]))
				{
					return new ApplicationStartupOptions(NormalizeConfigurationFilePath(args[index + 1]));
				}

				break;
			}

			return new ApplicationStartupOptions(NormalizeConfigurationFilePath(argument));
		}

		return new ApplicationStartupOptions();
	}

	#endregion
}

#endregion
