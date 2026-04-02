#region Class: PlatformLauncherResolver

namespace TreeTray.Services;

public sealed class PlatformLauncherResolver : IPlatformLauncherResolver
{
	#region Fields: Private

	private static readonly string[] MacExtensions =
	[
		".alias",
		".app",
		".command",
		".sh",
		".workflow"
	];

	private static readonly string[] LinuxExtensions =
	[
		".appimage",
		".desktop",
		".sh"
	];

	#endregion

	#region Methods: Private

	private static string GetExtension(string path)
	{
		return Path.GetExtension(path).ToLowerInvariant();
	}

	private static string GetNameWithoutBundleExtension(string path)
	{
		var name = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

		if (string.Equals(Path.GetExtension(name), ".app", StringComparison.OrdinalIgnoreCase))
		{
			return Path.GetFileNameWithoutExtension(name);
		}

		return Directory.Exists(path) ? name : Path.GetFileNameWithoutExtension(name);
	}

	private static bool IsExecutableOnLinux(string path)
	{
		if (!OperatingSystem.IsLinux() || Directory.Exists(path) || !File.Exists(path))
		{
			return false;
		}

		try
		{
			var unixFileMode = File.GetUnixFileMode(path);
			return unixFileMode.HasFlag(UnixFileMode.UserExecute)
				|| unixFileMode.HasFlag(UnixFileMode.GroupExecute)
				|| unixFileMode.HasFlag(UnixFileMode.OtherExecute);
		}
		catch
		{
			return false;
		}
	}

	private static bool IsSymbolicLink(string path)
	{
		try
		{
			if (Directory.Exists(path))
			{
				return new DirectoryInfo(path).LinkTarget is not null;
			}

			return new FileInfo(path).LinkTarget is not null;
		}
		catch
		{
			return false;
		}
	}

	private static IReadOnlyList<string> ParseDesktopExecTokens(string execValue)
	{
		var tokens = new List<string>();
		var buffer = new StringBuilder();
		var isInQuotes = false;
		char quoteCharacter = '\0';

		for (var index = 0; index < execValue.Length; index++)
		{
			var currentCharacter = execValue[index];

			if ((currentCharacter == '"' || currentCharacter == '\'')
				&& (!isInQuotes || currentCharacter == quoteCharacter))
			{
				if (isInQuotes && currentCharacter == quoteCharacter)
				{
					isInQuotes = false;
					quoteCharacter = '\0';
				}
				else
				{
					isInQuotes = true;
					quoteCharacter = currentCharacter;
				}

				continue;
			}

			if (!isInQuotes && char.IsWhiteSpace(currentCharacter))
			{
				if (buffer.Length > 0)
				{
					tokens.Add(buffer.ToString());
					buffer.Clear();
				}

				continue;
			}

			if (currentCharacter == '\\' && index + 1 < execValue.Length)
			{
				index++;
				buffer.Append(execValue[index]);
				continue;
			}

			buffer.Append(currentCharacter);
		}

		if (buffer.Length > 0)
		{
			tokens.Add(buffer.ToString());
		}

		return tokens
			.Select(token => token.StartsWith("%", StringComparison.Ordinal) ? string.Empty : token.Replace("%%", "%", StringComparison.Ordinal))
			.Where(token => !string.IsNullOrWhiteSpace(token))
			.ToArray();
	}

	private static string? ResolveDesktopExecValue(string path)
	{
		foreach (var line in File.ReadLines(path))
		{
			if (line.StartsWith("Exec=", StringComparison.Ordinal))
			{
				return line["Exec=".Length..].Trim();
			}
		}

		return null;
	}

	private static string? ResolveWorkingDirectory(string path)
	{
		if (Directory.Exists(path))
		{
			return Path.GetDirectoryName(path);
		}

		return Path.GetDirectoryName(Path.GetFullPath(path));
	}

	#endregion

	#region Methods: Public

	public LaunchCommand CreateLaunchCommand(string path)
	{
		ArgumentNullException.ThrowIfNull(path);

		if (OperatingSystem.IsWindows())
		{
			return new LaunchCommand(path, Array.Empty<string>(), ResolveWorkingDirectory(path), useShellExecute: true);
		}

		if (OperatingSystem.IsMacOS())
		{
			return new LaunchCommand("open", new[] { path }, ResolveWorkingDirectory(path), useShellExecute: false);
		}

		if (OperatingSystem.IsLinux()
			&& string.Equals(GetExtension(path), ".desktop", StringComparison.Ordinal))
		{
			var execValue = ResolveDesktopExecValue(path);
			var tokens = string.IsNullOrWhiteSpace(execValue)
				? Array.Empty<string>()
				: ParseDesktopExecTokens(execValue);

			if (tokens.Count > 0)
			{
				return new LaunchCommand(tokens[0], tokens.Skip(1), ResolveWorkingDirectory(path), useShellExecute: false);
			}

			return new LaunchCommand("xdg-open", new[] { path }, ResolveWorkingDirectory(path), useShellExecute: false);
		}

		return new LaunchCommand(path, Array.Empty<string>(), ResolveWorkingDirectory(path), useShellExecute: false);
	}

	public string GetDisplayName(string path)
	{
		ArgumentNullException.ThrowIfNull(path);
		return GetNameWithoutBundleExtension(path);
	}

	public bool IsLauncherPath(string path)
	{
		ArgumentNullException.ThrowIfNull(path);

		var extension = GetExtension(path);

		if (OperatingSystem.IsWindows())
		{
			return !Directory.Exists(path) && File.Exists(path);
		}

		if (OperatingSystem.IsMacOS())
		{
			if (Directory.Exists(path))
			{
				return string.Equals(extension, ".app", StringComparison.OrdinalIgnoreCase);
			}

			return MacExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)
				|| IsSymbolicLink(path)
				|| string.IsNullOrWhiteSpace(extension);
		}

		if (OperatingSystem.IsLinux())
		{
			if (Directory.Exists(path))
			{
				return false;
			}

			return LinuxExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)
				|| IsExecutableOnLinux(path)
				|| IsSymbolicLink(path);
		}

		return !Directory.Exists(path) && File.Exists(path);
	}

	#endregion
}

#endregion
