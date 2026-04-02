#region Interface: ILauncherCatalogService

namespace TreeTray.Abstractions;

public interface ILauncherCatalogService
{
	#region Methods: Public

	LauncherSnapshot Build(TreeTrayConfiguration configuration);

	#endregion
}

#endregion
