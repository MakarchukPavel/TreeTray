#region Class: LauncherExecutionService

namespace TreeTray.Services;

public sealed class LauncherExecutionService : ILauncherExecutionService
{
	#region Methods: Private

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

		Process.Start(processStartInfo);
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

		RunCommand(entry.Command);
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
