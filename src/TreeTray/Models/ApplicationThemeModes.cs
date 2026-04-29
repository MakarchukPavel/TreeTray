#region Class: ApplicationThemeModes

namespace TreeTray.Models;

public static class ApplicationThemeModes
{
	#region Fields: Public

	public const string Dark = "Dark";

	public const string Light = "Light";

	public const string System = "System";

	#endregion

	#region Methods: Public

	public static string Normalize(string? value)
	{
		if (string.Equals(value, Dark, StringComparison.OrdinalIgnoreCase))
		{
			return Dark;
		}

		if (string.Equals(value, Light, StringComparison.OrdinalIgnoreCase))
		{
			return Light;
		}

		return System;
	}

	#endregion
}

#endregion
