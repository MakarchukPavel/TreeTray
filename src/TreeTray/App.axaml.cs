#region Class: App

namespace TreeTray;

public partial class App : Application
{
	#region Methods: Public

	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		var bootstrapper = Program.Services.GetRequiredService<IApplicationBootstrapper>();
		bootstrapper.Initialize(this);

		base.OnFrameworkInitializationCompleted();
	}

	#endregion
}

#endregion
