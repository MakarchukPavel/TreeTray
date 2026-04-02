#region Class: Program

namespace TreeTray;

public static class Program
{
	#region Fields: Private

	private static ServiceProvider? _serviceProvider;

	#endregion

	#region Properties: Public

	public static IServiceProvider Services => _serviceProvider
		?? throw new InvalidOperationException("The service provider has not been created yet.");

	#endregion

	#region Methods: Public

	[STAThread]
	public static void Main(string[] args)
	{
		var startupOptions = Infrastructure.ApplicationStartupOptionsParser.Parse(args);
		_serviceProvider = Infrastructure.ServiceCollectionExtensions.CreateServiceProvider(startupOptions);
		BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
	}

	public static AppBuilder BuildAvaloniaApp()
	{
		return AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.LogToTrace();
	}

	#endregion
}

#endregion
