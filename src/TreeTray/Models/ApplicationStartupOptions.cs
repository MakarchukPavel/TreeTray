#region Class: ApplicationStartupOptions

namespace TreeTray.Models;

public sealed class ApplicationStartupOptions
{
	#region Constructors: Public

	public ApplicationStartupOptions(string? configurationFilePathOverride = null)
	{
		ConfigurationFilePathOverride = string.IsNullOrWhiteSpace(configurationFilePathOverride)
			? null
			: configurationFilePathOverride;
	}

	#endregion

	#region Properties: Public

	public string? ConfigurationFilePathOverride { get; }

	public bool HasCustomConfigurationFilePath => !string.IsNullOrWhiteSpace(ConfigurationFilePathOverride);

	#endregion
}

#endregion
