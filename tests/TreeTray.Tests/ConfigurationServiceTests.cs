using TreeTray.Abstractions;
using TreeTray.Models;
using TreeTray.Services;

namespace TreeTray.Tests;

public sealed class ConfigurationServiceTests
{
	[Fact]
	public void Load_WhenConfigDoesNotExist_CreatesDefaultConfigWithCtrlClickFlagEnabled()
	{
		using var fixture = new TestApplicationPaths();
		var service = new ConfigurationService(fixture);

		var configuration = service.Load();

		Assert.True(configuration.EnableCtrlLeftClickToLaunchFolderChildren);
		Assert.True(File.Exists(fixture.ConfigurationFilePath));
		var xml = File.ReadAllText(fixture.ConfigurationFilePath);
		Assert.Contains("<EnableCtrlLeftClickToLaunchFolderChildren>true</EnableCtrlLeftClickToLaunchFolderChildren>", xml, StringComparison.Ordinal);
	}

	[Fact]
	public void Load_ReadsCtrlClickFlagFromExistingConfiguration()
	{
		using var fixture = new TestApplicationPaths();
		Directory.CreateDirectory(fixture.ConfigurationDirectory);
		File.WriteAllText(
			fixture.ConfigurationFilePath,
			"""
			<?xml version="1.0" encoding="utf-8"?>
			<TreeTrayConfiguration>
			  <LaunchersDirectory>Launchers</LaunchersDirectory>
			  <EnableTrayIcon>true</EnableTrayIcon>
			  <EnableCtrlLeftClickToLaunchFolderChildren>false</EnableCtrlLeftClickToLaunchFolderChildren>
			</TreeTrayConfiguration>
			""");
		var service = new ConfigurationService(fixture);

		var configuration = service.Load();

		Assert.False(configuration.EnableCtrlLeftClickToLaunchFolderChildren);
		Assert.Equal(Path.GetFullPath("Launchers", fixture.ConfigurationDirectory), configuration.LaunchersDirectory);
	}

	private sealed class TestApplicationPaths : IApplicationPaths, IDisposable
	{
		private readonly string _rootDirectory = Path.Combine(Path.GetTempPath(), "TreeTray.Tests", Guid.NewGuid().ToString("N"));

		public string ApplicationDirectory => _rootDirectory;

		public string ConfigurationDirectory => Path.Combine(_rootDirectory, "config");

		public string ConfigurationFilePath => Path.Combine(ConfigurationDirectory, "TreeTray.settings.xml");

		public bool HasCustomConfigurationFilePath => true;

		public LaunchCommand GetCurrentApplicationLaunchCommand() => new("TreeTray.exe");

		public string GetDefaultLaunchersDirectory() => Path.Combine(_rootDirectory, "launchers");

		public void Dispose()
		{
			if (Directory.Exists(_rootDirectory))
			{
				Directory.Delete(_rootDirectory, recursive: true);
			}
		}
	}
}
