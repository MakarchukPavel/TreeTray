using TreeTray.Models;
using TreeTray.Services;

namespace TreeTray.Tests;

public sealed class LauncherExecutionServiceTests
{
	[Fact]
	public void EnsureLaunchStarted_AllowsNullProcessForShellExecuteLaunches()
	{
		var command = new LaunchCommand(
			@"C:\Launchers\Microsoft Teams.lnk",
			useShellExecute: true);

		var exception = Record.Exception(() => LauncherExecutionService.EnsureLaunchStarted(null, command));

		Assert.Null(exception);
	}

	[Fact]
	public void EnsureLaunchStarted_ThrowsForDirectProcessLaunches()
	{
		var command = new LaunchCommand("cmd.exe");

		var exception = Assert.Throws<InvalidOperationException>(
			() => LauncherExecutionService.EnsureLaunchStarted(null, command));

		Assert.Equal("Failed to launch 'cmd.exe'.", exception.Message);
	}
}
