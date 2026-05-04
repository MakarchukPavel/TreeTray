using TreeTray.Abstractions;
using TreeTray.Models;
using TreeTray.Services;

namespace TreeTray.Tests;

public sealed class LauncherCatalogServiceTests
{
	[Fact]
	public void Build_WhenNestedLaunchersAreAddedAfterInitialScan_RescansSubdirectoriesRecursively()
	{
		using var fixture = new LauncherCatalogFixture();
		var service = new LauncherCatalogService(new TestPlatformLauncherResolver());

		File.WriteAllText(Path.Combine(fixture.RootDirectory, "Root.lnk"), string.Empty);
		var initialSnapshot = service.Build(fixture.CreateConfiguration());

		var levelOneDirectory = Directory.CreateDirectory(Path.Combine(fixture.RootDirectory, "Level1"));
		var levelTwoDirectory = Directory.CreateDirectory(Path.Combine(levelOneDirectory.FullName, "Level2"));
		var levelThreeDirectory = Directory.CreateDirectory(Path.Combine(levelTwoDirectory.FullName, "Level3"));
		File.WriteAllText(Path.Combine(levelThreeDirectory.FullName, "Nested.lnk"), string.Empty);

		var reloadedSnapshot = service.Build(fixture.CreateConfiguration());

		Assert.Equal(1, initialSnapshot.LauncherCount);
		Assert.Equal(2, reloadedSnapshot.LauncherCount);

		var levelOneEntry = Assert.Single(reloadedSnapshot.RootEntries, entry => entry.DisplayName == "Level1");
		var levelTwoEntry = Assert.Single(levelOneEntry.Children, entry => entry.DisplayName == "Level2");
		var levelThreeEntry = Assert.Single(levelTwoEntry.Children, entry => entry.DisplayName == "Level3");
		var nestedLauncherEntry = Assert.Single(levelThreeEntry.Children);
		Assert.Equal("Nested", nestedLauncherEntry.DisplayName);
		Assert.Equal(LauncherEntryType.Launcher, nestedLauncherEntry.EntryType);
	}

	[Fact]
	public void Build_WhenSubdirectoryDisappearsDuringScan_KeepsOtherEntries()
	{
		using var fixture = new LauncherCatalogFixture();
		var volatileDirectoryPath = Path.Combine(fixture.RootDirectory, "Volatile");
		Directory.CreateDirectory(volatileDirectoryPath);
		File.WriteAllText(Path.Combine(volatileDirectoryPath, "Nested.lnk"), string.Empty);
		File.WriteAllText(Path.Combine(fixture.RootDirectory, "Stable.lnk"), string.Empty);

		var service = new LauncherCatalogService(new DeletingPlatformLauncherResolver(volatileDirectoryPath));

		var snapshot = service.Build(fixture.CreateConfiguration());

		Assert.Single(snapshot.RootEntries);
		Assert.Equal("Stable", snapshot.RootEntries[0].DisplayName);
		Assert.Equal(1, snapshot.LauncherCount);
	}

	private sealed class LauncherCatalogFixture : IDisposable
	{
		public LauncherCatalogFixture()
		{
			RootDirectory = Directory.CreateDirectory(
				Path.Combine(Path.GetTempPath(), "TreeTray.Tests", Guid.NewGuid().ToString("N"))).FullName;
		}

		public string RootDirectory { get; }

		public TreeTrayConfiguration CreateConfiguration()
		{
			return new TreeTrayConfiguration
			{
				LaunchersDirectory = RootDirectory
			};
		}

		public void Dispose()
		{
			if (Directory.Exists(RootDirectory))
			{
				Directory.Delete(RootDirectory, recursive: true);
			}
		}
	}

	private class TestPlatformLauncherResolver : IPlatformLauncherResolver
	{
		public LaunchCommand CreateLaunchCommand(string path) => new(path);

		public string GetDisplayName(string path) => Path.GetFileNameWithoutExtension(path);

		public virtual bool IsLauncherPath(string path) => File.Exists(path) && string.Equals(Path.GetExtension(path), ".lnk", StringComparison.OrdinalIgnoreCase);
	}

	private sealed class DeletingPlatformLauncherResolver : TestPlatformLauncherResolver
	{
		private readonly string _volatileDirectoryPath;

		public DeletingPlatformLauncherResolver(string volatileDirectoryPath)
		{
			_volatileDirectoryPath = volatileDirectoryPath;
		}

		public override bool IsLauncherPath(string path)
		{
			if (string.Equals(path, _volatileDirectoryPath, StringComparison.OrdinalIgnoreCase) && Directory.Exists(path))
			{
				Directory.Delete(path, recursive: true);
			}

			return base.IsLauncherPath(path);
		}
	}
}
