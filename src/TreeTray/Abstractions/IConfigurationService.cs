#region Interface: IConfigurationService

namespace TreeTray.Abstractions;

public interface IConfigurationService
{
	#region Methods: Public

	TreeTrayConfiguration Load();

	void Save(TreeTrayConfiguration configuration);

	#endregion
}

#endregion
