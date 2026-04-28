using TreeTray.Models;

namespace TreeTray.Tests;

public sealed class LauncherEntryLaunchBatchResolverTests
{
	[Fact]
	public void GetDirectLaunchableChildren_ReturnsOnlyDirectLaunchers()
	{
		var nestedLauncher = CreateLauncher("Nested launcher");
		var directLauncher = CreateLauncher("Direct launcher");
		var folderEntry = new LauncherEntry(
			LauncherEntryType.Folder,
			"Root folder",
			@"C:\Launchers\Root",
			children:
			[
				directLauncher,
				new LauncherEntry(
					LauncherEntryType.Folder,
					"Nested folder",
					@"C:\Launchers\Root\Nested",
					children: [nestedLauncher])
			]);

		var result = LauncherEntryLaunchBatchResolver.GetDirectLaunchableChildren(folderEntry);

		Assert.Single(result);
		Assert.Same(directLauncher, result[0]);
	}

	[Fact]
	public void GetDirectLaunchableChildren_ReturnsEmptyListForLauncher()
	{
		var launcherEntry = CreateLauncher("Standalone launcher");

		var result = LauncherEntryLaunchBatchResolver.GetDirectLaunchableChildren(launcherEntry);

		Assert.Empty(result);
	}

	private static LauncherEntry CreateLauncher(string displayName)
	{
		return new LauncherEntry(
			LauncherEntryType.Launcher,
			displayName,
			$@"C:\Launchers\{displayName}.lnk",
			new LaunchCommand("cmd.exe"));
	}
}
