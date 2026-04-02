#region Class: ApplicationBootstrapper

namespace TreeTray.Services;

public sealed class ApplicationBootstrapper : IApplicationBootstrapper
{
	#region Fields: Private

	private readonly IApplicationController _applicationController;

	#endregion

	#region Constructors: Public

	public ApplicationBootstrapper(IApplicationController applicationController)
	{
		_applicationController = applicationController;
	}

	#endregion

	#region Methods: Public

	public void Initialize(Application application)
	{
		_applicationController.Start(application);
	}

	#endregion
}

#endregion
