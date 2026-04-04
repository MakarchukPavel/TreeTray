#region Class: LauncherExecutionService

namespace TreeTray.Services;

public sealed class LauncherExecutionService : ILauncherExecutionService
{
	#region Methods: Private

	private static string BuildLaunchErrorMessage(LauncherEntry entry, Exception exception)
	{
		if (entry.Command is not null && IsBrokenShortcut(entry.Command, exception))
		{
			return string.Format(
				CultureInfo.CurrentCulture,
				"The program referenced by '{0}' was not found. Check the shortcut target or remove the broken launcher.{1}{1}Shortcut: {2}",
				entry.DisplayName,
				Environment.NewLine,
				entry.SourcePath);
		}

		if (IsMissingProgram(exception))
		{
			return string.Format(
				CultureInfo.CurrentCulture,
				"The program for '{0}' was not found.{1}{1}Path: {2}",
				entry.DisplayName,
				Environment.NewLine,
				entry.SourcePath);
		}

		return string.Format(
			CultureInfo.CurrentCulture,
			"TreeTray could not launch '{0}'.{1}{1}{2}",
			entry.DisplayName,
			Environment.NewLine,
			exception.Message);
	}

	private static bool IsBrokenShortcut(LaunchCommand command, Exception exception)
	{
		return OperatingSystem.IsWindows()
			&& command.UseShellExecute
			&& string.Equals(Path.GetExtension(command.FileName), ".lnk", StringComparison.OrdinalIgnoreCase)
			&& exception is Win32Exception { NativeErrorCode: 1223 or 2 or 3 };
	}

	private static bool IsMissingProgram(Exception exception)
	{
		return exception is FileNotFoundException
			or DirectoryNotFoundException
			or Win32Exception { NativeErrorCode: 2 or 3 };
	}

	private static void RunCommand(LaunchCommand command)
	{
		var processStartInfo = new ProcessStartInfo
		{
			FileName = command.FileName,
			UseShellExecute = command.UseShellExecute,
			WorkingDirectory = command.WorkingDirectory ?? AppContext.BaseDirectory
		};

		if (command.UseShellExecute)
		{
			if (command.Arguments.Count > 0)
			{
				processStartInfo.Arguments = string.Join(' ', command.Arguments);
			}
		}
		else
		{
			foreach (var argument in command.Arguments)
			{
				processStartInfo.ArgumentList.Add(argument);
			}
		}

		if (Process.Start(processStartInfo) is null)
		{
			throw new InvalidOperationException($"Failed to launch '{command.FileName}'.");
		}
	}

	private static LaunchCommand ResolveOpenDirectoryCommand(string directoryPath)
	{
		if (OperatingSystem.IsWindows())
		{
			return new LaunchCommand("explorer.exe", new[] { directoryPath }, useShellExecute: false);
		}

		if (OperatingSystem.IsMacOS())
		{
			return new LaunchCommand("open", new[] { directoryPath }, useShellExecute: false);
		}

		return new LaunchCommand("xdg-open", new[] { directoryPath }, useShellExecute: false);
	}

	#endregion

	#region Methods: Public

	public void Launch(LauncherEntry entry)
	{
		ArgumentNullException.ThrowIfNull(entry);

		if (entry.Command is null)
		{
			return;
		}

		try
		{
			RunCommand(entry.Command);
		}
		catch (Exception exception) when (exception is InvalidOperationException
			or FileNotFoundException
			or DirectoryNotFoundException
			or Win32Exception)
		{
			throw new LauncherExecutionException(BuildLaunchErrorMessage(entry, exception), exception);
		}
	}

	public void OpenDirectory(string directoryPath)
	{
		ArgumentNullException.ThrowIfNull(directoryPath);

		Directory.CreateDirectory(directoryPath);
		RunCommand(ResolveOpenDirectoryCommand(directoryPath));
	}

	#endregion
}

#endregion
