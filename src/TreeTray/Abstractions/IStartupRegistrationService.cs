#region Interface: IStartupRegistrationService

namespace TreeTray.Abstractions;

public interface IStartupRegistrationService
{
	#region Methods: Public

	void Apply(TreeTrayConfiguration configuration);

	#endregion
}

#endregion
